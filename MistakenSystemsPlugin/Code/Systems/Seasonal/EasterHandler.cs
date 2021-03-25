using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Seasonal
{
    internal class EasterHandler : Module
    {
        public EasterHandler(PluginHandler p) : base(p)
        {
            new Egg(1, RoomType.LczPlants, new Vector3(-5, 1.5f, 0));
            new Egg(2, RoomType.LczCafe, new Vector3(2, 3, -10));
            new Egg(3, RoomType.LczToilets, new Vector3(9.25f, 3, 0.5f));
            new Egg(4, RoomType.Lcz173, new Vector3(5, 17.5f, 3.5f));
            new Egg(5, RoomType.LczGlassBox, new Vector3(8, 5, -11));
            new Egg(6, RoomType.Lcz012, new Vector3(3, -5, 4));
            new Egg(7, RoomType.LczArmory, new Vector3(-4.2f, 1, -0.4f));
            new Egg(8, RoomType.Hcz939, new Vector3(9.8f, -15, 7.8f));
            new Egg(9, RoomType.Hcz049, new Vector3(9, 266, 6));
            new Egg(10, RoomType.HczServers, new Vector3(4, -3, 3.8f));
            new Egg(11, RoomType.HczNuke, new Vector3(-14.5f, 405, -5.5f));
            new Egg(12, RoomType.Hcz096, new Vector3(-3, 1, 5.8f));
            new Egg(13, RoomType.Hcz106, new Vector3(-4, -12, -33.5f));
            new Egg(14, RoomType.Hcz079, new Vector3(5.5f, -5, -1.5f));
            new Egg(15, RoomType.EzPcs, new Vector3(-9.7f, 2, 3));
            new Egg(16, RoomType.EzShelter, new Vector3(-9, 1, -2.5f));
            new Egg(17, RoomType.EzDownstairsPcs, new Vector3(1, -1, -2));
            new Egg(18, RoomType.EzUpstairsPcs, new Vector3(-9.5f, 5, -10));
            new Egg(19, RoomType.EzIntercom, new Vector3(-0.5f -7, 7.4f));
            new Egg(20, RoomType.Surface, new Vector3(48.5f, -10, 56.5f));
        }

        public override string Name => "Easter";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.CustomEvents.OnRequestPickItem += this.Handle<Exiled.Events.EventArgs.PickItemRequestEventArgs>((ev) => CustomEvents_OnRequestPickItem(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.CustomEvents.OnRequestPickItem -= this.Handle<Exiled.Events.EventArgs.PickItemRequestEventArgs>((ev) => CustomEvents_OnRequestPickItem(ev));
        }

        public static readonly string FilePath = Path.Combine(Paths.Plugins, "Easter");

        private void CustomEvents_OnRequestPickItem(Exiled.Events.EventArgs.PickItemRequestEventArgs ev)
        {
            if (!active)
                return;
            if (!Eggs.TryGetValue(ev.Pickup, out Egg egg))
                return;
            ev.IsAllowed = false;

            egg.OnPickup(ev.Player);
        }
        public static readonly List<Egg> EggsList = new List<Egg>();
        public static readonly Dictionary<Pickup, Egg> Eggs = new Dictionary<Pickup, Egg>();
        public static readonly Dictionary<byte, HashSet<string>> AlreadyFound = new Dictionary<byte, HashSet<string>>();

        private static readonly DateTime EnableDate =  new DateTime(2021, 4, 1);
        private static readonly DateTime DisableDate = new DateTime(2021, 4, 7);
        private bool active = false;
        private void Server_WaitingForPlayers()
        {
            active = false;
            if (Server.Port == 7791)
                active = true;
            else
            {
                var now = DateTime.Now;
                if (EnableDate <= now && DisableDate >= now)
                    active = true;
                if (!active)
                    return;
            }
            
            foreach (var egg in EggsList)
                egg.Spawn();
        }

        public class Egg
        {
            public byte Id;

            public RoomType Room;
            public Vector3 Offset;

            public Egg(byte id, RoomType room, Vector3 offset)
            {
                this.Id = id;
                this.Room = room;
                this.Offset = offset;
                AlreadyFound[Id] = new HashSet<string>();
                EggsList.Add(this);
                if (!File.Exists(MyPath))
                    File.Create(MyPath).Close();
            }
            private static readonly Vector3 Size = new Vector3(1, 1, 1);
            public void Spawn()
            {
                Log.Debug($"Spawning {this.Id}");
                var room = Map.Rooms.First(r => r.Type == this.Room);
                var basePos = room.Position;
                var offset = this.Offset;
                offset = room.transform.forward * -offset.x + room.transform.right * -offset.z + Vector3.up * offset.y;
                basePos += offset;
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
                
                gameObject.transform.localScale = Size;
                MEC.Timing.CallDelayed(2, () =>
                {
                    gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                    Mirror.NetworkServer.Spawn(gameObject);
                });
                var pickup = gameObject.GetComponent<Pickup>();
                pickup.SetupPickup(ItemType.SCP018, 0, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), basePos, Quaternion.identity);
                Eggs[pickup] = this;
            }
            private string MyPath => Path.Combine(FilePath, $"{this.Id}.txt");
            public void OnPickup(Player player)
            {
                if (AlreadyFound[this.Id].Contains(player.UserId))
                    player.ShowHintPulsating($"Już znalazłeś to <color=yellow>jajko({this.Id})</color>, szukaj dalej :)", 5, false, false);
                else
                {
                    foreach (var line in File.ReadAllLines(MyPath))
                        AlreadyFound[this.Id].Add(line);
                    if (AlreadyFound[this.Id].Contains(player.UserId))
                        player.ShowHintPulsating($"Już znalazłeś to <color=yellow>jajko({this.Id})</color>, szukaj dalej :)", 5, false, false);
                    else
                    {
                        player.ShowHintPulsating($"Brawo, znalazłeś <color=yellow>jajko({this.Id})</color>", 5, false, false);
                        EVO.Handler.AddProgres(2000, player.UserId);
                        File.AppendAllText(MyPath, $"{player.UserId}\n");
                    }
                }
            }
        }
    }
}
