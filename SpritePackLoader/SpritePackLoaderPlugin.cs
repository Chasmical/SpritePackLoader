using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using RogueLibsCore;

namespace SpritePackLoader
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(RogueLibs.GUID, "3.3.0")]
    public class SpritePackLoaderPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "abbysssal.streetsofrogue.spritepackloader";
        public const string PluginName = "SpritePackLoader";
        public const string PluginVersion = "1.1.0";

        public string PackPath { get; private set; } = null!;
        public string SpritePacksPath { get; private set; } = null!;
        public void Awake()
        {
            RoguePatcher patcher = new RoguePatcher(this);
            patcher.Prefix(typeof(AudioHandler), nameof(AudioHandler.SetupDics), nameof(AudioHandler_SetupDics_Prefix));
            patcher.Postfix(typeof(AudioHandler), nameof(AudioHandler.SetupDics));

            RogueLibs.CreateCustomUnlock(new MutatorUnlock("SpritePackTestingMode", true))
                .WithName(new CustomNameInfo
                {
                    English = "SpritePack Testing Mode",
                    Russian = @"Режим тестирования спрайтпаков",
                })
                .WithDescription(new CustomNameInfo
                {
                    English = "Gives the player an item to test item sprites",
                    Russian = @"Даёт игроку предмет для тестирования спрайтов предметов",
                });
            RogueLibs.CreateCustomItem<SpritePackTester>()
                .WithName(new CustomNameInfo
                {
                    English = "SpritePack Tester",
                    Russian = @"Тестер спрайтпаков",
                })
                .WithDescription(new CustomNameInfo
                {
                    English = "Spawns all available items' sprites around the player. Use again to destroy all sprites.",
                    Russian = @"Спавнит все спрайты доступных предметов вокруг игрока. Используйте ещё раз чтобы уничтожить все спрайты.",
                })
                .WithUnlock(new ItemUnlock
                {
                    IsAvailable = false,
                    IsAvailableInCC = false,
                    IsAvailableInItemTeleporter = true,
                    IsEnabled = false,

                });

            InitializeSprites();
        }

        private static readonly List<AudioClip> overrideClips = new List<AudioClip>();
        // ReSharper disable once IdentifierTypo
        public static void AudioHandler_SetupDics_Prefix(AudioHandler __instance, out bool __state)
            => __state = __instance.loadedDics;
        // ReSharper disable once IdentifierTypo
        public static void AudioHandler_SetupDics(AudioHandler __instance, ref bool __state)
        {
            if (__state) return;
            foreach (AudioClip clip in overrideClips)
            {
                __instance.audioClipList.Add(clip.name);
                __instance.audioClipRealList.Add(clip);
                __instance.audioClipDic[clip.name] = clip;
            }
            overrideClips.Clear();
        }
        public void InitializeSprites()
        {
            PackPath = Path.Combine(Paths.GameRootPath, "SpritePack");
            DirectoryInfo packDir = new DirectoryInfo(PackPath);
            if (packDir.Exists)
            {
                counterAll = 0;
                counterAudio = 0;
                Logger.LogMessage("Loading sprites from SpritePack directory...");
                DateTime start = DateTime.Now;
                ReadDirectory(packDir, new SpriteContext(null, 64));
                DateTime end = DateTime.Now;
                string? audioInsert = counterAudio > 0 ? $" and {counterAudio} audio files" : null;
                Logger.LogMessage($"Loaded {counterAll - counterAudio} sprites{audioInsert} in {(end - start).TotalSeconds:#.###} ms.");
            }
            SpritePacksPath = Path.Combine(Paths.BepInExRootPath, "spritepacks");
            DirectoryInfo spritePacksDir = new DirectoryInfo(SpritePacksPath);
            spritePacksDir.Create();
            foreach (FileInfo file in spritePacksDir.EnumerateFiles(@"*.spritepack", SearchOption.AllDirectories))
            {
                counterAll = 0;
                counterAudio = 0;
                Logger.LogMessage($"Loading sprites from {Path.GetFileNameWithoutExtension(file.FullName)} sprite pack.");
                DateTime start = DateTime.Now;
                ReadArchive(file, new SpriteContext(null, 64));
                DateTime end = DateTime.Now;
                string? audioInsert = counterAudio > 0 ? $" and {counterAudio} audio files" : null;
                Logger.LogMessage($"Loaded {counterAll - counterAudio} sprites{audioInsert} in {(end - start).TotalSeconds:#.###} ms.");
            }
        }

        private int counterAll;
        private int counterAudio;
        public void ReadDirectory(DirectoryInfo directory, SpriteContext dirCxt)
        {
            foreach (DirectoryInfo subDirectory in directory.EnumerateDirectories())
            {
                SpriteContext subDirCxt = new SpriteContext(dirCxt);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(subDirectory.FullName);
                ExtractContext(ref fileNameWithoutExt, ref subDirCxt);
                ReadDirectory(subDirectory, subDirCxt);
            }
            foreach (FileInfo file in directory.EnumerateFiles())
            {
                SpriteContext fileCxt = new SpriteContext(dirCxt);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FullName);
                ExtractContext(ref fileNameWithoutExt, ref fileCxt);
                byte[] rawData = File.ReadAllBytes(file.FullName);
                ReadSprite(fileNameWithoutExt, rawData, fileCxt, file.Extension);
            }
        }
        public void ReadArchive(FileInfo archiveFile, SpriteContext archiveCxt)
        {
            using (FileStream fileStream = archiveFile.OpenRead())
            using (ZipArchive archive = new ZipArchive(fileStream))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string[] paths = entry.FullName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    SpriteContext cxt = new SpriteContext(archiveCxt);
                    for (int i = 0; i < paths.Length - 1; i++)
                    {
                        string dir = paths[i];
                        ExtractContext(ref dir, ref cxt);
                    }
                    string fileName = paths[paths.Length - 1];
                    int extIndex = fileName.LastIndexOf('.');
                    if (extIndex != -1) fileName = fileName.Substring(0, extIndex);
                    ExtractContext(ref fileName, ref cxt);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        entry.Open().CopyTo(stream, 4 * 1024);
                        byte[] rawData = stream.ToArray();
                        ReadSprite(fileName, rawData, cxt, Path.GetExtension(entry.FullName));
                    }
                }
            }
        }
        private static readonly Dictionary<string, SpriteScope> scopeDict = new Dictionary<string, SpriteScope>
        {
            ["EXTRA"] = SpriteScope.Extra,
            ["ITEMS"] = SpriteScope.Items,
            ["OBJECTS"] = SpriteScope.Objects,
            ["FLOORS"] = SpriteScope.Floors,
            ["BULLETS"] = SpriteScope.Bullets,
            ["HAIR"] = SpriteScope.Hair,
            [@"FACIALHAIR"] = SpriteScope.FacialHair,
            ["HEADPIECES"] = SpriteScope.HeadPieces,
            ["AGENTS"] = SpriteScope.Agents,
            ["BODIES"] = SpriteScope.Bodies,
            ["WRECKAGE"] = SpriteScope.Wreckage,
            ["INTERFACE"] = SpriteScope.Interface,
            ["DECALS"] = SpriteScope.Decals,
            [@"WALLTOPS"] = SpriteScope.WallTops,
            ["WALLS"] = SpriteScope.Walls,
            [@"SPAWNERS"] = SpriteScope.Spawners,
            ["AUDIO"] = (SpriteScope)(-1),
        };
        public void ExtractContext(ref string fileName, ref SpriteContext cxt)
        {
            string[] split = fileName.Split('_');
            if (split.Length > 0)
            {
                string part = split[0];
                bool ignore = false;
                if (part.Length > 0 && part[0] == '%')
                {
                    part = part.Substring(1);
                    ignore = true;
                }
                if (scopeDict.TryGetValue(part, out SpriteScope scope))
                {
                    if (ignore) split[0] = part;
                    else
                    {
                        cxt.Scope = scope;
                        string[] newSplit = new string[split.Length - 1];
                        Array.Copy(split, 1, newSplit, 0, newSplit.Length);
                        split = newSplit;
                    }
                }
            }
            if (split.Length > 0)
            {
                string part = split[split.Length - 1];
                bool ignore = false;
                string[] dims = part.Split('x');
                if (part.Length > 0 && part[0] == '%')
                {
                    part = part.Substring(1);
                    ignore = true;
                }
                if (dims.Length == 2 && dims[0] == dims[1] && int.TryParse(dims[0], out int size))
                {
                    if (ignore) split[split.Length - 1] = part;
                    else
                    {
                        cxt.Size = size;
                        string[] newSplit = new string[split.Length - 1];
                        Array.Copy(split, 0, newSplit, 0, newSplit.Length);
                        split = newSplit;
                    }
                }
            }
            fileName = string.Join("_", split);
        }
        public void ReadSprite(string spriteName, byte[] rawData, SpriteContext cxt, string ext)
        {
            spriteName = spriteName.Replace('$', '/');
            if (cxt.IsValid())
                try
                {
                    if (cxt.Scope!.Value is (SpriteScope)(-1))
                    {
                        AudioType type = ext.ToUpperInvariant() switch
                        {
                            ".MP3" => AudioType.MPEG,
                            ".WAV" or ".WAVE" => AudioType.WAV,
                            ".OGG" or ".SPX" or ".OPUS" => AudioType.OGGVORBIS,
                            _ => throw new InvalidOperationException($"Unknown audio file extension: {ext}!"),
                        };
                        AudioClip clip = RogueUtilities.ConvertToAudioClip(rawData, type);
                        clip.name = spriteName;
                        overrideClips.Add(clip);
                    }
                    else
                    {
                        RogueLibs.CreateCustomSprite(spriteName, cxt.Scope.Value, rawData, cxt.Size!.Value);
                    }
                    counterAll++;
                }
                catch
                {
                    Logger.LogError($"Could not load a custom sprite \"{spriteName}\" ({cxt.Size}x{cxt.Size}) ({cxt.Scope})");
                }
            else
            {
                Logger.LogError($"Sprite data is incomplete for \"{spriteName}\":");
                if (!cxt.Size.HasValue) Logger.LogWarning("---- Size is unset. Use \"_SIZExSIZE\" after the sprite id.");
                if (!cxt.Scope.HasValue) Logger.LogWarning("---- Scope is unset. Use \"SCOPE_\" before the sprite id.");
            }
        }
    }
    public struct SpriteContext
    {
        public SpriteContext(SpriteScope? scope, int? size)
        {
            Scope = scope;
            Size = size;
        }
        public SpriteContext(SpriteContext context)
        {
            Scope = context.Scope;
            Size = context.Size;
        }
        public SpriteScope? Scope { get; set; }
        public int? Size { get; set; }
        public readonly Rect? Rect => Size.HasValue ? new Rect(0, 0, Size.Value, Size.Value) : new Rect?();

        public bool IsValid() => Scope.HasValue && Size.HasValue;
    }
}
