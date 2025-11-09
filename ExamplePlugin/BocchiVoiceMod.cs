using BaseVoiceoverLib;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using BocchiVoiceMod.Components;
using BocchiVoiceMod.Modules;

namespace BocchiVoiceMod
{
    [BepInDependency(R2API.SoundAPI.PluginGUID)]
    [BepInDependency("com.Moffein.BaseVoiceoverLib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.Dannyduartemgs.BocchiVoiceMod", "BocchiVoiceMod", "1.0.0")]
    public class BocchiVoiceMod : BaseUnityPlugin
    {
        public static ConfigEntry<bool> enableVoicelines;
        public static bool playedSeasonalVoiceline = false;
        public static AssetBundle assetBundle;

        public void Awake()
        {
            Files.PluginInfo = this.Info;
            RoR2Application.onLoad += OnLoad;
            new Content().Initialize();


            // 🧠 Load the Wwise soundbank (.sound file)
            SoundBanks.Init();

            // 🧠 Initialize the NetworkSoundEventDefs for this mod
            InitNSE();

            // 🧠 Config setting to enable/disable voicelines
            enableVoicelines = Config.Bind<bool>(
                new ConfigDefinition("Settings", "Enable Voicelines"),
                true,
                new ConfigDescription("Enable Bocchi voicelines when using the skin.")
            );
            enableVoicelines.SettingChanged += EnableVoicelines_SettingChanged;
        }

        private void EnableVoicelines_SettingChanged(object sender, EventArgs e)
        {
            RefreshNSE();
        }

        private void OnLoad()
        {
            // 🧠 Find Bocchi skin definition on a specific body (replace with your target body)
            SkinDef bocchiSkin = null;
            SkinDef[] skins = SkinCatalog.FindSkinsForBody(BodyCatalog.FindBodyIndex("CommandoBody"));
            foreach (SkinDef skinDef in skins)
            {
                if (skinDef.name == "BocchiSkin")
                {
                    bocchiSkin = skinDef;
                    break;
                }
            }

            if (!bocchiSkin)
            {
                Logger.LogError("[BocchiVoiceMod] ❌ Bocchi SkinDef not found. Voicelines will not work!");
                return;
            }

            // 🧠 Register the voiceover info to BaseVoiceoverLib
            VoiceoverInfo vo = new VoiceoverInfo(typeof(BocchiVoiceoverComponent), bocchiSkin, "CommandoBody");
            vo.selectActions += OnSkinSelect;

            RefreshNSE();
        }

        private void OnSkinSelect(GameObject mannequinObject)
        {
            if (!enableVoicelines.Value) return;

            bool played = false;

            // 🧠 Optional: Special seasonal voicelines
            if (!playedSeasonalVoiceline)
            {
                if ((DateTime.Today.Month == 1 && DateTime.Today.Day == 1))
                {
                    Util.PlaySound("Play_bocchi_NewYear", mannequinObject);
                    played = true;
                }
                else if (DateTime.Today.Month == 12 && (DateTime.Today.Day == 24 || DateTime.Today.Day == 25))
                {
                    Util.PlaySound("Play_bocchi_Xmas", mannequinObject);
                    played = true;
                }

                if (played)
                    playedSeasonalVoiceline = true;
            }

            if (!played)
            {
                // 🧠 Normal lobby voice
                if (Util.CheckRoll(10f))
                    Util.PlaySound("Play_bocchi_Lobby_Special", mannequinObject);
                else
                    Util.PlaySound("Play_bocchi_select", mannequinObject);

                Logger.LogInfo("[BocchiVoiceMod] Played skin select voiceline.");
            }
        }

        // 🧠 Create and register all the sounds
        private void InitNSE()
        {
            BocchiVoiceoverComponent.nseSpecial = RegisterNSE("Play_bocchi_skill1");
            BocchiVoiceoverComponent.nseBlock = RegisterNSE("Play_Bocchi_Blocked");
            BocchiVoiceoverComponent.nseShrineFail = RegisterNSE("Play_bocchi_skill4");
            BocchiVoiceoverComponent.nseShout = RegisterNSE("Play_Bocchi_Shout");
            BocchiVoiceoverComponent.nseTitle = RegisterNSE("Play_Bocchi_TitleDrop");
            BocchiVoiceoverComponent.nseIntro = RegisterNSE("Play_Bocchi_Intro");
            BocchiVoiceoverComponent.nseHurt = RegisterNSE("Play_Bocchi_TakeDamage");
            BocchiVoiceoverComponent.nseKanpeki = RegisterNSE("Play_Bocchi_Kanpeki");
            BocchiVoiceoverComponent.nseSmart = RegisterNSE("Play_Bocchi_Smart");
            BocchiVoiceoverComponent.nseLogic = RegisterNSE("Play_Bocchi_Logic");
            BocchiVoiceoverComponent.nseFactor = RegisterNSE("Play_Bocchi_Factor");
            BocchiVoiceoverComponent.nseThanks = RegisterNSE("Play_Bocchi_Thanks");
            BocchiVoiceoverComponent.nseIku = RegisterNSE("Play_Bocchi_Ikuwayo");
            BocchiVoiceoverComponent.nseMathTruth = RegisterNSE("Play_Bocchi_MathTruth");
        }

        private NetworkSoundEventDef RegisterNSE(string eventName)
        {
            var nse = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            nse.eventName = eventName;
            Content.networkSoundEventDefs.Add(nse);
            nseList.Add(new NSEInfo(nse));
            return nse;
        }

        public void RefreshNSE()
        {
            foreach (var nse in nseList)
                nse.ValidateParams();
        }

        public static List<NSEInfo> nseList = new List<NSEInfo>();

        public class NSEInfo
        {
            public NetworkSoundEventDef nse;
            public uint akId = 0u;
            public string eventName = string.Empty;

            public NSEInfo(NetworkSoundEventDef source)
            {
                this.nse = source;
                this.akId = source.akId;
                this.eventName = source.eventName;
            }

            private void DisableSound()
            {
                nse.akId = 0u;
                nse.eventName = string.Empty;
            }

            private void EnableSound()
            {
                nse.akId = this.akId;
                nse.eventName = this.eventName;
            }

            public void ValidateParams()
            {
                if (this.akId == 0u) this.akId = nse.akId;
                if (this.eventName == string.Empty) this.eventName = nse.eventName;

                if (!enableVoicelines.Value)
                    DisableSound();
                else
                    EnableSound();
            }
        }
    }
}
