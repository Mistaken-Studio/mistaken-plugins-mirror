using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gamer.EventManager.EventCreator;
using UnityEngine;
using MEC;
using RemoteAdmin;
using Exiled.API.Features;
using Exiled.API.Enums;
using Gamer.EventManager;

namespace CW
{
    public class CodeWhite : IEMEventClass
    {
        public override string Name { get; set; } = "CodeWhite";

        public override string Id => "cw";

        public override string Description { get; set; } = "Code White";
        public override Version Version => new Version(4, 0, 0);

        public override Dictionary<string, string> Translations => new Dictionary<string, string>
        {
            //{ "", "" }
        };
        bool Escaped = false;
        bool dt = false;
        public Player scientist = null;

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += Player_ChangingRole;
            GameObject.FindObjectOfType<Respawning.RespawnManager>().enabled = false;
            Round.IsLocked = true;
            var doors = Map.Doors;
            foreach (Door door in doors)
            {
                if (door.DoorName == "GATE_A" || door.DoorName == "GATE_B" || door.DoorName == "NUKE_SURFACE" || door.DoorName == "106_PRIMARY" || door.DoorName == "106_SECONDARY" || door.DoorName == "106_BOTTOM")
                {
                    door.Networklocked = true;
                    door.NetworkisOpen = false;
                }
                else if (door.DoorName == "049_ARMORY")
                {
                    door.NetworkisOpen = true;
                    door.Networklocked = true;
                }
            }
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= Player_ChangingRole;
        }

        public override void Register()
        {
        }

