﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using MEC;
using Mirror;
using UnityEngine;
using Gamer.Diagnostics;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using Gamer.Mistaken.Systems.Misc;
using Gamer.API.CustomItem;

namespace Gamer.Mistaken.BetterSCP.SCP1499
{
    public class SCP1499Handler : Module
    {
        private static PluginHandler StaticPlugin;
        public SCP1499Handler(PluginHandler plugin) : base(plugin)
        {
            new SCP1499CustomItem();
            StaticPlugin = plugin;
            plugin.RegisterTranslation("scp1499_info_cooldown", "<color=red>SCP 1499</color> is on cooldown for next <color=yellow>{0}</color> seconds");
            plugin.RegisterTranslation("scp1499_info_ready", "<color=red>SCP 1499</color> is <color=yellow>ready</color>");
            plugin.RegisterTranslation("scp1499_info_first", "<br>[<color=yellow>LMB</color>] <color=yellow>{0} <color=#FF{1}>|</color> {2}</color>");
            plugin.RegisterTranslation("scp1499_info_seconds", "<br>[<color=yellow>RMB</color>] <color=yellow>{0} <color=#FF{1}>|</color> {2}</color>");
            plugin.RegisterTranslation("scp1499_info_decont", "<br><color=#FFFF00{0}>LCZ Decontamination in {1}m {2}s");
            plugin.RegisterTranslation("scp1499_info_rework", "<br><color=#FF000055><b>!</b> SCP 1499 WAS RECENTLY REWORKED <b>!</b></color>");
            plugin.RegisterTranslation("scp1499_info_scp268", "You can't have both <color=yellow>SCP 268</color> and <color=yellow>SCP 1499</color> at the same time");
            plugin.RegisterTranslation("Info_012_Denied", "<b><color=red>Access Denied</color></b><br>This door <color=yellow>require</color> <b>Containment Level 3</b> access");

        }

        public override string Name => nameof(SCP1499Handler);

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.CustomEvents.OnRequestPickItem += this.Handle<Exiled.Events.EventArgs.PickItemRequestEventArgs>((ev) => CustomEvents_OnRequestPickItem(ev));
            Exiled.Events.Handlers.Player.ReceivingEffect += this.Handle<Exiled.Events.EventArgs.ReceivingEffectEventArgs>((ev) => Player_ReceivingEffect(ev));
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Scp079.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Scp079_InteractingDoor(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.CustomEvents.OnRequestPickItem -= this.Handle<Exiled.Events.EventArgs.PickItemRequestEventArgs>((ev) => CustomEvents_OnRequestPickItem(ev));
            Exiled.Events.Handlers.Player.ReceivingEffect -= this.Handle<Exiled.Events.EventArgs.ReceivingEffectEventArgs>((ev) => Player_ReceivingEffect(ev));
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Scp079.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Scp079_InteractingDoor(ev));
        }

