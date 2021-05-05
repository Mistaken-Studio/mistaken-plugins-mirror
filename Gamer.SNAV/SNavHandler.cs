using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Utilities;
using MEC;
using Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.SNAV
{
    /// <inheritdoc/>
    public class SNavHandler : Module
    {
        /// <summary>
        /// Enables Expereimental Feature
        /// </summary>
        public static readonly bool HideTablet = false;
        /// <inheritdoc/>
        public class SNavClasicItem : API.CustomItem.CustomItem
        {
            /// <inheritdoc/>
            public SNavClasicItem() => base.Register();
            /// <inheritdoc/>
            public override string ItemName => "SNav-3000";
            /// <inheritdoc/>
            public override ItemType Item => ItemType.WeaponManagerTablet;
            /// <inheritdoc/>
            public override Main.SessionVarType SessionVarType => Main.SessionVarType.CI_SNAV;
            /// <inheritdoc/>
            public override int Durability => 301;
            /// <inheritdoc/>
            public override Vector3 Size => new Vector3(2.0f, .50f, .50f);
            /// <inheritdoc/>
            public override Upgrade[] Upgrades => new Upgrade[]
            {
                new Upgrade
                {
                    Input = ItemType.WeaponManagerTablet,
                    Durability = 0,
                    Chance = 100,
                    KnobSetting = Scp914Knob.OneToOne
                }
            };
            /// <inheritdoc/>
            public override void OnStartHolding(Player player, Inventory.SyncItemInfo item)
            {
                Gamer.Mistaken.Base.GUI.PseudoGUIHandler.Ignore(player);
                RequireUpdate.Add(player);
                UpdateInterface(player);
                Timing.RunCoroutine(IUpdateInterface(player));
            }
            /// <inheritdoc/>
            public override void OnStopHolding(Player player, Inventory.SyncItemInfo item)
            {
                player.ShowHint("", 1); //Clear Hints
                Gamer.Mistaken.Base.GUI.PseudoGUIHandler.StopIgnore(player);
                UpdateVisibility(player, true);
            }
            /// <inheritdoc/>
            public override Pickup OnUpgrade(Pickup pickup, Scp914Knob setting)
            {
                if (setting == Scp914Knob.Fine || setting == Scp914Knob.VeryFine)
                    pickup.durability = 1.401f;
                return pickup;
            }
        }
        /// <inheritdoc/>
        public class SNavUltimateItem : API.CustomItem.CustomItem
        {
            /// <inheritdoc/>
            public SNavUltimateItem() => base.Register();
            /// <inheritdoc/>
            public override string ItemName => "SNav-Ultimate";
            /// <inheritdoc/>
            public override ItemType Item => ItemType.WeaponManagerTablet;
            /// <inheritdoc/>
            public override Main.SessionVarType SessionVarType => Main.SessionVarType.CI_SNAV;
            /// <inheritdoc/>
            public override int Durability => 401;
            /// <inheritdoc/>
            public override Vector3 Size => new Vector3(2.5f, .75f, .75f);
            /// <inheritdoc/>
            public override void OnStartHolding(Player player, Inventory.SyncItemInfo item)
            {
                Gamer.Mistaken.Base.GUI.PseudoGUIHandler.Ignore(player);
                RequireUpdate.Add(player);
                UpdateInterface(player);
                Timing.RunCoroutine(IUpdateInterface(player));
            }
            /// <inheritdoc/>
            public override void OnStopHolding(Player player, Inventory.SyncItemInfo item)
            {
                player.ShowHint("", 1); //Clear Hints
                Gamer.Mistaken.Base.GUI.PseudoGUIHandler.StopIgnore(player);
                UpdateVisibility(player, true);
            }
        }
        /// <summary>
        /// Rooms in LCZ
        /// </summary>
        public static Room[,] LCZRooms { get; private set; } = new Room[0, 0];
        /// <summary>
        /// Rooms in EZ and HCZ
        /// </summary>
        public static Room[,] EZ_HCZRooms { get; private set; } = new Room[0, 0];
        /// <summary>
        /// ClassD room rotation
        /// </summary>
        public static Rotation OffsetClassD = Rotation.UP;
        /// <summary>
        /// EZ Checkpoint rotation
        /// </summary>
        public static Rotation OffsetCheckpoint = Rotation.UP;

        /// <summary>
        /// Rooms looks
        /// </summary>
        public static readonly Dictionary<SNavRoomType, string[]> Presets = new Dictionary<SNavRoomType, string[]>()
        {
            {
                SNavRoomType.ERROR,
                new string[]
                {
                    "#########",
                    "#########",
                    "#########",
                }
            },
            {
                SNavRoomType.NONE,
                new string[]
                {
                    "         ",
                    "         ",
                    "         ",
                }
            },
            {
                SNavRoomType.HS_TB,
                new string[]
                {
                    "   | |   ",
                    "   | |   ",
                    "   | |   "
                }
            },
            {
                SNavRoomType.HS_LR,
                new string[]
                {
                    "_________",
                    "_________",
                    "         "
                }
            },
            {
                SNavRoomType.HC_LT,
                new string[]
                {
                    "___| |   ",
                    "_____|   ",
                    "         ",
                }
            },
            {
                SNavRoomType.HC_RT,
                new string[]
                {
                    "   | |___",
                    "   |_____",
                    "         "
                }
            },
            {
                SNavRoomType.HC_LB,
                new string[]
                {
                    "_____.   ",
                    "___. |   ",
                    "   | |   ",
                }
            },
            {
                SNavRoomType.HC_RB,
                new string[]
                {
                    "   ._____",
                    "   | .___",
                    "   | |   ",
                }
            },
            {
                SNavRoomType.IT_RL_T,
                new string[]
                {
                    "___| |___",
                    "_________",
                    "         "
                }
            },
            {
                SNavRoomType.IT_RL_B,
                new string[]
                {
                    "_________",
                    "___. .___",
                    "   | |   "
                }
            },
            {
                SNavRoomType.IT_TB_L,
                new string[]
                {
                    "___| |   ",
                    "___. |   ",
                    "   | |   "
                }
            },
            {
                SNavRoomType.IT_TB_R,
                new string[]
                {
                    "   | |___",
                    "   | .___",
                    "   | |   "
                }
            },
            {
                SNavRoomType.IX,
                new string[]
                {
                    "___| |___",
                    "___. .___",
                    "   | |   "
                }
            },
            {
                SNavRoomType.END_T,
                new string[]
                {
                    ".__| |__.",
                    "|  END  |",
                    "|_______|"
                }
            },
            {
                SNavRoomType.END_B,
                new string[]
                {
                    "._______.",
                    "|  END  |",
                    "|__. .__|"
                }
            },
            {
                SNavRoomType.END_R,
                new string[]
                {
                    "._______.",
                    "|  END  .",
                    "|_______|"
                }
            },
            {
                SNavRoomType.END_L,
                new string[]
                {
                    "._______.",
                    ".  END  |",
                    "|_______|"
                }
            },
            {
                SNavRoomType.CLASSD,
                new string[]
                {
                    "._______.",
                    "|_______.",
                    "         "
                }
            },
            {
                SNavRoomType.SCP_939_TB,
                new string[]
                {
                    ".__| |__.",
                    "|  939  |",
                    "|__. .__|"
                }
            },
            {
                SNavRoomType.SCP_939_RL,
                new string[]
                {
                    "._______.",
                    ". _939_ .",
                    "|_______|"
                }
            },
            {
                SNavRoomType.EZ_HCZ_CHECKPOINT_TB,
                new string[]
                {
                    ".__| |__.",
                    "|__.C.__|",
                    "   | |   "
                }
            },
            {
                SNavRoomType.EZ_HCZ_CHECKPOINT_RL,
                new string[]
                {
                    ".__|<mark>_</mark>|__.",
                    ".__ C __.",
                    "   |<mark>_</mark>|   "
                }
            },
            {
                SNavRoomType.LCZ_A_B,
                new string[]
                {
                    "._______.",
                    "|__.A.__|",
                    "   | |   "
                }
            },
            {
                SNavRoomType.LCZ_A_L,
                new string[]
                {
                    "   |<mark>_</mark>|__.",
                    "   |A __.",
                    "   |<mark>_</mark>|   "
                }
            },
            {
                SNavRoomType.LCZ_A_T,
                new string[]
                {
                    ".__| |__.",
                    "|___A___|",
                    "         "
                }
            },
            {
                SNavRoomType.LCZ_A_R,
                new string[]
                {
                    ".__|<mark>_</mark>|   ",
                    ".__ A|   ",
                    "   |<mark>_</mark>|   "
                }
            },
            {
                SNavRoomType.LCZ_B_B,
                new string[]
                {
                    "._______.",
                    "|__.B.__|",
                    "   | |   "
                }
            },
            {
                SNavRoomType.LCZ_B_L,
                new string[]
                {
                    "   |<mark>_</mark>|__.",
                    "   |B __.",
                    "   |<mark>_</mark>|   "
                }
            },
            {
                SNavRoomType.LCZ_B_T,
                new string[]
                {
                    ".__| |__.",
                    "|___B___|",
                    "         "
                }
            },
            {
                SNavRoomType.LCZ_B_R,
                new string[]
                {
                    ".__|<mark>_</mark>|   ",
                    ".__ B|   ",
                    "   |<mark>_</mark>|   "
                }
            },
            {
                SNavRoomType.NUKE_TB,
                new string[]
                {
                    ".__| |   ",
                    "|<mark>__</mark>. |   ",
                    "   | |   "
                }
            },
            {
                SNavRoomType.NUKE_RL,
                new string[]
                {
                    "._______.",
                    ".__. .__.",
                    "   |<mark>_</mark>|   "
                }
            },
            {
                SNavRoomType.SCP049_TB,
                new string[]
                {
                    "   | '<mark>_</mark>| ",
                    "   | |   ",
                    "   | |   "
                }
            },
            {
                SNavRoomType.SCP049_RL,
                new string[]
                {
                    ".|<mark>_</mark>|____.",
                    "._______.",
                    "         "
                }
            },
            {
                SNavRoomType.HID_TB,
                new string[]
                {
                    ".__| |   ",
                    "|    |   ",
                    "|__. |   "
                }
            },
            {
                SNavRoomType.HID_RL,
                new string[]
                {
                    "._______.",
                    ". _____ .",
                    "|__._.__|"
                }
            },
            {
                SNavRoomType.COMPUTERS_UPSTAIRS_TB,
                new string[]
                {
                    ".__| |__.",
                    "|       |",
                    "|<mark>__</mark>. .__|"
                }
            },
            {
                SNavRoomType.COMPUTERS_UPSTAIRS_RL,
                new string[]
                {
                    "._______.",
                    ". ______.",
                    "|______<mark>_</mark>|"
                }
            },
            {
                SNavRoomType.TESLA_TB,
                new string[]
                {
                    "   | |   ",
                    "   [ ]   ",
                    "   | |   "
                }
            },
            {
                SNavRoomType.TESLA_RL,
                new string[]
                {
                    "___._.___",
                    "___._.___",
                    "         "
                }
            },
        };
        /// <summary>
        /// Rooms where someone was on last scan
        /// </summary>
        public static readonly HashSet<Room> LastScan = new HashSet<Room>();
        /// <inheritdoc/>
        public override string Name => "SNavHandler";
        private static new __Log Log;
        /// <inheritdoc/>
        public SNavHandler(PluginHandler p) : base(p)
        {
            Log = base.Log;
            new SNavClasicItem();
            new SNavUltimateItem();
        }
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet += this.Handle<Exiled.Events.EventArgs.InsertingGeneratorTabletEventArgs>((ev) => Player_InsertingGeneratorTablet(ev));
            Exiled.Events.Handlers.Player.ActivatingWorkstation += this.Handle<Exiled.Events.EventArgs.ActivatingWorkstationEventArgs>((ev) => Player_ActivatingWorkstation(ev));
            Exiled.Events.Handlers.Scp914.UpgradingItems += this.Handle<Exiled.Events.EventArgs.UpgradingItemsEventArgs>((ev) => Scp914_UpgradingItems(ev));
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet -= this.Handle<Exiled.Events.EventArgs.InsertingGeneratorTabletEventArgs>((ev) => Player_InsertingGeneratorTablet(ev));
            Exiled.Events.Handlers.Player.ActivatingWorkstation -= this.Handle<Exiled.Events.EventArgs.ActivatingWorkstationEventArgs>((ev) => Player_ActivatingWorkstation(ev));
            Exiled.Events.Handlers.Scp914.UpgradingItems -= this.Handle<Exiled.Events.EventArgs.UpgradingItemsEventArgs>((ev) => Scp914_UpgradingItems(ev));
        }
        /// <summary>
        /// Spawns SNAV
        /// </summary>
        /// <param name="ultimate">If SNav should be ultimate</param>
        /// <param name="pos">Where SNav should be spawned</param>
        /// <returns>Spawned SNav</returns>
        public static Pickup SpawnSNAV(bool ultimate, Vector3 pos)
        {
            if (ultimate)
            {
                return MapPlus.Spawn(new Inventory.SyncItemInfo
                {
                    durability = 401000f,
                    id = ItemType.WeaponManagerTablet,
                }, pos, Quaternion.identity, new Vector3(2.5f, .75f, .75f));
            }
            else
            {
                return MapPlus.Spawn(new Inventory.SyncItemInfo
                {
                    durability = 301000f,
                    id = ItemType.WeaponManagerTablet,
                }, pos, Quaternion.identity, new Vector3(2.0f, .50f, .50f));
            }
        }

        private void Scp914_UpgradingItems(Exiled.Events.EventArgs.UpgradingItemsEventArgs ev)
        {
            foreach (var item in ev.Items.ToArray())
            {
                if (item.ItemId == ItemType.WeaponManagerTablet)
                {
                    switch (ev.KnobSetting)
                    {
                        case Scp914.Scp914Knob.Fine:
                            if (UnityEngine.Random.Range(1, 101) <= 25)
                            {
                                SpawnSNAV(false, ev.Scp914.output.position + Vector3.up);
                                ev.Items.Remove(item);
                                item.Delete();
                            }
                            break;
                    }
                }
            }
        }

        private void Player_ActivatingWorkstation(Exiled.Events.EventArgs.ActivatingWorkstationEventArgs ev)
        {
            if (ev.Player.Inventory.items.FirstOrDefault(i => i.id == ItemType.WeaponManagerTablet).durability >= 301000f)
            {
                ev.Player.SetGUI("snavWarn", Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, "Nie możesz włożyć <color=yellow>SNAV-a</color> do workstation", 5);
                ev.IsAllowed = false;
            }
        }

        private void Player_InsertingGeneratorTablet(Exiled.Events.EventArgs.InsertingGeneratorTabletEventArgs ev)
        {
            if (ev.Player.Inventory.items.FirstOrDefault(i => i.id == ItemType.WeaponManagerTablet).durability >= 301000f)
            {
                ev.Player.SetGUI("snavWarn", Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, "Nie możesz włożyć <color=yellow>SNAV-a</color> do generatora", 5);
                ev.IsAllowed = false;
            }
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(DoRoundLoop());
        }
        private static IEnumerator<float> DoRoundLoop()
        {
            var initOne = SpawnSNAV(true, Vector3.zero);
            MEC.Timing.CallDelayed(5, () => initOne.Delete());
            yield return Timing.WaitForSeconds(1);
            foreach (var item in PluginHandler.Config.SNavUltimateSpawns)
            {
                string[] data = item.Split(',');
                if (data.Length < 4 || !float.TryParse(data[1], out float x) || !float.TryParse(data[2], out float y) || !float.TryParse(data[3], out float z))
                {
                    Log.Error($"Config Error | \"{item}\" is not correct SNav Spawn Data");
                    continue;
                }
                var door = Map.Doors.FirstOrDefault(i => i.Type().ToString() == data[0]);
                if (door == null)
                {
                    Log.Warn("Invalid Data, Unknown Door \"data[0]\"");
                    continue;
                }
                SpawnSNAV(true, door.transform.position + (door.transform.forward * x) + (door.transform.right * z) + Vector3.up * y);
                yield return Timing.WaitForSeconds(0.1f);
            }
            foreach (var item in PluginHandler.Config.SNav3000Spawns)
            {
                string[] data = item.Split(',');
                if (data.Length < 4 || !float.TryParse(data[1], out float x) || !float.TryParse(data[2], out float y) || !float.TryParse(data[3], out float z))
                {
                    Log.Error($"Config Error | \"{item}\" is not correct SNav Spawn Data");
                    continue;
                }
                var door = Map.Doors.FirstOrDefault(i => i.Type().ToString() == data[0]);
                if (door == null)
                {
                    Log.Warn($"Invalid Data, Unknown Door \"{data[0]}\"");
                    continue;
                }
                SpawnSNAV(false, door.transform.position + (door.transform.forward * x) + (door.transform.right * z) + Vector3.up * y);
                yield return Timing.WaitForSeconds(0.1f);
            }

            //yield return Timing.WaitForSeconds(300);
            int rid = RoundPlus.RoundId;
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                yield return Timing.WaitForSeconds(15);
                LastScan.Clear();
                GateA = false;
                GateAHole = false;
                Nuke = false;
                GateB = false;
                Helipad = false;
                Escape = false;
                foreach (var player in RealPlayers.List.Where(p => p.IsAlive))
                {
                    if (player.CurrentRoom != null)
                        LastScan.Add(player.CurrentRoom);
                    if (player.Position.y > 900 && player.Position.y < 1012)
                    {
                        //194 +900 -44 -> 147 +900 -44
                        if (player.Position.x <= 194 && player.Position.x >= 147)
                        {
                            if (player.Position.z >= -44)
                            {
                                Escape = true;
                                continue;
                            }
                            else if (player.Position.z <= -73)
                            {
                                CassieRoom = true;
                                continue;
                            }
                        }
                        if (player.Position.x >= 148)
                        {
                            Helipad = true;
                            continue;
                        }
                        if (player.Position.x >= 68)
                        {
                            GateB = true;
                            continue;
                        }
                        if (player.Position.x <= 42 && player.Position.x >= 37 && player.Position.z <= -32 && player.Position.z >= -38)
                        {
                            Nuke = true;
                            continue;
                        }
                        if (player.Position.z >= -21)
                        {
                            GateA = true;
                            continue;
                        }
                        GateAHole = true;
                        continue;
                    }
                }
                RequireUpdateUltimate = true;
                yield return Timing.WaitForSeconds(1);
                RequireUpdateUltimate = false;
            }
        }

        private void Server_WaitingForPlayers()
        {
            try
            {
                Log.Debug("Try WaitingForPlayers");
                try
                {
                    if (MapPlus.Rooms == null)
                        Log.Error("Room List is Null");
                    else if (MapPlus.Rooms.Any(r => r == null))
                        Log.Error("Some Rooms Are Null");
                    else if (!MapPlus.Rooms.Any(r => r.Type == RoomType.LczClassDSpawn))
                        Log.Error("No ClassDSpwan Room");
                    else
                    {
                        var room = MapPlus.Rooms.First(r => r.Type == RoomType.LczClassDSpawn);
                        if (room == null)
                            Log.Error("Found ClassDSpwan Room but null");
                        else if (room.gameObject == null)
                            Log.Error("Found ClassDSpwan Room but gameObject is null");
                        else
                            OffsetClassD = GetRotateion(room);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error("CatchId: 1");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                try
                {
                    if (!MapPlus.Rooms.Any(r => r.Type == RoomType.HczEzCheckpoint))
                    {
                        MEC.Timing.CallDelayed(1, Server_WaitingForPlayers);
                        return;
                    }
                    var room = MapPlus.Rooms.First(r => r.Type == RoomType.HczEzCheckpoint);
                    OffsetCheckpoint = (Rotation)(((int)GetRotateion(room) + (int)OffsetClassD) % 4);
                }
                catch (System.Exception ex)
                {
                    Log.Error("CatchId: 2");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }

                List<int> Zzzzs = new List<int>();
                List<int> Xxxxs = new List<int>();
                try
                {
                    foreach (var item in MapPlus.Rooms.Where(r => r.Zone == ZoneType.LightContainment))
                    {
                        int z = (int)Math.Floor(item.Position.z);
                        int x = (int)Math.Floor(item.Position.x);
                        if (!Zzzzs.Contains(z))
                            Zzzzs.Add(z);
                        if (!Xxxxs.Contains(x))
                            Xxxxs.Add(x);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error("CatchId: 3");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                Xxxxs.Sort();
                Zzzzs.Sort();
                Zzzzs.Reverse();
                try
                {
                    LCZRooms = new Room[Zzzzs.Count, Xxxxs.Count];
                    for (int i = 0; i < Zzzzs.Count; i++)
                    {
                        try
                        {
                            var z = Zzzzs[i];
                            List<Room> roomsList = new List<Room>();
                            for (int j = 0; j < Xxxxs.Count; j++)
                            {
                                try
                                {
                                    var x = Xxxxs[j];
                                    LCZRooms[i, j] = MapPlus.Rooms.Where(r => r.Zone == ZoneType.LightContainment).FirstOrDefault(p => (int)Math.Floor(p.Position.z) == z && (int)Math.Floor(p.Position.x) == x);
                                }
                                catch (System.Exception ex)
                                {
                                    Log.Error("CatchId: 4.2");
                                    Log.Error(ex.Message);
                                    Log.Error(ex.StackTrace);
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Log.Error("CatchId: 4.1");
                            Log.Error(ex.Message);
                            Log.Error(ex.StackTrace);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error("CatchId: 4");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }

                Zzzzs.Clear();
                Xxxxs.Clear();
                try
                {
                    foreach (var item in MapPlus.Rooms.Where(r => (r.Zone == ZoneType.HeavyContainment || r.Zone == ZoneType.Entrance) && r.Type != RoomType.Pocket))
                    {
                        try
                        {
                            int z = (int)Math.Floor(item.Position.z);
                            int x = (int)Math.Floor(item.Position.x);
                            if (!Zzzzs.Contains(z))
                                Zzzzs.Add(z);
                            if (!Xxxxs.Contains(x))
                                Xxxxs.Add(x);
                        }
                        catch (System.Exception ex)
                        {
                            Log.Error("CatchId: 5.1");
                            Log.Error(ex.Message);
                            Log.Error(ex.StackTrace);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error("CatchId: 5");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                Xxxxs.Sort();
                Zzzzs.Sort();
                try
                {
                    for (int i = 0; i < Xxxxs.Count; i++)
                    {
                        try
                        {
                            var x = Xxxxs[i];
                            if (!MapPlus.Rooms.Where(r => (r.Zone == ZoneType.HeavyContainment || r.Zone == ZoneType.Entrance) && r.Type != RoomType.Pocket).Any(p => (int)Math.Floor(p.Position.x) == x))
                            {
                                Xxxxs.RemoveAt(i);
                                i--;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Log.Error("CatchId: 6.1");
                            Log.Error(ex.Message);
                            Log.Error(ex.StackTrace);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error("CatchId: 6");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                Zzzzs.Reverse();
                EZ_HCZRooms = new Room[Zzzzs.Count, Xxxxs.Count];
                try
                {
                    for (int i = 0; i < Zzzzs.Count; i++)
                    {
                        try
                        {
                            var z = Zzzzs[i];
                            List<Room> roomsList = new List<Room>();
                            for (int j = 0; j < Xxxxs.Count; j++)
                            {
                                try
                                {
                                    var x = Xxxxs[j];
                                    EZ_HCZRooms[i, j] = MapPlus.Rooms.Where(r => (r.Zone == ZoneType.HeavyContainment || r.Zone == ZoneType.Entrance) && r.Type != RoomType.Pocket).FirstOrDefault(p => (int)Math.Floor(p.Position.z) == z && (int)Math.Floor(p.Position.x) == x);
                                }
                                catch (System.Exception ex)
                                {
                                    Log.Error("CatchId: 7.2");
                                    Log.Error(ex.Message);
                                    Log.Error(ex.StackTrace);
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Log.Error("CatchId: 7.1");
                            Log.Error(ex.Message);
                            Log.Error(ex.StackTrace);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error("CatchId: 7");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("CatchId: 0");
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                MEC.Timing.CallDelayed(5, Server_WaitingForPlayers);
            }
        }

        private static bool RequireUpdateUltimate = false;
        private static readonly HashSet<Player> RequireUpdate = new HashSet<Player>();
        private static readonly Dictionary<Player, Room> LastRooms = new Dictionary<Player, Room>();

        private static void UpdateVisibility(Player p, bool visible)
        {
            if (!HideTablet)
                return;
            var old = p.ReferenceHub.transform.localScale;
            p.ReferenceHub.transform.localScale = new Vector3(visible ? p.ReferenceHub.transform.localScale.x : 0, p.ReferenceHub.transform.localScale.y, p.ReferenceHub.transform.localScale.z);
            var sendSpawnMessage = Server.SendSpawnMessage;
            if (sendSpawnMessage != null)
            {
                sendSpawnMessage.Invoke(null, new object[]
                {
                    p.ReferenceHub.characterClassManager.netIdentity,
                    p.Connection
                });
            }
            p.ReferenceHub.transform.localScale = old;
        }
        private static IEnumerator<float> IUpdateInterface(Player player)
        {
            yield return Timing.WaitForSeconds(0.5f);
            int i = 1;
            UpdateVisibility(player, false);
            while (player.CurrentItem.id == ItemType.WeaponManagerTablet)
            {
                if (!LastRooms.TryGetValue(player, out Room lastRoom) || lastRoom != player.CurrentRoom)
                {
                    LastRooms[player] = player.CurrentRoom;
                    RequireUpdate.Add(player);
                    i = 20;
                }
                if (i >= 19 || RequireUpdate.Contains(player) || (RequireUpdateUltimate && player.CurrentItem.durability == 401000f))
                {
                    UpdateInterface(player);
                    RequireUpdate.Remove(player);
                    i = 0;
                }
                else
                    i++;

                yield return Timing.WaitForSeconds(0.5f);
            }
            UpdateVisibility(player, true);
        }
        private static bool GateA = false;
        private static bool GateAHole = false;
        private static bool Nuke = false;
        private static bool GateB = false;
        private static bool Helipad = false;
        private static bool Escape = false;
        private static bool CassieRoom = false;
        private static void UpdateInterface(Player player)
        {
            bool Ultimate;
            if (player.CurrentItem.durability == 401000f)
                Ultimate = true;
            else if (player.CurrentItem.durability == 301000f)
                Ultimate = false;
            else
                return;
            string[] toWrite;
            if (player.Position.y < -500 && player.Position.y > -700)
            {
                toWrite =
@"
  |‾‾‾‾‾‾‾|‾‾‾|‾‾|
__|  /‾‾‾‾|   '  |
|   | <color=red>X</color>   |   |  |
‾‾|  \____|   |‾‾'
  |___________|
".Split('\n');
            }
            else if (player.Position.y < -700 && player.Position.y > -800)
            {
                toWrite =
@"
      _____________
     |      `     |
     | .    ,     |
.___.| .  |‾‾‾|   |
|   || .      |   |
| |‾`|__. |   |   |
| |_____| |‾‾‾‾‾‾‾`
|         |
‾‾‾‾‾‾‾‾‾‾`
".Split('\n');
            }
            else if (player.Position.y > 800)
            {
                var tmp =
@"    
             <color=gatea_color>._.</color>                                                          <color=escape_color>.______.</color>
         <color=gatea_color>.___| |___.</color>                                                      <color=escape_color>|      |</color>
         <color=gatea_color>|         |</color>                                                      <color=escape_color>|_|‾‾ ‾|</color>
       <color=gatea_color>|‾  GATE  A |</color>                                                      <color=escape_color>|ESCAPE|</color>
       <color=gatea_color>`‾|         |</color>                                                      <color=escape_color>| |____|</color>
          <color=gatea_color>‾‾|   |‾‾`</color>                                                      <color=escape_color>| |___.</color>
            <color=gatea_color>|   |</color>                                                         <color=escape_color>|___. |</color>
            <color=gatea_color>|   |</color>                                                             <color=escape_color>| |________.</color>
       <color=gatea_color>.____|   |_____.</color>                                                       <color=escape_color>|________. |</color>
       <color=gatea_color>|              |</color>                                                                <color=escape_color>| |</color>
       <color=gatea_color>`‾‾‾‾<color=gateahole_color>|   |</color>‾‾‾<color=gateahole_color>| |</color>     <color=nuke_color>._____.</color>                                                    <color=escape_color>| |</color>
            <color=gateahole_color>|   |   | |     <color=nuke_color>|NUKE |</color>                                                    <color=escape_color>: :</color>
            <color=gateahole_color>|   |   | |     <color=nuke_color>|_. ._|</color>                                                    <color=escape_color>| |</color>
       <color=gateahole_color>.____|   |___|_|_______| |_______.</color>                                            <color=escape_color>._| |_.</color>
       <color=gateahole_color>|                      | |   |   |</color><color=gateb_color>   ._|‾|                  </color><color=helipad_color>._________________</color><color=escape_color>|     |</color>
       <color=gateahole_color>|                      | |   |   |</color><color=gateb_color>   |   |__________________</color><color=helipad_color>|                       |</color>
<color=gateahole_color>|‾‾‾‾‾‾‾‾‾‾‾|   |‾‾‾\ ‾‾‾‾‾‾‾‾` `‾‾‾|‾‾‾|</color><color=gateb_color>‾‾‾|   GATE B             </color><color=helipad_color>                        |</color>
<color=gateahole_color>|           |   |    ‾‾‾‾‾‾‾        |   |</color><color=gateb_color>   `‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾</color><color=helipad_color>                        |</color>
<color=gateahole_color>| CAR ENTRY |   |                   |   |</color><color=gateb_color>                          </color><color=helipad_color>        HELIPAD         |</color>
<color=gateahole_color>|           |   |                   |   |</color><color=gateb_color>                          </color><color=helipad_color>                        |</color>
<color=gateahole_color>|___________|   |___________________|   |</color><color=gateb_color>__________________________</color><color=helipad_color>.                       |</color>
       <color=gateahole_color>|                                |</color><color=gateb_color>                          </color><color=helipad_color>|                       |</color>
       <color=gateahole_color>|                                |</color><color=gateb_color>                          </color><color=helipad_color>`‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾</color><color=cassieroom_color>|     |</color>
       <color=gateahole_color>`‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾`</color>                                            <color=cassieroom_color>|     |</color>
                                                                                     <color=cassieroom_color>|     |</color>
                                                                                     <color=cassieroom_color>`‾‾‾‾‾`</color>
";
                if (Ultimate)
                {
                    tmp = tmp.Replace("gatea_color", GateA ? "red" : "green");
                    tmp = tmp.Replace("gateahole_color", GateAHole ? "red" : "green");
                    tmp = tmp.Replace("nuke_color", Nuke ? "red" : "green");
                    tmp = tmp.Replace("gateb_color", GateB ? "red" : "green");
                    tmp = tmp.Replace("helipad_color", Helipad ? "red" : "green");
                    tmp = tmp.Replace("escape_color", Escape ? "red" : "green");
                    tmp = tmp.Replace("cassieroom_color", CassieRoom ? "red" : "green");

                    toWrite = tmp.Split('\n');
                }
                else
                {
                    tmp = tmp.Replace("gatea_color", "green");
                    tmp = tmp.Replace("gateahole_color", "green");
                    tmp = tmp.Replace("nuke_color", "green");
                    tmp = tmp.Replace("gateb_color", "green");
                    tmp = tmp.Replace("helipad_color", "green");
                    tmp = tmp.Replace("escape_color", "green");
                    tmp = tmp.Replace("cassieroom_color", "green");

                    toWrite = tmp.Split('\n');
                }
            }
            else
            {
                var rooms = GetRooms(player.Position.y);
                toWrite = new string[rooms.GetLength(0) * 3];
                for (int z = 0; z < rooms.GetLength(0); z++)
                {
                    for (int x = 0; x < rooms.GetLength(1); x++)
                    {
                        string color = "green";
                        string Name = "  END  ";
                        var room = rooms[z, x];
                        var tmp = GetRoomString(GetRoomType(room));
                        if (room == null)
                        {
                            toWrite[(z * 3) + 0] += tmp[0];
                            toWrite[(z * 3) + 1] += tmp[1];
                            toWrite[(z * 3) + 2] += tmp[2];
                            continue;
                        }
                        if (player.CurrentRoom == room)
                            color = "white";
                        else if (Warhead.IsInProgress)
                        {
                            if (room.Type == RoomType.HczNuke || room.Type == RoomType.EzGateA || room.Type == RoomType.EzGateB || room.Type == RoomType.LczChkpA || room.Type == RoomType.LczChkpB)
                                color = "red";
                        }
                        else if (MapPlus.IsLCZDecontaminated(35))
                        {
                            if (room.Type == RoomType.LczChkpA || room.Type == RoomType.LczChkpB)
                                color = "red";
                        }
                        if (Generator079.Generators.Any(g => g.CurRoom == room.Name && g.NetworkremainingPowerup > 0f))
                        {
                            var gen = Generator079.Generators.Find(g => g.CurRoom == room.Name);
                            if (gen.NetworkisTabletConnected)
                                color = "yellow";
                            else
                                color = "blue";
                        }
                        if (Ultimate)
                        {
                            if (LastScan.Contains(room) && player.CurrentRoom.GetHashCode() != room.GetHashCode())
                                color = "red";

                            switch (room.Type)
                            {
                                case RoomType.EzGateA:
                                    Name = "GATE  A";
                                    break;
                                case RoomType.EzGateB:
                                    Name = "GATE  B";
                                    break;
                                case RoomType.Hcz106:
                                    Name = "SCP 106";
                                    break;
                                case RoomType.Hcz079:
                                    Name = "SCP 079";
                                    break;
                                case RoomType.Hcz096:
                                    Name = "SCP 096";
                                    break;
                                case RoomType.Lcz012:
                                    Name = "SCP 012";
                                    break;
                                case RoomType.Lcz914:
                                    Name = "SCP 914";
                                    break;
                                case RoomType.Lcz173:
                                    Name = "SCP 173";
                                    break;
                                case RoomType.LczGlassBox:
                                    Name = "SCP 372";
                                    break;
                                case RoomType.LczCafe:
                                    Name = "   PC  ";
                                    break;
                                case RoomType.LczArmory:
                                    Name = "ARMORY ";
                                    break;
                            }
                        }


                        toWrite[(z * 3) + 0] += $"<color={color}>" + tmp[0] + "</color>";
                        toWrite[(z * 3) + 1] += $"<color={color}>" + tmp[1].Replace("  END  ", Name) + "</color>";
                        toWrite[(z * 3) + 2] += $"<color={color}>" + tmp[2] + "</color>";
                    }
                }
            }
            var list = NorthwoodLib.Pools.ListPool<string>.Shared.Rent(toWrite);
            list.RemoveAll(i => string.IsNullOrWhiteSpace(i));
            while (list.Count < 65)
                list.Insert(0, "");
            player.ShowHint("<voffset=25em><color=green><size=25%><align=left><mspace=0.5em>" + string.Join("<br>", list) + "</mspace></align></size></color></voffset>", 11);
            NorthwoodLib.Pools.ListPool<string>.Shared.Return(list);
        }
        /// <summary>
        /// Returns rooms based on <paramref name="pos"/>
        /// </summary>
        /// <param name="pos">Position</param>
        /// <returns>Rooms</returns>
        public static Room[,] GetRooms(float pos)
        {
            switch (pos)
            {
                case float x when x < -900 && x > -1100:
                    return EZ_HCZRooms;
                case float x when x < 100 && x > -100:
                    return LCZRooms;
                default:
                    return new Room[0, 0];
            }
        }
        /// <summary>
        /// Returns room rotation
        /// </summary>
        /// <param name="room">Room</param>
        /// <returns>Rotation</returns>
        public static Rotation GetRotateion(Room room)
        {
            if (room.transform.localEulerAngles.y == 0)
                return Rotation.RIGHT;
            else if (room.transform.localEulerAngles.y == 90)
                return Rotation.DOWN;
            else if (room.transform.localEulerAngles.y == 180)
                return Rotation.LEFT;
            else if (room.transform.localEulerAngles.y == 270)
                return Rotation.UP;
            else
            {
                Log.Error(room.transform.localEulerAngles.y);
                return (Rotation)(-99);
            }
        }
        /// <summary>
        /// Returns room type
        /// </summary>
        /// <param name="room">Room</param>
        /// <returns>Room type</returns>
        public static SNavRoomType GetRoomType(Room room)
        {
            switch (room?.Type)
            {
                case RoomType.EzCafeteria:
                case RoomType.EzConference:
                case RoomType.EzDownstairsPcs:
                case RoomType.EzPcs:
                case RoomType.EzStraight:

                case RoomType.HczServers:
                case RoomType.Unknown:

                case RoomType.LczAirlock:
                case RoomType.LczPlants:
                case RoomType.LczToilets:
                case RoomType.LczStraight:
                    {
                        var tmp = (Rotation)((int)GetRotateion(room) + (room.Zone == ZoneType.Entrance ? (int)OffsetCheckpoint : (int)OffsetClassD));
                        if ((int)tmp > 3)
                            tmp -= 4;
                        if (tmp == Rotation.UP || tmp == Rotation.DOWN)
                            return SNavRoomType.HS_LR;
                        else if (tmp == Rotation.RIGHT || tmp == Rotation.LEFT)
                            return SNavRoomType.HS_TB;
                        else
                            return SNavRoomType.ERROR;
                    }
                case RoomType.EzIntercom:
                case RoomType.EzCurve:

                case RoomType.HczCurve:

                case RoomType.LczCurve:
                    {
                        var tmp = (Rotation)((int)GetRotateion(room) + (room.Zone == ZoneType.Entrance ? (int)OffsetCheckpoint : (int)OffsetClassD));
                        if ((int)tmp > 3)
                            tmp -= 4;
                        switch (tmp)
                        {
                            case Rotation.UP:
                                return SNavRoomType.HC_LT;
                            case Rotation.RIGHT:
                                return SNavRoomType.HC_RT;
                            case Rotation.DOWN:
                                return SNavRoomType.HC_RB;
                            case Rotation.LEFT:
                                return SNavRoomType.HC_LB;
                            default:
                                return SNavRoomType.ERROR;
                        }
                    }

                case RoomType.HczTCross:
                case RoomType.HczArmory:

                case RoomType.LczTCross:
                    {
                        var tmp = (Rotation)((int)GetRotateion(room) + (room.Zone == ZoneType.Entrance ? (int)OffsetCheckpoint : (int)OffsetClassD));
                        if ((int)tmp > 3)
                            tmp -= 4;
                        switch (tmp)
                        {
                            case Rotation.UP:
                                return SNavRoomType.IT_TB_R;
                            case Rotation.RIGHT:
                                return SNavRoomType.IT_RL_B;
                            case Rotation.DOWN:
                                return SNavRoomType.IT_TB_L;
                            case Rotation.LEFT:
                                return SNavRoomType.IT_RL_T;
                            default:
                                return SNavRoomType.ERROR;
                        }
                    }
                case RoomType.EzCrossing:

                case RoomType.HczCrossing:

                case RoomType.LczCrossing:
                    {
                        return SNavRoomType.IX;
                    }
                case RoomType.EzGateA:
                case RoomType.EzGateB:
                case RoomType.EzCollapsedTunnel:
                case RoomType.EzShelter:
                case RoomType.EzVent:

                case RoomType.Hcz106:
                case RoomType.Hcz096:
                case RoomType.Hcz079:

                case RoomType.LczGlassBox:
                case RoomType.LczCafe:
                case RoomType.LczArmory:
                case RoomType.Lcz914:
                case RoomType.Lcz173:
                case RoomType.Lcz012:
                    {
                        var tmp = (Rotation)(((int)GetRotateion(room) + (room.Zone == ZoneType.Entrance ? (int)OffsetCheckpoint : (int)OffsetClassD)) % 4);
                        if ((int)tmp > 3)
                            tmp -= 4;
                        switch (tmp)
                        {
                            case Rotation.UP:
                                return SNavRoomType.END_R;
                            case Rotation.RIGHT:
                                return SNavRoomType.END_B;
                            case Rotation.DOWN:
                                return SNavRoomType.END_L;
                            case Rotation.LEFT:
                                return SNavRoomType.END_T;
                            default:
                                return SNavRoomType.ERROR;
                        }
                    }

                case RoomType.LczClassDSpawn:
                    return SNavRoomType.CLASSD;

                case RoomType.Hcz939:
                    {
                        var tmp = (Rotation)((int)(GetRotateion(room) + (int)OffsetClassD) % 4);
                        if (tmp == Rotation.UP || tmp == Rotation.DOWN)
                            return SNavRoomType.SCP_939_RL;
                        else if (tmp == Rotation.RIGHT || tmp == Rotation.LEFT)
                            return SNavRoomType.SCP_939_TB;
                        else
                            return SNavRoomType.ERROR;
                    }

                case RoomType.HczEzCheckpoint:
                    {
                        var tmp = (Rotation)((int)(GetRotateion(room) + (int)OffsetClassD) % 4);
                        if (tmp == Rotation.UP || tmp == Rotation.DOWN)
                            return SNavRoomType.EZ_HCZ_CHECKPOINT_RL;
                        else if (tmp == Rotation.RIGHT || tmp == Rotation.LEFT)
                            return SNavRoomType.EZ_HCZ_CHECKPOINT_TB;
                        else
                            return SNavRoomType.ERROR;
                    }

                case RoomType.HczNuke:
                    {
                        var tmp = (Rotation)((int)(GetRotateion(room) + (int)OffsetClassD) % 4);
                        if (tmp == Rotation.UP || tmp == Rotation.DOWN)
                            return SNavRoomType.NUKE_RL;
                        else if (tmp == Rotation.RIGHT || tmp == Rotation.LEFT)
                            return SNavRoomType.NUKE_TB;
                        else
                            return SNavRoomType.ERROR;
                    }

                case RoomType.Hcz049:
                    {
                        var tmp = (Rotation)((int)(GetRotateion(room) + (int)OffsetClassD) % 4);
                        if (tmp == Rotation.UP || tmp == Rotation.DOWN)
                            return SNavRoomType.SCP049_RL;
                        else if (tmp == Rotation.RIGHT || tmp == Rotation.LEFT)
                            return SNavRoomType.SCP049_TB;
                        else
                            return SNavRoomType.ERROR;
                    }

                case RoomType.HczHid:
                    {
                        var tmp = (Rotation)((int)(GetRotateion(room) + (int)OffsetClassD) % 4);
                        if (tmp == Rotation.UP || tmp == Rotation.DOWN)
                            return SNavRoomType.HID_RL;
                        else if (tmp == Rotation.RIGHT || tmp == Rotation.LEFT)
                            return SNavRoomType.HID_TB;
                        else
                            return SNavRoomType.ERROR;
                    }

                case RoomType.LczChkpA:
                case RoomType.HczChkpA:
                    {
                        var tmp = (Rotation)((int)(GetRotateion(room) + (int)OffsetClassD) % 4);
                        switch (tmp)
                        {
                            case Rotation.UP:
                                return SNavRoomType.LCZ_A_L;
                            case Rotation.RIGHT:
                                return SNavRoomType.LCZ_A_B;
                            case Rotation.DOWN:
                                return SNavRoomType.LCZ_A_R;
                            case Rotation.LEFT:
                                return SNavRoomType.LCZ_A_T;
                            default:
                                return SNavRoomType.ERROR;
                        }
                    }

                case RoomType.LczChkpB:
                case RoomType.HczChkpB:
                    {
                        var tmp = (Rotation)((int)(GetRotateion(room) + (int)OffsetClassD) % 4);
                        switch (tmp)
                        {
                            case Rotation.UP:
                                return SNavRoomType.LCZ_B_L;
                            case Rotation.RIGHT:
                                return SNavRoomType.LCZ_B_B;
                            case Rotation.DOWN:
                                return SNavRoomType.LCZ_B_R;
                            case Rotation.LEFT:
                                return SNavRoomType.LCZ_B_T;
                            default:
                                return SNavRoomType.ERROR;
                        }
                    }

                case RoomType.EzUpstairsPcs:
                    {
                        var tmp = (Rotation)((int)(GetRotateion(room) + (int)OffsetCheckpoint) % 4);
                        if (tmp == Rotation.UP || tmp == Rotation.DOWN)
                            return SNavRoomType.COMPUTERS_UPSTAIRS_RL;
                        else if (tmp == Rotation.RIGHT || tmp == Rotation.LEFT)
                            return SNavRoomType.COMPUTERS_UPSTAIRS_TB;
                        else
                            return SNavRoomType.ERROR;
                    }

                case RoomType.HczTesla:
                    {
                        var tmp = (Rotation)((int)GetRotateion(room) + (room.Zone == ZoneType.Entrance ? (int)OffsetCheckpoint : (int)OffsetClassD));
                        if ((int)tmp > 3)
                            tmp -= 4;
                        if (tmp == Rotation.UP || tmp == Rotation.DOWN)
                            return SNavRoomType.TESLA_RL;
                        else if (tmp == Rotation.RIGHT || tmp == Rotation.LEFT)
                            return SNavRoomType.TESLA_TB;
                        else
                            return SNavRoomType.ERROR;
                    }

                default:
                    return SNavRoomType.NONE;
            }
        }
        /// <summary>
        /// Returns room preset
        /// </summary>
        /// <param name="type">Room Type</param>
        /// <returns>Preset</returns>
        public static string[] GetRoomString(SNavRoomType type) => Presets[type];

        /// <summary>
        /// Room types
        /// </summary>
        public enum SNavRoomType
        {
#pragma warning disable CS1591
            ERROR,
            NONE,
            HS_TB,
            HS_LR,
            HC_LT,
            HC_LB,
            HC_RB,
            HC_RT,
            IT_RL_T,
            IT_RL_B,
            IT_TB_L,
            IT_TB_R,
            IX,
            SCP_939_TB,
            SCP_939_RL,
            EZ_HCZ_CHECKPOINT_TB,
            EZ_HCZ_CHECKPOINT_RL,

            LCZ_A_R,
            LCZ_A_T,
            LCZ_A_B,
            LCZ_A_L,

            LCZ_B_R,
            LCZ_B_T,
            LCZ_B_B,
            LCZ_B_L,

            NUKE_TB,
            NUKE_RL,

            SCP049_TB,
            SCP049_RL,

            HID_TB,
            HID_RL,

            COMPUTERS_UPSTAIRS_TB,
            COMPUTERS_UPSTAIRS_RL,

            END_T,
            END_B,
            END_R,
            END_L,

            CLASSD,

            TESLA_TB,
            TESLA_RL,
#pragma warning restore CS1591
        }
        /// <summary>
        /// Room rotation
        /// </summary>
        public enum Rotation
        {
#pragma warning disable CS1591
            UP,
            RIGHT,
            DOWN,
            LEFT
#pragma warning restore CS1591
        }
    }
}
