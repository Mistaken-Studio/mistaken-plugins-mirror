﻿using Exiled.API.Enums;
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
using Exiled.API.Extensions;
using Gamer.Diagnostics;
using Gamer.API.CustomItem;
using UnityEngine;
using Gamer.RoundLoggerSystem;

namespace Gamer.Taser
{
    /// <inheritdoc/>
    public class TaserHandler : Module
    {
        internal static readonly Vector3 Size = new Vector3(.75f, .75f, .75f);
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
            public override int Durability => 501;
            /// <inheritdoc/>
            public override Vector3 Size => TaserHandler.Size;
            /// <inheritdoc/>
            public override void OnStartHolding(Player player, Inventory.SyncItemInfo item)
            {
                Timing.RunCoroutine(UpdateInterface(player));
            }
            /// <inheritdoc/>
            public override void Spawn(Vector3 position)
            {
                float dur = 1.501f + (Index++) / 1000000f;
                MapPlus.Spawn(new Inventory.SyncItemInfo
                {
                    durability = dur,
                    id = ItemType.GunUSP,
                }, position, Quaternion.identity, Size);
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
            public override bool OnShoot(Player player, Inventory.SyncItemInfo item, GameObject target)
            {
                int dur = (int)this.GetInternalDurability(item);
                if(!Cooldowns.TryGetValue(dur, out DateTime time))
                    Cooldowns.Add(dur, DateTime.Now);
                if (DateTime.Now < time)
                    player.ShowHint("You have <color=yellow>no ammo</color>", true, 3, true);
                else
                {
                    Cooldowns[dur] = DateTime.Now.AddSeconds(Cooldown);
                    var targetPlayer = Player.Get(target);
                    if(targetPlayer != null)
                    {
                        player.ReferenceHub.weaponManager.RpcConfirmShot(true, player.ReferenceHub.weaponManager.curWeapon);
                        if (targetPlayer.GetSessionVar<bool>(Main.SessionVarType.CI_LIGHT_ARMOR) || targetPlayer.GetSessionVar<bool>(Main.SessionVarType.CI_ARMOR) || targetPlayer.GetSessionVar<bool>(Main.SessionVarType.CI_HEAVY_ARMOR))
                        {
                            RoundLogger.Log("TASER", "BLOCKED", $"{player.PlayerToString()} hit {targetPlayer.PlayerToString()} but the shot was blocked by an armor");
                            return false;
                        }
                        if (targetPlayer.IsHuman)
                        {
                            targetPlayer.EnableEffect<CustomPlayerEffects.Ensnared>(2);
                            targetPlayer.EnableEffect<CustomPlayerEffects.Flashed>(5);
                            targetPlayer.EnableEffect<CustomPlayerEffects.Deafened>(10);
                            targetPlayer.EnableEffect<CustomPlayerEffects.Blinded>(10);
                            targetPlayer.EnableEffect<CustomPlayerEffects.Amnesia>(5);
                            if (targetPlayer.CurrentItemIndex != -1)
                                targetPlayer.DropItem(targetPlayer.CurrentItem);
                            RoundLogger.Log("TASER", "HIT", $"{player.PlayerToString()} hit {targetPlayer.PlayerToString()}");
                            targetPlayer.Broadcast("<color=yellow>Taser</color>", 10, $"<color=yellow>You have been tased by: {player.Nickname} [{player.Role}]</color>");
                            targetPlayer.SendConsoleMessage($"You have been tased by: {player.Nickname} [{player.Role}]", "yellow");
                            return false;
                        }
                    }
                    else
                        player.ReferenceHub.weaponManager.RpcConfirmShot(false, player.ReferenceHub.weaponManager.curWeapon);
                }
                RoundLogger.Log("TASER", "HIT", $"{player.PlayerToString()} shot but didn't hit anyone");
                return false;
            }

            private const int Cooldown = 90;
            private readonly Dictionary<int, DateTime> Cooldowns = new Dictionary<int, DateTime>();
            private IEnumerator<float> UpdateInterface(Player player)
            {
                yield return Timing.WaitForSeconds(0.5f);
                while (player.CurrentItem.id == ItemType.GunUSP)
                {
                    if (!(player.CurrentItem.durability >= 1.501 && player.CurrentItem.durability <= 1.5011))
                        break;
                    int dur = (int)this.GetInternalDurability(player.CurrentItem);
                    if (!Cooldowns.TryGetValue(dur, out DateTime time))
                    {
                        Cooldowns.Add(dur, DateTime.Now);
                        time = DateTime.Now;
                    }
                    var diff = ((Cooldown - (time - DateTime.Now).TotalSeconds) / Cooldown) * 100;
                    string bar = "";
                    for (int i = 1; i <= 20; i++)
                    {
                        if (i * (100/20) > diff)
                            bar += "<color=red>|</color>";
                        else
                            bar += "|";
                    }
                    player.ShowHint($"<voffset=-20em><mspace=0.5em><color=yellow>[<color=green>{bar}</color>]</color></mspace></voffset>", false, 2, false);
                    yield return Timing.WaitForSeconds(0.5f);
                }
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
            float dur = 1.501f + (Index++) / 1000000f;
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
            while(toSpawn > 0)
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
            float dur = 1.501f + (Index++) / 1000000f;
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
