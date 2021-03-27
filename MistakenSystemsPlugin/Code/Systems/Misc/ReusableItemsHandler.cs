using Exiled.API.Enums;
using Exiled.API.Extensions;
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
            Exiled.Events.Handlers.Player.MedicalItemDequipped -= this.Handle<Exiled.Events.EventArgs.DequippedMedicalItemEventArgs>((ev) => Player_MedicalItemDequipped(ev));
        }

        private void CustomEvents_OnRequestPickItem(Exiled.Events.EventArgs.PickItemRequestEventArgs ev)
        {
            foreach (var item in ReusableItems)
            {
                if (ev.Pickup.ItemId == item.Type)
                {
                    if (ev.Pickup.durability == 0)
                    {
                        ev.Pickup.ItemId.Spawn(item.Uses, ev.Pickup.position, ev.Pickup.rotation);
                        ev.Pickup.Delete();
                        ev.IsAllowed = false;
                        return;
                    }
                    var items = ev.Player.Inventory.items.Where(i => i.id == item.Type).ToArray();
                    if (items.Length >= item.MaxItems)
                    {
                        float tmp = ev.Pickup.Networkdurability;
                        foreach (var invItem in items)
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
                            if (ev.Pickup.Networkdurability != tmp)
                            {
                                ev.Pickup.ItemId.Spawn(tmp, ev.Pickup.position, ev.Pickup.rotation);
                                ev.Pickup.Delete();
                                ev.Player.ShowHint($"<color=yellow>{ev.Pickup.Networkdurability}</color>/<color=yellow>{item.Uses}</color> uses left", false);
                            }
                            else
                            {
                                string name = item.Type.ToString();
                                if (!name.EndsWith("s"))
                                    name += "s";
                                ev.Player.ShowHintPulsating($"<b>Already</b> reached the limit of <color=yellow>{name}</color> (<color=yellow>{item.MaxItems} {name}</color>)", 2f);
                            }
                        }
                        ev.IsAllowed = false;
                        return;
                    }
                    ev.Player.ShowHint($"<color=yellow>{ev.Pickup.durability}</color>/<color=yellow>{item.Uses}</color> uses left", false);
                    break;
                }
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            Timing.CallDelayed(1, () =>
            {
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
            yield return Timing.WaitForSeconds(.1f);
            ItemType itemType = p.CurrentItem.id;
            var reusable = ReusableItems.First(i => i.Type == itemType);
            p.ShowHint($"<color=yellow>{p.CurrentItem.durability}</color>/<color=yellow>{reusable.Uses}</color> uses left", false, 5, false);
            yield return Timing.WaitForSeconds(1f);
            int iterator = 999;
            while (p.CurrentItem.id == itemType)
            {
                iterator++;
                if(iterator >= 4)
                {
                    if (!p.GetEffectActive<CustomPlayerEffects.Amnesia>())
                    {
                        p.ShowHint($"<color=yellow>{p.CurrentItem.durability}</color>/<color=yellow>{reusable.Uses}</color> uses left", false, 5, false);
                        iterator = 0;
                    }
                    else
                        iterator = 999;
                }

                yield return Timing.WaitForSeconds(1);
            }
            p.ShowHint("", false, .1f, false);
        }

        private void Player_UsingMedicalItem(Exiled.Events.EventArgs.UsingMedicalItemEventArgs ev)
        {
            foreach (var item in ReusableItems)
            {
                if (ev.Item == item.Type)
                    UsingMedical[ev.Player] = (ev.Item, ev.Player.CurrentItem.durability);
            }
        }

        private readonly Dictionary<Player, (ItemType Item, float Durability)> UsingMedical = new Dictionary<Player, (ItemType Item, float Durability)>();
        private void Player_StoppingMedicalItem(Exiled.Events.EventArgs.StoppingMedicalItemEventArgs ev) => UsingMedical.Remove(ev.Player);
        
        private void Player_MedicalItemDequipped(Exiled.Events.EventArgs.DequippedMedicalItemEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (UsingMedical.TryGetValue(ev.Player, out (ItemType Item, float Durability) data))
            {
                if (data.Durability > 1)
                {
                    ev.Player.AddItem(new Inventory.SyncItemInfo
                    {
                        id = data.Item,
                        durability = data.Durability - 1,
                    });
                }
                UsingMedical.Remove(ev.Player);
            }
        }
    }
}
