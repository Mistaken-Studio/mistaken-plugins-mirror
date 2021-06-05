﻿using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.Staff;
using Gamer.Utilities;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Xname.Radio
{
    internal class HelmetsHandler : Module
    {
        /// <inheritdoc/>
        public HelmetsHandler(IPlugin<IConfig> plugin) : base(plugin)
        {
        }
        /// <inheritdoc/>
        public override string Name => "HelmetsHandler";
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangedRole += this.Handle<Exiled.Events.EventArgs.ChangedRoleEventArgs>((ev) => Player_ChangedRole(ev));
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.ChangedRole -= this.Handle<Exiled.Events.EventArgs.ChangedRoleEventArgs>((ev) => Player_ChangedRole(ev));
        }
        private void Player_ChangedRole(Exiled.Events.EventArgs.ChangedRoleEventArgs ev)
        {
            var inv = Server.Host.Inventory;
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(inv.pickupPrefab, ev.Player.CameraTransform);
            gameObject.transform.rotation = Quaternion.identity;
            //gameObject.transform.localPosition = new Vector3(0, 0.2f, 0);
            NetworkServer.Spawn(gameObject);
            gameObject.GetComponent<Pickup>().SetupPickup(ItemType.Flashlight, 0, inv.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), Vector3.zero, Quaternion.identity);
            gameObject.transform.localPosition = ev.Player.CameraTransform.position;
            this.CallDelayed(1f, () => ev.Player.SendConsoleMessage($"{}", "green"), "helmetdelay");
        }
    }
}   