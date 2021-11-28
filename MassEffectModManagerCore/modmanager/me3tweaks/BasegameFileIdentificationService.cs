﻿//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using LegendaryExplorerCore.Gammtek.Extensions;
//using LegendaryExplorerCore.Misc;
//using LegendaryExplorerCore.Packages;
//using ME3TweaksModManager.modmanager.diagnostics;
//using ME3TweaksModManager.modmanager.objects.mod;
//using ME3TweaksCoreWPF;
//using Newtonsoft.Json;

//namespace ME3TweaksModManager.modmanager.me3tweaks
//{
//    public class BasegameFileIdentificationService
//    {

//        private static Dictionary<string, CaseInsensitiveDictionary<List<BasegameFileIdentificationService.BasegameCloudDBFile>>> LocalBasegameFileIdentificationService;

//        private static void LoadLocalBasegameIdentificationService()
//        {
//            if (LocalBasegameFileIdentificationService != null) return;

//            var file = M3Utilities.GetLocalBasegameIdentificationServiceFile();
//            if (File.Exists(file))
//            {
//                try
//                {
//                    LocalBasegameFileIdentificationService =
//                        JsonConvert
//                            .DeserializeObject<
//                                Dictionary<string, CaseInsensitiveDictionary<
//                                    List<BasegameFileIdentificationService.BasegameCloudDBFile>>>>(
//                                File.ReadAllText(file));
//                    M3Log.Information(@"Loaded Local Basegame File Identification Service");
//                }
//                catch (Exception e)
//                {
//                    M3Log.Error($@"Error loading local BGFIS: {e.Message}");
//                    LocalBasegameFileIdentificationService = MOnlineContent.getBlankBGFIDB();
//                }
//            }
//            else
//            {
//                M3Log.Information(@"Loaded blank Local Basegame File Identification Service");
//                LocalBasegameFileIdentificationService = getBlankBGFIDB();
//            }
//        }

//        public static void AddLocalBasegameIdentificationEntries(List<BasegameCloudDBFile> entries)
//        {
//            LoadLocalBasegameIdentificationService();

//            bool updated = false;
//            // Update the DB
//            foreach (var entry in entries)
//            {
//                string gameKey = entry.game == @"0" ? @"LELAUNCHER" : M3Utilities.GetGameFromNumber(entry.game).ToString();
//                if (LocalBasegameFileIdentificationService.TryGetValue(gameKey, out var gameDB))
//                {
//                    List<BasegameCloudDBFile> existingInfos;
//                    if (!gameDB.TryGetValue(entry.file, out existingInfos))
//                    {
//                        existingInfos = new List<BasegameCloudDBFile>();
//                        gameDB[entry.file] = existingInfos;
//                    }

//                    if (existingInfos.All(x => x.hash != entry.hash))
//                    {
//                        // new info
//                        entry.file = null; // Do not serialize this
//                        entry.game = null; // Do not serialize this
//                        existingInfos.Add(entry);
//                        updated = true;
//                    }
//                }
//            }

//            // Serialize it back to disk
//            if (updated)
//            {
//#if DEBUG
//                var outText = JsonConvert.SerializeObject(LocalBasegameFileIdentificationService, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
//#else
//                var outText = JsonConvert.SerializeObject(LocalBasegameFileIdentificationService, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
//#endif
//                try
//                {
//                    File.WriteAllText(M3Utilities.GetLocalBasegameIdentificationServiceFile(), outText);
//                    M3Log.Information(@"Updated Local Basegame File Identification Service");

//                }
//                catch (Exception e)
//                {
//                    // bwomp bwomp
//                    M3Log.Error($@"Error saving local BGFIS: {e.Message}");
//                }
//            }
//            else
//            {
//                M3Log.Information(@"Local Basegame File Identification Service did not need updating");

//            }
//        }


//        /// <summary>
//        /// Looks up information about a basegame file using the Basegame File Identification Service
//        /// </summary>
//        /// <param name="target"></param>
//        /// <param name="fullfilepath"></param>
//        /// <returns></returns>
//        public static BasegameCloudDBFile GetBasegameFileSource(GameTargetWPF target, string fullfilepath, string md5 = null)
//        {
//            // Check local first
//            LoadLocalBasegameIdentificationService();
//            if (LocalBasegameFileIdentificationService.TryGetValue(target.Game.ToString(), out var infosForGameL))
//            {
//                var relativeFilename = fullfilepath.Substring(target.TargetPath.Length + 1).ToUpper();

//                if (infosForGameL.TryGetValue(relativeFilename, out var items))
//                {
//                    md5 ??= M3Utilities.CalculateMD5(fullfilepath);
//                    var match = items.FirstOrDefault(x => x.hash == md5);
//                    if (match != null)
//                    {
//                        return match;
//                    }
//                }
//            }

//            if (App.BasegameFileIdentificationService == null) return null; // ME3Tweaks DB not loaded
//            if (App.BasegameFileIdentificationService.TryGetValue(target.Game.ToString(), out var infosForGame))
//            {
//                var relativeFilename = fullfilepath.Substring(target.TargetPath.Length + 1).ToUpper();

//                if (infosForGame.TryGetValue(relativeFilename, out var items))
//                {
//                    md5 ??= M3Utilities.CalculateMD5(fullfilepath);
//                    return items.FirstOrDefault(x => x.hash == md5);
//                }
//            }

//            return null;
//        }

//        public class BasegameCloudDBFile
//        {
//            public string file { get; set; }
//            public string hash { get; set; }
//            public string source { get; set; }
//            public string game { get; set; }
//            public int size { get; set; }
//            public BasegameCloudDBFile() { }
//            public BasegameCloudDBFile(string fullfilepath, int size, GameTargetWPF gameTarget, Mod modBeingInstalled, string md5 = null)
//            {
//                this.file = fullfilepath.Substring(gameTarget.TargetPath.Length + 1);
//                this.hash = md5 ?? M3Utilities.CalculateMD5(fullfilepath);
//                this.game = gameTarget.Game.ToGameNum().ToString();
//                this.size = size;
//                this.source = modBeingInstalled.ModName + @" " + modBeingInstalled.ModVersionString;
//            }

//            public BasegameCloudDBFile(string relativePathToRoot, int size, MEGame game, string humanName, string md5)
//            {
//                this.file = relativePathToRoot;
//                this.hash = md5 ?? M3Utilities.CalculateMD5(relativePathToRoot);
//                this.game = game.ToGameNum().ToString(); // due to how json serializes stuff we have to convert it here.
//                this.size = size;
//                this.source = humanName;
//            }
//        }
//    }
//}