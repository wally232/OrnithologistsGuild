﻿using System;
using System.Xml.Serialization;
using StardewValley;

namespace OrnithologistsGuild.Game.Items
{
    [XmlType("Mods_Ivy_OrnithologistsGuild_JojaBinoculars")]
    public class JojaBinoculars : Binoculars
    {
        public JojaBinoculars(): base(ModEntry.dgaPack.Find("JojaBinoculars"), 4)
        {
        }

        public JojaBinoculars(string arg) : this()
        {
            // Required for DGA
        }

        public override bool performUseAction(GameLocation location)
        {
            if (Game1.random.NextDouble() < 0.1)
            {
                Game1.drawObjectDialogue("Binoculars jammed when you tried to focus them");

                return false;
            }

            return base.performUseAction(location);
        }

        public override Item getOne()
        {
            var ret = new JojaBinoculars();
            ret.Quality = this.Quality;
            ret.Stack = 1;
            ret.Price = this.Price;
            ret.ObjectColor = this.ObjectColor;
            ret._GetOneFrom(this);
            return ret;
        }
    }
}

