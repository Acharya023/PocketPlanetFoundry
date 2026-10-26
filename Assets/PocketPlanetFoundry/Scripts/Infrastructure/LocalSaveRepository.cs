using System;
using System.IO;
using UnityEngine;

namespace PocketPlanetFoundry
{
    public static class LocalSaveRepository
    {
        static string MainPath => Path.Combine(Application.persistentDataPath, "foundry-save.json");
        static string BackupPath => MainPath + ".bak";
        public static FoundrySave Load()
        {
            FoundrySave save = Read(MainPath) ?? Read(BackupPath) ?? new FoundrySave();
            if (save.schemaVersion < 1) save.schemaVersion = 1;
            save.ore = Math.Max(0, save.ore); save.lifetimeOre = Math.Max(save.ore, save.lifetimeOre);
            save.drills = Math.Max(0, save.drills); save.extractors = Math.Max(0, save.extractors); save.refineries = Math.Max(0, save.refineries);
            return save;
        }
        static FoundrySave Read(string path)
        {
            try { return File.Exists(path) ? JsonUtility.FromJson<FoundrySave>(File.ReadAllText(path)) : null; }
            catch (Exception e) { Debug.LogWarning("Save read failed: " + e.Message); return null; }
        }
        public static void Save(FoundrySave save)
        {
            try
            {
                save.schemaVersion = 1; save.saveRevision++; save.lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string temp = MainPath + ".tmp"; File.WriteAllText(temp, JsonUtility.ToJson(save));
                if (File.Exists(MainPath)) File.Copy(MainPath, BackupPath, true);
                if (File.Exists(MainPath)) File.Delete(MainPath);
                File.Move(temp, MainPath);
            }
            catch (Exception e) { Debug.LogError("Save failed: " + e.Message); }
        }
    }
}
