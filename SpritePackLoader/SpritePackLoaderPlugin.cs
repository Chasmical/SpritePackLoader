using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx;
using RogueLibsCore;

namespace SpritePackLoader
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(RogueLibs.GUID, RogueLibs.CompiledVersion)]
    public class SpritePackLoaderPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "abbysssal.streetsofrogue.spritepackloader";
        public const string PluginName = "SpritePackLoader";
        public const string PluginVersion = "0.3";

        public string PackPath { get; private set; }
        public string SpritePacksPath { get; private set; }
        public void Awake()
        {
            RogueLibs.CreateCustomUnlock(new MutatorUnlock("SpritePackTestingMode", true))
                .WithName(new CustomNameInfo
                {
                    English = "SpritePack Testing Mode",
                    Russian = "Режим тестирования спрайтпаков",
                })
                .WithDescription(new CustomNameInfo
                {
                    English = "Gives the player an item to test item sprites",
                    Russian = "Даёт игроку предмет для тестирования спрайтов предметов",
                });
            RogueLibs.CreateCustomItem<SpritePackTester>()
                .WithName(new CustomNameInfo
                {
                    English = "SpritePack Tester",
                    Russian = "Тестер спрайтпаков",
                })
                .WithDescription(new CustomNameInfo
                {
                    English = "Spawns all available items' sprites around the player. Use again to destroy all sprites.",
                    Russian = "Спавнит все спрайты доступных предметов вокруг игрока. Используйте ещё раз чтобы уничтожить все спрайты.",
                })
                .WithSprite(Properties.Resources.SpritePackTester)
                .WithUnlock(new ItemUnlock { IsAvailable = false, IsAvailableInCC = false, IsEnabled = false });

            InitializeSprites();
        }
        public void InitializeSprites()
        {
            PackPath = Path.Combine(Paths.GameRootPath, "SpritePack");
            DirectoryInfo packDir = new DirectoryInfo(PackPath);
            if (packDir.Exists)
            {
                counter = 0;
                Logger.LogMessage("Loading sprites from SpritePack directory...");
                DateTime start = DateTime.Now;
                ReadDirectory(packDir, new SpriteContext(null, 64));
                DateTime end = DateTime.Now;
                Logger.LogMessage($"Loaded {counter} sprites in {(end - start).TotalSeconds:#.###} ms.");
            }
            SpritePacksPath = Path.Combine(Paths.BepInExRootPath, "spritepacks");
            DirectoryInfo spritepacksDir = new DirectoryInfo(SpritePacksPath);
            spritepacksDir.Create();
            foreach (FileInfo file in spritepacksDir.EnumerateFiles("*.spritepack", SearchOption.AllDirectories))
            {
                counter = 0;
                Logger.LogMessage($"Loading sprites from {Path.GetFileNameWithoutExtension(file.FullName)} sprite pack.");
                DateTime start = DateTime.Now;
                ReadArchive(file, new SpriteContext(null, 64));
                DateTime end = DateTime.Now;
                Logger.LogMessage($"Loaded {counter} sprites in {(end - start).TotalSeconds:#.###} ms.");
            }
        }

        private int counter;
        public void ReadDirectory(DirectoryInfo directory, SpriteContext dirCxt)
        {
            foreach (DirectoryInfo subdirectory in directory.EnumerateDirectories())
            {
                SpriteContext subdirCxt = new SpriteContext(dirCxt);
                string name = Path.GetFileNameWithoutExtension(subdirectory.FullName);
                ExtractContext(ref name, ref subdirCxt);
                ReadDirectory(subdirectory, subdirCxt);
            }
            foreach (FileInfo file in directory.EnumerateFiles())
            {
                SpriteContext fileCxt = new SpriteContext(dirCxt);
                string name = Path.GetFileNameWithoutExtension(file.FullName);
                ExtractContext(ref name, ref fileCxt);
                byte[] rawData = File.ReadAllBytes(file.FullName);
                ReadSprite(name, rawData, fileCxt);
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
                    string name = paths[paths.Length - 1];
                    int extIndex = name.LastIndexOf('.');
                    if (extIndex != -1) name = name.Substring(0, extIndex);
                    ExtractContext(ref name, ref cxt);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        entry.Open().CopyTo(stream, 4 * 1024);
                        byte[] rawData = stream.ToArray();
                        ReadSprite(name, rawData, cxt);
                    }
                }
            }
        }
        private static readonly Dictionary<string, SpriteScope> scopeDict = new Dictionary<string, SpriteScope>
        {
            ["ITEMS"] = SpriteScope.Items,
            ["OBJECTS"] = SpriteScope.Objects,
            ["FLOORS"] = SpriteScope.Floors,
            ["EXTRA"] = SpriteScope.Extra,
        };
        public void ExtractContext(ref string name, ref SpriteContext cxt)
        {
            string[] split = name.Split('_');
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
            name = string.Join("_", split);
        }
        public void ReadSprite(string spriteName, byte[] rawData, SpriteContext cxt)
        {
            if (cxt.IsValid())
                try
                {
                    RogueLibs.CreateCustomSprite(spriteName, cxt.Scope.Value, rawData, cxt.Size.Value);
                    counter++;
                    return;
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
        public Rect? Rect => Size.HasValue ? new Rect(0, 0, Size.Value, Size.Value) : new Rect?();

        public bool IsValid() => Scope.HasValue && Size.HasValue;
    }
}
