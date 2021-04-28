using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.CommandsExtender.Commands;
using Gamer.Mistaken.Systems.End;
using Gamer.Utilities;
using Interactables.Interobjects.DoorUtils;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.CommandsExtender
{
    internal class CommandsHandler : Module
    {
        public CommandsHandler(PluginHandler plugin) : base(plugin)
        {

        }

        public override string Name => "CommandsExtender";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.Destroying += this.Handle<Exiled.Events.EventArgs.DestroyingEventArgs>((ev) => Player_Destroying(ev));
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.RespawningTeam += this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.Dying += this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.Destroying -= this.Handle<Exiled.Events.EventArgs.DestroyingEventArgs>((ev) => Player_Destroying(ev));
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.RespawningTeam -= this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.Dying -= this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
        }

        private void Server_RoundStarted()
        {
            Timing.CallDelayed(5, () =>
            {
                foreach (var item in SwapSCPCommand.SwapCooldown.ToArray())
                {
                    if (RealPlayers.List.Any(p => p.UserId == item.Key))
                    {
                        var player = RealPlayers.List.First(p => p.UserId == item.Key);
                        if (player.Team == Team.SCP)
                        {
                            if (item.Value == 1)
                                SwapSCPCommand.SwapCooldown.Remove(player.UserId);
                            else
                                SwapSCPCommand.SwapCooldown[player.UserId]--;
                        }
                    }
                }
            });
        }

        private void Server_RespawningTeam(Exiled.Events.EventArgs.RespawningTeamEventArgs ev)
        {
            if (RespawnLockCommand.RespawnLock)
                ev.Players.Clear();
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player == null)
                return;
            try
            {
                TryUnHandcuffCommand.Tried.Remove(ev.Player.UserId);
                VanishHandler.SetGhost(ev.Player, false);
                if (VanishHandler.Vanished.ContainsKey(ev.Player.Id))
                {
                    VanishHandler.SetGhost(ev.Player, false);
                    ev.Player.Broadcast("VANISH", 10, "Ghostmode: <color=red><b>DISABLED</b></color>");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }

            if (ev.NewRole == RoleType.Spectator && SpecRoleCommand.SpecRole != RoleType.Spectator && SpecRoleCommand.SpecRole != RoleType.None)
                ev.NewRole = SpecRoleCommand.SpecRole;
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (MuteAllCommand.GlobalMuteActive && !ev.Player.IsMuted)
            {
                ev.Player.IsMuted = true;
                MuteAllCommand.Muted.Add(ev.Player.UserId);
            }
            else if (!MuteAllCommand.GlobalMuteActive && ev.Player.IsMuted && MuteAllCommand.Muted.Contains(ev.Player.UserId))
            {
                ev.Player.IsMuted = false;
                MuteAllCommand.Muted.Remove(ev.Player.UserId);
            }
        }

        private void Player_Destroying(Exiled.Events.EventArgs.DestroyingEventArgs ev)
        {
            if (!ev.Player.IsReadyPlayer())
                return;
            if (MuteAllCommand.Muted.Contains(ev.Player.UserId))
            {
                ev.Player.IsMuted = false;
                MuteAllCommand.Muted.Remove(ev.Player.UserId);
            }
            if (TalkCommand.Active.TryGetValue(ev.Player.UserId, out int[] players))
            {
                foreach (var playerId in players)
                {
                    if (TalkCommand.SavedInfo.TryGetValue(playerId, out (Vector3 Pos, RoleType Role, float HP, float AP, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762) data))
                    {
                        TalkCommand.SavedInfo.Remove(playerId);
                        Player p = RealPlayers.Get(playerId);
                        if (p == null)
                            continue;
                        p.Role = data.Role;
                        Timing.CallDelayed(0.5f, () =>
                        {
                            if (!p.IsConnected)
                                return;
                            p.Position = data.Pos;
                            p.Health = data.HP;
                            p.ArtificialHealth = data.AP;
                            p.Inventory.Clear();
                            foreach (var item in data.Inventory)
                                p.Inventory.items.Add(item);
                            p.Ammo[(int)AmmoType.Nato9] = data.Ammo9;
                            p.Ammo[(int)AmmoType.Nato556] = data.Ammo556;
                            p.Ammo[(int)AmmoType.Nato762] = data.Ammo762;
                        });
                    }
                }
                TalkCommand.Active.Remove(ev.Player.UserId);
            }
        }

        public static readonly Dictionary<string, (Player, Player)> LastAttackers = new Dictionary<string, (Player, Player)>();
        public static readonly Dictionary<string, (Player, Player)> LastVictims = new Dictionary<string, (Player, Player)>();
        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (!ev.Target.IsReadyPlayer())
                return;
            if (DmgInfoCommand.Active.Contains(ev.Target.Id))
                ev.Target.Broadcast("DMG INFO", 10, $"({ev.Attacker.Id}) {ev.Attacker.Nickname} | {ev.Attacker.UserId}\n{ev.DamageType.name} | {ev.Amount}");
            if (!LastAttackers.TryGetValue(ev.Target.UserId, out (Player, Player) attackers))
                LastAttackers[ev.Target.UserId] = (null, ev.Attacker);
            else
                LastAttackers[ev.Target.UserId] = (attackers.Item1, ev.Attacker);
            if (ev.Attacker?.UserId == null)
                return;
            if (!LastVictims.TryGetValue(ev.Attacker.UserId, out (Player, Player) victims))
                LastVictims[ev.Attacker.UserId] = (null, ev.Target);
            else
                LastVictims[ev.Attacker.UserId] = (victims.Item1, ev.Target);
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (!ev.Target.IsReadyPlayer())
                return;
            if (ev.Target.Role == RoleType.NtfCommander)
                TeslaOnCommand.AlreadyUsed.Remove(ev.Target.UserId);
            if (!LastAttackers.TryGetValue(ev.Target.UserId, out (Player, Player) attackers))
                LastAttackers[ev.Target.UserId] = (ev.Killer, null);
            else
                LastAttackers[ev.Target.UserId] = (ev.Killer, attackers.Item2);
            if (ev.Killer?.UserId == null)
                return;
            if (!LastVictims.TryGetValue(ev.Killer.UserId, out (Player, Player) victims))
                LastVictims[ev.Killer.UserId] = (ev.Target, null);
            else
                LastVictims[ev.Killer.UserId] = (ev.Target, victims.Item2);
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (MDestroyCommand.Active.Contains(ev.Player.Id))
                ev.Door.BreakDoor();
            MDestroyCommand.Active.Remove(ev.Player.Id);
            if (MOpenCommand.Active.Contains(ev.Player.Id))
                ev.Door.NetworkTargetState = true;
            MOpenCommand.Active.Remove(ev.Player.Id);
            if (MCloseCommand.Active.Contains(ev.Player.Id))
                ev.Door.NetworkTargetState = false;
            MCloseCommand.Active.Remove(ev.Player.Id);
            if (MLockCommand.Active.Contains(ev.Player.Id))
                ev.Door.NetworkActiveLocks |= (byte)DoorLockReason.AdminCommand;
            MLockCommand.Active.Remove(ev.Player.Id);
            if (MUnlockCommand.Active.Contains(ev.Player.Id))
                ev.Door.NetworkActiveLocks = (byte)(ev.Door.NetworkActiveLocks & ~((byte)DoorLockReason.AdminCommand));
            MUnlockCommand.Active.Remove(ev.Player.Id);
        }

        private void Server_RestartingRound()
        {
            MDestroyCommand.Active.Clear();
            MOpenCommand.Active.Clear();
            MCloseCommand.Active.Clear();
            MLockCommand.Active.Clear();
            MUnlockCommand.Active.Clear();
            RespawnLockCommand.RespawnLock = false;
            SpecRoleCommand.SpecRole = RoleType.Spectator;
            SwapSCPCommand.AlreadyChanged.Clear();
            MuteAllCommand.Muted.Clear();
            VanishHandler.Vanished.Clear();
            TalkCommand.SavedInfo.Clear();
            TalkCommand.Active.Clear();
            TeslaOnCommand.AlreadyUsed.Clear();
        }
    }
}


