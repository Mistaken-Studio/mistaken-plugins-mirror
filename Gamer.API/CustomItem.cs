using Exiled.API.Features;
using Gamer.Utilities;
using Scp914;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.API
{
    namespace CustomItem
    {
        /// <summary>
        /// Custom Item Class
        /// </summary>
        public abstract class CustomItem
        {
            /// <summary>
            /// All registered custom classes
            /// </summary>
            public static readonly List<CustomItem> CustomItemTypes = new List<CustomItem>();
            /// <summary>
            /// List of <see cref="ItemType"/> that custom items uses
            /// </summary>
            public static readonly HashSet<ItemType> CustomItemsFastCheckCache = new HashSet<ItemType>();
            /// <summary>
            /// Registeres class
            /// </summary>
            protected void Register()
            {
                if (!CustomItemTypes.Contains(this))
                    CustomItemTypes.Add(this);
                CustomItemsFastCheckCache.Add(Item);
            }
            /// <summary>
            /// Item Name
            /// </summary>
            public abstract string ItemName { get; }
            /// <summary>
            /// Item Name
            /// </summary>
            public abstract Utilities.Main.SessionVarType SessionVarType { get; }
            /// <summary>
            /// Item Type
            /// </summary>
            public abstract ItemType Item { get; }
            /// <summary>
            /// Identifing Durability
            /// </summary>
            public abstract int Durability { get; }
            /// <summary>
            /// Size when on floor
            /// </summary>
            public virtual Vector3 Size { get; } = Vector3.one;
            /// <summary>
            /// List of rules how to upgrade something to this custom item in 914
            /// </summary>
            public virtual Upgrade[] Upgrades { get; } = new Upgrade[0];
            /// <summary>
            /// Called when requesting pickup
            /// </summary>
            /// <param name="player">Requesting player</param>
            /// <param name="pickup">Pickup</param>
            /// <returns>If should be allowed</returns>
            public virtual bool OnPrePickup(Player player, Pickup pickup) => true;
            /// <summary>
            /// Called when picking up item
            /// </summary>
            /// <param name="player">Picking player</param>
            /// <param name="pickup">Pickup</param>
            /// <returns>If shoud be allowed</returns>
            public virtual bool OnPickup(Player player, Pickup pickup)
            {
                player.SetSessionVar(SessionVarType, true);
                return true;
            }
            /// <summary>
            /// Called when dropping item
            /// </summary>
            /// <param name="player">Dropping player</param>
            /// <param name="item">Item</param>
            /// <returns>If should be allowed</returns>
            public virtual bool OnDrop(Player player, Inventory.SyncItemInfo item)
            {
                player.SetSessionVar(SessionVarType, false);
                return true;
            }
            /// <summary>
            /// Called when player starts holding item
            /// </summary>
            /// <param name="player">Holding player</param>
            /// <param name="item">Holded item</param>
            public virtual void OnStartHolding(Player player, Inventory.SyncItemInfo item) { }
            /// <summary>
            /// Called when player stops holding item
            /// </summary>
            /// <param name="player">Holding player</param>
            /// <param name="item">No longer holded item</param>
            public virtual void OnStopHolding(Player player, Inventory.SyncItemInfo item) { }
            /// <summary>
            /// Called when player shoots using item
            /// </summary>
            /// <param name="player">Shooting player</param>
            /// <param name="item">Item instance</param>
            /// <param name="target">Target that was hit</param>
            /// <param name="position">Hit position</param>
            /// <returns>If should be allowed</returns>
            public virtual bool OnShoot(Player player, Inventory.SyncItemInfo item, GameObject target, Vector3 position) => true;
            /// <summary>
            /// Called when reloading item
            /// </summary>
            /// <param name="player">Reloading player</param>
            /// <param name="item">Reloaded item</param>
            /// <returns>If should be allowed</returns>
            public virtual bool OnReload(Player player, Inventory.SyncItemInfo item) => true;
            /// <summary>
            /// Called when throwing (as grenade) item
            /// </summary>
            /// <param name="player">Throwing player</param>
            /// <param name="item">Thrown item</param>
            /// <param name="slow">If is a slow throw</param>
            /// <returns>If should be allowed</returns>
            public virtual bool OnThrow(Player player, Inventory.SyncItemInfo item, bool slow)
            {
                player.SetSessionVar(SessionVarType, false);
                return true;
            }
            /// <summary>
            /// Called when player is forceclassed with item in inventory
            /// </summary>
            /// <param name="player">Forceclassed player</param>
            public virtual void OnForceclass(Player player) => player.SetSessionVar(SessionVarType, false);
            /// <summary>
            /// Called on round restart
            /// </summary>
            public virtual void OnRestart() { }
            /// <summary>
            /// Called when item is in 914
            /// </summary>
            /// <param name="pickup">Item</param>
            /// <param name="setting">Setting</param>
            /// <returns>Upgrading result</returns>
            public virtual Pickup OnUpgrade(Pickup pickup, Scp914Knob setting) => pickup;
            /// <summary>
            /// Spawns item
            /// </summary>
            /// <param name="position">Position</param>
            /// <param name="innerDurability">Inner durability</param>
            public virtual void Spawn(Vector3 position, float innerDurability = 0f)
            {
                MapPlus.Spawn(new Inventory.SyncItemInfo
                {
                    id = Item,
                    durability = 1f + (Durability / 1000f) + (innerDurability / 1000000),
                }, position, Quaternion.identity, Size);
            }
            /// <summary>
            /// Returns internal durability
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Internal durability</returns>
            public float GetInternalDurability(Inventory.SyncItemInfo item) => GetInternalDurability(item.durability);
            /// <summary>
            /// Returns internal durability
            /// </summary>
            /// <param name="durability">Full durability</param>
            /// <returns>Internal durability</returns>
            public float GetInternalDurability(float durability)
            {
                if (((durability - 1) * 1000).ToString().Length == 3)
                    return 0;
                return (((durability - 1) * 1000) % 1) * 1000;
            }
            /// <summary>
            /// Sets internal durability
            /// </summary>
            /// <param name="player">Player which has item</param>
            /// <param name="item">Item</param>
            /// <param name="value">Internal durability</param>
            public void SetInternalDurability(Player player, Inventory.SyncItemInfo item, float value)
            {
                float fullValue = 1 + (Durability + (value / 1000)) / 1000;
                int index = player.Inventory.items.IndexOf(item);
                var info = player.Inventory.items[index];
                info.durability = fullValue;
                player.Inventory.items[index] = info;
            }
            /// <summary>
            /// Upgrade structure
            /// </summary>
            public struct Upgrade
            {
                /// <summary>
                /// 914 Setting
                /// </summary>
                public Scp914Knob KnobSetting;
                /// <summary>
                /// Input item type
                /// </summary>
                public ItemType Input;
                /// <summary>
                /// Input item durability or <see langword="null"/> is any durability
                /// </summary>
                public float? Durability;
                /// <summary>
                /// Chance of upgrading in %
                /// </summary>
                public float Chance;
            }
        }
    }
}
