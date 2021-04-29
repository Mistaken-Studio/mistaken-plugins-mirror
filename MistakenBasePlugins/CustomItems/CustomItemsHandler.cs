using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.API.CustomItem;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Utilities;
using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.Mistaken.Base.CustomItems
{
    /// <inheritdoc/>
    public class CustomItemsHandler : Module
    {
        /// <inheritdoc/>
        public CustomItemsHandler(PluginHandler p) : base(p)
        {
            /*if (Server.Port != 7791)
                this.Enabled = false;*/
        }
        /// <inheritdoc/>
        public override string Name => "CustomItems";
        /// <inheritdoc/>
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
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }
        /// <inheritdoc/>
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
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (!ev.Target.IsHuman)
                return;
            foreach (var customItem in CustomItem.CustomItemTypes)
                customItem.OnForceclass(ev.Target);
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
                        if (upgrade.KnobSetting == ev.KnobSetting)
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
                if (result != null)
                {
                    var custimItem = GetCustomItem(result);
                    if (custimItem != null)
                        custimItem.Spawn(ev.Scp914.output.position, custimItem.GetInternalDurability(result.durability));
                    else
                        result?.ItemId.Spawn(result.durability, ev.Scp914.output.position);
                }      
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
            if (ev.IsAllowed)
                customItem.OnStopHolding(ev.Player, ev.Player.CurrentItem);
        }

        private void CustomEvents_OnRequestPickItem(Exiled.Events.EventArgs.PickItemRequestEventArgs ev)
        {
            var customItem = GetCustomItem(ev.Pickup);
            if (customItem == null)
                return;
            ev.Player.SetGUI("citem_pickup", GUI.PseudoGUIHandler.Position.BOTTOM, $"Podnosisz <color=yellow>{customItem.ItemName}</color>", 3);
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
                if (ev.Pickup == null || ev.Pickup.gameObject == null)
                    return;
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
            if (ev.IsAllowed)
            {
                if (ev.Player.CurrentItem == ev.Item)
                    customItem.OnStopHolding(ev.Player, ev.Item);
            }
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
            if (!ev.Player.IsHuman)
                return;
            foreach (var customItem in CustomItem.CustomItemTypes)
                customItem.OnForceclass(ev.Player);
        }

        private void Player_Shooting(Exiled.Events.EventArgs.ShootingEventArgs ev)
        {
            var customItem = GetCustomItem(ev.Shooter.CurrentItem);
            if (customItem == null)
                return;
            ev.IsAllowed = customItem.OnShoot(ev.Shooter, ev.Shooter.CurrentItem, ev.Target, ev.Position);
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
        /// <summary>
        /// Returns custom item
        /// </summary>
        /// <param name="item">Pickup</param>
        /// <returns>CustomItem</returns>
        public static CustomItem GetCustomItem(Pickup item)
        {
            if (item == null)
                return null;
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
        /// <summary>
        /// If player has custom item
        /// </summary>
        /// <param name="player">Player</param>
        /// <returns>If has custom item</returns>
        public static bool HasCustomItem(Player player)
        {
            foreach (var item in player.Inventory.items)
            {
                if (!CustomItem.CustomItemsFastCheckCache.Contains(item.id))
                    continue;
                foreach (var customItem in CustomItem.CustomItemTypes)
                {
                    if (customItem.Item == item.id && customItem.Durability == Math.Floor((item.durability - 1) * 1000))
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns custom item
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>CustomItem</returns>
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
