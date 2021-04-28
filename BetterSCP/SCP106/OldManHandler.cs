using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.BetterSCP.SCP106
{
    class SCP106Handler : Module
    {
        public SCP106Handler(PluginHandler p) : base(p)
        {
            plugin.RegisterTranslation("scp106_start_message", "<color=red><b><size=500%>UWAGA</size></b></color><br><br><br><br><br><br><size=90%>Rozgrywka jako <color=red>SCP 106</color> na tym serwerze jest zmodyfikowana, <color=red>SCP 106</color> po użyciu przycisku do <color=yellow>tworzenia portalu</color> przeniesie się do <color=yellow>losowego</color> pomieszczenia w <color=yellow><b>innej</b></color> strefie niż ta w której obecnie się znajduje<br><br>Przycisk do <color=yellow>użycia portalu</color> działa tak samo tylko że przenosi do <color=yellow>losowego</color> pomieszczenia w <color=yellow><b>tej samej</b></color> strefie w jakiej znajduje się <color=red>SCP 106</color></size>");
            WelcomeMessage = plugin.ReadTranslation("scp106_start_message");
        }

        public override string Name => nameof(SCP106Handler);
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Scp106.Teleporting += this.Handle<Exiled.Events.EventArgs.TeleportingEventArgs>((ev) => Scp106_Teleporting(ev));
            Exiled.Events.Handlers.Scp106.CreatingPortal += this.Handle<Exiled.Events.EventArgs.CreatingPortalEventArgs>((ev) => Scp106_CreatingPortal(ev));
            Exiled.Events.Handlers.Scp106.Containing += this.Handle<Exiled.Events.EventArgs.ContainingEventArgs>((ev) => Scp106_Containing(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Map.Decontaminating += this.Handle<Exiled.Events.EventArgs.DecontaminatingEventArgs>((ev) => Map_Decontaminating(ev));
            Exiled.Events.Handlers.Player.Dying += this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.FailingEscapePocketDimension += this.Handle<Exiled.Events.EventArgs.FailingEscapePocketDimensionEventArgs>((ev) => Player_FailingEscapePocketDimension(ev));
            Exiled.Events.Handlers.Player.EscapingPocketDimension += this.Handle<Exiled.Events.EventArgs.EscapingPocketDimensionEventArgs>((ev) => Player_EscapingPocketDimension(ev));
            Exiled.Events.Handlers.Player.EnteringFemurBreaker += this.Handle<Exiled.Events.EventArgs.EnteringFemurBreakerEventArgs>((ev) => Player_EnteringFemurBreaker(ev));
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
        }
        public override void OnDisable()
        {
            Gamer.Mistaken.Systems.InfoMessage.InfoMessageManager.WelcomeMessages.Remove(RoleType.Scp106);
            Exiled.Events.Handlers.Scp106.Teleporting -= this.Handle<Exiled.Events.EventArgs.TeleportingEventArgs>((ev) => Scp106_Teleporting(ev));
            Exiled.Events.Handlers.Scp106.CreatingPortal -= this.Handle<Exiled.Events.EventArgs.CreatingPortalEventArgs>((ev) => Scp106_CreatingPortal(ev));
            Exiled.Events.Handlers.Scp106.Containing -= this.Handle<Exiled.Events.EventArgs.ContainingEventArgs>((ev) => Scp106_Containing(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Map.Decontaminating -= this.Handle<Exiled.Events.EventArgs.DecontaminatingEventArgs>((ev) => Map_Decontaminating(ev));
            Exiled.Events.Handlers.Player.Dying -= this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.FailingEscapePocketDimension -= this.Handle<Exiled.Events.EventArgs.FailingEscapePocketDimensionEventArgs>((ev) => Player_FailingEscapePocketDimension(ev));
            Exiled.Events.Handlers.Player.EscapingPocketDimension -= this.Handle<Exiled.Events.EventArgs.EscapingPocketDimensionEventArgs>((ev) => Player_EscapingPocketDimension(ev));
            Exiled.Events.Handlers.Player.EnteringFemurBreaker -= this.Handle<Exiled.Events.EventArgs.EnteringFemurBreakerEventArgs>((ev) => Player_EnteringFemurBreaker(ev));
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
        }

        private void Server_WaitingForPlayers()
        {
            MEC.Timing.CallDelayed(5, () =>
            {
                if (Rooms == null || Rooms.Length == 0)
                    Server_WaitingForPlayers();
            });
            Rooms = Map.Rooms.Where(r => r != null && !DisallowedRoomTypes.Contains(r.Type)).ToArray();
        }

        private readonly string WelcomeMessage;

        private void Player_EnteringFemurBreaker(Exiled.Events.EventArgs.EnteringFemurBreakerEventArgs ev)
        {
            MEC.Timing.CallDelayed(10, () =>
            {
                if (!OneOhSixContainer.used)
                    Cassie.Message("SCP 1 0 6 is READY FOR RECONTAINMENT");
            });
        }

        private void Player_FailingEscapePocketDimension(Exiled.Events.EventArgs.FailingEscapePocketDimensionEventArgs ev)
        {
            if (ev.Player.IsScp)
                ev.IsAllowed = false;
        }

        private void Player_EscapingPocketDimension(Exiled.Events.EventArgs.EscapingPocketDimensionEventArgs ev)
        {
            if (ev.Player.IsScp)
                ev.IsAllowed = false;
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (ev.Target.Role == RoleType.Scp106 && !ev.Target.IsGodModeEnabled && ev.HitInformation.GetDamageType() != DamageTypes.Recontainment && ev.HitInformation.GetDamageType() != DamageTypes.RagdollLess)
            {
                ev.Target?.SendConsoleMessage($"You have been killed by {ev.Killer?.ToString(false)} using {ev.HitInformation.GetDamageName()}", "green");
                MapPlus.Broadcast("BetterSCP.SCP106", 10, $"{ev.Target?.ToString(false)} killed by {ev.Killer?.ToString(false)} using {ev.HitInformation.GetDamageName()}", Broadcast.BroadcastFlags.AdminChat);
                InTeleportExecution.Add(ev.Target.Id);
                ev.Target.IsGodModeEnabled = true;
                ev.IsAllowed = false;
                MEC.Timing.CallDelayed(3, () =>
                {
                    ev.Target.IsGodModeEnabled = false;
                    ev.Target.Role = RoleType.Spectator;
                    MEC.Timing.CallDelayed(2, () =>
                    {
                        ev.Target.ReferenceHub.characterClassManager.CallTargetDeathScreen(ev.Target.Connection, ev.HitInformation);
                    });
                    Cassie.Message("SCP 1 0 6 RECONTAINED SUCCESSFULLY");
                    InTeleportExecution.Remove(ev.Target.Id);
                });
                Vector3 newTarget = new Vector3(0, -2000, 0);
                Scp106PlayerScript s106 = ev.Target.ReferenceHub.scp106PlayerScript;
                s106.NetworkportalPosition = newTarget;
                s106.CallCmdUsePortal();
            }
        }
        private void Scp106_Containing(Exiled.Events.EventArgs.ContainingEventArgs ev)
        {
            Vector3 newTarget = Map.Doors.FirstOrDefault(d => d.Type() == DoorType.Scp106Primary)?.transform.position ?? default;
            if (newTarget == default)
            {
                RealPlayers.Get(RoleType.Scp106).ToList().ForEach(p => p.SendConsoleMessage("[106] Not teleporting to cell, cell not found | Code: 5.1", "red"));
                return;
            }
            foreach (var player in RealPlayers.Get(RoleType.Scp106).ToArray())
            {
                player.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Scp207>();
                player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<CustomPlayerEffects.Scp207>(4);
                if (Vector3.Distance(player.Position, newTarget) < 20)
                {
                    player.SendConsoleMessage("[106] Not teleporting to cell, too close | Code: 5.2", "red");
                    continue;
                }
                TeleportOldMan(player, newTarget, true);
            }
        }
        private void Map_Decontaminating(Exiled.Events.EventArgs.DecontaminatingEventArgs ev)
        {
            foreach (var player in RealPlayers.Get(RoleType.Scp106).ToArray())
            {
                if (player.CurrentRoom?.Zone != ZoneType.LightContainment)
                    continue;
                TeleportOldMan(player, GetRandomRoom(player, false), true);
            }
        }

        private Room[] Rooms;
        private Room RandomRoom
        {
            get
            {
                if (Rooms == null || Rooms.Length == 0)
                    Rooms = MapPlus.Rooms.Where(r => r != null && !DisallowedRoomTypes.Contains(r.Type)).ToArray();//throw new System.Exception("Rooms are not ready to generate");
                return Rooms[UnityEngine.Random.Range(0, Rooms.Length)] ?? RandomRoom;
            }
        }
        private static readonly RoomType[] DisallowedRoomTypes = new RoomType[]
       {
            RoomType.EzShelter,
            RoomType.EzCollapsedTunnel,
            RoomType.HczTesla,
            RoomType.Lcz173,
            RoomType.Hcz939,
            RoomType.Pocket,
            RoomType.Hcz096
       };

        private void Server_RoundStarted()
        {
            Cooldown.Clear();
            Log.Info("Setting Rooms");
            Rooms = Map.Rooms.Where(r => r != null && !DisallowedRoomTypes.Contains(r.Type)).ToArray();
            LastRooms.Clear();
            InTeleportExecution.Clear();
            Timing.RunCoroutine(LockStart());
            Timing.RunCoroutine(CamperHell());
            Timing.RunCoroutine(MoveFromPocket());
        }

        private int CamperPoints = 0;
        private bool SpawnO5 = false;
        private IEnumerator<float> CamperHell()
        {
            CamperPoints = 0;
            SpawnO5 = false;
            var room = Map.Rooms.First(r => r.Type == RoomType.Lcz914);
            var door = Map.Doors.First(r => r.Type() == DoorType.Scp914);
            if (room == null)
                yield break;
            yield return Timing.WaitForSeconds(1f);
            while (Round.IsStarted && !Map.IsLCZDecontaminated)
            {
                foreach (var player in room.Players)
                {
                    if (!player.IsScp)
                        continue;
                    if (player.Role == RoleType.Scp0492)
                        continue;
                    if (!(door.NetworkTargetState || player.Role == RoleType.Scp106))
                        continue;
                    if (room.Players.Contains(player))
                        CamperPoints += 1;
                }

                if (CamperPoints > 180)
                    SpawnO5 = true;

                yield return Timing.WaitForSeconds(1);
            }

            if (SpawnO5)
                ItemType.KeycardO5.Spawn(0, new Vector3(180, 1000, -91));
        }

        private IEnumerator<float> LockStart()
        {
            yield return Timing.WaitForSeconds(0.1f);
            foreach (var player in RealPlayers.Get(RoleType.Scp106))
            {
                player.Position = new Vector3(0, -1998, 0);
                Cooldown.Add(player.Id);
                MEC.Timing.CallDelayed(25, () => Cooldown.Remove(player.Id));
            }
            while (Round.ElapsedTime.TotalSeconds < 25)
            {
                foreach (var player in RealPlayers.Get(RoleType.Scp106))
                {
                    TeleportOldMan(player, new Vector3(UnityEngine.Random.Range(-2, 2), -2000, UnityEngine.Random.Range(-2, 2)));
                }
                Gamer.Mistaken.Systems.InfoMessage.InfoMessageManager.WelcomeMessages[RoleType.Scp106] = $"{WelcomeMessage}<br>Zostaniesz wypuszczony za <color=yellow>{Mathf.RoundToInt(25 - (float)Round.ElapsedTime.TotalSeconds)}</color>s";
                yield return Timing.WaitForSeconds(1);
            }
            Gamer.Mistaken.Systems.InfoMessage.InfoMessageManager.WelcomeMessages[RoleType.Scp106] = WelcomeMessage;
            foreach (var player in RealPlayers.Get(Team.SCP))
            {
                while ((player.Position.y < -1900 || (player.Role != RoleType.Scp106 && Vector3.Distance(player.Position, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp106)) < 10)) && player.Team == Team.SCP)
                {
                    if (player.Role == RoleType.Scp106)
                    {
                        player.IsGodModeEnabled = true;
                        TeleportOldMan(player, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp106) + Vector3.down * 2);
                        yield return Timing.WaitForSeconds(10);
                        player.IsGodModeEnabled = false;

                        if (player.Position.y < 1900) player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp106);
                    }
                    else
                        player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(player.Role);
                    yield return Timing.WaitForSeconds(1);
                }
            }
        }

        private IEnumerator<float> MoveFromPocket()
        {
            yield return Timing.WaitForSeconds(5);
            int rid = RoundPlus.RoundId;
            yield return Timing.WaitForSeconds(55);
            while(RealPlayers.Any(RoleType.Scp106) && rid == RoundPlus.RoundId)
            {
                foreach (var player in RealPlayers.Get(RoleType.Scp106).Where(p => p.IsInPocketDimension))
                    player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp106);
                yield return Timing.WaitForSeconds(20);
            }
        }

        private void TeleportOldMan(Player player, Vector3 target, bool force = false)
        {
            if (!force)
            {
                if (player.Role != RoleType.Scp106)
                {
                    player.SendConsoleMessage("[106] Not 106 | Code: 1.1", "red");
                    return;
                }
                if (player.ReferenceHub.playerEffectsController.GetEffect<CustomPlayerEffects.Ensnared>().Enabled)
                {
                    player.SendConsoleMessage("[106] Ensnared active | Code: 1.2", "red");
                    return;
                }
                if (InTeleportExecution.Contains(player.Id))
                {
                    player.SendConsoleMessage("[106] Already teleporting | Code: 1.3", "red");
                    return;
                }
                if (Warhead.IsDetonated)
                {
                    player.SendConsoleMessage("[106] Detonated | Code: 1.4", "red");
                    return;
                }
                if (Cooldown.Contains(player.Id) && Round.ElapsedTime.TotalSeconds > 25)
                {
                    player.SendConsoleMessage("[106] Cooldown | Code: 1.5", "red");
                    return;
                }
            }
            InTeleportExecution.Add(player.Id);
            Vector3 oldPos = player.Position;
            MEC.Timing.CallDelayed(2, () =>
            {
                InTeleportExecution.Remove(player.Id);
                if (Warhead.IsDetonated)
                    player.Position = oldPos;
            });
            Scp106PlayerScript s106 = player.ReferenceHub.scp106PlayerScript;
            s106.NetworkportalPosition = target;
            s106.CallCmdUsePortal();
        }

        private readonly HashSet<int> InTeleportExecution = new HashSet<int>();
        private readonly HashSet<int> Cooldown = new HashSet<int>();
        private readonly Dictionary<int, List<Vector3>> LastRooms = new Dictionary<int, List<Vector3>>();
        private void Scp106_CreatingPortal(Exiled.Events.EventArgs.CreatingPortalEventArgs ev)
        {
            ev.IsAllowed = false;
            if (Round.ElapsedTime.TotalSeconds < 25)
            {
                ev.Player.SendConsoleMessage("[106] Too early | Code: 2.1", "red");
                return;
            }
            if (Cooldown.Contains(ev.Player.Id))
            {
                ev.Player.SendConsoleMessage("[106] Cooldown | Code: 2.2", "red");
                return;
            }
            TeleportOldMan(ev.Player, GetRandomRoom(ev.Player, false));
        }

        private void Scp106_Teleporting(Exiled.Events.EventArgs.TeleportingEventArgs ev)
        {
            if (ev.Player.ReferenceHub.playerEffectsController.GetEffect<CustomPlayerEffects.Ensnared>().Enabled)
            {
                ev.IsAllowed = false;
                ev.Player.SendConsoleMessage("[106] Ensnared active | Code: 3.2", "red");
                return;
            }
            if (InTeleportExecution.Contains(ev.Player.Id))
            {
                Timing.RunCoroutine(DoPostTeleport(ev.Player));
                return;
            }
            if (Round.ElapsedTime.TotalSeconds < 25)
            {
                ev.Player.SendConsoleMessage("[106] Too early | Code: 3.1", "red");
                return;
            }
            if (Warhead.IsDetonated)
            {
                ev.Player.SendConsoleMessage("[106] Detonated | Code: 3.4", "red");
                ev.IsAllowed = false;
                return;
            }
            if (Cooldown.Contains(ev.Player.Id))
            {
                ev.Player.SendConsoleMessage("[106] Cooldown | Code: 3.5", "red");
                ev.IsAllowed = false;
                return;
            }
            Vector3 newTarget = GetRandomRoom(ev.Player, true);
            ev.Player.ReferenceHub.scp106PlayerScript.NetworkportalPosition = newTarget;
            ev.PortalPosition = newTarget;
            Vector3 oldPos = ev.Player.Position;
            Timing.RunCoroutine(DoPostTeleport(ev.Player));
            MEC.Timing.CallDelayed(8f, () =>
            {
                if (Warhead.IsDetonated)
                    ev.Player.Position = oldPos;
            });
        }

        private IEnumerator<float> DoPostTeleport(Player player)
        {
            player.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Ensnared>(7.7f);
            player.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Blinded>(4);
            player.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Deafened>(4);
            if (Round.ElapsedTime.TotalSeconds > 25)
            {
                Cooldown.Add(player.Id);
                for (int i = 0; i < 15; i++)
                {
                    if (!player.IsConnected)
                        break;
                    Base.GUI.PseudoGUIHandler.Set(player, "scp106", Base.GUI.PseudoGUIHandler.Position.TOP, $"Cooldown: <color=yellow>{15 - i}</color>s");
                    yield return Timing.WaitForSeconds(1);
                }
                Cooldown.Remove(player.Id);
                Base.GUI.PseudoGUIHandler.Set(player, "scp106", Base.GUI.PseudoGUIHandler.Position.TOP, null);
            }
        }

        private Vector3 GetRandomRoom(Player player, bool sameZone)
        {
            ZoneType zone = player.CurrentRoom?.Zone ?? ZoneType.Unspecified;
            if (zone == ZoneType.LightContainment && Map.IsLCZDecontaminated)
                sameZone = false;
            Room targetRoom = RandomRoom;
            int trie = 0;
            bool first = true;
            if (!LastRooms.ContainsKey(player.Id))
                LastRooms.Add(player.Id, new List<Vector3>());
            while (!IsRoomOK(targetRoom, sameZone, zone, ref first) || (LastRooms[player.Id].Contains(targetRoom.Position) && targetRoom.Position.y < 800))
            {
                targetRoom = RandomRoom;
                trie++;
                if (trie >= 1000)
                {
                    Log.Warn("Failed to generate teleport position in 1000 tries");
                    player.SendConsoleMessage("[106] Failed to generate | Code: 4.1", "red");
                    return new Vector3(0, 1002, 0);
                }
            }

            Log.Debug($"New position is {targetRoom.Position} | {sameZone} | {zone} | {targetRoom.Zone}");
            //player.SendConsoleMessage($"[BETTER OLD MAN] New position is {targetRoom.Position} | {targetRoom.Zone} | {targetRoom.Type}", "yellow");
            LastRooms[player.Id].Add(targetRoom.Position);
            if (LastRooms[player.Id].Count > 3)
                LastRooms[player.Id].RemoveAt(0);
            if (targetRoom.Position.y > 800)
                return Random.Range(0, 2) == 1 ? targetRoom.Position : new Vector3(86, 992, -49);
            else
                return targetRoom.Position + Vector3.down * 0.2f;
        }

        private bool IsRoomOK(Room room, bool sameZone, ZoneType targetZone, ref bool first)
        {
            if (room?.gameObject == null)
                return false;
            if (first &&
                UnityEngine.Random.Range(1, 6) == 2 &&
                (
                    (
                        Round.ElapsedTime.TotalMinutes > 20 &&
                        RealPlayers.List.Where(p => p.IsAlive && p.Team != Team.SCP).Count() < 5
                    )
                    &&
                    room.Players.Count() == 0
                )
            )
                return false;
            first = false;
            if (DisallowedRoomTypes.Contains(room.Type))
                return false;
            if (sameZone && targetZone != room.Zone)
                return false;
            if (!sameZone && targetZone == room.Zone)
                return false;
            if (LightContainmentZoneDecontamination.DecontaminationController.Singleton.IsLCZDecontaminated(30) && room.Zone == ZoneType.LightContainment)
                return false;
            if (!UnityEngine.Physics.Raycast(room.Position + Vector3.up / 2, Vector3.down, 5))
                return false;
            return true;
        }
    }
}
