﻿using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Mistaken.Base.Staff;
using PlayableScps;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.Mistaken.BetterSCP.Global
{
    /// <inheritdoc/>
    public class GlobalHandler : Module
    {
        /// <inheritdoc/>
        public GlobalHandler(PluginHandler p) : base(p)
        {
        }
        /// <inheritdoc/>
        public override string Name => nameof(GlobalHandler);
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Server.SendingConsoleCommand += this.Handle<Exiled.Events.EventArgs.SendingConsoleCommandEventArgs>((ev) => Server_SendingConsoleCommand(ev));
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Server.SendingConsoleCommand -= this.Handle<Exiled.Events.EventArgs.SendingConsoleCommandEventArgs>((ev) => Server_SendingConsoleCommand(ev));
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }
        #region Panic
        private static readonly Dictionary<string, DateTime> LastSeeTime = new Dictionary<string, DateTime>();
        private static readonly Func<Player, Action<Player>> OnEnterVision = (player) => (scp) =>
        {
            if (!scp.IsScp || scp.Role == RoleType.Scp079)
                return;
            if (!player.IsHuman)
                return;
            if (player.GetEffectActive<CustomPlayerEffects.Flashed>())
                return;
            var scpPosition = scp.Position;
            if (Vector3.Dot((player.Position - scpPosition).normalized, scp.ReferenceHub.PlayerCameraReference.forward) >= 0.1f)
            {
                VisionInformation visionInformation = VisionInformation.GetVisionInformation(player.ReferenceHub, scpPosition, -0.1f, 30f, true, true, scp.ReferenceHub.localCurrentRoomEffects);
                if (visionInformation.IsLooking)
                {
                    if (LastSeeTime.TryGetValue(player.UserId, out DateTime lastSeeTime) && (DateTime.Now - lastSeeTime).TotalSeconds < 60)
                    {
                        LastSeeTime[player.UserId] = DateTime.Now;
                        return;
                    }
                    player.EnableEffect<CustomPlayerEffects.Invigorated>(5, true);
                    if (!player.GetEffectActive<CustomPlayerEffects.Panic>())
                        player.EnableEffect<CustomPlayerEffects.Panic>(15, true);
                    player.SetGUI("panic", Base.GUI.PseudoGUIHandler.Position.MIDDLE, "Zaczynasz <color=yellow>panikować</color>", 3);
                    LastSeeTime[player.UserId] = DateTime.Now;
                }
            }
        };
        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            //Panic
            Mistaken.Systems.Components.InRange.Spawn(ev.Player.CameraTransform, Vector3.forward * 10f, new Vector3(10, 5, 20), OnEnterVision(ev.Player));
            //AntyDuo
            Mistaken.Systems.Components.InRange.Spawn(ev.Player.CameraTransform, Vector3.zero, new Vector3(PluginHandler.Anty173_096DuoDistance, 5, PluginHandler.Anty173_096DuoDistance), OnEnter(ev.Player), OnExit(ev.Player));
        }
        #endregion
        #region AntyDuo
        internal static bool DmgMultiplayer = false;
        private static readonly Func<Player, Action<Player>> OnEnter = (player1) => (player2) =>
        {
            if (!(player1.Role == RoleType.Scp096 || player1.Role == RoleType.Scp173))
                return;
            if (!(player2.Role == RoleType.Scp096 || player2.Role == RoleType.Scp173))
                return;
            if (player1.Role == player2.Role)
                return;
            DmgMultiplayer = true;
            player1.EnableEffect<CustomPlayerEffects.Concussed>();
            player1.SetGUI("antyDuo", Base.GUI.PseudoGUIHandler.Position.MIDDLE, $"Jesteś za blisko <color=yellow>{(player2.Role == RoleType.Scp173 ? "SCP 173" : "SCP 096")}</color>, z tego powodu będziesz dostawał <color=yellow>150</color>% obrażeń");
            player2.EnableEffect<CustomPlayerEffects.Concussed>();
            player2.SetGUI("antyDuo", Base.GUI.PseudoGUIHandler.Position.MIDDLE, $"Jesteś za blisko <color=yellow>{(player1.Role == RoleType.Scp173 ? "SCP 173" : "SCP 096")}</color>, z tego powodu będziesz dostawał <color=yellow>150</color>% obrażeń");
        };
        private static readonly Func<Player, Action<Player>> OnExit = (player1) => (player2) =>
        {
            if (!(player1.Role == RoleType.Scp096 || player1.Role == RoleType.Scp173))
                return;
            if (!(player2.Role == RoleType.Scp096 || player2.Role == RoleType.Scp173))
                return;
            if (player1.Role == player2.Role)
                return;
            DmgMultiplayer = false;
            player1.DisableEffect<CustomPlayerEffects.Concussed>();
            player2.DisableEffect<CustomPlayerEffects.Concussed>();
            player1.SetGUI("antyDuo", Base.GUI.PseudoGUIHandler.Position.MIDDLE, null);
            player2.SetGUI("antyDuo", Base.GUI.PseudoGUIHandler.Position.MIDDLE, null);

        };

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            //AntyDuo
            if (DmgMultiplayer)
            {
                if (ev.Target.Role == RoleType.Scp173 || ev.Target.Role == RoleType.Scp096)
                    ev.Amount *= 1.5f;
            }
            //Block Cola Damage
            if (ev.DamageType == DamageTypes.Scp207 && ev.Target.Side == Side.Scp)
                ev.IsAllowed = false;
        }
        #endregion
        private void Server_SendingConsoleCommand(Exiled.Events.EventArgs.SendingConsoleCommandEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (!ev.Player.IsActiveDev())
                return;
            if (ev.Name == "npc__test")
            {
                SCP0492.SCP0492Handler.Spawn(ev.Player.CurrentRoom);
                ev.IsAllowed = true;
                ev.ReturnMessage = "Done";
            }
            else if (ev.Name == "dur")
            {
                ev.Player.Inventory.items.ModifyDuration(ev.Player.CurrentItemIndex, float.Parse(ev.Arguments[0]));
                ev.IsAllowed = true;
                ev.ReturnMessage = "Done";
            }
            else if (ev.Name == "int")
            {
                ev.Player.CurrentRoom.SetLightIntensity(float.Parse(ev.Arguments[0]));
                ev.IsAllowed = true;
                ev.ReturnMessage = "Done";
            }
            else if (ev.Name == "a1")
            {
                ev.IsAllowed = true;
                ev.ReturnMessage = "Done";
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab, ev.Player.CameraTransform);
                gameObject.transform.localPosition = Vector3.forward * 10f;
                gameObject.transform.localScale = new Vector3(10, 5, 20);
                gameObject.transform.rotation = Quaternion.identity;
                gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                Mirror.NetworkServer.Spawn(gameObject);
                var pickup = gameObject.GetComponent<Pickup>();
                pickup.SetupPickup(ItemType.WeaponManagerTablet, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
            }
        }
    }
}
