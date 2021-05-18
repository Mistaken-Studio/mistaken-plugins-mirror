using Exiled.API.Features;
using Exiled.Events.EventArgs;
using HarmonyLib;
using Respawning.NamingRules;
using System.Linq;

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(NineTailedFoxNamingRule), nameof(NineTailedFoxNamingRule.PlayEntranceAnnouncement))]
    internal static class AnnouncingNtfEntrance
    {
        private static bool Prefix(ref string regular)
        {
            int scpsLeft = Player.List.Where(
                (Player player) =>
                    player.IsScp &&
                    player.Role != RoleType.Scp0492 &&
                    (!player.SessionVariables.TryGetValue("IsNPC", out object isNPC) || !(bool)isNPC))
                .Count();
            string[] unitInformations = regular.Split('-');

            var ev = new AnnouncingNtfEntranceEventArgs(scpsLeft, unitInformations[0], int.Parse(unitInformations[1]));

            Exiled.Events.Handlers.Map.OnAnnouncingNtfEntrance(ev);

            regular = $"{ev.UnitName}-{ev.UnitNumber}";

            return ev.IsAllowed;
        }
    }
}
