using Exiled.API.Features;
using HarmonyLib;
using Respawning.NamingRules;

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(NineTailedFoxNamingRule), "GetCassieUnitName")]
    internal static class SCPDeathFixPatch
    {
        public static bool Prefix(ref string regular)
        {
            Log.Debug(regular);
            if (regular.StartsWith("<color="))
                regular = regular.Substring(12, regular.Length - 12 - 8);
            Log.Debug(regular);
            return true;
        }
    }
}
