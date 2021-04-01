using Exiled.API.Extensions;
using Exiled.API.Features;
using Scp914;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.API
{
    namespace CustomItem
    {
        public abstract class CustomItem
        {
            public static readonly List<CustomItem> CustomItemTypes = new List<CustomItem>();
            public static readonly HashSet<ItemType> CustomItemsFastCheckCache = new HashSet<ItemType>();

            protected void Register()
            {
                if (!CustomItemTypes.Contains(this))
                    CustomItemTypes.Add(this);
                CustomItemsFastCheckCache.Add(Item);
            }

            public abstract string ItemName { get; }
            public abstract ItemType Item { get; }
            public abstract int Durability { get; }
            public virtual Vector3 Size { get; } = Vector3.one;
            public virtual Upgrade[] Upgrades { get; } = new Upgrade[0];
            public virtual bool OnPrePickup(Player player, Pickup pickup) => true;
            public virtual bool OnPickup(Player player, Pickup pickup) => true;
            public virtual bool OnDrop(Player player, Inventory.SyncItemInfo item) => true;
            public virtual void OnStartHolding(Player player, Inventory.SyncItemInfo item) { }
            public virtual void OnStopHolding(Player player, Inventory.SyncItemInfo item) { }
            public virtual bool OnShoot(Player player, Inventory.SyncItemInfo item, GameObject target) => true;
            public virtual bool OnReload(Player player, Inventory.SyncItemInfo item) => true;
            public virtual bool OnThrow(Player player, Inventory.SyncItemInfo item, bool slow) => true;
            public virtual void OnForceclass(Player player) { }
            public virtual void OnRestart() { }
            public virtual Pickup OnUpgrade(Pickup pickup, Scp914Knob setting) => pickup;

            public virtual void Spawn(Vector3 position)
            {
                this.Item.Spawn(this.Durability, position);
            }

            public float GetInternalDurability(Inventory.SyncItemInfo item) => GetInternalDurability(item.durability);
            public float GetInternalDurability(float durability)
            {
                if (((durability - 1) * 1000).ToString().Length == 3)
                    return 0;
                return (((durability - 1) * 1000) % 1) * 1000;
            }
            public void SetInternalDurability(Player player, Inventory.SyncItemInfo item, float value)
            {
                float fullValue = 1 + (this.Durability + (value / 1000)) / 1000;
                int index = player.Inventory.items.IndexOf(item);
                var info = player.Inventory.items[index];
                info.durability = fullValue;
                player.Inventory.items[index] = info;
            }

            public struct Upgrade
            {
                public Scp914Knob KnobSetting;
                public ItemType Input;
                public float? Durability;
                public float Chance;
            }
        }
    }
}
