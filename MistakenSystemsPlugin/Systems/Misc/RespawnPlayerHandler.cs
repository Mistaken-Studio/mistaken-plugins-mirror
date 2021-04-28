#pragma warning disable IDE0079
#pragma warning disable IDE0060

using Exiled.API.Enums;
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

namespace Gamer.Mistaken.Systems.Misc
{
    internal class RespawnPlayerHandler : Module
    {
        public RespawnPlayerHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "RespawnPlayer";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Left += this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
            Exiled.Events.Handlers.Scp106.Containing += this.Handle<Exiled.Events.EventArgs.ContainingEventArgs>((ev) => Scp106_Containing(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Left -= this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
            Exiled.Events.Handlers.Scp106.Containing -= this.Handle<Exiled.Events.EventArgs.ContainingEventArgs>((ev) => Scp106_Containing(ev));
        }

        private void Server_RestartingRound()
        {
            Change106 = true;
        }

        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            if(Round.IsStarted)
                RespawnPlayer(ev.Player);
        }

        private static bool Change106 = true;
        private void Scp106_Containing(Exiled.Events.EventArgs.ContainingEventArgs ev)
        {
            Change106 = false;
        }

        internal static void RespawnPlayer(Player currentPlayer)
        {
            if (!currentPlayer.IsReadyPlayer())
                return;
            if (RespawnSCP(currentPlayer))
            {
                RoundLogger.Log("RESPAWN", "SCP", "Respawning SCP");
            }
            else if (currentPlayer.Team != Team.RIP)
            {
                currentPlayer.Kill("Heart Attack");
                RoundLogger.Log("RESPAWN", "HUMAN", "Respawning human");
            }
        }

        internal static bool RespawnSCP(Player currentSCP)
        {
            if (currentSCP.Team == Team.SCP && currentSCP.Role != RoleType.Scp0492 && (Change106 || currentSCP.Role != RoleType.Scp106))
            {
                var spectators = RealPlayers.List.Where(p => p.IsDead && !p.IsOverwatchEnabled).OrderBy(p => p.ReferenceHub.characterClassManager.DeathTime).ToArray();
                if (spectators.Length == 0)
                {
                    MapPlus.Broadcast("RESPAWN", 10, $"SCP player Change, ({currentSCP.Id}) {currentSCP.Nickname} {currentSCP.UserId} -> Nobody", Broadcast.BroadcastFlags.AdminChat);
                    currentSCP.Kill("Heart Attack");
                }
                else
                {
                    MapPlus.Broadcast("RESPAWN", 10, $"SCP player Change, ({currentSCP.Id}) {currentSCP.Nickname} {currentSCP.UserId} -> ({spectators[0].Id}) {spectators[0].Nickname} {spectators[0].UserId}", Broadcast.BroadcastFlags.AdminChat);
                    spectators[0].Role = currentSCP.Role;
                    if (currentSCP.Role == RoleType.Scp079)
                    {
                        spectators[0].Energy = currentSCP.Energy;
                        spectators[0].Level = currentSCP.Level;
                        spectators[0].Experience = currentSCP.Experience;
                        spectators[0].Camera = currentSCP.Camera;
                    }
                    else
                    {
                        Timing.CallDelayed(1, () =>
                        {
                            spectators[0].Position = currentSCP.Position + Vector3.up;
                        });
                        spectators[0].Health = currentSCP.Health;
                        spectators[0].ArtificialHealth = currentSCP.ArtificialHealth;
                        spectators[0].MaxArtificialHealth = currentSCP.MaxArtificialHealth;
                    }
                    currentSCP.Role = RoleType.Spectator;
                    spectators[0].Broadcast(10, $"Player {currentSCP.Nickname} left game so you were moved to his position");
                }
                return true;
            }
            return false;
        }
    }
}
