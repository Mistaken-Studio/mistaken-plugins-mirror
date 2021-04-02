using Exiled.API.Enums;
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
using Exiled.API.Extensions;
using Gamer.Diagnostics;
using Gamer.API.CustomItem;
using System.Runtime.CompilerServices;

namespace Gamer.Mistaken.Systems.CustomItems
{
    public class CustomItemsHandler : Module
    {
        public CustomItemsHandler(PluginHandler p) : base(p)
        {
            /*if (Server.Port != 7791)
                this.Enabled = false;*/
        }

        public override string Name => "CustomItems";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Shooting += this.Handle<Exiled.Events.EventArgs.ShootingEventArgs>((ev) => Player_Shooting(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.ChangingItem += this.Handle<Exiled.Events.EventArgs.ChangingItemEventArgs>((ev) => Player_ChangingItem(ev));
            Exiled.Events.Handlers.Player.DroppingItem += this.Handle<Exiled.Events.EventArgs.DroppingItemEventArgs>((ev) => Player_DroppingItem(ev));
            Exiled.Events.Handlers.Player.PickingUpItem += this.Handle<Exiled.Events.EventArgs.PickingUpItemEventArgs>((ev) => Player_PickingUpItem(ev));
            Exiled.Events.Handlers.Player.ReloadingWeapon += this.Handle<Exiled.Events.EventArgs.ReloadingWeaponEventArgs>((ev) => Player_ReloadingWeapon(ev));
            Exiled.Events.Handlers.Player.ItemDropped += this.Handle<Exiled.Events.EventArgs.ItemDroppedEventArgs>((ev) => Player_ItemDropped(ev));
            Exiled.Events.Handlers.CustomEvents.OnRequestPickItem += this.Handle<Exiled.Events.EventArgs.PickItemRequestEventArgs>((ev) => CustomEvents_OnRequestPickItem(ev));
            Exiled.Events.Handlers.Player.ThrowingGrenade += this.Handle<Exiled.Events.EventArgs.ThrowingGrenadeEventArgs>((ev) => Player_ThrowingGrenade(ev));
            Exiled.Events.Handlers.Scp914.UpgradingItems += this.Handle<Exiled.Events.EventArgs.UpgradingItemsEventArgs>((ev) => Scp914_UpgradingItems(ev));
            Exiled.Events.Handlers.Player.Handcuffing += this.Handle<Exiled.Events.EventArgs.HandcuffingEventArgs>((ev) => Player_Handcuffing(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Shooting -= this.Handle<Exiled.Events.EventArgs.ShootingEventArgs>((ev) => Player_Shooting(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.ChangingItem -= this.Handle<Exiled.Events.EventArgs.ChangingItemEventArgs>((ev) => Player_ChangingItem(ev));
            Exiled.Events.Handlers.Player.DroppingItem -= this.Handle<Exiled.Events.EventArgs.DroppingItemEventArgs>((ev) => Player_DroppingItem(ev));
            Exiled.Events.Handlers.Player.PickingUpItem -= this.Handle<Exiled.Events.EventArgs.PickingUpItemEventArgs>((ev) => Player_PickingUpItem(ev));
            Exiled.Events.Handlers.Player.ReloadingWeapon -= this.Handle<Exiled.Events.EventArgs.ReloadingWeaponEventArgs>((ev) => Player_ReloadingWeapon(ev));
            Exiled.Events.Handlers.Player.ItemDropped -= this.Handle<Exiled.Events.EventArgs.ItemDroppedEventArgs>((ev) => Player_ItemDropped(ev));
            Exiled.Events.Handlers.CustomEvents.OnRequestPickItem -= this.Handle<Exiled.Events.EventArgs.PickItemRequestEventArgs>((ev) => CustomEvents_OnRequestPickItem(ev));
            Exiled.Events.Handlers.Player.ThrowingGrenade -= this.Handle<Exiled.Events.EventArgs.ThrowingGrenadeEventArgs>((ev) => Player_ThrowingGrenade(ev));
            Exiled.Events.Handlers.Scp914.UpgradingItems -= this.Handle<Exiled.Events.EventArgs.UpgradingItemsEventArgs>((ev) => Scp914_UpgradingItems(ev));
            Exiled.Events.Handlers.Player.Handcuffing -= this.Handle<Exiled.Events.EventArgs.HandcuffingEventArgs>((ev) => Player_Handcuffing(ev));
        }

        private void Player_Handcuffing(Exiled.Events.EventArgs.HandcuffingEventArgs ev)
        {
            foreach (var item in ev.Target.Inventory.items)
            {
                var customItem = GetCustomItem(item);
                if (customItem == null)
                    continue;
                customItem.OnDrop(ev.Target, item);
            } 
        }

        private void Scp914_UpgradingItems(Exiled.Events.EventArgs.UpgradingItemsEventArgs ev)
        {
            foreach (var item in ev.Items.ToArray())
            {
                foreach (var customItem in CustomItem.CustomItemTypes)
                {
                    foreach (var upgrade in customItem.Upgrades)
                    {
                        if(upgrade.KnobSetting == ev.KnobSetting)
                        {
                            if (item.ItemId == upgrade.Input && (upgrade.Durability == null || item.durability == upgrade.Durability))
                            {
                                if (upgrade.Chance == 100 || upgrade.Chance >= UnityEngine.Random.Range(0, 100))
                                {
                                    customItem.Spawn(ev.Scp914.output.position);
                                    ev.Items.Remove(item);
                                    item.Delete();
                                    goto foreach_end;
                                }
                            }
                        }
                    }
                }
                var thisCustomItem = GetCustomItem(item);
                if (thisCustomItem == null)
                    continue;
                var result = thisCustomItem.OnUpgrade(item, ev.KnobSetting);
                result?.ItemId.Spawn(result.durability, ev.Scp914.output.position);
                ev.Items.Remove(item);
                item.Delete();
                foreach_end:;
            }
        }

        private void Player_ThrowingGrenade(Exiled.Events.EventArgs.ThrowingGrenadeEventArgs ev)
        {
            var customItem = GetCustomItem(ev.Player.CurrentItem);
            if (customItem == null)
                return;
            ev.IsAllowed = customItem.OnThrow(ev.Player, ev.Player.CurrentItem, ev.IsSlow);
        }

        private void CustomEvents_OnRequestPickItem(Exiled.Events.EventArgs.PickItemRequestEventArgs ev)
        {
            var customItem = GetCustomItem(ev.Pickup);
            if (customItem == null)
                return;
            ev.Player.ShowHintPulsating($"Podnosisz <color=yellow>{customItem.ItemName}</color>", 3, false, false);
            customItem.OnPrePickup(ev.Player, ev.Pickup);
        }

        private void Player_ItemDropped(Exiled.Events.EventArgs.ItemDroppedEventArgs ev)
        {
            var customItem = GetCustomItem(ev.Pickup);
            if (customItem == null)
                return;
            if (customItem.Size == Vector3.one)
                return;
            MEC.Timing.CallDelayed(0.1f, () =>
            {
                MapPlus.Spawn(new Inventory.SyncItemInfo
                {
                    id = ev.Pickup.itemId,
                    durability = ev.Pickup.durability,
                    modBarrel = ev.Pickup.weaponMods.Barrel,
                    modSight = ev.Pickup.weaponMods.Sight,
                    modOther = ev.Pickup.weaponMods.Other
                }, ev.Pickup.position, ev.Pickup.rotation, customItem.Size);
                ev.Pickup.Delete();
            });
        }

        private void Player_ReloadingWeapon(Exiled.Events.EventArgs.ReloadingWeaponEventArgs ev)
        {
            var customItem = GetCustomItem(ev.Player.CurrentItem);
            if (customItem == null)
                return;
            ev.IsAllowed = customItem.OnReload(ev.Player, ev.Player.CurrentItem);
        }

        private void Player_PickingUpItem(Exiled.Events.EventArgs.PickingUpItemEventArgs ev)
        {
            var customItem = GetCustomItem(ev.Pickup);
            if (customItem == null)
                return;
            ev.IsAllowed = customItem.OnPickup(ev.Player, ev.Pickup);
        }

        private void Player_DroppingItem(Exiled.Events.EventArgs.DroppingItemEventArgs ev)
        {
            var customItem = GetCustomItem(ev.Item);
            if (customItem == null)
                return;
            ev.IsAllowed = customItem.OnDrop(ev.Player, ev.Item);
        }

        private void Player_ChangingItem(Exiled.Events.EventArgs.ChangingItemEventArgs ev)
        {
            CustomItem customItem;
            if (ev.OldItem != null)
            {
                customItem = GetCustomItem(ev.OldItem);
                if (customItem != null)
                    customItem.OnStopHolding(ev.Player, ev.OldItem);
            }
            if (ev.NewItem != null)
            {
                customItem = GetCustomItem(ev.NewItem);
                if (customItem != null)
                    customItem.OnStartHolding(ev.Player, ev.NewItem);
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (!HasCustomItem(ev.Player))
                return;
            foreach (var item in ev.Player.Inventory.items)
            {
                var customItem = GetCustomItem(item);
                if (customItem == null)
                    continue;
                customItem.OnForceclass(ev.Player);
            }
        }

        private void Player_Shooting(Exiled.Events.EventArgs.ShootingEventArgs ev)
        {
            var customItem = GetCustomItem(ev.Shooter.CurrentItem);
            if (customItem == null)
                return;
            ev.IsAllowed = customItem.OnShoot(ev.Shooter, ev.Shooter.CurrentItem, ev.Target);
        }

        private void Server_RestartingRound()
        {
            foreach (var item in CustomItem.CustomItemTypes)
                item.OnRestart();
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(DoRoundLoop());
        }

        private IEnumerator<float> DoRoundLoop()
        {
            yield return Timing.WaitForSeconds(1);
            while (Round.IsStarted)
            {
                foreach (var item in Pickup.Instances.ToArray())
                {
                    var customItem = GetCustomItem(item);
                    if (customItem == null)
                        continue;
                    if (customItem.Size == Vector3.one || Vector3.Distance(customItem.Size, item.transform.localScale) < 0.1f)
                        continue;
                    MapPlus.Spawn(new Inventory.SyncItemInfo
                    {
                        durability = item.durability,
                        id = customItem.Item,
                        modBarrel = item.weaponMods.Barrel,
                        modSight = item.weaponMods.Sight,
                        modOther = item.weaponMods.Other
                    }, item.position, item.rotation, customItem.Size);
                    item.Delete();
                }
                yield return Timing.WaitForSeconds(5);
            }
        }

        public static CustomItem GetCustomItem(Pickup item)
        {
            if (!CustomItem.CustomItemsFastCheckCache.Contains(item.ItemId))
                return null;
            foreach (var customItem in CustomItem.CustomItemTypes)
            {
                var dur = (item.durability - 1) * 1000;
                if (customItem.Item == item.ItemId && customItem.Durability == dur - (dur % 1))
                    return customItem;
            }
            return null;
        }
        public static bool HasCustomItem(Player player)
        {
            if (!player.Inventory.items.Any(item => CustomItem.CustomItemsFastCheckCache.Contains(item.id)))
                return false;
            foreach (var customItem in CustomItem.CustomItemTypes)
            {
                foreach (var item in player.Inventory.items)
                {
                    if (customItem.Item == item.id && customItem.Durability == Math.Floor((item.durability - 1) * 1000))
                        return true;
                }
            }
            return false;
        }
        public static CustomItem GetCustomItem(Inventory.SyncItemInfo item)
        {
            if (!CustomItem.CustomItemsFastCheckCache.Contains(item.id))
                return null;
            foreach (var customItem in CustomItem.CustomItemTypes)
            {
                var dur = (item.durability - 1) * 1000;
                if (customItem.Item == item.id && customItem.Durability == dur - (dur % 1))
                    return customItem;
            }
            return null;
        }
    }
}