        private void Server_RoundStarted()
        {
            var doors = Map.Doors.ToList();
            Timing.CallDelayed(120, () =>
            {
                Cassie.Message("PITCH_.45 .G2 PITCH_0.94 ATTENTION ALL MTFUNIT NU 7 . GATE A AND B LOCKDOWN DEACTIVATED .G2 DETECTED CODE WHITE PITCH_0.1 .G3", false, true);
                foreach (Door door in doors)
                {
                    if (door.DoorName == "GATE_A" || door.DoorName == "GATE_B")
                    {
                        door.Networklocked = false;
                        door.NetworkisOpen = true;
                    }
                }
            });
            Timing.CallDelayed(2, () =>
            {
                List<string> scirooms = new List<string>();
                scirooms.Add("096");
                scirooms.Add("049_ARMORY");
                int rep = 0;
                var players = Player.List.Count();
                var nofaggs = Player.List.Where(p => p.Role == RoleType.Scientist || p.Role == RoleType.FacilityGuard || p.Role == RoleType.ChaosInsurgency).ToArray();
                var notenoughplayers = Player.List;

                //if (players == 1)
                //{
                //DevMode = true;
                //var player = notenoughplayers[0];
                //player.SlowChangeRole(RoleType.Scientist);
                //player.Broadcast(10, "<color=red>Dyrektor</color> uciekł w bezpieczne miejsce i nakazał podesłać wsparcie <color=blue>MFO</color>. Ty jesteś wsparciem.");
                //player.Broadcast(10, "Jesteś <color=green>Rebeliantem Chaosu</color>. Twoim zadaniem jest zabicie <color=red>Dyrektora</color> i odparcie ataku/ów <color=blue>MFO</color>.");
                //player.Broadcast(10, "Jesteś <color=red>Dyrektorem Placówki</color>. Twoim zadaniem jest ucieczka z placówki. <color=green>Rebelia Chaosu</color> chce twojej śmierci.");
                //}
                if (players < 4)
                {
                    OnEnd();
                }
                if (players > 4)
                {
                    foreach (Player player in Player.List.Where(p => p.Role == RoleType.ClassD || p.Team == Team.SCP))
                    {
                        player.SlowChangeRole(RoleType.ChaosInsurgency, doors.Find(d => d.DoorName == "173").transform.position + Vector3.up * 2);
                    }
                    scientist = nofaggs[UnityEngine.Random.Range(0, nofaggs.Count())];
                    scientist.SlowChangeRole(RoleType.Scientist, doors.Find(d => d.DoorName == scirooms[UnityEngine.Random.Range(0, 2)]).transform.position + Vector3.up * 2);

                    foreach (Player faggot in nofaggs)
                    {
                        if (scientist.Id != faggot.Id)
                        {
                            switch (UnityEngine.Random.Range(0, 3))
                            {
                                case 0:
                                    faggot.SlowChangeRole(RoleType.NtfCadet);
                                    break;
                                case 1:
                                    faggot.SlowChangeRole(RoleType.NtfLieutenant);
                                    break;
                                case 2:
                                    faggot.SlowChangeRole(RoleType.NtfCommander);
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (Player player in notenoughplayers)
                    {
                        if (rep < 2)
                        {
                            player.SlowChangeRole(RoleType.ChaosInsurgency, doors.Find(d => d.DoorName == "173").transform.position + Vector3.up * 2);
                            rep++;
                        }
                        else if (rep == 2)
                        {
                            player.SlowChangeRole(RoleType.NtfLieutenant);
                            rep++;
                        }
                        else if (rep != 0 && rep == 3)
                        {
                            player.SlowChangeRole(RoleType.Scientist, doors.Find(d => d.DoorName == scirooms[UnityEngine.Random.Range(0, 2)]).transform.position + Vector3.up * 2);
                        }
                    }
                }
                Timing.CallDelayed(5, () =>
                {
                    foreach (Player p in Player.List)
                    {
                        if (p.Role == RoleType.Scientist)
                        {
                            var items = p.Inventory.items.Where(i => i.id == ItemType.KeycardScientist);
                            foreach (var item in items)
                            {
                                p.RemoveItem(item);
                            }
                            p.AddItem(ItemType.KeycardFacilityManager);
                            p.AddItem(ItemType.GunUSP);
                            p.AddItem(ItemType.Medkit);
                            p.Ammo[(int)AmmoType.Nato9] = 18;
                        }
                        if (p.Role == RoleType.NtfCadet || p.Role == RoleType.NtfLieutenant || p.Role == RoleType.NtfCommander)
                        {
                            var items = p.Inventory.items.Where(i => i.id == ItemType.KeycardNTFCommander || i.id == ItemType.KeycardNTFLieutenant || i.id == ItemType.KeycardSeniorGuard || i.id == ItemType.Disarmer);
                            foreach (var item in items)
                            {
                                p.RemoveItem(item);
                            }
                            p.AddItem(ItemType.KeycardO5);
                        }
                        if (p.Role == RoleType.ChaosInsurgency)
                        {
                            var items = p.Inventory.items.Where(i => i.id == ItemType.KeycardChaosInsurgency || i.id == ItemType.Disarmer);
                            foreach (var item in items)
                            {
                                p.RemoveItem(item);
                            }
                            p.AddItem(ItemType.KeycardO5);
                            p.AddItem(ItemType.Radio);
                        }
                    }
                });
            });
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            var doors = Map.Doors.ToList();
            var players = Player.List;
            if (ev.Player.Role == RoleType.ChaosInsurgency)
            {
                ev.Player.Broadcast(10, "Jesteś <color=green>Rebeliantem Chaosu</color>. Twoim zadaniem jest zabicie <color=red>Dyrektora</color> i odparcie ataku/ów <color=blue>MFO</color>.");
            }
            if (ev.Player.Role == RoleType.Scientist && ev.Player.Id == scientist.Id)
            {
                ev.Player.Broadcast(10, "Jesteś <color=red>Dyrektorem Placówki</color>. Twoim zadaniem jest ucieczka z placówki. <color=green>Rebelia Chaosu</color> chce twojej śmierci.");
            }
            if (Player.List.Where(p => p.Role == RoleType.Scientist).Count() == 0 && ev.Player.Role != RoleType.NtfScientist && !Escaped && !dt)
            {
                Cassie.Message("PITCH_.45.G2 PITCH_0.94 FACILITY SCAN INITIATED. . . . . .G2 SCAN COMPLETED.FACILITY MANAGER TERMINATED BELL_END", false, true);
                dt = true;
            }
            if (ev.Player.Role == RoleType.NtfScientist)
            {
                Escaped = true;
                var specplayers = Player.List.Where(p => p.Role == RoleType.Spectator);
                ev.Player.SlowChangeRole(RoleType.Spectator);
                int i = 0;
                foreach (Player faggot in specplayers)
                {
                    if (i < 7)
                        switch (UnityEngine.Random.Range(0, 3))
                        {
                            case 0:
                                faggot.SlowChangeRole(RoleType.NtfCadet);
                                break;
                            case 1:
                                faggot.SlowChangeRole(RoleType.NtfLieutenant);
                                break;
                            case 2:
                                faggot.SlowChangeRole(RoleType.NtfCommander);
                                //op plz nerf
                                break;
                        }
                    i++;
                }
                Round.IsLocked = false;
            }
        }
    }
}