        private void Scp079_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (ev.Door.Type() == Exiled.API.Enums.DoorType.Scp012Bottom)
            {
                if (ev.Player.Energy < 110)
                {
                    ev.IsAllowed = false;
                    ev.Player.ReferenceHub.scp079PlayerScript.RpcNotEnoughMana(110, ev.Player.Energy);
                    return;
                }
                ev.Player.Energy -= 110;
                ev.Player.ReferenceHub.scp079PlayerScript.AddExperience(50);
            }
        }

        private void Player_ReceivingEffect(Exiled.Events.EventArgs.ReceivingEffectEventArgs ev)
        {
            if (ev.Effect.GetType() == typeof(CustomPlayerEffects.Flashed) && ev.Player.IsScp)
                ev.IsAllowed = false;
        }

        public class SCP1499CustomItem : CustomItem
        {
            public SCP1499CustomItem() => base.Register();

            public override string ItemName => "SCP-1499";
            public override ItemType Item => ItemType.GrenadeFlash;
            public override int Durability => 149;
            public override Vector3 Size => new Vector3(1.5f, 0.5f, 1.5f);
            public const float CooldownLength = 90;

            public override bool OnThrow(Player player, Inventory.SyncItemInfo item, bool slow)
            {
                if (Warhead.IsDetonated)
                    return true;
                Vector3 Target;
                int Damage = 0;
                bool ReturnFlash = false;
                bool ThrowFlash = false;
                bool EnablePocket = false;
                float SlowPocketTime = 10;
                try
                {
                    if (player.Position.y > -1990)
                    {
                        Target = new Vector3(0, -1996, 0);
                        EnablePocket = true;
                        if (Cooldown.Ticks > DateTime.Now.Ticks)
                            return false;
                    }
                    else
                        Target = (slow ? SecondFlashPosition : FirstFlashPosition) + new Vector3(0, 1, 0);
                    Cooldown = DateTime.Now.AddSeconds(CooldownLength);
                    MEC.Timing.RunCoroutine(Use1499(player, Target, EnablePocket, Damage, ReturnFlash, SlowPocketTime));
                    return ThrowFlash;
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                    return false;
                }
            }

            public override void OnStartHolding(Player player, Inventory.SyncItemInfo item)
            {
                MEC.Timing.RunCoroutine(UpdateFlashCooldown(player));
            }
        }

        private void CustomEvents_OnRequestPickItem(Exiled.Events.EventArgs.PickItemRequestEventArgs ev)
        {
            if (
                (ev.Pickup.ItemId == ItemType.GrenadeFlash && ev.Pickup.durability == 1.149f && ev.Player.Inventory.items.Any(i => i.id == ItemType.SCP268)) ||
                (ev.Pickup.ItemId == ItemType.SCP268 && ev.Player.Inventory.items.Any(i => i.id == ItemType.GrenadeFlash && i.durability == 1.149f))
                )
            {
                ev.IsAllowed = false;
                ev.Player.ShowHintPulsating(plugin.ReadTranslation("scp1499_info_scp268"), 5);
            }
        }

        private static IEnumerator<float> UpdateFlashCooldown(Player player)
        {
            yield return MEC.Timing.WaitForSeconds(0.1f);
            while (player?.CurrentItem.id == ItemType.GrenadeFlash && Round.IsStarted)
            {
                try
                {
                    var cooldown = Math.Round((Cooldown - DateTime.Now).TotalSeconds);
                    string message;
                    if ((cooldown >= 0) && player?.Position.y > -1900)
                        message = StaticPlugin.ReadTranslation("scp1499_info_cooldown", cooldown);
                    else
                        message = StaticPlugin.ReadTranslation("scp1499_info_ready");
                    if (MapPlus.IsLCZDecontaminated(45))
                    {
                        if (FirstFlashRoom?.Zone == ZoneType.LightContainment)
                            FirstFlashRoom = GetFreeRoom(FirstFlashRoom);
                        if (SecondFlashRoom?.Zone == ZoneType.LightContainment)
                            SecondFlashRoom = GetFreeRoom(SecondFlashRoom);
                    }
                    message += StaticPlugin.ReadTranslation("scp1499_info_first", ForceLength(FirstFlashRoom?.Zone.ToString(), 16), GetRoomColor(FirstFlashRoom), ForceLength(FirstFlashRoom?.Type.ToString(), 15));
                    message += StaticPlugin.ReadTranslation("scp1499_info_seconds", ForceLength(SecondFlashRoom?.Zone.ToString(), 16), GetRoomColor(SecondFlashRoom), ForceLength(SecondFlashRoom?.Type.ToString(), 15));
                    if (!MapPlus.IsLCZDecontaminated(out float lczTime))
                        message += StaticPlugin.ReadTranslation("scp1499_info_decont", GetTimeColor(lczTime), ((lczTime - (lczTime % 60)) / 60).ToString("00"), Mathf.RoundToInt(lczTime % 60).ToString("00"));
                    int pulse = ((int)Round.ElapsedTime.TotalSeconds % 8) + 4;
                    //message += plugin?.ReadTranslation("scp1499_info_rework", GetHexChar(pulse) + GetHexChar(pulse));
                    if (RealPlayers.List.Contains(player)) player.ShowHint(message, 2);
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                yield return MEC.Timing.WaitForSeconds(1f);
            }
        }

        private static string ForceLength(string input, int length)
        {
            while (input.Length < length)
                input += " ";
            return input;
        }

        private static string GetTimeColor(float time)
        {
            try
            {
                var input = Math.Round((1 - (time / MapPlus.DecontaminationEndTime)) * 8);
                string tor = "";
                tor += GetHexChar((int)input % 16);
                input /= 16;
                return GetHexChar((int)input % 16) + tor;
            }
            catch
            {
                return "00FF00";
            }
        }
        private static string GetHexChar(int input)
        {
            switch (input)
            {
                case 10:
                    return "A";
                case 11:
                    return "B";
                case 12:
                    return "C";
                case 13:
                    return "D";
                case 14:
                    return "E";
                case 15:
                    return "F";
                default:
                    return input.ToString();
            }
        }

        private static string GetRoomColor(Room room)
        {
            try
            {
                if (room?.Players?.Any(p => p?.IsAlive ?? false) ?? true)
                    return "0000";
                float nearest = 9999;
                foreach (var player in RealPlayers.List.Where(p => p?.IsAlive ?? false))
                {
                    float distance = Vector3.Distance(player.Position, room.Position);
                    if (distance < nearest)
                        nearest = distance;
                }

                if (nearest > 25)
                    return "FFFF";
                else if (nearest > 15)
                    return "b5b5";
                else
                    return "6666";
            }
            catch
            {
                return "00FF00";
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            Used1499.RemoveAll(p => p.UserId == ev.Player.UserId);
            if (ev.IsEscaped || ev.NewRole == RoleType.Spectator)
            {
                if (ev.Player.Inventory.items.Any(i => i.id == ItemType.GrenadeFlash && i.durability == 1.149f))
                {
                    ev.Player.RemoveItem(ev.Player.Inventory.items.First(i => i.id == ItemType.GrenadeFlash));
                    var pos = ev.Player.Position;
                    MEC.Timing.CallDelayed(1, () =>
                    {
                        MapPlus.Spawn(new Inventory.SyncItemInfo
                        {
                            id = ItemType.GrenadeFlash,
                            durability = 1.149f,
                        }, ev.Player.Role == RoleType.Spectator ? pos : ev.Player.Position, Quaternion.identity, new Vector3(1.5f, 0.5f, 1.5f));
                    });
                }
            }
        }

        private void Server_RoundStarted()
        {
            Vector3 positionToSpawn;
            Vector3 s012 = new Vector3(0, 1002, 0);
            foreach (var door in Map.Doors)
            {
                if (door.Type() == DoorType.Scp012Bottom)
                {
                    var deoe = door.GetComponent<DoorEventOpenerExtension>();
                    if (door != null)
                        GameObject.Destroy(deoe);
                    var door012 = door as BreakableDoor;
                    door.RequiredPermissions.RequiredPermissions = KeycardPermissions.ContainmentLevelThree;
                    door012._ignoredDamageSources = DoorDamageType.Grenade | DoorDamageType.Scp096 | DoorDamageType.Weapon;
                    s012 = door.transform.position + (door.transform.right * 1.5f) + (door.transform.forward * 8.5f);
                    break;
                }
            }

            positionToSpawn = s012 + Vector3.up;
            MEC.Timing.CallDelayed(5, () =>
            {
                var tmp = ItemType.GrenadeFlash.Spawn(1.149f, Vector3.zero);
                MEC.Timing.CallDelayed(5, () => tmp.Delete());
                MapPlus.Spawn(new Inventory.SyncItemInfo
                {
                    id = ItemType.GrenadeFlash,
                    durability = 1.149f,
                }, positionToSpawn, Quaternion.identity, new Vector3(1.5f, 0.5f, 1.5f));
            });


            Rooms = MapPlus.Rooms.ToArray();
            FirstFlashRoom = GetFreeRoom(null);
            SecondFlashRoom = GetFreeRoom(null);
        }

        private void Server_RestartingRound()
        {
            FirstFlashRoom = null;
            SecondFlashRoom = null;
            Used1499.Clear();
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (ev.Door.Type() == DoorType.Scp012Bottom && !ev.Player.IsBypassModeEnabled && ev.Player.Role != RoleType.Scp079)
            {
                var currentItemType = ev.Player.CurrentItem.id;
                if (!currentItemType.IsKeycard() || !(currentItemType == ItemType.KeycardO5 || currentItemType == ItemType.KeycardFacilityManager || currentItemType == ItemType.KeycardContainmentEngineer))
                {
                    ev.IsAllowed = false;
                    ev.Player.ShowHintPulsating(plugin.ReadTranslation("Info_012_Denied"), 2f, true, true);
                }
            }
        }

        private static Vector3 FirstFlashPosition => FirstFlashRoom?.Position ?? new Vector3();
        private static Vector3 SecondFlashPosition => SecondFlashRoom?.Position ?? new Vector3();
        private static Room FirstFlashRoom;
        private static Room SecondFlashRoom;

        private static IEnumerator<float> Use1499(Player player, Vector3 pos, bool pocket, int damage, bool returnFlash, float slowTimePocket)
        {
            yield return MEC.Timing.WaitForSeconds(1f);
            if (!player.IsConnected)
                yield break;
            ReferenceHub rh = player.ReferenceHub;
            var pec = rh.playerEffectsController;
            try
            {
                player.Position = pos;
                if (pocket)
                {
                    pec.GetEffect<CustomPlayerEffects.Corroding>().IsInPd = true;
                    player.EnableEffect<CustomPlayerEffects.Corroding>();
                }
                else
                {
                    if (slowTimePocket == 0)
                    {
                        pec.GetEffect<CustomPlayerEffects.Corroding>().IsInPd = false;
                        player.DisableEffect<CustomPlayerEffects.Corroding>();
                    }
                    player.EnableEffect<CustomPlayerEffects.Flashed>(10);
                    player.EnableEffect<CustomPlayerEffects.Blinded>(15);
                    player.EnableEffect<CustomPlayerEffects.Deafened>(15);
                    player.EnableEffect<CustomPlayerEffects.Concussed>(30);

                    FirstFlashRoom = GetFreeRoom(FirstFlashRoom);
                    SecondFlashRoom = GetFreeRoom(SecondFlashRoom);

                    Achievement(player);
                }
                if (damage != 0)
                    player.Health -= damage;
            }
            catch { }
            if (returnFlash)
                player.AddItem(ItemType.GrenadeFlash);
            if (slowTimePocket != 0 && !pocket)
            {
                yield return MEC.Timing.WaitForSeconds(slowTimePocket);
                try
                {
                    if (!player.IsInPocketDimension)
                    {
                        pec.GetEffect<CustomPlayerEffects.Corroding>().IsInPd = false;
                        player.DisableEffect<CustomPlayerEffects.Corroding>();
                    }
                }
                catch { }
            }
        }

        private static readonly List<Player> Used1499 = new List<Player>();
        private static void Achievement(Player player)
        {
            try
            {
                Used1499.Add(player);
                CustomAchievements.RoundEventHandler.AddProggress("IsItSafe", player);
                int amount = Used1499.Where(item => item.UserId == player.UserId).Count();
                if (amount == 2)
                    CustomAchievements.RoundEventHandler.ForceLevel("Use1499", player, CustomAchievements.CustomAchievements.Level.BRONZE);
                else if (amount == 5)
                    CustomAchievements.RoundEventHandler.ForceLevel("Use1499", player, CustomAchievements.CustomAchievements.Level.SILVER);
                else if (amount == 8)
                    CustomAchievements.RoundEventHandler.ForceLevel("Use1499", player, CustomAchievements.CustomAchievements.Level.GOLD);
                else if (amount == 10)
                    CustomAchievements.RoundEventHandler.ForceLevel("Use1499", player, CustomAchievements.CustomAchievements.Level.DIAMOND);
                else if (amount == 12)
                    CustomAchievements.RoundEventHandler.ForceLevel("Use1499", player, CustomAchievements.CustomAchievements.Level.EXPERT);
            }
            catch (System.NullReferenceException e)
            {
                Log.Error(e.Message);
                Log.Error(e.Source);
                Log.Error(e.StackTrace);
            }
        }

        public static DateTime Cooldown = new DateTime();
        private static readonly HashSet<RoomType> DisallowedRoomTypes = new HashSet<RoomType>
        {
            RoomType.EzShelter,
            RoomType.EzCollapsedTunnel,
            RoomType.HczTesla,
            RoomType.Lcz173,
            RoomType.Hcz939,
            RoomType.LczArmory,
            RoomType.Pocket,
        };

        private static Room[] Rooms;
        private static Room RandomRoom => Rooms[UnityEngine.Random.Range(0, Rooms.Length)] ?? RandomRoom;

        private static Room GetFreeRoom(Room current)
        {
            Room targetRoom = RandomRoom;
            int trie = 0;
            while (!IsRoomOK(targetRoom) || (current?.Position == targetRoom.Position || FirstFlashPosition == targetRoom.Position || SecondFlashPosition == targetRoom.Position || (current != FirstFlashRoom && FirstFlashRoom?.Zone == targetRoom?.Zone) || (current != SecondFlashRoom && SecondFlashRoom?.Zone == targetRoom?.Zone)))
            {
                targetRoom = RandomRoom;
                trie++;
                if (trie >= 1000)
                {
                    Log.Error("Failed to generate teleport position in 1000 tries");
                    return Map.Rooms.First(r => r.Type == RoomType.Surface);
                }
            }

            Log.Debug($"New position is {targetRoom.Position} | {targetRoom.Zone}");
            return targetRoom;
        }
        private static bool IsRoomOK(Room room)
        {
            if (room == null)
                return false;
            if (DisallowedRoomTypes.Contains(room.Type))
                return false;
            if (MapPlus.IsLCZDecontaminated(60) && room.Zone == ZoneType.LightContainment)
                return false;
            if (!UnityEngine.Physics.Raycast(room.Position + Vector3.up / 2, Vector3.down, 5))
                return false;
            return true;
        }
    }
}
