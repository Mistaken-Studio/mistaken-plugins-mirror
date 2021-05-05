using Exiled.API.Features;
using Gamer.API.CustomItem;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using Interactables.Interobjects.DoorUtils;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Taser
{
    /// <inheritdoc/>
    public class TaserHandler : Module
    {
        internal static readonly Vector3 Size = new Vector3(.75f, .75f, .75f);
        internal static readonly HashSet<ItemType> usableItems = new HashSet<ItemType>()
        {
            ItemType.MicroHID,
            ItemType.Medkit,
            ItemType.Painkillers,
            ItemType.SCP018,
            ItemType.SCP207,
            ItemType.SCP268,
            ItemType.SCP500,
            ItemType.GrenadeFrag,
            ItemType.GrenadeFlash,
            ItemType.Adrenaline
        };
        /// <summary>
        /// USP that applies some effects on target.
        /// </summary>
        public class TaserItem : CustomItem
        {
            /// <inheritdoc/>
            public TaserItem() => base.Register();
            /// <inheritdoc/>
            public override string ItemName => "Taser";
            /// <inheritdoc/>
            public override ItemType Item => ItemType.GunUSP;
            /// <inheritdoc/>
            public override Main.SessionVarType SessionVarType => Main.SessionVarType.CI_TASER;
            /// <inheritdoc/>
            public override int Durability => 501;
            /// <inheritdoc/>
            public override Vector3 Size => TaserHandler.Size;
            /// <inheritdoc/>
            public override void OnStartHolding(Player player, Inventory.SyncItemInfo item)
            {
                Timing.RunCoroutine(UpdateInterface(player));
            }
            /// <inheritdoc/>
            public override void Spawn(Vector3 position, float innerDurability = 0f)
            {
                float dur = Durability + (Index++);
                MapPlus.Spawn(new Inventory.SyncItemInfo
                {
                    durability = dur,
                    id = ItemType.GunUSP,
                }, position, Quaternion.identity, Size);
            }
            /// <summary>
            /// Gives Taser to <paramref name="player"/>
            /// </summary>
            /// <param name="player">Player to give taser to</param>
            public static void Give(Player player)
            {
                if (player.Inventory.items.Count < 8)
                {
                    float dur = 501 + (Index++);
                    player.AddItem(new Inventory.SyncItemInfo
                    {
                        durability = dur,
                        id = ItemType.GunUSP,
                    });
                    player.SetSessionVar(Main.SessionVarType.CI_TASER, true);
                }
            }
            /// <inheritdoc/>
            public override Upgrade[] Upgrades => new Upgrade[]
            {
                new Upgrade
                {
                    Chance = 100,
                    Durability = null,
                    Input = ItemType.GunUSP,
                    KnobSetting = Scp914.Scp914Knob.OneToOne
                }
            };
            /// <inheritdoc/>
            public override bool OnReload(Player player, Inventory.SyncItemInfo item)
            {
                return false;
            }
            /// <inheritdoc/>
            public override bool OnShoot(Player player, Inventory.SyncItemInfo item, GameObject target, Vector3 position)
            {
                int dur = (int)GetInternalDurability(item);
                if (!Cooldowns.TryGetValue(dur, out DateTime time))
                    Cooldowns.Add(dur, DateTime.Now);
                if (DateTime.Now < time)
                {
                    player.SetGUI("taserAmmo", Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, "You have <color=yellow>no ammo</color>", 3);
                }
                else
                {
                    if (player.GetEffectActive<CustomPlayerEffects.Scp268>())
                        player.DisableEffect<CustomPlayerEffects.Scp268>();
                    Cooldowns[dur] = DateTime.Now.AddSeconds(Cooldown);
                    var targetPlayer = Player.Get(target);
                    if (targetPlayer != null)
                    {
                        player.ReferenceHub.weaponManager.RpcConfirmShot(true, player.ReferenceHub.weaponManager.curWeapon);
                        if (targetPlayer.GetSessionVar<bool>(Main.SessionVarType.CI_LIGHT_ARMOR) || targetPlayer.GetSessionVar<bool>(Main.SessionVarType.CI_ARMOR) || targetPlayer.GetSessionVar<bool>(Main.SessionVarType.CI_HEAVY_ARMOR))
                        {
                            RoundLogger.Log("TASER", "BLOCKED", $"{player.PlayerToString()} hit {targetPlayer.PlayerToString()} but effects were blocked by an armor");
                            return false;
                        }
                        if (targetPlayer.IsHuman)
                        {
                            targetPlayer.EnableEffect<CustomPlayerEffects.Ensnared>(2);
                            targetPlayer.EnableEffect<CustomPlayerEffects.Flashed>(5);
                            targetPlayer.EnableEffect<CustomPlayerEffects.Deafened>(10);
                            targetPlayer.EnableEffect<CustomPlayerEffects.Blinded>(10);
                            targetPlayer.EnableEffect<CustomPlayerEffects.Amnesia>(5);
                            if (targetPlayer.CurrentItemIndex != -1 && !usableItems.Contains(targetPlayer.CurrentItem.id))
                            {
                                Exiled.Events.Handlers.Player.OnDroppingItem(new Exiled.Events.EventArgs.DroppingItemEventArgs(targetPlayer, targetPlayer.CurrentItem));
                                var pickup = MapPlus.Spawn(targetPlayer.CurrentItem, targetPlayer.Position, Quaternion.identity, Vector3.one);
                                //targetPlayer.DropItem(targetPlayer.CurrentItem);
                                targetPlayer.RemoveItem(targetPlayer.CurrentItem);
                                Exiled.Events.Handlers.Player.OnItemDropped(new Exiled.Events.EventArgs.ItemDroppedEventArgs(targetPlayer, pickup));
                            }
                            RoundLogger.Log("TASER", "HIT", $"{player.PlayerToString()} hit {targetPlayer.PlayerToString()}");
                            targetPlayer.Broadcast("<color=yellow>Taser</color>", 10, $"<color=yellow>You have been tased by: {player.Nickname} [{player.Role}]</color>");
                            targetPlayer.SendConsoleMessage($"You have been tased by: {player.Nickname} [{player.Role}]", "yellow");
                            return false;
                        }
                    }
                    else
                    {
                        var colliders = UnityEngine.Physics.OverlapSphere(position, 0.1f);
                        if (colliders != null)
                        {
                            foreach (var _item in colliders)
                            {
                                if (!Mistaken.Systems.Misc.DoorHandler.Doors.TryGetValue(_item.gameObject, out var door) || door == null)
                                    continue;
                                door.ServerChangeLock(DoorLockReason.NoPower, true);
                                MEC.Timing.CallDelayed(10, () => door.ServerChangeLock(DoorLockReason.NoPower, false));
                                player.ReferenceHub.weaponManager.RpcConfirmShot(true, player.ReferenceHub.weaponManager.curWeapon);
                            }
                            player.ReferenceHub.weaponManager.RpcConfirmShot(false, player.ReferenceHub.weaponManager.curWeapon);
                            RoundLogger.Log("TASER", "HIT", $"{player.PlayerToString()} hit door");
                            return false;
                        }
                    }
                }
                RoundLogger.Log("TASER", "HIT", $"{player.PlayerToString()} didn't hit anyone");
                return false;
            }

            private const int Cooldown = 90;
            private readonly Dictionary<int, DateTime> Cooldowns = new Dictionary<int, DateTime>();
            private IEnumerator<float> UpdateInterface(Player player)
            {
                yield return Timing.WaitForSeconds(0.5f);
                while (player.CurrentItem.id == ItemType.GunUSP)
                {
                    if (!(player.CurrentItem.durability >= 501000f && player.CurrentItem.durability <= 502000f))
                        break;
                    int dur = (int)GetInternalDurability(player.CurrentItem);
                    if (!Cooldowns.TryGetValue(dur, out DateTime time))
                    {
                        Cooldowns.Add(dur, DateTime.Now);
                        time = DateTime.Now;
                    }
                    var diff = ((Cooldown - (time - DateTime.Now).TotalSeconds) / Cooldown) * 100;
                    string bar = "";
                    for (int i = 1; i <= 20; i++)
                    {
                        if (i * (100 / 20) > diff)
                            bar += "<color=red>|</color>";
                        else
                            bar += "|";
                    }
                    player.SetGUI("taser", Gamer.Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"Trzymasz <color=yellow>Taser</color><br><mspace=0.5em><color=yellow>[<color=green>{bar}</color>]</color></mspace>");
                    yield return Timing.WaitForSeconds(1f);
                }
                player.SetGUI("taser", Gamer.Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
            }
            /// <inheritdoc/>
            public override void OnStopHolding(Player player, Inventory.SyncItemInfo item)
            {
                player.SetGUI("taser", Gamer.Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
            }
            /// <inheritdoc/>
            public override void OnForceclass(Player player)
            {
                player.SetGUI("taser", Gamer.Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
            }
        }
        /// <inheritdoc/>
        public override string Name => "TaserHandler";
        /// <inheritdoc/>
        public TaserHandler(PluginHandler p) : base(p)
        {
            new TaserItem();
        }
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        private static int Index = 1;
        /// <summary>
        /// Spawns taser in the specified <paramref name="position"/> and returns spawned taser.
        /// </summary>
        /// <param name="position">Position where taser will be spawned</param>
        /// <returns>Spawned taser as <see cref="Pickup"/></returns>
        public static Pickup SpawnTaser(Vector3 position)
        {
            float dur = 501f + (Index++);
            return MapPlus.Spawn(new Inventory.SyncItemInfo
            {
                durability = dur,
                id = ItemType.GunUSP,
            }, position, Quaternion.identity, Size);
        }

        private void Server_RoundStarted()
        {
            Index = 1;
            var initOne = SpawnTaser(Vector3.zero);
            MEC.Timing.CallDelayed(5, () => initOne.Delete());
            var lockers = LockerManager.singleton.lockers.Where(i => i.chambers.Length == 9).ToArray();
            int toSpawn = 1;
            while (toSpawn > 0)
            {
                var locker = lockers[UnityEngine.Random.Range(0, lockers.Length)];
                locker.AssignPickup(SpawnTaser(locker.chambers[UnityEngine.Random.Range(0, locker.chambers.Length)].spawnpoint.position));
                toSpawn--;
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.IsEscaped)
                return;
            if (ev.Player.GetSessionVar<bool>(Main.SessionVarType.ITEM_LESS_CLSSS_CHANGE))
                return;
            float dur = 501f + (Index++);
            if (ev.NewRole == RoleType.FacilityGuard)
            {
                ev.Items.Remove(ItemType.GunUSP);
                MEC.Timing.CallDelayed(.25f, () =>
                {
                    if (ev.Player.Inventory.items.Count >= 8)
                    {
                        MapPlus.Spawn(new Inventory.SyncItemInfo
                        {
                            durability = dur,
                            id = ItemType.GunUSP,
                        }, ev.Player.Position, Quaternion.identity, Size);
                    }
                    else
                        ev.Player.Inventory.AddNewItem(ItemType.GunUSP, dur);
                });
            }
        }
    }
}
