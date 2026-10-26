using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PocketPlanetFoundry
{
    public sealed class FoundryUI : MonoBehaviour
    {
        FoundryController game;
        Canvas canvas;
        Text oreText, rateText, zoneText, drillText, extractorText, refineryText, researchText, messageText;
        Button mineButton, drillButton, extractorButton, refineryButton, tapResearchButton, productionResearchButton, offlineResearchButton, launchButton;
        GameObject offlinePanel;
        Text offlineCopy;
        Font font;

        void Awake()
        {
            game = FoundryController.Instance;
            if (game == null) { Destroy(gameObject); return; }
            Build();
            game.Changed += Refresh;
            Refresh();
            if (game.PendingOfflineSeconds > 30 && game.PendingOfflineOre > 0) ShowOffline();
        }
        void OnDestroy() { if (game != null) game.Changed -= Refresh; }

        void Build()
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var eventSystem = FindAnyObjectByType<EventSystem>();
            if (eventSystem == null) { new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)); }
            var go = new GameObject("Pocket Planet Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); scaler.matchWidthOrHeight = .5f;
            Image bg = MakePanel(go.transform, "Background", new Color32(10, 22, 39, 255), new Vector2(0,0), new Vector2(1,1), Vector2.zero, Vector2.zero);
            bg.raycastTarget=false;
            var stars = MakeText(go.transform,"Stars", "✦     ·        ✧       ·      ✦     ·         ✧", 31, new Color32(83,119,151,255), TextAnchor.MiddleCenter, new Vector2(0, .72f), new Vector2(1,.96f), Vector2.zero, Vector2.zero); stars.raycastTarget=false;
            MakeText(go.transform,"Title", "POCKET PLANET FOUNDRY", 32, new Color32(221,238,248,255), TextAnchor.MiddleCenter, new Vector2(.05f,.91f),new Vector2(.95f,.975f),Vector2.zero,Vector2.zero);
            oreText=MakeText(go.transform,"Ore", "0 Ore", 42, Color.white, TextAnchor.MiddleCenter,new Vector2(.05f,.82f),new Vector2(.95f,.91f),Vector2.zero,Vector2.zero);
            rateText=MakeText(go.transform,"Rate", "0 Ore / sec", 24,new Color32(137,219,207,255),TextAnchor.MiddleCenter,new Vector2(.05f,.78f),new Vector2(.95f,.83f),Vector2.zero,Vector2.zero);
            zoneText=MakeText(go.transform,"Zone", "CRATER CAMP",25,new Color32(255,208,121,255),TextAnchor.MiddleCenter,new Vector2(.05f,.68f),new Vector2(.95f,.74f),Vector2.zero,Vector2.zero);
            mineButton=MakeButton(go.transform,"Planet", "", new Vector2(.22f,.43f),new Vector2(.78f,.67f), new Color32(32,112,139,255), Mine);
            // Planet disc visual, placed as non-interactive child. The button itself is the tap target.
            var planet=MakePanel(mineButton.transform,"Planet Surface",new Color32(49,124,139,255),new Vector2(.04f,.04f),new Vector2(.96f,.96f),Vector2.zero,Vector2.zero);
            planet.raycastTarget=false;
            var core=MakePanel(mineButton.transform,"Crater",new Color32(30,79,99,255),new Vector2(.38f,.46f),new Vector2(.61f,.68f),Vector2.zero,Vector2.zero); core.raycastTarget=false;
            var ptxt=MakeText(mineButton.transform,"Tap Label","TAP TO MINE",25,Color.white,TextAnchor.MiddleCenter,new Vector2(0,.08f),new Vector2(1,.28f),Vector2.zero,Vector2.zero); ptxt.raycastTarget=false;
            MakeText(go.transform,"Buildings Header","AUTOMATION",19,new Color32(152,180,199,255),TextAnchor.MiddleLeft,new Vector2(.07f,.37f),new Vector2(.93f,.405f),Vector2.zero,Vector2.zero);
            drillButton=MakeButton(go.transform,"Drill", "", new Vector2(.06f,.31f),new Vector2(.94f,.37f),new Color32(29,54,73,255),()=>game.BuyDrill()); drillText=drillButton.GetComponentInChildren<Text>();
            extractorButton=MakeButton(go.transform,"Extractor", "",new Vector2(.06f,.25f),new Vector2(.94f,.31f),new Color32(29,54,73,255),()=>game.BuyExtractor()); extractorText=extractorButton.GetComponentInChildren<Text>();
            refineryButton=MakeButton(go.transform,"Refinery", "",new Vector2(.06f,.19f),new Vector2(.94f,.25f),new Color32(29,54,73,255),()=>game.BuyRefinery()); refineryText=refineryButton.GetComponentInChildren<Text>();
            tapResearchButton=MakeButton(go.transform,"Tap Research", "",new Vector2(.06f,.125f),new Vector2(.49f,.185f),new Color32(38,76,80,255),()=>game.ResearchTap()); tapResearchButton.GetComponentInChildren<Text>().fontSize=18;
            productionResearchButton=MakeButton(go.transform,"Production Research", "",new Vector2(.51f,.125f),new Vector2(.94f,.185f),new Color32(38,76,80,255),()=>game.ResearchProduction()); productionResearchButton.GetComponentInChildren<Text>().fontSize=18;
            offlineResearchButton=MakeButton(go.transform,"Offline Research", "",new Vector2(.06f,.06f),new Vector2(.49f,.12f),new Color32(38,76,80,255),()=>game.ResearchOffline()); offlineResearchButton.GetComponentInChildren<Text>().fontSize=18;
            launchButton=MakeButton(go.transform,"Launch", "",new Vector2(.51f,.06f),new Vector2(.94f,.12f),new Color32(156,90,43,255),ConfirmLaunch); launchButton.GetComponentInChildren<Text>().fontSize=18;
            researchText=MakeText(go.transform,"Stardust", "STARDUST 0  •  RESEARCH", 16,new Color32(232,196,118,255),TextAnchor.MiddleCenter,new Vector2(.05f,.015f),new Vector2(.95f,.055f),Vector2.zero,Vector2.zero);
            messageText=MakeText(go.transform,"Message", "",16,new Color32(235,177,131,255),TextAnchor.MiddleCenter,new Vector2(.05f,.39f),new Vector2(.95f,.43f),Vector2.zero,Vector2.zero);
            offlinePanel=BuildModal(go.transform);
        }

        GameObject BuildModal(Transform parent)
        {
            var overlay=MakePanel(parent,"Offline Overlay",new Color(0,0,0,.78f),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject; overlay.SetActive(false);
            var card=MakePanel(overlay.transform,"Offline Card",new Color32(22,43,59,255),new Vector2(.08f,.35f),new Vector2(.92f,.67f),Vector2.zero,Vector2.zero);
            MakeText(card.transform,"Welcome Back","WELCOME BACK",29,Color.white,TextAnchor.MiddleCenter,new Vector2(.05f,.72f),new Vector2(.95f,.94f),Vector2.zero,Vector2.zero);
            offlineCopy=MakeText(card.transform,"Offline Reward","",22,new Color32(194,224,231,255),TextAnchor.MiddleCenter,new Vector2(.08f,.38f),new Vector2(.92f,.72f),Vector2.zero,Vector2.zero);
            MakeButton(card.transform,"Claim","CLAIM ORE",new Vector2(.08f,.08f),new Vector2(.48f,.32f),new Color32(35,131,131,255),()=>{game.ClaimOffline(false);overlay.SetActive(false);Refresh();});
            MakeButton(card.transform,"Double","WATCH AD  •  2×",new Vector2(.52f,.08f),new Vector2(.92f,.32f),new Color32(109,103,53,255),()=>{game.ClaimOffline(false);overlay.SetActive(false);Refresh();});
            return overlay;
        }
        public void OnReturnFromBackground() { if(game.PendingOfflineSeconds>30 && game.PendingOfflineOre>0) ShowOffline(); Refresh(); }
        void ShowOffline()
        {
            offlineCopy.text=$"While you were away\n{FormatTime(game.PendingOfflineSeconds)}\n\nYou collected {Fmt(game.PendingOfflineOre)} Ore.";
            var btns=offlinePanel.GetComponentsInChildren<Button>();
            // No ad provider is configured in the starter. Hide the ad action until a real adapter is installed.
            if(btns.Length>1) btns[1].gameObject.SetActive(false);
            offlinePanel.SetActive(true);
        }
        void ConfirmLaunch()
        {
            if(game.State.lifetimeOre<FoundryRules.LaunchLifetimeOre) { messageText.text=$"Launch unlocks at {Fmt(FoundryRules.LaunchLifetimeOre)} lifetime Ore."; return; }
            messageText.text=$"Launch confirmed: +{Fmt(FoundryRules.LaunchReward(game.State))} Stardust. Tap Launch again to confirm.";
            launchButton.onClick.RemoveAllListeners(); launchButton.onClick.AddListener(()=>{game.Launch(); launchButton.onClick.RemoveAllListeners();launchButton.onClick.AddListener(ConfirmLaunch);messageText.text="New colony established.";});
        }
        void Mine() { game.Mine(); }
        void Refresh()
        {
            if(game==null||game.State==null)return;
            var s=game.State; oreText.text=Fmt(s.ore)+" Ore"; rateText.text=Fmt(FoundryRules.ProductionPerSecond(s))+" Ore / sec";
            int zone=FoundryRules.CurrentZone(s); zoneText.text=zone==1?"CRATER CAMP":zone==2?"REFINERY RIDGE":"LAUNCH BASIN";
            drillText.text=$"DRILL  Lv {s.drills}     +1 Ore/sec each     Cost {Fmt(FoundryRules.DrillCost(s))}";
            extractorText.text=zone<2?$"EXTRACTOR  •  Unlocks at {Fmt(FoundryRules.ZoneTwoLifetimeOre)} lifetime Ore":$"EXTRACTOR  Lv {s.extractors}     +18/sec each     Cost {Fmt(FoundryRules.ExtractorCost(s))}";
            refineryText.text=zone<3?$"REFINERY  •  Unlocks at {Fmt(FoundryRules.ZoneThreeLifetimeOre)} lifetime Ore":$"REFINERY  Lv {s.refineries}     +60/sec each     Cost {Fmt(FoundryRules.RefineryCost(s))}";
            extractorButton.interactable=zone>=2 && s.ore>=FoundryRules.ExtractorCost(s); refineryButton.interactable=zone>=3 && s.ore>=FoundryRules.RefineryCost(s);
            drillButton.interactable=s.ore>=FoundryRules.DrillCost(s);
            tapResearchButton.GetComponentInChildren<Text>().text=$"TAP +2  Lv{s.tapResearch}\n{Fmt(FoundryRules.TapResearchCost(s))} Stardust";
            productionResearchButton.GetComponentInChildren<Text>().text=$"PRODUCTION +25%  Lv{s.productionResearch}\n{Fmt(FoundryRules.ProductionResearchCost(s))} Stardust";
            offlineResearchButton.GetComponentInChildren<Text>().text=$"OFFLINE +10%  Lv{s.offlineResearch}\n{Fmt(FoundryRules.OfflineResearchCost(s))} Stardust";
            tapResearchButton.interactable=s.stardust>=FoundryRules.TapResearchCost(s); productionResearchButton.interactable=s.stardust>=FoundryRules.ProductionResearchCost(s); offlineResearchButton.interactable=s.stardust>=FoundryRules.OfflineResearchCost(s);
            researchText.text=$"STARDUST {Fmt(s.stardust)}  •  RESEARCH PERSISTS  •  LAUNCHES {s.launches}";
            launchButton.GetComponentInChildren<Text>().text=s.lifetimeOre>=FoundryRules.LaunchLifetimeOre?$"LAUNCH SHIP  +{Fmt(FoundryRules.LaunchReward(s))} Stardust":"SHIP  •  "+Fmt(s.lifetimeOre)+" / "+Fmt(FoundryRules.LaunchLifetimeOre);
            launchButton.interactable=s.lifetimeOre>=FoundryRules.LaunchLifetimeOre;
        }
        Button MakeButton(Transform parent,string name,string label,Vector2 amin,Vector2 amax,Color color,Action action)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button)); go.transform.SetParent(parent,false);
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=amin;rt.anchorMax=amax;rt.offsetMin=new Vector2(3,3);rt.offsetMax=new Vector2(-3,-3);
            var image=go.GetComponent<Image>(); image.color=color;
            var button=go.GetComponent<Button>();button.targetGraphic=image;button.onClick.AddListener(()=>action());
            var text=MakeText(go.transform,"Label",label,20,Color.white,TextAnchor.MiddleCenter,new Vector2(.03f,.03f),new Vector2(.97f,.97f),Vector2.zero,Vector2.zero);text.fontStyle=FontStyle.Bold;text.raycastTarget=false;
            return button;
        }
        Image MakePanel(Transform parent,string name,Color color,Vector2 amin,Vector2 amax,Vector2 omin,Vector2 omax)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var rt=go.GetComponent<RectTransform>();rt.anchorMin=amin;rt.anchorMax=amax;rt.offsetMin=omin;rt.offsetMax=omax;
            var image=go.GetComponent<Image>();image.color=color;return image;
        }
        Text MakeText(Transform parent,string name,string value,int size,Color color,TextAnchor align,Vector2 amin,Vector2 amax,Vector2 omin,Vector2 omax)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);var rt=go.GetComponent<RectTransform>();rt.anchorMin=amin;rt.anchorMax=amax;rt.offsetMin=omin;rt.offsetMax=omax;
            var text=go.GetComponent<Text>();text.font=font;text.text=value;text.fontSize=size;text.color=color;text.alignment=align;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;return text;
        }
        static string Fmt(double n)
        {
            if(double.IsNaN(n)||double.IsInfinity(n))return "0";
            if(n>=1e12)return (n/1e12).ToString("0.##")+"T";if(n>=1e9)return (n/1e9).ToString("0.##")+"B";if(n>=1e6)return (n/1e6).ToString("0.##")+"M";if(n>=1e3)return (n/1e3).ToString("0.##")+"K";return n.ToString("0.##");
        }
        static string FormatTime(long sec)=>$"{sec/3600}h {(sec%3600)/60}m";
    }
}





