using System;
using UnityEngine;

namespace PocketPlanetFoundry
{
    public interface IRewardedAdService { bool IsReady { get; } void Show(Action<bool> completed); }
    public interface IAnalyticsService { void Track(string eventName); }
    public sealed class NoAdsService : IRewardedAdService { public bool IsReady => false; public void Show(Action<bool> completed) => completed?.Invoke(false); }
    public sealed class NoAnalyticsService : IAnalyticsService { public void Track(string eventName) { } }

    public sealed class FoundryController : MonoBehaviour
    {
        public static FoundryController Instance { get; private set; }
        public FoundrySave State { get; private set; }
        public long PendingOfflineSeconds { get; private set; }
        public double PendingOfflineOre { get; private set; }
        public event Action Changed;
        readonly IRewardedAdService ads = new NoAdsService();
        readonly IAnalyticsService analytics = new NoAnalyticsService();
        float tick;
        bool paused;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Create() { if (FindAnyObjectByType<FoundryController>() == null) new GameObject("Foundry Game").AddComponent<FoundryController>(); }
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            State = LocalSaveRepository.Load();
            CalculateOffline();
            new GameObject("Foundry UI").AddComponent<FoundryUI>();
        }
        void Update()
        {
            tick += Time.unscaledDeltaTime;
            if (tick >= 15f) { float dt=tick; tick=0; FoundryRules.AddOre(State,FoundryRules.ProductionPerSecond(State)*dt); Changed?.Invoke(); LocalSaveRepository.Save(State); }
        }
        void CalculateOffline()
        {
            long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            PendingOfflineSeconds=State.lastSavedUtc>0?Math.Min(FoundryRules.OfflineCapSeconds,Math.Max(0,now-State.lastSavedUtc)):0;
            PendingOfflineOre=FoundryRules.OfflineReward(State,PendingOfflineSeconds);
        }
        void OnApplicationPause(bool pause)
        {
            if (pause && State!=null) { paused=true; LocalSaveRepository.Save(State); }
            else if (!pause && paused && State!=null) { paused=false; CalculateOffline(); Changed?.Invoke(); FindAnyObjectByType<FoundryUI>()?.OnReturnFromBackground(); }
        }
        void OnApplicationQuit() { if(State!=null)LocalSaveRepository.Save(State); }
        public void Mine() { FoundryRules.AddOre(State,FoundryRules.TapYield(State)); analytics.Track("mine"); Changed?.Invoke(); }
        public bool BuyDrill() { if(!FoundryRules.SpendOre(State,FoundryRules.DrillCost(State)))return false;State.drills++;Commit("upgrade_purchased");return true; }
        public bool BuyExtractor() { if(FoundryRules.CurrentZone(State)<2||!FoundryRules.SpendOre(State,FoundryRules.ExtractorCost(State)))return false;State.extractors++;Commit("upgrade_purchased");return true; }
        public bool BuyRefinery() { if(FoundryRules.CurrentZone(State)<3||!FoundryRules.SpendOre(State,FoundryRules.RefineryCost(State)))return false;State.refineries++;Commit("upgrade_purchased");return true; }
        public bool ResearchTap() { double c=FoundryRules.TapResearchCost(State);if(State.stardust<c)return false;State.stardust-=c;State.tapResearch++;Commit("research_purchased");return true; }
        public bool ResearchProduction() { double c=FoundryRules.ProductionResearchCost(State);if(State.stardust<c)return false;State.stardust-=c;State.productionResearch++;Commit("research_purchased");return true; }
        public bool ResearchOffline() { double c=FoundryRules.OfflineResearchCost(State);if(State.stardust<c)return false;State.stardust-=c;State.offlineResearch++;Commit("research_purchased");return true; }
        public void ClaimOffline(bool doubleReward)
        {
            double reward=PendingOfflineOre*(doubleReward?2d:1d);
            FoundryRules.AddOre(State,reward);PendingOfflineOre=0;PendingOfflineSeconds=0;State.lastOfflineClaimId++;
            Commit("offline_reward_claimed");
        }
        public void Launch()
        {
            if(State.lifetimeOre<FoundryRules.LaunchLifetimeOre)return;
            double reward=FoundryRules.LaunchReward(State);State.stardust+=reward;State.launches++;
            State.ore=0;State.lifetimeOre=0;State.drills=0;State.extractors=0;State.refineries=0;
            Commit("prestige_completed");
        }
        void Commit(string eventName) { analytics.Track(eventName);LocalSaveRepository.Save(State);Changed?.Invoke(); }
    }
}


