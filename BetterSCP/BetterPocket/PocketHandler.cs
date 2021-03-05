using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using Mirror;
using UnityEngine;

namespace Gamer.Mistaken.BetterSCP.Pocket
{
    class PocketHandler : Module
    {
        public PocketHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => nameof(PocketHandler);
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Dying += this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.DroppingItem += this.Handle<Exiled.Events.EventArgs.DroppingItemEventArgs>((ev) => Player_DroppingItem(ev));
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.EscapingPocketDimension += this.Handle<Exiled.Events.EventArgs.EscapingPocketDimensionEventArgs>((ev) => Player_EscapingPocketDimension(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            //Exiled.Events.Handlers.CustomEvents.OnTransmitPositionData += this.Handle<Exiled.Events.EventArgs.TransmitPositionEventArgs>((ev) => CustomEvents_OnTransmitPositionData(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Dying -= this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.DroppingItem -= this.Handle<Exiled.Events.EventArgs.DroppingItemEventArgs>((ev) => Player_DroppingItem(ev));
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.EscapingPocketDimension -= this.Handle<Exiled.Events.EventArgs.EscapingPocketDimensionEventArgs>((ev) => Player_EscapingPocketDimension(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            //Exiled.Events.Handlers.CustomEvents.OnTransmitPositionData -= this.Handle<Exiled.Events.EventArgs.TransmitPositionEventArgs>((ev) => CustomEvents_OnTransmitPositionData(ev));
        }

        public static List<int> InPocket { get; } = new List<int>();

        private Room[] Rooms;
        private Room RandomRoom
        {
            get
            {
                if (Rooms == null)
                    SetRooms();
                return Rooms[UnityEngine.Random.Range(0, Rooms.Length)] ?? RandomRoom;
            }
        }

        private void SetRooms()
        {
            Rooms = MapPlus.Rooms.Where(r => !DisallowedRoomTypes.Contains(r.Type) && r != null).ToArray();
        }

        private static readonly RoomType[] DisallowedRoomTypes = new RoomType[]
        {
            RoomType.EzShelter,
            RoomType.EzCollapsedTunnel,
            RoomType.HczTesla,
            RoomType.Lcz173,
            RoomType.Hcz939,
            RoomType.Pocket,
        };

        /*public static readonly Vector3 GhostPos = Vector3.up * 6000;
        private void CustomEvents_OnTransmitPositionData(Exiled.Events.EventArgs.TransmitPositionEventArgs ev)
        {
            if (ev.Player.IsHuman && ev.Player.Position.y < -1900)
            {
                for (int i = 0; i < ev.PositionMessages.Length; i++)
                    ev.PositionMessages[i] = new PlayerPositionData(GhostPos, 0, ev.PositionMessages[i].playerID);
            }
        }*/

        private void Server_RoundStarted()
        {
            Log.Info("Setting Rooms");
            SetRooms();
        }

        private void Player_EscapingPocketDimension(Exiled.Events.EventArgs.EscapingPocketDimensionEventArgs ev)
        {
            if (Rooms == null)
                return;
            int trie = 0;
            bool forceNext = false;
            Room targetRoom = RandomRoom;
            var position = targetRoom.Position + Vector3.up * 2;
            while (!IsRoomOK(targetRoom) || forceNext)
            {
                forceNext = false;
                targetRoom = RandomRoom;
                position = targetRoom.Position + Vector3.up * 2;
                trie++;
                if (trie >= 1000)
                {
                    position = ev.TeleportPosition;
                    targetRoom = null;
                    Log.Error("Failed to generate pocket exit position in 1000 tries");
                    break;
                }
            }
            Log.Debug($"Teleported {ev.Player?.Nickname} to {position} | {targetRoom?.Type} | {targetRoom?.Zone}");
            ev.Player.SendConsoleMessage($"[BETTER POCKET] Teleported to {position} | {targetRoom?.Type} | {targetRoom?.Zone}", "yellow");
            ev.TeleportPosition = position;
            ev.IsAllowed = false;
            ev.Player.Position = ev.TeleportPosition;
            var pec = ev.Player.ReferenceHub.playerEffectsController;
            pec.EnableEffect<CustomPlayerEffects.Flashed>(2);
            pec.EnableEffect<CustomPlayerEffects.Blinded>(5);
            pec.EnableEffect<CustomPlayerEffects.Deafened>(10);
            pec.EnableEffect<CustomPlayerEffects.Concussed>(10);
            InPocket.Remove(ev.Player.Id);
            if (global::PocketDimensionTeleport.RefreshExit)
                MapGeneration.ImageGenerator.pocketDimensionGenerator.GenerateRandom();
            else
                Log.Debug("Randomizing Pocket Exits disabled");
        }

        private bool IsRoomOK(Room room)
        {
            if (DisallowedRoomTypes.Contains(room.Type))
                return false;
            if (MapPlus.IsLCZDecontaminated(60) && room.Zone == ZoneType.LightContainment)
                return false;
            if (!UnityEngine.Physics.Raycast(room.Position, Vector3.down, 5))
                return false;
            return true;
        }

        private void Player_DroppingItem(Exiled.Events.EventArgs.DroppingItemEventArgs ev)
        {
            if (ev.Player.Position.y < -1900)
                ev.IsAllowed = false;
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.Target.Position.y < -1900 && ev.HitInformations.GetDamageType() != DamageTypes.RagdollLess)
            {
                if (ev.Target.Health <= ev.Amount)
                {
                    OnKilledINPocket(ev.Target);
                    ev.IsAllowed = false;
                }
            }
        }

        private void Server_WaitingForPlayers()
        {
            InPocket.Clear();
            Manager = GameObject.FindObjectOfType<RagdollManager>();
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (InPocket.Contains(ev.Target.Id))
                InPocket.Remove(ev.Target.Id);

            if (ev.Target.Position.y < -1900)
                ThrowItems(ev.Target);
        }

        private static void ThrowItems(Player player)
        {
            var items = player.Inventory.items;
            player.Ammo[(int)AmmoType.Nato556] = 0;
            player.Ammo[(int)AmmoType.Nato762] = 0;
            player.Ammo[(int)AmmoType.Nato9] = 0;
            foreach (var item in items.ToArray())
            {
                try
                {
                    GameObject gameObject = GameObject.Instantiate(player.Inventory.pickupPrefab);
                    NetworkServer.Spawn(gameObject);
                    gameObject.GetComponent<Pickup>().SetupPickup(
                        item.id,
                        item.durability,
                        player.GameObject,
                        new Pickup.WeaponModifiers(false, item.modSight, item.modBarrel, item.modOther),
                        Map.Rooms[UnityEngine.Random.Range(0, Map.Rooms.Count)].Position + new Vector3(0, 2, 0),
                        Quaternion.identity
                    );
                }
                catch (System.Exception e)
                {
                    Log.Error(e.Message);
                    Log.Error(e.StackTrace);
                }
            }
            player.ClearInventory();
        }

        private static RagdollManager Manager;
        internal static void OnKilledINPocket(Player player)
        {
            ThrowItems(player);
            try
            {
                try
                {
                    foreach (var p in Player.Get(RoleType.Scp106))
                        Mistaken.CustomAchievements.RoundEventHandler.AddProggress("OldMan", p);
                }
                catch(System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                
                Manager.SpawnRagdoll(
                    Map.Rooms[UnityEngine.Random.Range(0, Map.Rooms.Count)].Position + new Vector3(0, 3, 0),
                    Quaternion.identity,
                    Vector3.down * 20,
                    (int)player.Role,
                    new PlayerStats.HitInfo(99999, "WORLD", DamageTypes.Pocket, -1),
                    false,
                    player.UserId,
                    player.Nickname,
                    player.Id
                );
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }
            player.Kill(DamageTypes.RagdollLess);
        }
    }
}
