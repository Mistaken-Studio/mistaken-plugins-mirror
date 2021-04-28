using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using Grenades;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

namespace Gamer.Mistaken.Systems.End
{
    internal class VanishHandler : Module
    {
        public static Dictionary<int, int> Vanished { get; } = new Dictionary<int, int>();

        public VanishHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "Vanish";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(IRoundStarted());
        }

        private IEnumerator<float> IRoundStarted()
        {
            yield return Timing.WaitForSeconds(1);
            while(Round.IsStarted)
            {
                yield return Timing.WaitForSeconds(1);
                var start = DateTime.Now;
                foreach (var player in RealPlayers.List)
                {
                    bool cond = player.IsHuman && player.Position.y < -1900;
                    player.TargetGhostsHashSet.RemoveWhere(i => !Pets.PetsHandler.PetsIds.Contains(i));
                    foreach (var seenPlayer in Player.List)
                    {
                        if(cond)
                        {
                            Hide(player, seenPlayer.Id);
                            continue;
                        }
                        if (Vanished.Count == 0)
                            continue;
                        if (seenPlayer == player)
                            continue;
                        if (!Vanished.TryGetValue(seenPlayer.Id, out int level))
                            continue;
                        if (level == 1)
                        {
                            if (!HasAdminChat(player.ReferenceHub.serverRoles.Permissions))
                                Hide(player, seenPlayer.Id);
                        }
                        else if (level == 2)
                        {
                            if (!HasAdminChat(player.ReferenceHub.serverRoles.Permissions) || !(player.Team == Team.TUT || player.Team == Team.RIP))
                                Hide(player, seenPlayer.Id);
                        }
                        else if (level == 3)
                        {
                            if (!HasAdminChat(player.ReferenceHub.serverRoles.Permissions) || player.ReferenceHub.serverRoles.KickPower < seenPlayer.ReferenceHub.serverRoles.KickPower)
                                Hide(player, seenPlayer.Id);
                        }
                    }
                }

                Diagnostics.MasterHandler.LogTime("VanisHandler", "IRoundStarted", start, DateTime.Now);
            }
        }

        private static void Hide(Player seeingPlayer, int seenPlayer)
        {
            seeingPlayer.TargetGhostsHashSet.Add(seenPlayer);
        }

        //Level 1 -> All Admin
        //Level 2 -> Tutorial and Spectator Admins Only
        //Level 3 -> Older and Same Admin Only
        internal static void SetGhost(Player player, bool value, byte level = 1, bool silent = false)
        {
            if (value)
            {
                if (Vanished.ContainsKey(player.Id))
                    SetGhost(player, false, level, true);
                RoundLogger.Log("VANISH", "ENABLED", $"Vanish enabled for {player.PlayerToString()}, type {level}");
                Vanished.Add(player.Id, level);
                LOFH.LOFH.AddVanish(player.UserId, level);
                player.SetSessionVar(Main.SessionVarType.VANISH, level);
                if (!silent)
                    AnnonymousEvents.Call("VANISH", (player, level));
            }
            else
            {
                if (Vanished.ContainsKey(player.Id))
                {
                    RoundLogger.Log("VANISH", "DISABLED", $"Vanish disabled for {player.PlayerToString()}, type {level}");
                    if (!silent)
                        AnnonymousEvents.Call("VANISH", (player, (byte)0));
                }
                Vanished.Remove(player.Id);
                LOFH.LOFH.RemoveVanish(player.UserId);
                player.SetSessionVar(Main.SessionVarType.VANISH, (byte)0);
            }
        }

        private static bool HasAdminChat(ulong perm)
        {
            if (perm == 0)
                return false;
            return (perm & (ulong)PlayerPermissions.AdminChat) != 0;
        }
    }
}
