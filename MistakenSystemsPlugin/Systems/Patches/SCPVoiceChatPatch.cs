using Assets._Scripts.Dissonance;
using HarmonyLib;
using System.Collections.Generic;

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(DissonanceUserSetup), "CallCmdAltIsActive")]
    internal static class SCPVoiceChatPatch
    {
        internal static readonly HashSet<string> HasAccessToSCPAlt = new HashSet<string>();
        public static readonly List<RoleType> MimicedRoles = new List<RoleType>();
        public static bool Prefix(DissonanceUserSetup __instance, bool value)
        {
            CharacterClassManager ccm = ReferenceHub.GetHub(__instance.gameObject).characterClassManager;

            if (MimicedRoles.Contains(ccm.CurClass))
            {
                if (value && (__instance.NetworkspeakingFlags & SpeakingFlags.MimicAs939) == 0)
                    __instance.NetworkspeakingFlags |= SpeakingFlags.MimicAs939;
                else if (!value && (__instance.NetworkspeakingFlags & SpeakingFlags.MimicAs939) != 0)
                    __instance.NetworkspeakingFlags ^= SpeakingFlags.MimicAs939;
            }
            else if (ccm.IsAnyScp() && HasAccessToSCPAlt.Contains(ccm.UserId))
            {
                if (value && (__instance.NetworkspeakingFlags & SpeakingFlags.MimicAs939) == 0)
                    __instance.NetworkspeakingFlags |= SpeakingFlags.MimicAs939;
                else if (!value && (__instance.NetworkspeakingFlags & SpeakingFlags.MimicAs939) != 0)
                    __instance.NetworkspeakingFlags ^= SpeakingFlags.MimicAs939;
            }
            return true;
        }
    }
}
