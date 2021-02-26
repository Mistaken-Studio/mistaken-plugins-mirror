using Exiled.API.Enums;
using Exiled.API.Features;
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
    internal class ReusableItemsHandler : Module
    {
        public ReusableItemsHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "ReusableItems";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.StoppingMedicalItem += this.Handle<Exiled.Events.EventArgs.StoppingMedicalItemEventArgs>((ev) => Player_StoppingMedicalItem(ev));
            Exiled.Events.Handlers.Player.UsingMedicalItem += this.Handle<Exiled.Events.EventArgs.UsingMedicalItemEventArgs>((ev) => Player_UsingMedicalItem(ev));
            Exiled.Events.Handlers.Player.ChangingItem += this.Handle<Exiled.Events.EventArgs.ChangingItemEventArgs>((ev) => Player_ChangingItem(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.CustomEvents.OnRequestPickItem += this.Handle<Exiled.Events.EventArgs.PickItemRequestEventArgs>((ev) => CustomEvents_OnRequestPickItem(ev));
            Exiled.Events.Handlers.Player.PickingUpItem += this.Handle<Exiled.Events.EventArgs.PickingUpItemEventArgs>((ev) => Player_PickingUpItem(ev));
            Exiled.Events.Handlers.Player.MedicalItemDequipped += this.Handle<Exiled.Events.EventArgs.DequippedMedicalItemEventArgs>((ev) => Player_MedicalItemDequipped(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.StoppingMedicalItem -= this.Handle<Exiled.Events.EventArgs.StoppingMedicalItemEventArgs>((ev) => Player_StoppingMedicalItem(ev));
            Exiled.Events.Handlers.Player.UsingMedicalItem -= this.Handle<Exiled.Events.EventArgs.UsingMedicalItemEventArgs>((ev) => Player_UsingMedicalItem(ev));
            Exiled.Events.Handlers.Player.ChangingItem -= this.Handle<Exiled.Events.EventArgs.ChangingItemEventArgs>((ev) => Player_ChangingItem(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.CustomEvents.OnRequestPickItem -= this.Handle<Exiled.Events.EventArgs.PickItemRequestEventArgs>((ev) => CustomEvents_OnRequestPickItem(ev));
            Exiled.Events.Handlers.Player.PickingUpItem -= this.Handle<Exiled.Events.EventArgs.PickingUpItemEventArgs>((ev) => Player_PickingUpItem(ev));
            Exiled.Events.Handlers.Player.MedicalItemDequipped -= this.Handle<Exiled.Events.EventArgs.DequippedMedicalItemEventArgs>((ev) => Player_MedicalItemDequipped(ev));
        }

        private void Player_PickingUpItem(Exiled.Events.EventArgs.PickingUpItemEventArgs ev)
        {
            foreach (var item in ReusableItems)
            {
                if (ev.Pickup.ItemId == item.Type)
                {
                    if (ev.Player.Inventory.items.Where(i => i.id == item.Type).Count() >= item.MaxItems)
                    {
                        float tmp = ev.Pickup.durability;
                        foreach (var invItem in ev.Player.Inventory.items.Where(i => i.id == item.Type))
                        {
                            int diff = item.Uses - (int)invItem.durability;
                            if (diff > 0 && tmp >= diff)
                            {
                                ev.Player.Inventory.items.ModifyDuration(ev.Player.Inventory.items.IndexOf(invItem), item.Uses);
                                tmp -= diff;
                            }
                        }
                        if (tmp == 0)
                            ev.Pickup.Delete();
                        else
                        {
                            if (ev.Pickup.durability != tmp)
                                ev.Pickup.Networkdurability = tmp;
                            ev.Pickup.InUse = false;
                        }
                        ev.IsAllowed = false;
                    }
                    break;
                }
            }
        }

        private void CustomEvents_OnRequestPickItem(Exiled.Events.EventArgs.PickItemRequestEventArgs ev)
        {
            foreach (var item in ReusableItems)
            {
                if (ev.Pickup.ItemId == item.Type)
                {
                    if (ev.Pickup.durability == 0)
                        ev.Pickup.Networkdurability = item.Uses;
                    if (ev.Player.Inventory.items.Where(i => i.id == item.Type).Count() >= item.MaxItems)
                    {
                        ev.IsAllowed = false;
                        string name = item.Type.ToString();
                        if (!name.EndsWith("s"))
                            name += "s";
                        ev.Player.ShowHintPulsating($"<b>Already</b> reached the limit of <color=yellow>{name}</color> (<color=yellow>{item.MaxItems} {name}</color>)", 2f);
                    }
                    else
                        ev.Player.ShowHint($"<color=yellow>{ev.Pickup.durability}</color>/<color=yellow>{item.Uses}</color> uses left", false);
                    break;
                }
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            Timing.CallDelayed(1, () => {
                foreach (var element in ReusableItems)
                {
                    foreach (var item in ev.Player.Inventory.availableItems)
                    {
                        if (item.id == element.Type)
                            item.durability = element.Uses;
                    }
                }
            });
        }

        private void Server_RoundStarted()
        {
            UsingMedical.Clear();
        }

        private void Player_ChangingItem(Exiled.Events.EventArgs.ChangingItemEventArgs ev)
        {
            foreach (var item in ReusableItems)
            {
                if (ev.NewItem.id == item.Type && ev.OldItem.id != item.Type)
                {
                    if (ev.NewItem.durability < 1 || ev.NewItem.durability > item.Uses)
                        ev.Player.Inventory.items.ModifyDuration(ev.Player.Inventory.items.FindIndex(i => i.uniq == ev.NewItem.uniq), item.Uses);
                    Timing.RunCoroutine(InformUsesLeft(ev.Player));
                }
            }

        }

        public class ReusableItem
        {
            public ItemType Type;
            public int Uses;
            public int MaxItems;
        }

        public static readonly ReusableItem[] ReusableItems = new ReusableItem[]
        {
            new ReusableItem
            {
                Type = ItemType.Medkit,
                Uses = 2,
                MaxItems = 2,
            },
            new ReusableItem
            {
                Type = ItemType.Painkillers,
                Uses = 5,
                MaxItems = 1,
            },
            new ReusableItem
            {
                Type = ItemType.SCP500,
                Uses = 2,
                MaxItems = 1,
            }
        };

        IEnumerator<float> InformUsesLeft(Player p)
        {
            yield return Timing.WaitForSeconds(.25f);
            while (ReusableItems.Any(i => i.Type == p.CurrentItem.id))
            {
                if(!p.ReferenceHub.playerEffectsController.GetEffect<CustomPlayerEffects.Amnesia>().Enabled)
                    p.ShowHint($"<color=yellow>{p.CurrentItem.durability}</color>/<color=yellow>{ReusableItems.First(i => i.Type == p.CurrentItem.id).Uses}</color> uses left", 1);
                yield return Timing.WaitForSeconds(1);
            }
        }

        private void Player_UsingMedicalItem(Exiled.Events.EventArgs.UsingMedicalItemEventArgs ev)
        {
            foreach (var item in ReusableItems)
            {
                if (ev.Item == item.Type)
                    UsingMedical.Add(new KeyValuePair<int, KeyValuePair<ItemType, float>>(ev.Player.Id, new KeyValuePair<ItemType, float>(ev.Item, ev.Player.CurrentItem.durability)));
            }
        }

        //PlayerId, (typ, durability)
        private readonly List<KeyValuePair<int, KeyValuePair<ItemType, float>>> UsingMedical = new List<KeyValuePair<int, KeyValuePair<ItemType, float>>>();
        private void Player_StoppingMedicalItem(Exiled.Events.EventArgs.StoppingMedicalItemEventArgs ev)
        {
            UsingMedical.RemoveAll(i => i.Key == ev.Player.Id);
        }
        private void Player_MedicalItemDequipped(Exiled.Events.EventArgs.DequippedMedicalItemEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (UsingMedical.Any(i => i.Key == ev.Player.Id && i.Value.Key == ev.Item))
            {
                var data = UsingMedical.First(i => i.Key == ev.Player.Id && i.Value.Key == ev.Item);
                if (data.Value.Value > 1)
                {
                    ev.Player.AddItem(new Inventory.SyncItemInfo
                    {
                        id = data.Value.Key,
                        durability = data.Value.Value - 1,
                    });
                }
                UsingMedical.Remove(data);
            }
        }
    }
}
