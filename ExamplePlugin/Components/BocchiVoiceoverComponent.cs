using UnityEngine;
using RoR2;
using System.Collections.Generic;
using UnityEngine.Networking;
using BaseVoiceoverLib;

namespace BocchiVoiceMod.Components
{
    public class BocchiVoiceoverComponent : BaseVoiceoverComponent
    {
        // === Static Network Sound Event Defs ===
        public static NetworkSoundEventDef nseShout, nseSpecial, nseBlock, nseShrineFail, nseTitle, nseIntro, nseHurt, nseKanpeki, nseSmart, nseLogic, nseFactor, nseThanks, nseIku, nseMathTruth;

        // === Cooldowns for preventing spam ===
        private float lowHealthCooldown = 0f;
        private float blockedCooldown = 0f;
        private float specialCooldown = 0f;
        private float levelCooldown = 0f;
        private float shrineOfChanceFailCooldown = 0f;


        protected override void Start()
        {
            base.Start();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (lowHealthCooldown > 0f) lowHealthCooldown -= Time.fixedDeltaTime;
            if (blockedCooldown > 0f) blockedCooldown -= Time.fixedDeltaTime;
            if (specialCooldown > 0f) specialCooldown -= Time.fixedDeltaTime;
            if (levelCooldown > 0f) levelCooldown -= Time.fixedDeltaTime;
            if (shrineOfChanceFailCooldown > 0f) levelCooldown -= Time.fixedDeltaTime;
        }

        // === Hooks from BaseVoiceoverComponent ===
        public override void PlaySpawn()
        {
            TryPlaySound("Play_bocchi_spawn", 1.0f, false);
        }

        public override void PlayJump() { }
        public override void PlayDeath()
        {
            TryPlaySound("Play_bocchi_death", 1.5f, true);
        }

        public override void PlayHurt(float percentHPLost)
        {
            if (percentHPLost >= 0.1f)
            {
                TryPlayNetworkSound(nseHurt, 0f, false);
            }
        }

        public override void PlayTeleporterStart()
        {
            TryPlaySound("Play_bocchi_skill3", 1.95f, false);
        }
        public override void PlayTeleporterFinish()
        {
            TryPlaySound("Play_bocchi_levelup", 3.8f, false);
        }
        public override void PlayVictory()
        {
            TryPlaySound("Play_bocchi_Victory", 3.8f, true);
        }

        public override void PlayLevelUp()
        {
            if (levelCooldown > 0f) return;
            bool played = TryPlaySound("Play_bocchi_levelup", 3f, false);
            if (played) levelCooldown = 60f;
        }
        public override void PlayLowHealth()
        {
            if (lowHealthCooldown > 0f) return;
            bool played = TryPlaySound("Play_bocchi_LowHealth", 1f, false);
            if (played) lowHealthCooldown = 45f;
        }

        public override void PlaySpecialAuthority(GenericSkill skill)
        {
            if (specialCooldown > 0f) return;
            bool played = TryPlayNetworkSound(nseSpecial, 0f, false);
            if (played) specialCooldown = 15f;
        }

        public void PlayAcquireLegendary()
        {
            TryPlaySound("Play_bocchi_FindLegendary", 5.75f, false);
        }
        public override void PlayShrineOfChanceFailServer()
        {
            if (!NetworkServer.active || shrineOfChanceFailCooldown > 0f) return;
            bool played = TryPlaySound("Play_bocchi_skill4", 1f, false);
            if (played) shrineOfChanceFailCooldown = 60f;
        }

        public override bool ComponentEnableVoicelines()
        {
            return BocchiVoiceMod.enableVoicelines.Value;
        }


        
    }
}

