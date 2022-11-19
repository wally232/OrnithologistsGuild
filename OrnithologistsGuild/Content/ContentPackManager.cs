﻿using System;
using System.Collections.Generic;
using System.IO;
using StardewModdingAPI;

namespace OrnithologistsGuild.Content
{
    public class ContentPackManager
    {
        private const string FILENAME = "content.json";

        internal static ContentPatcher.IContentPatcherAPI CP;

        public static List<ContentPackDef> ContentPackDefs = new List<ContentPackDef>();

        public static Dictionary<string, BirdieDef> BirdieDefs = new Dictionary<string, BirdieDef>();

        public static void Initialize()
        {
            CP = ModEntry.Instance.Helper.ModRegistry.GetApi<ContentPatcher.IContentPatcherAPI>("Pathoschild.ContentPatcher");

            ModEntry.Instance.Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        public static void LoadAll()
        {
            foreach (IContentPack contentPack in ModEntry.Instance.Helper.ContentPacks.GetOwned())
            {
                Load(contentPack);
            }
        }

        public static void Load(IContentPack contentPack)
        {
            ModEntry.Instance.Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");

            if (!contentPack.HasFile(FILENAME))
            {
                ModEntry.Instance.Monitor.Log(@$"Error in content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version}: a ""{FILENAME}"" file is required", LogLevel.Error);
                return;
            }

            ContentPackDef contentPackDef;
            try
            {
                contentPackDef = contentPack.ReadJsonFile<ContentPackDef>(FILENAME);
                if (contentPackDef.FormatVersion != ContentPackDef.FORMAT_VERSION)
                {
                    throw new Exception($"FormatVersion must be {ContentPackDef.FORMAT_VERSION} (got {contentPackDef.FormatVersion})");
                }

                contentPackDef.ContentPack = contentPack;
                ContentPackDefs.Add(contentPackDef);
            }
            catch (Exception e)
            {
                ModEntry.Instance.Monitor.Log(@$"Error in content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version}: {e.ToString()}", LogLevel.Error);
                return;
            }

            foreach (var birdieDef in contentPackDef.Birdies)
            {
                try
                {
                    birdieDef.UniqueID = $"{contentPack.Manifest.UniqueID}.birdie.{birdieDef.ID}";
                    birdieDef.ContentPackDef = contentPackDef;
                    birdieDef.LoadAssets(contentPack);

                    BirdieDefs.Add(birdieDef.UniqueID, birdieDef);
                }
                catch (Exception e)
                {
                    ModEntry.Instance.Monitor.Log(@$"Error in content pack: {contentPackDef.ContentPack.Manifest.Name} {contentPackDef.ContentPack.Manifest.Version} (in {birdieDef.ID}): {e.ToString()}", LogLevel.Error);
                }
            }

        }

        public static void LoadBuiltIn()
        {
            IContentPack contentPack = ModEntry.Instance.Helper.ContentPacks.CreateTemporary(
               directoryPath: Path.Combine(ModEntry.Instance.Helper.DirectoryPath, "content-pack"),
               id: "BuiltIn",
               name: "Stardew Valley birds",
               description: "Built-in Stardew Valley birds.",
               author: "ConcernedApe",
               version: ModEntry.Instance.ModManifest.Version
            );

            Load(contentPack);
        }

        private static void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs _)
        {
            // Parse conditions on second tick
            if (CP.IsConditionsApiReady)
            {
                ModEntry.Instance.Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;

                // Parse all conditions
                foreach (var contentPackDef in ContentPackDefs)
                {
                    foreach (var birdieDef in contentPackDef.Birdies)
                    {
                        try
                        {
                            birdieDef.ParseConditions(CP);
                        } catch (Exception e)
                        {
                            ModEntry.Instance.Monitor.Log(@$"Error in content pack: {contentPackDef.ContentPack.Manifest.Name} {contentPackDef.ContentPack.Manifest.Version} (in {birdieDef.ID}): {e.ToString()}", LogLevel.Error);
                        }
                    }
                }
            }
        }
    }
}