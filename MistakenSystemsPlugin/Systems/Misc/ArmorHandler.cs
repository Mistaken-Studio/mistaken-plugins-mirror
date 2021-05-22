#pragma warning disable IDE0079
#pragma warning disable IDE1006

using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Gamer.API.CustomItem;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Utilities;
using MEC;
using Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Misc
{
    public class ArmorHandler : Module
    {
        public ArmorHandler(PluginHandler p) : base(p) { }
        public override string Name => "Armor";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.PickingUpItem += this.Handle<Exiled.Events.EventArgs.PickingUpItemEventArgs>((ev) => Player_PickingUpItem(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.DroppingItem += this.Handle<Exiled.Events.EventArgs.DroppingItemEventArgs>((ev) => Player_DroppingItem(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.PickingUpItem -= this.Handle<Exiled.Events.EventArgs.PickingUpItemEventArgs>((ev) => Player_PickingUpItem(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.DroppingItem -= this.Handle<Exiled.Events.EventArgs.DroppingItemEventArgs>((ev) => Player_DroppingItem(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        //25?
        public class Armor : CustomItem
        {
            public static Armor Instance = new Armor();
            public Armor() => Register();
            public override string ItemName => "Pancerz";
            public override Main.SessionVarType SessionVarType => Main.SessionVarType.CI_ARMOR;
            public override ItemType Item => ItemType.Coin;
            public override int Durability => 001;
            public static readonly HashSet<Player> BlockInteractions = new HashSet<Player>();
            public override void OnRestart()
            {
                BlockInteractions.Clear();
            }
            public void OnWear(Player player, Pickup pickup, bool fast) => OnWear(player, pickup.durability, fast);
            public void OnWear(Player player, float pickupDurability, bool fast)
            {
                float durability = GetInternalDurability(pickupDurability);
                if (!player.IsConnected)
                    return;
                if (!fast)
                {
                    if (BlockInteractions.Contains(player))
                        return;
                    Gamer.Utilities.BetterCourotines.CallDelayed(0.1f, () => BlockInteractions.Add(player), "Armor.Unlock");
                    //player.SetGUI("Armor", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Putting on <color=yellow>{this.ItemName}</color>", 5);
                    player.EnableEffect<CustomPlayerEffects.Ensnared>(5);
                }
                Gamer.Utilities.BetterCourotines.CallDelayed(5 * (fast ? 0 : 1), () =>
                {
                    if (!player.IsAlive)
                        return;
                    player.SetGUI("ArmorWear", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Nosisz <color=yellow>{ItemName}</color>");
                    player.EnableEffect<CustomPlayerEffects.Panic>();
                    Shield.ShieldedManager.Add(new Shield.Shielded(player, (int)Math.Ceiling(durability), durability / 60, 30, 0, 0.85f));
                    if (player.ArtificialHealth < 10 && !player.ReferenceHub.playerEffectsController.GetEffect<CustomPlayerEffects.Scp207>().Enabled)
                        player.ArtificialHealth = 10;
                    BlockInteractions.Remove(player);
                }, "Armor.Wear");
            }
            public void OnUnWear(Player player, bool fast)
            {
                if (!player.IsConnected)
                    return;
                if (!fast)
                {
                    if (BlockInteractions.Contains(player))
                        return;
                    Gamer.Utilities.BetterCourotines.CallDelayed(0.1f, () => BlockInteractions.Add(player), "Armor.Unlock");
                    player.SetGUI("Armor", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Upuszczasz <color=yellow>{ItemName}</color>", 3);
                    player.EnableEffect<CustomPlayerEffects.Ensnared>(3);
                }
                Gamer.Utilities.BetterCourotines.CallDelayed(3 * (fast ? 0 : 1), () =>
                {
                    if (player.IsConnected)
                        player.DisableEffect<CustomPlayerEffects.Panic>();
                    Shield.ShieldedManager.Remove(player);
                    BlockInteractions.Remove(player);
                    player.SetGUI("ArmorWear", Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
                }, "Armor.Unwear");
            }
            public override void OnForceclass(Player player)
            {
                base.OnForceclass(player);
                player.SetGUI("ArmorWear", Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
                OnUnWear(player, true);
            }
            public override bool OnPickup(Player player, Pickup pickup)
            {
                base.OnPickup(player, pickup);
                if (BlockInteractions.Contains(player))
                    return false;
                if (player.Inventory.items.Any(i => i.id == pickup.ItemId))
                {
                    player.SetGUI("Armor", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Już <b>osiągnąłeś</b> limit <color=yellow>{ItemName}s</color> (<color=yellow>{1} {ItemName}</color>)", 2);
                    return false;
                }
                OnWear(player, pickup, false);
                return true;
            }
            public override bool OnPrePickup(Player player, Pickup pickup)
            {
                if (BlockInteractions.Contains(player))
                    return false;
                if (player.Inventory.items.Any(i => i.id == pickup.ItemId))
                {
                    player.SetGUI("Armor", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Już <b>osiągnąłeś</b> limit <color=yellow>{ItemName}s</color> (<color=yellow>{1} {ItemName}</color>)", 2);
                    return false;
                }
                return true;
            }
            public override bool OnDrop(Player player, Inventory.SyncItemInfo item)
            {
                base.OnDrop(player, item);
                if (BlockInteractions.Contains(player))
                    return false;

                OnUnWear(player, false);
                return true;
            }
            public override Vector3 Size => new Vector3(3, 3, 5);
            public override Pickup OnUpgrade(Pickup pickup, Scp914Knob setting)
            {
                switch (setting)
                {
                    case Scp914Knob.Rough:
                        pickup.Networkdurability = Durability * 1000f + 1;
                        break;
                    case Scp914Knob.Coarse:
                        pickup.Networkdurability = Durability * 1000f + 10;
                        break;
                    case Scp914Knob.OneToOne:
                        int rand = UnityEngine.Random.Range(0, 3);
                        if(rand == 0)
                            pickup.Networkdurability = LiteArmor.Instance.Durability * 1000f + 15;
                        else if(rand == 1)
                            pickup.Networkdurability = Armor.Instance.Durability * 1000f + 25;
                        else if(rand == 2)
                            pickup.Networkdurability = HeavyArmor.Instance.Durability * 1000f + 100;
                        break;
                    case Scp914Knob.Fine:
                        pickup.Networkdurability = Durability * 1000f + UnityEngine.Random.Range(25, 30);
                        break;
                    case Scp914Knob.VeryFine:
                        pickup.Networkdurability = Durability * 1000f + UnityEngine.Random.Range(1, 40);
                        break;
                }
                return pickup;
            }
            #region Spawn
            public static void GiveDelayed(Player player, int innerDurability = 25) => Gamer.Utilities.BetterCourotines.CallDelayed(0.2f, () => Give(player, innerDurability), "Armor.GiveDelayed");
            public static bool Give(Player player, int innerDurability = 25) => Instance._give(player, innerDurability);
            private bool _give(Player player, int innerDurability = 25)
            {
                if (player.HasItem(ItemType.Coin))
                    return false;
                if (player.Inventory.items.Count >= 8)
                {
                    _spawn(player.Position, innerDurability);
                    return true;
                }
                float dur = Durability * 1000f + innerDurability;
                player.AddItem(new Inventory.SyncItemInfo
                {
                    id = Item,
                    durability = dur
                });
                base.OnPickup(player, null);
                OnWear(player, dur, true);
                return true;
            }
            public static void Spawn(Vector3 position, int innerDurability = 25) => Instance._spawn(position, innerDurability);
            private void _spawn(Vector3 position, int innerDurability = 25)
            {
                float dur = Durability * 1000f + innerDurability;
                MapPlus.Spawn(new Inventory.SyncItemInfo
                {
                    id = Item,
                    durability = dur
                }, position, Quaternion.Euler(90, 0, 0), Size);
            }
            #endregion
        }
        //15?
        public class LiteArmor : CustomItem
        {
            public static LiteArmor Instance = new LiteArmor();
            public LiteArmor() => Register();
            public override string ItemName => "Lekki Pancerz";
            public override Main.SessionVarType SessionVarType => Main.SessionVarType.CI_LIGHT_ARMOR;
            public override ItemType Item => ItemType.Coin;
            public override int Durability => 002;
            public void OnWear(Player player, Pickup pickup, bool fast) => OnWear(player, pickup.durability, fast);
            public void OnWear(Player player, float pickupDurability, bool fast)
            {
                float durability = GetInternalDurability(pickupDurability);
                if (!player.IsConnected)
                    return;
                if (!fast)
                {
                    if (Armor.BlockInteractions.Contains(player))
                        return;
                    Gamer.Utilities.BetterCourotines.CallDelayed(0.1f, () => Armor.BlockInteractions.Add(player), "LiteArmor.Unlock");
                    //player.SetGUI("Armor", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Putting on <color=yellow>{this.ItemName}</color>", 2);
                    player.EnableEffect<CustomPlayerEffects.Ensnared>(2);
                }
                Gamer.Utilities.BetterCourotines.CallDelayed(2 * (fast ? 0 : 1), () =>
                {
                    Shield.ShieldedManager.Add(new Shield.Shielded(player, (int)Math.Ceiling(durability), durability / 60, 30, 0, 0.5f));
                    Armor.BlockInteractions.Remove(player);
                    player.SetGUI("ArmorWear", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Nosisz <color=yellow>{ItemName}</color>");
                }, "LiteArmor.Wear");
            }
            public void OnUnWear(Player player, bool fast)
            {
                if (!player.IsConnected)
                    return;
                if (!fast)
                {
                    if (Armor.BlockInteractions.Contains(player))
                        return;
                    Gamer.Utilities.BetterCourotines.CallDelayed(0.1f, () => Armor.BlockInteractions.Add(player), "LiteArmor.Unlock");
                    player.SetGUI("Armor", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Upuszczasz <color=yellow>{ItemName}</color>", 2);
                    player.EnableEffect<CustomPlayerEffects.Ensnared>(1);
                }
                Gamer.Utilities.BetterCourotines.CallDelayed(1 * (fast ? 0 : 1), () =>
                {
                    Shield.ShieldedManager.Remove(player);
                    Armor.BlockInteractions.Remove(player);
                    player.SetGUI("ArmorWear", Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
                }, "LiteArmor.UnWear");
            }
            public override void OnForceclass(Player player)
            {
                base.OnForceclass(player);
                player.SetGUI("ArmorWear", Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
                OnUnWear(player, true);
            }
            public override bool OnPickup(Player player, Pickup pickup)
            {
                base.OnPickup(player, pickup);
                if (Armor.BlockInteractions.Contains(player))
                    return false;
                if (player.Inventory.items.Any(i => i.id == pickup.ItemId))
                {
                    player.SetGUI("Armor", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Już <b>osiągnąłeś</b> limit <color=yellow>{ItemName}s</color> (<color=yellow>{1} {ItemName}</color>)", 2);
                    return false;
                }
                OnWear(player, pickup, false);
                return true;
            }
            public override bool OnPrePickup(Player player, Pickup pickup)
            {
                if (Armor.BlockInteractions.Contains(player))
                    return false;
                if (player.Inventory.items.Any(i => i.id == pickup.ItemId))
                {
                    player.SetGUI("Armor", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Już <b>osiągnąłeś</b> limit <color=yellow>{ItemName}s</color> (<color=yellow>{1} {ItemName}</color>)", 2);
                    return false;
                }
                return true;
            }
            public override bool OnDrop(Player player, Inventory.SyncItemInfo item)
            {
                base.OnDrop(player, item);
                if (Armor.BlockInteractions.Contains(player))
                    return false;

                OnUnWear(player, false);
                return true;
            }
            public override Vector3 Size => new Vector3(2, 2, 4);
            public override Pickup OnUpgrade(Pickup pickup, Scp914Knob setting)
            {
                switch (setting)
                {
                    case Scp914Knob.Rough:
                        pickup.Networkdurability = Durability * 1000f + 1;
                        break;
                    case Scp914Knob.Coarse:
                        pickup.Networkdurability = Durability * 1000f + 5;
                        break;
                    case Scp914Knob.OneToOne:
                        int rand = UnityEngine.Random.Range(0, 3);
                        if (rand == 0)
                            pickup.Networkdurability = LiteArmor.Instance.Durability * 1000f + 15;
                        else if (rand == 1)
                            pickup.Networkdurability = Armor.Instance.Durability * 1000f + 25;
                        else if (rand == 2)
                            pickup.Networkdurability = HeavyArmor.Instance.Durability * 1000f + 100;
                        break;
                    case Scp914Knob.Fine:
                        pickup.Networkdurability = Durability * 1000f + UnityEngine.Random.Range(15, 20);
                        break;
                    case Scp914Knob.VeryFine:
                        pickup.Networkdurability = Durability * 1000f + UnityEngine.Random.Range(1, 30);
                        break;
                }
                return pickup;
            }
            #region Spawn
            public static void GiveDelayed(Player player, int innerDurability = 25) => Gamer.Utilities.BetterCourotines.CallDelayed(0.2f, () => Give(player, innerDurability), "LiteArmor.GiveDelayed");
            public static bool Give(Player player, int innerDurability = 25) => Instance._give(player, innerDurability);
            private bool _give(Player player, int innerDurability = 25)
            {
                if (player.HasItem(ItemType.Coin))
                    return false;
                if (player.Inventory.items.Count >= 8)
                {
                    _spawn(player.Position, innerDurability);
                    return true;
                }
                float dur = Durability * 1000f + innerDurability;
                player.AddItem(new Inventory.SyncItemInfo
                {
                    id = Item,
                    durability = dur
                });
                base.OnPickup(player, null);
                OnWear(player, dur, true);
                return true;
            }
            public static void Spawn(Vector3 position, int innerDurability = 25) => Instance._spawn(position, innerDurability);
            private void _spawn(Vector3 position, int innerDurability = 25)
            {
                float dur = Durability * 1000f + innerDurability;
                MapPlus.Spawn(new Inventory.SyncItemInfo
                {
                    id = Item,
                    durability = dur
                }, position, Quaternion.Euler(90, 0, 0), Size);
            }
            #endregion
        }
        //100?
        public class HeavyArmor : CustomItem
        {
            public static HeavyArmor Instance = new HeavyArmor();
            public HeavyArmor() => Register();
            public override string ItemName => "Ciężki Pancerz";
            public override Main.SessionVarType SessionVarType => Main.SessionVarType.CI_HEAVY_ARMOR;
            public override ItemType Item => ItemType.Coin;
            public override int Durability => 003;
            public void OnWear(Player player, Pickup pickup, bool fast) => OnWear(player, pickup.durability, fast);
            public void OnWear(Player player, float pickupDurability, bool fast)
            {
                float durability = GetInternalDurability(pickupDurability);
                if (!player.IsConnected)
                    return;
                if (!fast)
                {
                    if (Armor.BlockInteractions.Contains(player))
                        return;
                    Gamer.Utilities.BetterCourotines.CallDelayed(0.1f, () => Armor.BlockInteractions.Add(player), "HeavyArmor.Unlock");
                    //player.SetGUI("Armor", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Putting on <color=yellow>{this.ItemName}</color>", 8);
                    player.EnableEffect<CustomPlayerEffects.Ensnared>(8);
                }
                Gamer.Utilities.BetterCourotines.CallDelayed(8 * (fast ? 0 : 1), () =>
                {
                    player.EnableEffect<CustomPlayerEffects.Disabled>();
                    Shield.ShieldedManager.Add(new Shield.Shielded(player, (int)Math.Ceiling(durability), durability / 60, 30, 0, 1f));
                    if (player.ArtificialHealth < 30 && !player.ReferenceHub.playerEffectsController.GetEffect<CustomPlayerEffects.Scp207>().Enabled)
                        player.ArtificialHealth = 30;
                    Armor.BlockInteractions.Remove(player);
                    player.SetGUI("ArmorWear", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Nosisz <color=yellow>{ItemName}</color>");
                }, "HeavyArmor.Wear");
            }
            public void OnUnWear(Player player, bool fast)
            {
                if (!player.IsConnected)
                    return;
                if (!fast)
                {
                    if (Armor.BlockInteractions.Contains(player))
                        return;
                    Gamer.Utilities.BetterCourotines.CallDelayed(0.1f, () => Armor.BlockInteractions.Add(player), "HeavyArmor.Unlock");
                    player.SetGUI("Armor", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Upuszczasz <color=yellow>{ItemName}</color>", 5);
                    player.EnableEffect<CustomPlayerEffects.Ensnared>(5);
                }
                Gamer.Utilities.BetterCourotines.CallDelayed(5 * (fast ? 0 : 1), () =>
                {
                    if (player.IsConnected)
                        player.DisableEffect<CustomPlayerEffects.Disabled>();
                    Shield.ShieldedManager.Remove(player);
                    Armor.BlockInteractions.Remove(player);
                    player.SetGUI("ArmorWear", Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
                }, "HeavyArmor.UnWear");
            }
            public override void OnForceclass(Player player)
            {
                base.OnForceclass(player);
                player.SetGUI("ArmorWear", Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
                OnUnWear(player, true);
            }
            public override bool OnPickup(Player player, Pickup pickup)
            {
                base.OnPickup(player, pickup);
                if (Armor.BlockInteractions.Contains(player))
                    return false;
                if (player.Inventory.items.Any(i => i.id == pickup.ItemId))
                {
                    player.SetGUI("Armor", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Już <b>osiągnąłeś</b> limit  <color=yellow>{ItemName}s</color> (<color=yellow>{1} {ItemName}</color>)", 2);
                    return false;
                }
                OnWear(player, pickup, false);
                return true;
            }
            public override bool OnPrePickup(Player player, Pickup pickup)
            {
                if (Armor.BlockInteractions.Contains(player))
                    return false;
                if (player.Inventory.items.Any(i => i.id == pickup.ItemId))
                {
                    player.SetGUI("Armor", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Już <b>osiągnąłeś</b> limit <color=yellow>{ItemName}</color> (<color=yellow>{1} {ItemName}</color>)", 2);
                    return false;
                }
                return true;
            }
            public override bool OnDrop(Player player, Inventory.SyncItemInfo item)
            {
                base.OnDrop(player, item);
                if (Armor.BlockInteractions.Contains(player))
                    return false;

                OnUnWear(player, false);
                return true;
            }
            public override Vector3 Size => new Vector3(3.5f, 3.5f, 10);
            public override Pickup OnUpgrade(Pickup pickup, Scp914Knob setting)
            {
                switch (setting)
                {
                    case Scp914Knob.Rough:
                        pickup.Networkdurability = Durability * 1000f + 1;
                        break;
                    case Scp914Knob.Coarse:
                        pickup.Networkdurability = Durability * 1000f + 50;
                        break;
                    case Scp914Knob.OneToOne:
                        int rand = UnityEngine.Random.Range(0, 3);
                        if (rand == 0)
                            pickup.Networkdurability = LiteArmor.Instance.Durability * 1000f + 15;
                        else if (rand == 1)
                            pickup.Networkdurability = Armor.Instance.Durability * 1000f + 25;
                        else if (rand == 2)
                            pickup.Networkdurability = HeavyArmor.Instance.Durability * 1000f + 100;
                        break;
                    case Scp914Knob.Fine:
                        pickup.Networkdurability = Durability * 1000f + UnityEngine.Random.Range(100, 150);
                        break;
                    case Scp914Knob.VeryFine:
                        pickup.Networkdurability = Durability * 1000f + UnityEngine.Random.Range(1, 200);
                        break;
                }
                return pickup;
            }
            #region Spawn
            public static void GiveDelayed(Player player, int innerDurability = 25) => Gamer.Utilities.BetterCourotines.CallDelayed(0.2f, () => Give(player, innerDurability), "HeavyArmor.GiveDelayed");
            public static bool Give(Player player, int innerDurability = 25) => Instance._give(player, innerDurability);
            private bool _give(Player player, int innerDurability = 25)
            {
                if (player.HasItem(ItemType.Coin))
                    return false;
                if (player.Inventory.items.Count >= 8)
                {
                    _spawn(player.Position, innerDurability);
                    return true;
                }
                float dur = Durability * 1000f + innerDurability;
                player.AddItem(new Inventory.SyncItemInfo
                {
                    id = Item,
                    durability = dur
                });
                base.OnPickup(player, null);
                OnWear(player, dur, true);
                return true;
            }
            public static void Spawn(Vector3 position, int innerDurability = 25) => Instance._spawn(position, innerDurability);
            private void _spawn(Vector3 position, int innerDurability = 25)
            {
                float dur = Durability * 1000f + innerDurability;
                MapPlus.Spawn(new Inventory.SyncItemInfo
                {
                    id = Item,
                    durability = dur
                }, position, Quaternion.Euler(90, 0, 0), Size);
            }
            #endregion
        }
        private void Server_RoundStarted()
        {
            this.RunCoroutine(SpawnItems(), "SpawnItems");
        }
        private IEnumerator<float> SpawnItems()
        {
            var door = Map.Doors.First(d => d.Type() == DoorType.LczArmory);
            var pos = door.transform.position + Vector3.up * 2 + door.transform.forward * 7 + door.transform.right * -4.5f;
            Inventory inv = Pickup.Inv;
            for (int i = 0; i < 5; i++)
            {
                Armor.Spawn(pos, 25);
                yield return Timing.WaitForSeconds(0.1f);
            }
            var door2 = Map.Doors.First(d => d.Type() == DoorType.HczArmory);
            var pos2 = door2.transform.position + Vector3.up * 2 + -door2.transform.right * 2 + -door2.transform.forward;
            for (int j = 0; j < 10; j++)
            {
                Armor.Spawn(pos2, 25);
                yield return Timing.WaitForSeconds(0.1f);
            }
        }
        private void Player_DroppingItem(Exiled.Events.EventArgs.DroppingItemEventArgs ev)
        {
            if (Armor.BlockInteractions.Contains(ev.Player))
                ev.IsAllowed = false;
        }
        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.IsEscaped)
                return;
            if (ev.Player.GetSessionVar<bool>(Main.SessionVarType.ITEM_LESS_CLSSS_CHANGE))
                return;
            switch (ev.NewRole)
            {
                case RoleType.ChaosInsurgency:
                case RoleType.NtfCommander:
                    Armor.GiveDelayed(ev.Player, 25);
                    break;
            }
        }
        private void Player_PickingUpItem(PickingUpItemEventArgs ev)
        {
            if (Armor.BlockInteractions.Contains(ev.Player))
                ev.IsAllowed = false;
        }
    }
}
