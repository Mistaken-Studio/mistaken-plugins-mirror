using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base;
using Gamer.Utilities;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Misc
{
    public class HierarchyHandler : Module
    {
        public HierarchyHandler(PluginHandler p) : base(p)
        {
        }


        public override string Name => "Hierarchy";
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangedRole -= this.Handle<Exiled.Events.EventArgs.ChangedRoleEventArgs>((ev) => Player_ChangedRole(ev));
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RespawningTeam -= this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
            Exiled.Events.Handlers.Scp049.FinishingRecall -= this.Handle<Exiled.Events.EventArgs.FinishingRecallEventArgs>((ev) => Scp049_FinishingRecall(ev));
        }
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangedRole += this.Handle<Exiled.Events.EventArgs.ChangedRoleEventArgs>((ev) => Player_ChangedRole(ev));
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RespawningTeam += this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
            Exiled.Events.Handlers.Scp049.FinishingRecall += this.Handle<Exiled.Events.EventArgs.FinishingRecallEventArgs>((ev) => Scp049_FinishingRecall(ev));
        }

        private void Scp049_FinishingRecall(Exiled.Events.EventArgs.FinishingRecallEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            this.CallDelayed(1, () => UpdateAll(), "ChangedRoleLate");
        }

        private void Server_RespawningTeam(Exiled.Events.EventArgs.RespawningTeamEventArgs ev)
        {
            DisableUpdate = true;
            this.CallDelayed(5, () =>
            {
                DisableUpdate = false;
                UpdateAll();
            });
        }

        private static bool DisableUpdate = false;
        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            foreach (var item in RealPlayers.List.Where(p => p != ev.Player && p.Connection != null))
            {
                try
                {
                    ev.Player.SendFakeSyncVar(item.Connection.identity, typeof(CharacterClassManager), nameof(CharacterClassManager.NetworkCurSpawnableTeamType), 0);
                }
                catch(System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
            }
        }

        private void Player_ChangedRole(Exiled.Events.EventArgs.ChangedRoleEventArgs ev)
        {
            if (ev.Player.Team == Team.MTF)
            {
                this.CallDelayed(1, () =>
                {
                    foreach (var item in RealPlayers.List.Where(p => p != ev.Player && p.Connection != null))
                        item.SendFakeSyncVar(ev.Player.Connection.identity, typeof(CharacterClassManager), nameof(CharacterClassManager.NetworkCurSpawnableTeamType), 0);

                }, "LateForceNoBaseGameHierarchy");
            }
            if (ev.Player.IsAlive)
                this.CallDelayed(1, () => UpdateAll(), "ChangedRoleLate");
        }

        public static void UpdateAll()
        {
            if (DisableUpdate)
                return;
            foreach (var player in RealPlayers.List)
            {
                if (player.Team != Team.MTF && player.Team != Team.RSC)
                {
                    foreach (var p in RealPlayers.List)
                    {
                        CustomInfoHandler.SetTarget(p, "unit", null, player);
                        CustomInfoHandler.SetTarget(p, "hierarchii", null, player);
                    }
                    continue;
                }
                foreach (var p in RealPlayers.List)
                {
                    if (player == p)
                        continue;
                    if ((p.Team != Team.MTF && p.Team != Team.RSC))
                    {
                        CustomInfoHandler.SetTarget(p, "unit", null, player);
                        CustomInfoHandler.SetTarget(p, "hierarchii", null, player);
                        continue;
                    }

                    CustomInfoHandler.SetTarget(p, "hierarchii", GetDiff(player, p), player);
                    if (p.Team == Team.MTF)
                        CustomInfoHandler.SetTarget(p, "unit", "Unit: " + p.UnitName, player);
                    else
                        CustomInfoHandler.SetTarget(p, "unit", null, player);
                }
            }
        }
        private static string GetDiff(Player player1, Player player2)
        {
            int player1Lvl = GetHierarchiiLevel(player1, player2);
            int player2Lvl = GetHierarchiiLevel(player2, player1);

            if (player1Lvl == -1 || player2Lvl == -1)
                return null;
            if (player1.GetSessionVar<bool>(Main.SessionVarType.CC_TAU5) || player2.GetSessionVar<bool>(Main.SessionVarType.CC_TAU5))
                return "<b>Ten sam poziom uprawnień</b>";
            if (player1Lvl > player2Lvl)
                return $"<b>Wydawaj rozkazy</b>";
            else if (player1Lvl == player2Lvl)
                return $"<b>Ten sam poziom uprawnień</b>";
            else if (player1Lvl < player2Lvl)
                return $"<b>Wykonuj rozkazy</b>";
            
            return $"<b>Wykryto błąd (Niewykonalny kod się wykonał) ({player1Lvl})|({player2Lvl})</b>";
        }
        private static int GetHierarchiiLevel(Player player, Player compared)
        {
            int lvl = 0;
            switch (player.Role)
            {
                case RoleType.FacilityGuard:
                    lvl = 100;
                    break;
                case RoleType.NtfCadet:
                    lvl = 200;
                    break;
                case RoleType.NtfScientist:
                case RoleType.NtfLieutenant:
                    lvl = 300;
                    break;
                case RoleType.NtfCommander:
                    lvl = 400;
                    break;

                case RoleType.Scientist:
                    if(compared.Role == RoleType.Scientist)
                    {
                        if (player.GetSessionVar<bool>(Main.SessionVarType.CC_DEPUTY_FACILITY_MANAGER))
                            return 999;
                        else if (compared.GetSessionVar<bool>(Main.SessionVarType.CC_DEPUTY_FACILITY_MANAGER))
                            return -999;

                        if (player.GetSessionVar<bool>(Main.SessionVarType.CC_ZONE_MANAGER))
                            return 998;
                        else if (compared.GetSessionVar<bool>(Main.SessionVarType.CC_ZONE_MANAGER))
                            return -998;

                        return 0;
                    }
                    if (player.GetSessionVar<bool>(Main.SessionVarType.CC_DEPUTY_FACILITY_MANAGER) && Map.IsLCZDecontaminated)
                        return 501;
                    return -1;
            }
            if (player.GetSessionVar<bool>(Main.SessionVarType.CC_GUARD_COMMANDER))
                return 500;
            if (player.GetSessionVar<bool>(Main.SessionVarType.CC_DEPUTY_FACILITY_MANAGER) && Map.IsLCZDecontaminated)
                return 501;
            if (compared.Role == RoleType.Scientist)
                return -1;
            int index = Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames.FindIndex(x => x.UnitName == player.UnitName);
            return lvl + 99 - index;
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(SyncData());
        }

        private IEnumerator<float> SyncData()
        {
            DisableUpdate = true;
            yield return Timing.WaitForSeconds(5);
            DisableUpdate = false;
            while (Round.IsStarted)
            {
                UpdateAll();
                yield return Timing.WaitForSeconds(10);
            }
        }
    }
}
