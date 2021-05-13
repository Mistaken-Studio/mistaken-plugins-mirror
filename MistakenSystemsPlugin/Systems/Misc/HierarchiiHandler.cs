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
    public class HierarchiiHandler : Module
    {
        public HierarchiiHandler(PluginHandler p) : base(p)
        {
        }


        public override string Name => "Hierarchii";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangedRole += this.Handle<Exiled.Events.EventArgs.ChangedRoleEventArgs>((ev) => Player_ChangedRole(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangedRole -= this.Handle<Exiled.Events.EventArgs.ChangedRoleEventArgs>((ev) => Player_ChangedRole(ev));
        }

        private void Player_ChangedRole(Exiled.Events.EventArgs.ChangedRoleEventArgs ev)
        {
            if (ev.Player.IsAlive)
            {
                ev.Player.InfoArea = PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo | PlayerInfoArea.Nickname | PlayerInfoArea.PowerStatus | PlayerInfoArea.Role;
                UpdateAll();
            }
        }

        private void UpdateAll()
        {
            foreach (var player in RealPlayers.List)
            {
                foreach (var p in RealPlayers.List)
                {
                    if (player == p)
                        continue;
                    if (p.Team != Team.MTF && p.Team != Team.RSC)
                    {
                        if(p.Team != Team.MTF)
                            CustomInfoHandler.SetTarget(p, "unit", null, player);
                        CustomInfoHandler.SetTarget(p, "hierarchii", null, player);
                        continue;
                    }
                    CustomInfoHandler.SetTarget(p, "hierarchii", GetDiff(player, p), player);
                    if (p.Team == Team.MTF)
                        CustomInfoHandler.SetTarget(p, "unit", player.UnitName, player);
                }
            }
        }

        private string GetDiff(Player player1, Player player2)
        {
            if (player1.GetSessionVar<bool>(Main.SessionVarType.CC_TAU5) || player2.GetSessionVar<bool>(Main.SessionVarType.CC_TAU5))
                return "Ten sam poziom uprawnień";

            int player1Lvl = GetHierarchiiLevel(player1, player2);
            int player2Lvl = GetHierarchiiLevel(player2, player1);

            if (player1Lvl > player2Lvl)
                return "Wydawaj rozkazy";
            else if (player1Lvl == player2Lvl)
                return "Ten sam poziom uprawnień";
            else if (player1Lvl < player2Lvl)
                return "Wykonuj rozkazy";
            else
                return $"Wykryto błąd ({player1Lvl})|({player2Lvl})";
        }

        private int GetHierarchiiLevel(Player player, Player compared)
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
                    break;
            }
            if (player.GetSessionVar<bool>(Main.SessionVarType.CC_GUARD_COMMANDER))
                lvl = 500;
            else if (player.GetSessionVar<bool>(Main.SessionVarType.CC_DEPUTY_FACILITY_MANAGER) && Map.IsLCZDecontaminated)
                lvl = 501;
            else
            {
                int index = Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames.FindIndex(x => x.UnitName == player.UnitName);
                lvl += 99 - index;
            }


            return lvl;
        }

        private void Server_RoundStarted()
        {

        }
    }
}
