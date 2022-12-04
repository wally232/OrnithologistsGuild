﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrnithologistsGuild.Content;
using StardewValley;
using System.Collections.Generic;
using StateMachine;
using StardewValley.TerrainFeatures;

namespace OrnithologistsGuild.Game.Critters
{
    public partial class BetterBirdie : StardewValley.BellsAndWhistles.Critter
    {
        public BirdieDef BirdieDef;
        private GameLocation Environment;

        public Fsm<BetterBirdieState, BetterBirdieTrigger> StateMachine;

        public bool IsFlying { get {
            return StateMachine.Current.Identifier == BetterBirdieState.Relocating || StateMachine.Current.Identifier == BetterBirdieState.FlyingAway;
        } }

        // Perch
        public Perch Perch;
        public bool IsPerched
        {
            get { return Perch != null; }
        }
        public bool IsRoosting
        {
            get { return IsPerched && Perch.Tree != null; }
        }

        // Timers
        private int CharacterCheckTimer = 200;

        public bool IsSpotted; // Whether player has spotted bird with binoculars

        private float FlightOffset; // Individual birds fly at slightly different speeds

        public BetterBirdie(BirdieDef birdieDef, int tileX, int tileY, Perch perch = null) : base(0, Vector2.Zero)
        {
            BirdieDef = birdieDef;
            Perch = perch;

            if (birdieDef.AssetPath != null)
            {
                var internalAssetName = birdieDef.ContentPackDef.ContentPack.ModContent.GetInternalAssetName(birdieDef.AssetPath).BaseName;

                baseFrame = birdieDef.BaseFrame;
                sprite = new AnimatedSprite(internalAssetName, baseFrame, 32, 32);
            } else
            {
                baseFrame = birdieDef.BaseFrame;
                sprite = new AnimatedSprite(critterTexture, baseFrame, 32, 32);
            }

            flip = Game1.random.NextDouble() < 0.5;

            // Determine position
            if (Perch == null)
            {
                position = new Vector2(tileX * Game1.tileSize, tileY * Game1.tileSize);

                // Center on tile
                position.X += Game1.tileSize / 2;
                position.Y += Game1.tileSize / 2;
            } else
            {
                position = Perch.Position;
            }
            startingPosition = position;

            FlightOffset = (float)Game1.random.NextDouble() - 0.5f;

            InitializeStateMachine();
        }

        public override bool update(GameTime time, GameLocation environment)
        {
            Environment = environment;

            if (!IsFlying)
            {
                if (IsRoosting)
                {
                    // Fly away when tree is chopped
                    if (Perch.Tree.health.Value < Tree.startingHealth)
                    {
                        Frighten();
                    }

                    // Fly away when tree is shaken (see TreePatches)
                }
                else
                {
                    CheckCharacterProximity(time, environment);
                }
            }

            StateMachine.Update(time.ElapsedGameTime);

            updateEmote(time);

            return base.update(time, environment);
        }

        public Tuple<Vector2, Perch> GetRandomRelocationTileOrPerch()
        {
            if (Game1.random.NextDouble() < 0) // TODO 0.7
            {
                // Try to find clear tile to relocate to
                for (int trial = 0; trial < 50; trial++)
                {
                    var randomTile = Environment.getRandomTile();

                    // Get a 3x3 patch around the random tile
                    var randomRect = new Microsoft.Xna.Framework.Rectangle((int)randomTile.X - 1, (int)randomTile.Y - 1, 3, 3);

                    if (Environment.isAreaClear(randomRect) && Utility.isThereAFarmerOrCharacterWithinDistance(randomTile, BirdieDef.GetContextualCautiousness(), Environment) != null) continue; // Character nearby

                    var position = randomTile * Game1.tileSize;

                    // Center on tile
                    position.X += 32f;
                    position.Y += 32f;

                    var distance = Vector2.Distance(base.position, position);
                    if (distance < 10 * Game1.tileSize || distance > 50 * Game1.tileSize) continue; // Too close/far

                    var distanceX = MathF.Abs(base.position.X - position.X);
                    var distanceY = MathF.Abs(base.position.Y - position.Y);
                    if (distanceX < 5 * Game1.tileSize || distanceY < 5 * Game1.tileSize) continue; // Too straight (lol)

                    return new Tuple<Vector2, Perch>(position, null);
                }
            }
            else
            {
                // Try to find an available perch to relocate to
                var perch = Perch.GetRandomAvailablePerch(BirdieDef, Game1.player.currentLocation); 
                if (perch != null)
                {
                    return new Tuple<Vector2, Perch>(perch.Position, perch);
                }
            }

            return null;
        }

