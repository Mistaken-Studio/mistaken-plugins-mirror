using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using Grenades;
using MEC;
using Mirror;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared.SocketAdmin;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using Gamer.Diagnostics;
using MistakenSocket.Shared.CentralToSL;
using MistakenSocket.Shared.API;
using MistakenSocket.Shared.ClientToCentral;
using MistakenSocket.Shared;

using Gamer.RoundLoggerSystem;

namespace Gamer.Mistaken.Systems.End
{
    internal class RoundLogHandler : Module
    {
        public RoundLogHandler(PluginHandler p) : base(p)
        {
            RoundLogger.IniIfNotAlready();
            RoundLogger.OnEnd += RoundLogHandler_OnEnd;
        }
        public override string Name => "RoundLog";
        public override void OnEnable()
        {
            MEC.Timing.CallDelayed(1, () =>
            {
                Exiled.Events.Handlers.Player.Banned += this.Handle<Exiled.Events.EventArgs.BannedEventArgs>((ev) => Player_Banned(ev));
                Exiled.Events.Handlers.Player.Kicked += this.Handle<Exiled.Events.EventArgs.KickedEventArgs>((ev) => Player_Kicked(ev));
                Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
                Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
                Exiled.Events.Handlers.Player.PreAuthenticating += this.Handle<Exiled.Events.EventArgs.PreAuthenticatingEventArgs>((ev) => Player_PreAuthenticating(ev));
                Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
                Exiled.Events.Handlers.CustomEvents.OnFirstTimeJoined += this.Handle<Exiled.Events.EventArgs.FirstTimeJoinedEventArgs>((ev) => CustomEvents_OnFirstTimeJoined(ev));
                Exiled.Events.Handlers.Player.Destroying += this.Handle<Exiled.Events.EventArgs.DestroyingEventArgs>((ev) => Player_Destroying(ev));
                Exiled.Events.Handlers.Player.Left += this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
                Exiled.Events.Handlers.Player.Escaping += this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
                Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
                Exiled.Events.Handlers.Player.ChangingGroup += this.Handle<Exiled.Events.EventArgs.ChangingGroupEventArgs>((ev) => Player_ChangingGroup(ev));
                Exiled.Events.Handlers.Player.ThrowingGrenade += this.Handle<Exiled.Events.EventArgs.ThrowingGrenadeEventArgs>((ev) => Player_ThrowingGrenade(ev));
                Exiled.Events.Handlers.Player.Handcuffing += this.Handle<Exiled.Events.EventArgs.HandcuffingEventArgs>((ev) => Player_Handcuffing(ev));
                Exiled.Events.Handlers.Player.RemovingHandcuffs += this.Handle<Exiled.Events.EventArgs.RemovingHandcuffsEventArgs>((ev) => Player_RemovingHandcuffs(ev));
                Exiled.Events.Handlers.Player.IntercomSpeaking += this.Handle<Exiled.Events.EventArgs.IntercomSpeakingEventArgs>((ev) => Player_IntercomSpeaking(ev));
                Exiled.Events.Handlers.Player.ActivatingWarheadPanel += this.Handle<Exiled.Events.EventArgs.ActivatingWarheadPanelEventArgs>((ev) => Player_ActivatingWarheadPanel(ev));
                Exiled.Events.Handlers.Player.ReceivingEffect += this.Handle<Exiled.Events.EventArgs.ReceivingEffectEventArgs>((ev) => Player_ReceivingEffect(ev));
                Exiled.Events.Handlers.Scp914.Activating += this.Handle<Exiled.Events.EventArgs.ActivatingEventArgs>((ev) => Scp914_Activating(ev));
                Exiled.Events.Handlers.Scp079.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Scp079_InteractingDoor(ev));
                Exiled.Events.Handlers.Scp079.InteractingTesla += this.Handle<Exiled.Events.EventArgs.InteractingTeslaEventArgs>((ev) => Scp079_InteractingTesla(ev));
                Exiled.Events.Handlers.Scp096.AddingTarget += this.Handle<Exiled.Events.EventArgs.AddingTargetEventArgs>((ev) => Scp096_AddingTarget(ev));
                Exiled.Events.Handlers.Scp096.Enraging += this.Handle<Exiled.Events.EventArgs.EnragingEventArgs>((ev) => Scp096_Enraging(ev));
                Exiled.Events.Handlers.Warhead.Detonated += this.Handle(() => Warhead_Detonated(), "WarheadDetonated");
                Exiled.Events.Handlers.Warhead.Starting += this.Handle<Exiled.Events.EventArgs.StartingEventArgs>((ev) => Warhead_Starting(ev));
                Exiled.Events.Handlers.Warhead.Stopping += this.Handle<Exiled.Events.EventArgs.StoppingEventArgs>((ev) => Warhead_Stopping(ev));
                Exiled.Events.Handlers.Server.RoundEnded += this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
                Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
                Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
                Exiled.Events.Handlers.Server.RespawningTeam += this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
                Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += this.Handle<Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs>((ev) => Server_SendingRemoteAdminCommand(ev));
                Exiled.Events.Handlers.Server.SendingConsoleCommand += this.Handle<Exiled.Events.EventArgs.SendingConsoleCommandEventArgs>((ev) => Server_SendingConsoleCommand(ev));
                Exiled.Events.Handlers.Map.Decontaminating += this.Handle<Exiled.Events.EventArgs.DecontaminatingEventArgs>((ev) => Map_Decontaminating(ev));
                Exiled.Events.Handlers.Map.GeneratorActivated += this.Handle<Exiled.Events.EventArgs.GeneratorActivatedEventArgs>((ev) => Map_GeneratorActivated(ev));
                Exiled.Events.Handlers.Map.ExplodingGrenade += this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
                Exiled.Events.Handlers.CustomEvents.OnBroadcast += this.Handle<Exiled.Events.EventArgs.BroadcastEventArgs>((ev) => CustomEvents_OnBroadcast(ev));
            });
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Banned -= this.Handle<Exiled.Events.EventArgs.BannedEventArgs>((ev) => Player_Banned(ev));
            Exiled.Events.Handlers.Player.Kicked -= this.Handle<Exiled.Events.EventArgs.KickedEventArgs>((ev) => Player_Kicked(ev));
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.PreAuthenticating -= this.Handle<Exiled.Events.EventArgs.PreAuthenticatingEventArgs>((ev) => Player_PreAuthenticating(ev));
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.CustomEvents.OnFirstTimeJoined -= this.Handle<Exiled.Events.EventArgs.FirstTimeJoinedEventArgs>((ev) => CustomEvents_OnFirstTimeJoined(ev));
            Exiled.Events.Handlers.Player.Destroying -= this.Handle<Exiled.Events.EventArgs.DestroyingEventArgs>((ev) => Player_Destroying(ev));
            Exiled.Events.Handlers.Player.Left -= this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
            Exiled.Events.Handlers.Player.Escaping -= this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.ChangingGroup -= this.Handle<Exiled.Events.EventArgs.ChangingGroupEventArgs>((ev) => Player_ChangingGroup(ev));
            Exiled.Events.Handlers.Player.ThrowingGrenade -= this.Handle<Exiled.Events.EventArgs.ThrowingGrenadeEventArgs>((ev) => Player_ThrowingGrenade(ev));
            Exiled.Events.Handlers.Player.Handcuffing -= this.Handle<Exiled.Events.EventArgs.HandcuffingEventArgs>((ev) => Player_Handcuffing(ev));
            Exiled.Events.Handlers.Player.RemovingHandcuffs -= this.Handle<Exiled.Events.EventArgs.RemovingHandcuffsEventArgs>((ev) => Player_RemovingHandcuffs(ev));
            Exiled.Events.Handlers.Player.IntercomSpeaking -= this.Handle<Exiled.Events.EventArgs.IntercomSpeakingEventArgs>((ev) => Player_IntercomSpeaking(ev));
            Exiled.Events.Handlers.Player.ActivatingWarheadPanel -= this.Handle<Exiled.Events.EventArgs.ActivatingWarheadPanelEventArgs>((ev) => Player_ActivatingWarheadPanel(ev));
            Exiled.Events.Handlers.Player.ReceivingEffect -= this.Handle<Exiled.Events.EventArgs.ReceivingEffectEventArgs>((ev) => Player_ReceivingEffect(ev));
            Exiled.Events.Handlers.Scp914.Activating -= this.Handle<Exiled.Events.EventArgs.ActivatingEventArgs>((ev) => Scp914_Activating(ev));
            Exiled.Events.Handlers.Scp079.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Scp079_InteractingDoor(ev));
            Exiled.Events.Handlers.Scp079.InteractingTesla -= this.Handle<Exiled.Events.EventArgs.InteractingTeslaEventArgs>((ev) => Scp079_InteractingTesla(ev));
            Exiled.Events.Handlers.Scp096.AddingTarget -= this.Handle<Exiled.Events.EventArgs.AddingTargetEventArgs>((ev) => Scp096_AddingTarget(ev));
            Exiled.Events.Handlers.Scp096.Enraging -= this.Handle<Exiled.Events.EventArgs.EnragingEventArgs>((ev) => Scp096_Enraging(ev));
            Exiled.Events.Handlers.Warhead.Detonated -= this.Handle(() => Warhead_Detonated(), "WarheadDetonated");
            Exiled.Events.Handlers.Warhead.Starting -= this.Handle<Exiled.Events.EventArgs.StartingEventArgs>((ev) => Warhead_Starting(ev));
            Exiled.Events.Handlers.Warhead.Stopping -= this.Handle<Exiled.Events.EventArgs.StoppingEventArgs>((ev) => Warhead_Stopping(ev));
            Exiled.Events.Handlers.Server.RoundEnded -= this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RespawningTeam -= this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= this.Handle<Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs>((ev) => Server_SendingRemoteAdminCommand(ev));
            Exiled.Events.Handlers.Server.SendingConsoleCommand -= this.Handle<Exiled.Events.EventArgs.SendingConsoleCommandEventArgs>((ev) => Server_SendingConsoleCommand(ev));
            Exiled.Events.Handlers.Map.Decontaminating -= this.Handle<Exiled.Events.EventArgs.DecontaminatingEventArgs>((ev) => Map_Decontaminating(ev));
            Exiled.Events.Handlers.Map.GeneratorActivated -= this.Handle<Exiled.Events.EventArgs.GeneratorActivatedEventArgs>((ev) => Map_GeneratorActivated(ev));
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= this.Handle<Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs>((ev) => Server_SendingRemoteAdminCommand(ev));
            Exiled.Events.Handlers.Server.SendingConsoleCommand -= this.Handle<Exiled.Events.EventArgs.SendingConsoleCommandEventArgs>((ev) => Server_SendingConsoleCommand(ev));
            Exiled.Events.Handlers.CustomEvents.OnBroadcast -= this.Handle<Exiled.Events.EventArgs.BroadcastEventArgs>((ev) => CustomEvents_OnBroadcast(ev));
        }

        private void RoundLogHandler_OnEnd(RoundLogger.LogMessage[] logs)
        {
            string dir = Paths.Plugins + "/RoundLogger/";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            dir += Server.Port + "/";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.AppendAllLines(dir + $"{DateTime.Now:yyyy-MM-dd_HH:mm:ss}.log", logs.Select(i => i.ToString()));
        }

        private void CustomEvents_OnBroadcast(Exiled.Events.EventArgs.BroadcastEventArgs ev)
        {
            if (ev.Type == Broadcast.BroadcastFlags.AdminChat)
            {
                RoundLogger.Log("GAME EVENT", "ADMIN CHAT", $"{ev.AdminName} sent \"{ev.Content}\"");
            }
            else
            {
                RoundLogger.Log("GAME EVENT", "BROADCAST", $"Broadcasted \"{ev.Content}\" to {ev.Targets.Length} players");
            }
        }

        private void Map_GeneratorActivated(Exiled.Events.EventArgs.GeneratorActivatedEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "GENERATOR", $"Generator in {ev.Generator.CurRoom} activated");
        }
        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "GRENADE", $"{PTS(ev.Thrower)}'s {(ev.IsFrag ? "Frag" : "Flash")} exploading on {ev.TargetToDamages.Count} target ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Map_Decontaminating(Exiled.Events.EventArgs.DecontaminatingEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "DECONTAMINATION", $"Decontamination finished ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Server_SendingConsoleCommand(Exiled.Events.EventArgs.SendingConsoleCommandEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "CONSOLE", $"{PTS(ev.Player)} run {ev.Name} with args ({string.Join(", ",ev.Arguments)}) with result: {(ev.Allow ? "Allowed" : "Disallowed")}");
        }
        private void Server_SendingRemoteAdminCommand(Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "RA", $"{PTS(ev.Sender) ?? "Consols"} run {ev.Name} with args ({string.Join(", ", ev.Arguments)}) with result: {(ev.Success ? "Success" : "Failure")}");
        }
        private void Server_RespawningTeam(Exiled.Events.EventArgs.RespawningTeamEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "TEAM RESPAWN", $"Spawning {(ev.NextKnownTeam == Respawning.SpawnableTeamType.ChaosInsurgency ? "Chaos" : "MTF")}, {ev.Players.Count} players out of max {ev.MaximumRespawnAmount} ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Server_RoundStarted()
        {
            RoundLogger.Log("GAME EVENT", "ROUND STARTED", $"Round has been started");
        }
        private void Server_WaitingForPlayers()
        {
            RoundLogger.Log("GAME EVENT", "ROUND READY", $"Waiting for players to join");
        }
        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "ROUND END", $"Round has ended, {ev.LeadingTeam} won, restarting in {ev.TimeToRestart}s");
        }
        private void Warhead_Stopping(Exiled.Events.EventArgs.StoppingEventArgs ev)
        {
            RoundLogger.Log("WARHEAD EVENT", "STOP", $"{PTS(ev.Player)} stoped warhead ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Warhead_Starting(Exiled.Events.EventArgs.StartingEventArgs ev)
        {
            RoundLogger.Log("WARHEAD EVENT", "START", $"{PTS(ev.Player)} started warhead ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Warhead_Detonated()
        {
            RoundLogger.Log("WARHEAD EVENT", "DETONATED", $"Warhead Detonated");
        }
        private void Scp096_Enraging(Exiled.Events.EventArgs.EnragingEventArgs ev)
        {
            RoundLogger.Log("SCP096 EVENT", "ENRAGE", $"{PTS(ev.Player)} enraged {PTS(Player.Get(ev.Scp096.Hub))} ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Scp096_AddingTarget(Exiled.Events.EventArgs.AddingTargetEventArgs ev)
        {
            RoundLogger.Log("SCP096 EVENT", "ADD TARGET", $"{PTS(ev.Target)} is {PTS(ev.Scp096)}'s target ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Scp079_InteractingTesla(Exiled.Events.EventArgs.InteractingTeslaEventArgs ev)
        {
            RoundLogger.Log("SCP079 EVENT", "TESLA", $"{PTS(ev.Player)} trigered tesla ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Scp079_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            RoundLogger.Log("SCP079 EVENT", "DOOR", $"{PTS(ev.Player)} interacted with door ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Scp914_Activating(Exiled.Events.EventArgs.ActivatingEventArgs ev)
        {
            RoundLogger.Log("SCP914 EVENT", "ACTIVATION", $"{PTS(ev.Player)} activated 914 ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Player_ReceivingEffect(Exiled.Events.EventArgs.ReceivingEffectEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "RECIVE EFFECT", $"Updated status of {ev.Effect.GetType().Name} for {PTS(ev.Player)} from {ev.CurrentState} to {ev.State}, duration: {ev.Duration}s ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Player_ActivatingWarheadPanel(Exiled.Events.EventArgs.ActivatingWarheadPanelEventArgs ev)
        {
            RoundLogger.Log("WARHEAD EVENT", "PANEL", $"{PTS(ev.Player)} unlocked button ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Player_IntercomSpeaking(Exiled.Events.EventArgs.IntercomSpeakingEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "INTERCOM", $"{PTS(ev.Player)} is using intercom ({(ev.IsAllowed ? "allowed" : "disallowed")})");
        }
        private void Player_RemovingHandcuffs(Exiled.Events.EventArgs.RemovingHandcuffsEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "HANDCUFF", $"{PTS(ev.Target)} was uncuffed, cuffer was {PTS(ev.Cuffer)} ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Player_ThrowingGrenade(Exiled.Events.EventArgs.ThrowingGrenadeEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "GRENADE", $"{PTS(ev.Player)} thrown {ev.Type} {(ev.IsSlow ? "slowly " : "")}with fuse {ev.FuseTime} ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Player_Handcuffing(Exiled.Events.EventArgs.HandcuffingEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "HANDCUFF", $"{PTS(ev.Target)} was cuffed, by {PTS(ev.Cuffer)} ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Player_ChangingGroup(Exiled.Events.EventArgs.ChangingGroupEventArgs ev)
        {
            RoundLogger.Log("ADMIN EVENT", "CHANGE GROUP", $"{PTS(ev.Player)}'s group was changed to {ev.NewGroup.BadgeText} ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "CLASS CHANGE", $"{PTS(ev.Player)} changed role from {ev.Player.Role} to {ev.NewRole} | Escape: {ev.IsEscaped}");
        }
        private void Player_Escaping(Exiled.Events.EventArgs.EscapingEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "ESCAPE", $"{PTS(ev.Player)} escaped ({(ev.IsAllowed ? "allowed" : "denied")})");
        }
        private void CustomEvents_OnFirstTimeJoined(Exiled.Events.EventArgs.FirstTimeJoinedEventArgs ev)
        {
            RoundLogger.Log("NETWORK EVENT", "PLAYER JOINED FIRST TIME", $"{PTS(ev.Player)} joined first time");
        }
        private void Player_Destroying(Exiled.Events.EventArgs.DestroyingEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "PLAYER DESTROYED", $"{PTS(ev.Player)} was destroyed");
        }
        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            RoundLogger.Log("NETWORK EVENT", "PLAYER LEFT", $"{PTS(ev.Player)} left");
        }
        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            RoundLogger.Log("NETWORK EVENT", "PLAYER VERIFIED", $"{PTS(ev.Player)} was verified");
        }
        private void Player_PreAuthenticating(Exiled.Events.EventArgs.PreAuthenticatingEventArgs ev)
        {
            RoundLogger.Log("NETWORK EVENT", "PLAYER PREAUTHED", $"Preauthing {ev.UserId} from {ev.Request.RemoteEndPoint.Address} ({ev.Country}) with flags {ev.Flags}, {(ev.IsAllowed ? "allowed" : "denied")}");
        }
        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "DAMAGE", $"{PTS(ev.Target)} was hurt by {PTS(ev.Attacker) ?? "WORLD"} using {ev.DamageType.name}, done {ev.Amount} damage");
        }
        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            RoundLogger.Log("GAME EVENT", "KILL", $"{PTS(ev.Target)} was killed by {PTS(ev.Killer) ?? "WORLD"} using {ev.HitInformations.GetDamageName()}");
        }
        private void Player_Kicked(Exiled.Events.EventArgs.KickedEventArgs ev)
        {
            RoundLogger.Log("ADMIN EVENT", "KICK", $"{PTS(ev.Target)} was kicked with reason {ev.Reason}");
        }
        private void Player_Banned(Exiled.Events.EventArgs.BannedEventArgs ev)
        {
            RoundLogger.Log("ADMIN EVENT", "BAN", $"{PTS(ev.Target)} was banned by {PTS(ev.Issuer)} with reason {ev.Details.Reason}");
        }

        private string PTS(Player player) => player == null ? null : $"({player.Id}) {player.Nickname}";
    }
}
