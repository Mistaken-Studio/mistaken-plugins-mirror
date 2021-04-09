using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Systems.Staff;
using Gamer.Utilities;
using Gamer.Mistaken.Utilities.APILib;
using LiteNetLib.Utils;
using MEC;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared;
using MistakenSocket.Shared.API;
using Newtonsoft.Json;
using UnityEngine;

namespace Gamer.Mistaken.CustomWhitelist
{
    public class Handler : Module
    {
        public override bool IsBasic => true;
        public static ReadOnlyCollection<string> PublicWhitelist => 
            new ReadOnlyCollection<string>(Whitelist.ToArray());
             
        internal static HashSet<string> Whitelist = new HashSet<string>();

        public Handler(PluginHandler plugin) : base(plugin)
        {
            MEC.Timing.CallDelayed(5, () => ReloadData());

            plugin.RegisterTranslation("cwhitelist_deny_whitelist_pl", "Nie jesteś na whiteliście!   Jeżeli chcesz się na nią dostać wejdź na naszego discorda(discord.mistaken.pl w przeglądarce) po więcej informacji");
            plugin.RegisterTranslation("cwhitelist_deny_serverfull_pl", "Server jest pełen!   {current_players}/{max_players}");
            plugin.RegisterTranslation("cwhitelist_deny_serverfull_reserved_pl", "Server jest pełen, Wszystkie 5 rezerwowanych slotów jest zajęte  {current_players}/{max_players}");
        }

        public override string Name => "CustomWhitelist";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.PreAuthenticating += this.Handle<Exiled.Events.EventArgs.PreAuthenticatingEventArgs>((ev) => Player_PreAuthenticating(ev));
            Exiled.Events.Handlers.Player.Left += this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.CustomEvents.OnFirstTimeJoined += this.Handle<Exiled.Events.EventArgs.FirstTimeJoinedEventArgs>((ev) => CustomEvents_OnFirstTimeJoined(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.PreAuthenticating -= this.Handle<Exiled.Events.EventArgs.PreAuthenticatingEventArgs>((ev) => Player_PreAuthenticating(ev));
            Exiled.Events.Handlers.Player.Left -= this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.CustomEvents.OnFirstTimeJoined -= this.Handle<Exiled.Events.EventArgs.FirstTimeJoinedEventArgs>((ev) => CustomEvents_OnFirstTimeJoined(ev));
        }


        private void CustomEvents_OnFirstTimeJoined(Exiled.Events.EventArgs.FirstTimeJoinedEventArgs ev)
        {
            Log.Debug("Connected player");

            if (ReservedSlot.Users.Contains(ev.Player.UserId))
                Log.Debug("Connected player with static reserved slot");
            if (Ranks.RanksHandler.ReservedSlots.Contains(ev.Player.UserId))
                Log.Debug("Connected player with dynamic reserved slot");

            //Staff or override
            if (ev.Player.IsStaff())
            {
                Log.Debug("Connected player as staff");
                ConnectedPrioritySlots.Add(ev.Player.UserId);
                return;
            }

            //Not full
            if (Mirror.LiteNetLib4Mirror.LiteNetLib4MirrorCore.Host.ConnectedPeersCount < RealSlots)
                return;
            //Has Reserved Slot
            else if (ReservedSlot.Users.Contains(ev.Player.UserId) || Ranks.RanksHandler.ReservedSlots.Contains(ev.Player.UserId))
            {
                if (Mirror.LiteNetLib4Mirror.LiteNetLib4MirrorCore.Host.ConnectedPeersCount >= RealSlots + ReservedSlots)
                {
                }
                else
                    ConnectedReservedSlots.Add(ev.Player.UserId);
            }
        }

        private void Server_RestartingRound()
        {
            ConnectedPrioritySlots.Clear();
            ConnectedReservedSlots.Clear();
            ReloadData();
        }

        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            ConnectedPrioritySlots.Remove(ev.Player.UserId);
            ConnectedReservedSlots.Remove(ev.Player.UserId);
        }

