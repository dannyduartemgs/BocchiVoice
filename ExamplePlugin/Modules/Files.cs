using System.IO;
using BepInEx;

namespace BocchiVoiceMod.Modules
{
    public static class Files
    {
        public static PluginInfo PluginInfo;
        public static string assemblyDir => Path.GetDirectoryName(PluginInfo.Location);
    }
}
