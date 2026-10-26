using System;
using UnityEngine;

namespace PocketPlanetFoundry
{
    [Serializable]
    public sealed class FoundrySave
    {
        public int schemaVersion = 1;
        public int saveRevision;
        public long lastSavedUtc;
        public double ore;
        public double lifetimeOre;
        public int drills;
        public int extractors;
        public int refineries;
        public int tapResearch;
        public int productionResearch;
        public int offlineResearch;
        public double stardust;
        public int launches;
        public bool removeForcedAds;
        public bool musicOn = true;
        public bool sfxOn = true;
        public long lastOfflineClaimId;
    }

    public static class FoundryRules
    {
        public const long OfflineCapSeconds = 8 * 60 * 60;
        public const double ZoneTwoLifetimeOre = 1000;
        public const double ZoneThreeLifetimeOre = 100000;
        public const double LaunchLifetimeOre = 250000;

        public static double TapYield(FoundrySave s) => 1d + s.tapResearch * 2d;
        public static double ProductionPerSecond(FoundrySave s)
        {
            double baseRate = s.drills * 1d + s.extractors * 18d + s.refineries * 60d;
            return baseRate * (1d + s.productionResearch * .25d) * (1d + s.stardust * .05d);
        }
        public static double DrillCost(FoundrySave s) => Cost(10d, 1.15d, s.drills);
        public static double ExtractorCost(FoundrySave s) => Cost(250d, 1.18d, s.extractors);
        public static double RefineryCost(FoundrySave s) => Cost(1500d, 1.2d, s.refineries);
        public static double TapResearchCost(FoundrySave s) => Cost(3d, 2.1d, s.tapResearch);
        public static double ProductionResearchCost(FoundrySave s) => Cost(5d, 2.3d, s.productionResearch);
        public static double OfflineResearchCost(FoundrySave s) => Cost(8d, 2.5d, s.offlineResearch);
        static double Cost(double start, double growth, int level) => Math.Ceiling(start * Math.Pow(growth, level));
        public static int CurrentZone(FoundrySave s) => s.lifetimeOre >= ZoneThreeLifetimeOre ? 3 : s.lifetimeOre >= ZoneTwoLifetimeOre ? 2 : 1;
        public static double LaunchReward(FoundrySave s) => Math.Max(1d, Math.Floor(Math.Sqrt(s.lifetimeOre / LaunchLifetimeOre)));
        public static double OfflineReward(FoundrySave s, long elapsed) => ProductionPerSecond(s) * Math.Min(Math.Max(0, elapsed), OfflineCapSeconds) * (1d + s.offlineResearch * .1d);
        public static void AddOre(FoundrySave s, double amount) { if (amount <= 0 || double.IsNaN(amount) || double.IsInfinity(amount)) return; s.ore += amount; s.lifetimeOre += amount; }
        public static bool SpendOre(FoundrySave s, double amount) { if (s.ore < amount || amount <= 0) return false; s.ore -= amount; return true; }
    }
}
