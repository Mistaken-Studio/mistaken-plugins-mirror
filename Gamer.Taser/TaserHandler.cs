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
using Exiled.API.Extensions;
using Gamer.Diagnostics;
using Gamer.API.CustomItem;
using UnityEngine;

namespace Gamer.Taser
{
    public class TaserHandler : Module
    {
        internal static readonly Vector3 Size = new Vector3(.75f, .75f, .75f);
        public class TaserItem : CustomItem
        {
            public TaserItem() => base.Register();
            public override string ItemName => "Taser";
            public override ItemType Item => ItemType.GunUSP;
            public override int Durability => 501;
            public override Vector3 Size => TaserHandler.Size;
            public override void OnStartHolding(Player player, Inventory.SyncItemInfo item)
            {
                Timing.RunCoroutine(UpdateInterface(player));
            }

            public override bool OnReload(Player player, Inventory.SyncItemInfo item)
            {
                return false;
            }

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
                        if (targetPlayer.IsHuman)
                        {
                            targetPlayer.EnableEffect<CustomPlayerEffects.Ensnared>(2);
                            targetPlayer.EnableEffect<CustomPlayerEffects.Flashed>(5);
                            targetPlayer.EnableEffect<CustomPlayerEffects.Deafened>(10);
                            targetPlayer.EnableEffect<CustomPlayerEffects.Blinded>(10);
                            targetPlayer.EnableEffect<CustomPlayerEffects.Amnesia>(5);
                            if (targetPlayer.CurrentItemIndex != -1)
                                targetPlayer.DropItem(targetPlayer.CurrentItem);
                        }
                        else
                        {
                            switch (targetPlayer.Role)
                            {
                                case RoleType.Scp049:
                                case RoleType.Scp0492:
                                case RoleType.Scp93953:
                                case RoleType.Scp93989:
                                    targetPlayer.EnableEffect<CustomPlayerEffects.Ensnared>(2);
                                    targetPlayer.EnableEffect<CustomPlayerEffects.Flashed>(5);
                                    targetPlayer.EnableEffect<CustomPlayerEffects.Deafened>(10);
                                    targetPlayer.EnableEffect<CustomPlayerEffects.Blinded>(10);
                                    break;
                                default:
                                    break;
                            }
                        }
                        targetPlayer.Broadcast("<color=yellow>Taser</color>", 10, $"<color=yellow>You have been tased by: {player.Nickname} [{player.Role}]</color>");
                        targetPlayer.SendConsoleMessage($"You have been tased by: {player.Nickname} [{player.Role}]", "yellow");
                        player.ReferenceHub.weaponManager.RpcConfirmShot(true, player.ReferenceHub.weaponManager.curWeapon);
                    }
                    else
                        player.ReferenceHub.weaponManager.RpcConfirmShot(false, player.ReferenceHub.weaponManager.curWeapon);
                }
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

        public override string Name => "TaserHandler";
        public TaserHandler(PluginHandler p) : base(p)
        {
            new TaserItem();
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        private static int Index = 1;
        public static Pickup SpawnTaser(Vector3 pos)
        {
            float dur = 1.501f + (Index++) / 1000000f;
            return MapPlus.Spawn(new Inventory.SyncItemInfo
            {
                durability = dur,
                id = ItemType.GunUSP,
            }, pos, Quaternion.identity, Size);
        }

        private void Server_RoundStarted()
        {
            Index = 1;
            var initOne = SpawnTaser(Vector3.zero);
            MEC.Timing.CallDelayed(5, () => initOne.Delete());
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
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
