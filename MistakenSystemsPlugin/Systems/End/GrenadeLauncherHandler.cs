using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
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
using Gamer.Diagnostics;
using Gamer.API.CustomItem;

namespace Gamer.Mistaken.Systems.End
{
    internal class GrenadeLauncherHandler : Module
    {
        private new static __Log Log;
        public GrenadeLauncherHandler(PluginHandler p) : base(p)
        {
            Log = base.Log;
            new GrenadeLauncher();
        }

        public override string Name => "GrenadeLauncher";
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
            
        }

        private class GrenadeLauncher : CustomItem
        {
            public GrenadeLauncher() => base.Register();
            public override string ItemName => "Grenade Launcher";
            public override ItemType Item => ItemType.GunUSP;
            public override int Durability => 589;
            public override Vector3 Size => new Vector3(2f, 1.5f, 1.5f);
            public readonly int MagSize = 3;

            public override bool OnReload(Player player, Inventory.SyncItemInfo item)
            {
                var dur = this.GetInternalDurability(item);
                if (dur != 0)
                {
                    Mistaken.Base.GUI.PseudoGUIHandler.Set(player, "grenadeLauncherWarn", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, "Nie możesz przeładować nie mając pustego magazynka", 3);
                    return false;
                }

                if(!player.Inventory.items.Any(i => i.id == ItemType.GrenadeFrag))
                {
                    Mistaken.Base.GUI.PseudoGUIHandler.Set(player, "grenadeLauncherWarn", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, "Nie masz amunicji(Granat Odłamkowy)", 3);
                    return false;
                }

                this.SetInternalDurability(player, item, MagSize + 1);
                player.RemoveItem(player.Inventory.items.First(i => i.id == ItemType.GrenadeFrag));
                Mistaken.Base.GUI.PseudoGUIHandler.Set(player, "grenadeLauncherWarn", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, "Przeładowano", 3);
                return false;
            }
            public override bool OnShoot(Player player, Inventory.SyncItemInfo item, GameObject _, Vector3 position)
            {
                int dur = (int)Math.Floor(this.GetInternalDurability(item));
                Log.Debug($"Ammo: {dur} | {item.durability}");
                if (dur == 0)
                {
                    Mistaken.Base.GUI.PseudoGUIHandler.Set(player, "grenadeLauncherWarn", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, "Nie możesz strzelać z pustym magazynkiem", 3);
                    player.ReferenceHub.weaponManager.RpcEmptyClip();
                    return false;
                }
                GrenadeSettings settings = player.GrenadeManager.availableGrenades[0];
                Grenade component = UnityEngine.Object.Instantiate(settings.grenadeInstance).GetComponent<Grenade>();
                component.InitData(player.GrenadeManager, player.ReferenceHub.playerMovementSync.PlayerVelocity, player.CameraTransform.forward, 2.5f);
                NetworkServer.Spawn(component.gameObject);
                this.SetInternalDurability(player, item, dur - 1);
                return false;
            }
            public override void OnStartHolding(Player player, Inventory.SyncItemInfo item)
            {
                Log.Debug(player.Ammo[(int)AmmoType.Nato9]);
                player.Ammo[(int)AmmoType.Nato9] += 1;
                Log.Debug(player.Ammo[(int)AmmoType.Nato9]);
                base.OnStartHolding(player, item);

                Mistaken.Base.GUI.PseudoGUIHandler.Set(player, "grenadeLauncher", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Trzymasz <color=yellow>{this.ItemName}</color>");
            }
            public override void OnStopHolding(Player player, Inventory.SyncItemInfo item)
            {
                Log.Debug(player.Ammo[(int)AmmoType.Nato9]);
                player.Ammo[(int)AmmoType.Nato9]--;
                Log.Debug(player.Ammo[(int)AmmoType.Nato9]);
                Mistaken.Base.GUI.PseudoGUIHandler.Set(player, "grenadeLauncher", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
                base.OnStopHolding(player, item);
            }
        }
    }
}