        private void CheckCharacterProximity(GameTime time, GameLocation environment)
        {
            CharacterCheckTimer -= time.ElapsedGameTime.Milliseconds;
            if (CharacterCheckTimer < 0)
            {
                CharacterCheckTimer = 200;

                if (!IsFlying && Utility.isThereAFarmerOrCharacterWithinDistance(position / Game1.tileSize, BirdieDef.GetContextualCautiousness(), environment) != null)
                {
                    Frighten();
                }
            }
        }

        public void Frighten() {
            StateMachine.Trigger(Game1.random.NextDouble() < 0 ? BetterBirdieTrigger.FlyAway : BetterBirdieTrigger.Relocate); // TODO 0.7
        }
    
        #region Rendering
        private void drawEmote(SpriteBatch b)
        {
            if (isEmoting)
            {
                Vector2 localPosition = getLocalPosition(Game1.viewport);
                localPosition.Y -= 118f;
                b.Draw(Game1.emoteSpriteSheet, localPosition, new Microsoft.Xna.Framework.Rectangle(currentEmoteFrame * 16 % Game1.emoteSpriteSheet.Width, currentEmoteFrame * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White /** alpha*/, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)position.Y / 10000f);
            }
        }

        public override void drawAboveFrontLayer(SpriteBatch b)
        {
            if (IsFlying || IsRoosting)
            {
                base.draw(b);
                drawEmote(b);
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!(IsFlying || IsRoosting))
            {
                base.draw(b);
                drawEmote(b);
            }
        }

        public Vector2 getLocalPosition(xTile.Dimensions.Rectangle viewport)
        {
            Vector2 vector = position;
            return new Vector2(vector.X - (float)viewport.X - (0.5f * Game1.tileSize), vector.Y - (float)viewport.Y + (float)yJumpOffset);
        }

        private List<FarmerSprite.AnimationFrame> GetFlyingAnimation()
        {
            return new List<FarmerSprite.AnimationFrame> {
                    new FarmerSprite.AnimationFrame (baseFrame + 6, (int)MathF.Round(0.27f * BirdieDef.FlapDuration)),
                    new FarmerSprite.AnimationFrame (baseFrame + 7, (int)MathF.Round(0.23f * BirdieDef.FlapDuration), secondaryArm: false, flip, frameBehavior: (Farmer who) =>
                    {
                        // Make bird shoot up a bit while flapping for more realistic flight
                        // e.g. flapDuration = 500, gravityAffectedDY = 4
                        // e.g. flapDuration = 250, gravityAffectedDY = 2
                        gravityAffectedDY = -(BirdieDef.FlapDuration * (4f/500f));

                        // Play flapping noise
                        if (Utility.isOnScreen(position, Game1.tileSize)) Game1.playSound("batFlap");
                    }),
                    new FarmerSprite.AnimationFrame (baseFrame + 8, (int)MathF.Round(0.27f * BirdieDef.FlapDuration)),
                    new FarmerSprite.AnimationFrame (baseFrame + 7, (int)MathF.Round(0.23f * BirdieDef.FlapDuration))
                };
        }
        #endregion
    }
}
