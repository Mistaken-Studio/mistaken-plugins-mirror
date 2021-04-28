#pragma warning disable IDE0079
#pragma warning disable IDE0060

using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using MEC;
using Mirror;
using MistakenSocket.Client.SL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using Gamer.Diagnostics;
using MistakenSocket.Shared.CentralToSL;

namespace Gamer.Mistaken.Systems.End
{
    public class OverwatchHandler : Module
    {
        public OverwatchHandler(PluginHandler p) : base(p)
        {
        }
        public override string Name => "Overwatch";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RoundEnded += this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
            Exiled.Events.Handlers.Player.Destroying += this.Handle<Exiled.Events.EventArgs.DestroyingEventArgs>((ev) => Player_Destroying(ev));
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += this.Handle<Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs>((ev) => Server_SendingRemoteAdminCommand(ev));
            Exiled.Events.Handlers.Server.SendingConsoleCommand += this.Handle<Exiled.Events.EventArgs.SendingConsoleCommandEventArgs>((ev) => Server_SendingConsoleCommand(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RoundEnded -= this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
            Exiled.Events.Handlers.Player.Destroying -= this.Handle<Exiled.Events.EventArgs.DestroyingEventArgs>((ev) => Player_Destroying(ev));
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= this.Handle<Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs>((ev) => Server_SendingRemoteAdminCommand(ev));
            Exiled.Events.Handlers.Server.SendingConsoleCommand -= this.Handle<Exiled.Events.EventArgs.SendingConsoleCommandEventArgs>((ev) => Server_SendingConsoleCommand(ev));
        }

        private void Server_SendingConsoleCommand(Exiled.Events.EventArgs.SendingConsoleCommandEventArgs ev)
        {
            if (ev.Player?.IsHost ?? true)
                return;
            if (InOverwatch.ContainsKey(ev.Player.UserId))
                InOverwatch[ev.Player.UserId] = DateTime.Now;
        }

        private void Server_SendingRemoteAdminCommand(Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs ev)
        {
            if (!ev.CommandSender.IsPlayer())
                return;
            if (InOverwatch.ContainsKey(ev.Sender.UserId))
                InOverwatch[ev.Sender.UserId] = DateTime.Now;
        }

        private void Player_Destroying(Exiled.Events.EventArgs.DestroyingEventArgs ev)
        {
            if (!(ev.Player?.IsReadyPlayer() ?? false))
                return;
            InOverwatch.Remove(ev.Player.UserId);
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            InOverwatch.Clear();
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(DoRoundLoop());
        }

        public static readonly HashSet<string> InLongOverwatch = new HashSet<string>();

        public static readonly Dictionary<string, DateTime> InOverwatch = new Dictionary<string, DateTime>();

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (InLongOverwatch.Contains(ev.Player.UserId))
            {
                ev.Player.IsOverwatchEnabled = true;
                ev.Player.Broadcast("AUTO OVERWATCH", 10, "<color=cyan>Overwatch enabled</color>");
            }
        }

        private static IEnumerator<float> DoRoundLoop()
        {
            yield return Timing.WaitForSeconds(1);
            while(Round.IsStarted)
            {
                foreach (var player in Player.List.Where(p => p.CheckPermissions(PlayerPermissions.Overwatch)))
                {
                    if (InLongOverwatch.Contains(player.UserId) && !player.IsOverwatchEnabled)
                    {
                        InOverwatch.Remove(player.UserId);
                        InLongOverwatch.Remove(player.UserId);
                        player.SetSessionVar(Main.SessionVarType.LONG_OVERWATCH, false);
                        Base.GUI.PseudoGUIHandler.Set(player, "long_overwatch", Base.GUI.PseudoGUIHandler.Position.TOP, null);
                        AnnonymousEvents.Call("LONG_OVERWATCH", (player, false));
                        continue;
                    }
                    if (InOverwatch.TryGetValue(player.UserId, out DateTime updateTime))
                    {
                        if (!player.IsOverwatchEnabled)
                        {
                            InOverwatch.Remove(player.UserId);
                            continue;
                        }
                        if ((DateTime.Now - updateTime).TotalMinutes >= 5)
                        {
                            InOverwatch.Remove(player.UserId);
                            InLongOverwatch.Add(player.UserId);
                            player.SetSessionVar(Main.SessionVarType.LONG_OVERWATCH, true);
                            Base.GUI.PseudoGUIHandler.Set(player, "long_overwatch", Base.GUI.PseudoGUIHandler.Position.TOP, "Active: <color=red>Long Overwatch</color>");
                            AnnonymousEvents.Call("LONG_OVERWATCH", (player, true));
                        }
                    }
                    else if(!InLongOverwatch.Contains(player.UserId))
                    {
                        if (player.IsOverwatchEnabled)
                            InOverwatch.Add(player.UserId, DateTime.Now);
                    }
                }
                yield return Timing.WaitForSeconds(1);
            }
        }
    }
}
