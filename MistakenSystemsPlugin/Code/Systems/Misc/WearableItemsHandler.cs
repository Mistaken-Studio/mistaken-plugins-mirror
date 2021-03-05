using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Gamer.Diagnostics;
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
    internal class WearableItemsHandler : Module
    {
        public WearableItemsHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "WerableItems";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.PickingUpItem += this.Handle<Exiled.Events.EventArgs.PickingUpItemEventArgs>((ev) => Player_PickingUpItem(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.DroppingItem += this.Handle<Exiled.Events.EventArgs.DroppingItemEventArgs>((ev) => Player_DroppingItem(ev));
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.CustomEvents.OnRequestPickItem += this.Handle<Exiled.Events.EventArgs.PickItemRequestEventArgs>((ev) => PickingItemPatch_OnRequestPickItem(ev));
            Exiled.Events.Handlers.Player.Handcuffing += this.Handle<Exiled.Events.EventArgs.HandcuffingEventArgs>((ev) => Player_Handcuffing(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.PickingUpItem -= this.Handle<Exiled.Events.EventArgs.PickingUpItemEventArgs>((ev) => Player_PickingUpItem(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.DroppingItem -= this.Handle<Exiled.Events.EventArgs.DroppingItemEventArgs>((ev) => Player_DroppingItem(ev));
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.CustomEvents.OnRequestPickItem -= this.Handle<Exiled.Events.EventArgs.PickItemRequestEventArgs>((ev) => PickingItemPatch_OnRequestPickItem(ev));
            Exiled.Events.Handlers.Player.Handcuffing -= this.Handle<Exiled.Events.EventArgs.HandcuffingEventArgs>((ev) => Player_Handcuffing(ev));
        }

        private void Player_Handcuffing(HandcuffingEventArgs ev)
        {
            foreach (var item in WearableItems.Where(p => ev.Target.Inventory.items.Any(i => i.id == p.Type)))
                item.OnUnWear.Invoke(ev.Target, true);
        }

        private void PickingItemPatch_OnRequestPickItem(PickItemRequestEventArgs ev)
        {
            if (!Working.ContainsKey(ev.Player.Id))
                Working[ev.Player.Id] = false;
            if (Working[ev.Player.Id])
            {
                ev.IsAllowed = false;
                return;
            }
            foreach (var item in WearableItems)
            {
                if (ev.Pickup.ItemId == item.Type)
                {
                    if (ev.Player.Inventory.items.Any(i => i.id == item.Type))
                    {
                        ev.IsAllowed = false;
                        string name = item.Name;
                        ev.Player.ShowHintPulsating($"<b>Already</b> reached the limit of <color=yellow>{name}s</color> (<color=yellow>{1} {name}</color>)", 2f, true, true);
                        return;
                    }
                }
            }
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(SpawnItems());
            Timing.RunCoroutine(DoSizeLoop());
        }

        private IEnumerator<float> SpawnItems()
        {
            var door = Map.Doors.First(d => d.Type() ==  DoorType.LczArmory);
            var pos = door.transform.position + Vector3.up * 2 + door.transform.forward * 7 + door.transform.right * -4.5f;
            Inventory inv = Pickup.Inv;
            for (int i = 0; i < 5; i++)
            {
                MapPlus.Spawn(new Inventory.SyncItemInfo
                {
                    durability = 25,
                    id = ItemType.Coin
                }, pos, Quaternion.Euler(90, 0, 0), new Vector3(3, 3, 5));
                yield return Timing.WaitForSeconds(0.1f);
            }
            var door2 = Map.Doors.First(d => d.Type() == DoorType.HczArmory);
            var pos2 = door2.transform.position + Vector3.up * 2 + -door2.transform.right * 2 + -door2.transform.forward;
            for (int j = 0; j < 10; j++)
            {
                MapPlus.Spawn(new Inventory.SyncItemInfo
                {
                    durability = 25,
                    id = ItemType.Coin
                }, pos2, Quaternion.Euler(90, 0, 0), new Vector3(3, 3, 5));
                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        private static IEnumerator<float> DoSizeLoop()
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId; while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                foreach (var pickup in Pickup.Instances.ToArray())
                {
                    if (pickup.ItemId == ItemType.Coin)
                    {
                        if (pickup.transform.localScale == Vector3.one)
                        {
                            MapPlus.Spawn(new Inventory.SyncItemInfo
                            {
                                durability = pickup.durability,
                                id = ItemType.Coin,
                            }, pickup.position, pickup.rotation, new Vector3(3, 3, 5));
                            pickup.Delete();
                        }
                    }
                }
                yield return Timing.WaitForSeconds(1);
            }
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            Working[ev.Player.Id] = false;
        }

        private void Player_DroppingItem(Exiled.Events.EventArgs.DroppingItemEventArgs ev)
        {
            if (!Working.ContainsKey(ev.Player.Id))
                Working[ev.Player.Id] = false;
            if (Working[ev.Player.Id])
            {
                ev.IsAllowed = false;
                return;
            }

            var data = WearableItems.FirstOrDefault(i => i.Type == ev.Item.id);
            if (data == null)
                return;

            data.OnUnWear.Invoke(ev.Player, false);
        }

        public static readonly Dictionary<int, bool> Working = new Dictionary<int, bool>();

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            switch(ev.NewRole)
            {
                case RoleType.ChaosInsurgency:
                case RoleType.NtfCommander:
                    {
                        ev.Items.Add(ItemType.Coin);
                        break;
                    }
            }
            foreach (var element in WearableItems)
                element.OnUnWear.Invoke(ev.Player, true);
            Timing.CallDelayed(1, () =>
            {
                try
                {
                    ev.Player.Inventory.items.ModifyDuration(ev.Player.Inventory.items.FindIndex(i => i.id == ItemType.Coin), 25);
                }
                catch { }
                foreach (var element in WearableItems)
                {
                    if (!element.WearedBy.Contains(ev.Player.Id) && ev.Player.Inventory.items.Any(i => i.id == element.Type))
                        element.OnWear.Invoke(ev.Player, ev.Player.Inventory.items.First(i => i.id == element.Type).durability, true);
                }
            });
        }

        private void Server_RestartingRound()
        {
            Working.Clear();
            foreach (var item in WearableItems)
                item.WearedBy.Clear();
        }

        private void Player_PickingUpItem(PickingUpItemEventArgs ev)
        {
            if (!Working.ContainsKey(ev.Player.Id))
                Working[ev.Player.Id] = false;
            if (Working[ev.Player.Id])
            {
                ev.IsAllowed = false;
                return;
            }
            if (ev.Pickup.ItemId == ItemType.Coin && ev.Pickup.durability == 0) 
                ev.Pickup.durability = 25;
            foreach (var item in WearableItems)
            {
                if (ev.Pickup.ItemId == item.Type)
                {
                    if (ev.Player.Inventory.items.Any(i => i.id == item.Type))
                    {
                        ev.IsAllowed = false;
                        string name = item.Name;
                        ev.Player.ShowHintPulsating($"<b>Already</b> reached the limit of <color=yellow>{name}s</color> (<color=yellow>{1} {name}</color>)", 2f, true, true);
                        Log.Error("If patch is working, this shouldn't be displayed");
                        return;
                    }
                    item.OnWear.Invoke(ev.Player, ev.Pickup.durability, false);
                }
            }
        }

        public class WearableItem
        {
            public string Name;
            public ItemType Type;
            public Action<Player, float, bool> OnWear;
            public Action<Player, bool> OnUnWear;
            public readonly List<int> WearedBy = new List<int>(); 
        }

        public static readonly WearableItem[] WearableItems = new WearableItem[]
        {
            new WearableItem
            {
                Name = "Armor",
                Type = ItemType.Coin,
                OnWear = (player, durablility, fast) =>
                {
                    if(!player.IsConnected)
                        return;
                    if(!fast)
                    {
                        if(Working[player.Id])
                            return;
                        Working[player.Id] = true;
                        player.ShowHintPulsating($"Putting on <color=yellow>Armor</color>", 2f, true, true);
                        player.EnableEffect<CustomPlayerEffects.Ensnared>(5);
                    }
                    Timing.CallDelayed(2 * (fast ? 0 : 1), () => {
                        player.EnableEffect<CustomPlayerEffects.Panic>();
                        Shield.ShieldedManager.Add(new Shield.Shielded(player, (int)Math.Floor(durablility), durablility / 60, 30, 0, 0.85f));
                        if(player.ArtificialHealth < 10 && !player.ReferenceHub.playerEffectsController.GetEffect<CustomPlayerEffects.Scp207>().Enabled)
                            player.ArtificialHealth = 10;
                        WearableItems.FirstOrDefault(i => i.Type == ItemType.Coin)?.WearedBy.Add(player.Id);
                        Working[player.Id] = false;
                    });
                },
                OnUnWear = (player, fast) =>
                {
                    if(!fast && player.IsConnected)
                    {
                        if(Working[player.Id])
                            return;
                        Working[player.Id] = true;
                        player.ShowHintPulsating($"Droping <color=yellow>Armor</color>", 2f, true, true);
                        player.EnableEffect<CustomPlayerEffects.Ensnared>(3);
                    }
                    MEC.Timing.CallDelayed(1 * (fast ? 0 : 1), () => {
                        if(player.IsConnected)
                            player.DisableEffect<CustomPlayerEffects.Panic>();
                        Shield.ShieldedManager.Remove(player);
                        WearableItems.FirstOrDefault(i => i.Type == ItemType.Coin)?.WearedBy.Remove(player.Id);
                        Working[player.Id] = false;
                    });
                }
            }
        };
    }
}
