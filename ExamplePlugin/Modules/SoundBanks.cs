using R2API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BocchiVoiceMod.Modules
{
    public static class SoundBanks
    {
        private static bool initialized = false;

        public static string SoundBankDirectory => Files.assemblyDir;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            string bankPath = Path.Combine(SoundBankDirectory, "BocchiSoundbank.sound");

            if (!File.Exists(bankPath))
            {
                UnityEngine.Debug.LogError("[BocchiVoiceMod] ❌ Soundbank not found: " + bankPath);
                return;
            }

            byte[] data = File.ReadAllBytes(bankPath);
            SoundAPI.SoundBanks.Add(data);
            UnityEngine.Debug.Log("[BocchiVoiceMod] ✅ Loaded soundbank: " + bankPath);
        }
    }
}