        public const int ReservedSlots = 10;
        private void Player_PreAuthenticating(Exiled.Events.EventArgs.PreAuthenticatingEventArgs ev)
        {
            Log.Debug("Connecting player");
            if (PluginHandler.Config.WhitelistEnabled)
            {
                if ((!Whitelist?.Contains(ev.UserId) ?? true) && !((CentralAuthPreauthFlags)ev.Flags).HasFlagFast(CentralAuthPreauthFlags.IgnoreWhitelist))
                {
                    string reason = plugin.ReadTranslation("cwhitelist_deny_whitelist_pl");
                    var writer = new NetDataWriter();
                    writer.Put((byte)10);
                    writer.Put(reason);
                    ev.Request.Reject(writer);
                    Exiled.API.Features.Log.Info($"Rejecting {ev.UserId} with reason: {reason}");
                    return;
                }
            }

            if (ReservedSlot.Users.Contains(ev.UserId))
                Log.Debug("Connecting player with static reserved slot");
            if (Ranks.RanksHandler.ReservedSlots.Contains(ev.UserId))
                Log.Debug("Connecting player with dynamic reserved slot");

            //Staff or override
            if (ev.UserId.IsStaff() || ((CentralAuthPreauthFlags)ev.Flags).HasFlagFast(CentralAuthPreauthFlags.ReservedSlot))
            {
                Log.Debug("Connecting player as staff");
                ConnectedPrioritySlots.Add(ev.UserId);
                return;
            }

            //Not full
            if (Mirror.LiteNetLib4Mirror.LiteNetLib4MirrorCore.Host.ConnectedPeersCount < RealSlots)
                return;
            //Has Reserved Slot
            else if (ReservedSlot.Users.Contains(ev.UserId) || Ranks.RanksHandler.ReservedSlots.Contains(ev.UserId))
            {
                if (Mirror.LiteNetLib4Mirror.LiteNetLib4MirrorCore.Host.ConnectedPeersCount >= RealSlots + ReservedSlots)
                {
                    string reason = plugin.ReadTranslation("cwhitelist_deny_serverfull_reserved_pl");
                    reason = reason.Replace("{current_players}", Mirror.LiteNetLib4Mirror.LiteNetLib4MirrorCore.Host.ConnectedPeersCount.ToString()).Replace("{max_players}", CustomNetworkManager.slots.ToString());
                    var writer = new NetDataWriter();
                    writer.Put((byte)10);
                    writer.Put(reason);
                    ev.Request.Reject(writer);
                    ev.Disallow();
                    Exiled.API.Features.Log.Info($"Rejecting {ev.UserId} with reason: {reason}");
                }
                else
                    ConnectedReservedSlots.Add(ev.UserId);
            }
            //No reserved Slot
            else
            {
                string reason = plugin.ReadTranslation("cwhitelist_deny_serverfull_pl");
                reason = reason.Replace("{current_players}", Mirror.LiteNetLib4Mirror.LiteNetLib4MirrorCore.Host.ConnectedPeersCount.ToString()).Replace("{max_players}", CustomNetworkManager.slots.ToString());
                var writer = new NetDataWriter();
                writer.Put((byte)10);
                writer.Put(reason);
                ev.Request.Reject(writer);
                ev.Disallow();
                Exiled.API.Features.Log.Info($"Rejecting {ev.UserId} with reason: {reason}");
            }
        }

        private readonly HashSet<string> ConnectedPrioritySlots = new HashSet<string>();
        private readonly HashSet<string> ConnectedReservedSlots = new HashSet<string>();
        public int RealSlots => 
            CustomNetworkManager.slots + ConnectedPrioritySlots.Count;
            
        internal static void ReloadData()
        {
            if (!PluginHandler.Config.WhitelistEnabled) 
                return;
            Timing.CallDelayed(5, () =>
            {
                SSL.Client.Send(MessageType.WHITELIST_SL_REQUEST, null).GetResponseDataCallback((result) =>
                {
                    if (result.Type != ResponseType.OK)
                        return;
                    var data = (string[])result.Payload.Deserialize(0, 0, out _, false, typeof(string[]));
                    Whitelist = data.ToHashSet() ?? new HashSet<string>();
                });
            });   
        }
    }
}
