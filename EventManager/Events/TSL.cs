using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.EventManager;
using Gamer.EventManager.EventCreator;
using Gamer.Utilities;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EventManager.Events
{
    class TSL : IEMEventClass
    {
        public override string Name { get; set; } = "TSL";

        public override string Id => "tsl";

        public override string Description { get; set; } = "Trouble in Secret Laboratory";
        public override Gamer.EventManager.EventCreator.Version Version => new Gamer.EventManager.EventCreator.Version(4, 0, 0);

        public override Dictionary<string, string> Translations => new Dictionary<string, string>
        {
            { "I", "Jesteś <color=lime>Niewinnym</color>. Wspólnie z <color=#00B7EB>Detektywem/ami</color> musicie odgadnąć kto jest <color=red>Zdrajcą</color>." },
            { "T", "Jesteś <color=red>Zdrajcą</color>. Twoim zadaniem jest zabicie wszystkich <color=lime>Niewinnych</color> i <color=#00B7EB>Detektywów</color>. Dostęp do sklepu pod komendą .tshop" },
            { "D", "Jesteś <color=#00B7EB>Detektywem</color>. Musisz odgadnąć kto jest <color=red>Zdrajcą</color> pośród <color=lime>Niewinnych</color>. Pod ` więcej informacji." },
            { "DC", "Jesteś <color=#00B7EB>Detektywem</color>. Musisz odgadnąć kto jest <color=red>Zdrajcą</color> pośród <color=green>Niewinnych</color>. Jeśli podejrzewasz kogoś o bycie <color=red>Zdrajcą</color>, zwiąż go lub zabij. Dostęp do sklepu pod komendą .tshop" }
        };

        public Dictionary<int, int> pb = new Dictionary<int, int>();
        public Dictionary<int, string> armor = new Dictionary<int, string>();
        public readonly List<int> ATTD = new List<int>();
        public readonly List<int> Dcooldown = new List<int>();
        public readonly List<int> Bcooldown = new List<int>();
        public readonly List<int> innocents = new List<int>();
        public readonly List<int> traitors = new List<int>();
        public readonly List<int> detectives = new List<int>();

        public override void OnIni()
        {
            Exiled.Events.Handlers.Player.Shooting += Player_Shooting;
            Exiled.Events.Handlers.Player.Died += Player_Died;
            Exiled.Events.Handlers.Player.MedicalItemUsed += Player_MedicalItemUsed;
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += Player_ChangingRole;
            Exiled.Events.Handlers.Player.Hurting += Player_Hurting;
            Exiled.Events.Handlers.Scp914.UpgradingItems += Scp914_UpgradingItems;
            Exiled.Events.Handlers.Player.ItemDropped += Player_ItemDropped;
            Exiled.Events.Handlers.Map.ExplodingGrenade += Map_ExplodingGrenade;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Player.Shooting -= Player_Shooting;
            Exiled.Events.Handlers.Player.Died -= Player_Died;
            Exiled.Events.Handlers.Player.MedicalItemUsed -= Player_MedicalItemUsed;
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= Player_ChangingRole;
            Exiled.Events.Handlers.Player.Hurting -= Player_Hurting;
            Exiled.Events.Handlers.Scp914.UpgradingItems -= Scp914_UpgradingItems;
            Exiled.Events.Handlers.Player.ItemDropped -= Player_ItemDropped;
            Exiled.Events.Handlers.Map.ExplodingGrenade -= Map_ExplodingGrenade;
        }

        public override void Register()
        {
        }

        private void Server_RoundStarted()
        {
            #region ini
            Pickup.Instances.ForEach(x => x.Delete());
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Gamer.Mistaken.Systems.Utilities.API.Map.RespawnLock = true;
            Round.IsLocked = true;
            foreach (var door in Map.Doors)
            {
                var doorType = door.Type();
                if (doorType == DoorType.CheckpointLczA || doorType == DoorType.CheckpointLczB)
                    door.ServerChangeLock(DoorLockReason.DecontLockdown, true);
                else if (doorType == DoorType.Scp012 || doorType == DoorType.Scp914 || doorType == DoorType.Scp012Bottom || doorType == DoorType.LczArmory)
                {
                    door.NetworkTargetState = true;
                    door.ServerChangeLock(DoorLockReason.AdminCommand, true);
                }
            }
            foreach (var e in Map.Lifts)
            {
                if (e.elevatorName.StartsWith("El"))
                    e.Network_locked = true;
            }
            #endregion
            var players = Gamer.Utilities.RealPlayers.List;
            var nop = players.Count();
            traitors.Clear();
            detectives.Clear();
            innocents.Clear();
            pb.Clear();
            ATTD.Clear();
            armor.Clear();
            Ranks();
            Randweapon();
            Dru();
            if (nop > 4)
            {
                int nod = 0;
                int not = 0;
                int noi = 0;
                double t;
                double d;
                double i;
                t = nop * 0.25;
                d = nop * 0.13;
                t = System.Math.Round(t);
                d = System.Math.Round(d);
                i = nop - t - d;
                foreach (Player player in players)
                {
                    switch (UnityEngine.Random.Range(0, 3))
                    {
                        case 0:
                            if (t != not)
                            {
                                traitors.Add(player.Id);
                                player.SlowChangeRole(RoleType.ClassD);
                                not++;
                            }
                            else if (d != nod)
                            {
                                detectives.Add(player.Id);
                                player.SlowChangeRole(RoleType.FacilityGuard);
                                nod++;
                            }
                            else if (i != noi)
                            {
                                innocents.Add(player.Id);
                                player.SlowChangeRole(RoleType.ClassD);
                                noi++;
                            }
                            break;
                        case 1:
                            if (d != nod)
                            {
                                detectives.Add(player.Id);
                                player.SlowChangeRole(RoleType.FacilityGuard);
                                nod++;
                            }
                            else if (i != noi)
                            {
                                innocents.Add(player.Id);
                                player.SlowChangeRole(RoleType.ClassD);
                                noi++;
                            }
                            else if (t != not)
                            {
                                traitors.Add(player.Id);
                                player.SlowChangeRole(RoleType.ClassD);
                                not++;
                            }
                            break;
                        case 2:
                            if (i != noi)
                            {
                                innocents.Add(player.Id);
                                player.SlowChangeRole(RoleType.ClassD);
                                noi++;
                            }
                            else if (t != not)
                            {
                                traitors.Add(player.Id);
                                player.SlowChangeRole(RoleType.ClassD);
                                not++;
                            }
                            else if (d != nod)
                            {
                                detectives.Add(player.Id);
                                player.SlowChangeRole(RoleType.FacilityGuard);
                                nod++;
                            }
                            break;
                    }
                }
                Credits();
            }
            else if (nop == 1)
            {
                foreach (Player player in players)
                {
                    innocents.Add(player.Id);
                    traitors.Add(player.Id);
                    detectives.Add(player.Id);
                    player.SlowChangeRole(RoleType.FacilityGuard);
                    Credits();
                }
            }
            else if (nop == 2)
            {
                int i = 0;
                foreach (Player player in players)
                {
                    if (i == 0)
                    {
                        detectives.Add(player.Id);
                        player.SlowChangeRole(RoleType.FacilityGuard);
                        i++;
                    }
                    else
                    {
                        traitors.Add(player.Id);
                        player.SlowChangeRole(RoleType.ClassD);
                    }
                }
                Credits();
            }
            else if (nop == 3)
            {
                int i = 0;
                foreach (Player player in players)
                {
                    if (i == 0)
                    {
                        detectives.Add(player.Id);
                        player.SlowChangeRole(RoleType.FacilityGuard);
                        i++;
                    }
                    else if (i == 1)
                    {
                        traitors.Add(player.Id);
                        player.SlowChangeRole(RoleType.ClassD);
                        i++;
                    }
                    else
                    {
                        innocents.Add(player.Id);
                        player.SlowChangeRole(RoleType.ClassD);
                    }
                }
                Credits();
            }
            else if (nop == 4)
            {
                foreach (Player player in players)
                {
                    if (player.Role == RoleType.ClassD)
                    {
                        innocents.Add(player.Id);
                        player.SlowChangeRole(RoleType.ClassD);
                    }
                    if (player.Role == RoleType.FacilityGuard)
                    {
                        traitors.Add(player.Id);
                        player.SlowChangeRole(RoleType.ClassD);
                    }
                    if (player.Team == Team.SCP)
                    {
                        detectives.Add(player.Id);
                        player.SlowChangeRole(RoleType.FacilityGuard);
                    }
                }
                Credits();
            }
            Timing.CallDelayed(3, () =>
            {
                foreach (Player player in players)
                {
                    if (detectives.Contains(player.Id))
                    {
                        player.ClearInventory();
                        player.AddItem(ItemType.Disarmer);
                        player.AddItem(ItemType.Medkit);
                        player.Ammo[(int)AmmoType.Nato556] = 30;
                        player.Ammo[(int)AmmoType.Nato762] = 30;
                        player.Ammo[(int)AmmoType.Nato9] = 30;
                    }
                    if (traitors.Contains(player.Id))
                    {
                        player.AddItem(ItemType.SCP500);
                        player.Ammo[(int)AmmoType.Nato556] = 30;
                        player.Ammo[(int)AmmoType.Nato762] = 30;
                        player.Ammo[(int)AmmoType.Nato9] = 30;
                    }
                    if (innocents.Contains(player.Id))
                    {
                        player.AddItem(ItemType.Medkit);
                        player.Ammo[(int)AmmoType.Nato556] = 0;
                        player.Ammo[(int)AmmoType.Nato762] = 0;
                        player.Ammo[(int)AmmoType.Nato9] = 0;
                    }
                }
            });
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            var rooms = Map.Rooms.Where(i => i.Zone == ZoneType.LightContainment).ToList();
            var room = rooms[UnityEngine.Random.Range(0, rooms.Count())];
            Timing.CallDelayed(1, () =>
            {
                ev.Player.Position = room.Position + Vector3.up * 2;
            });
            Timing.CallDelayed(3, () =>
            {
                if (innocents.Contains(ev.Player.Id))
                {
                    ev.Player.Broadcast(10, Gamer.EventManager.EventManager.EMLB + Translations["I"]);
                    ev.Player.SendConsoleMessage(Translations["I"], "yellow");
                }
                if (traitors.Contains(ev.Player.Id))
                {
                    ev.Player.Broadcast(10, Gamer.EventManager.EventManager.EMLB + Translations["T"]);
                    ev.Player.SendConsoleMessage(Translations["T"], "yellow");
                    List<string> traitornames = new List<string>();
                    if (traitors.Count() > 1)
                    {
                        foreach (var item in traitors)
                        {
                            traitornames.Add(Player.Get(item).Nickname);
                        }
                        ev.Player.SendConsoleMessage("Twoimi towarzyszami są " + string.Join("\n", traitornames), "red");
                    }
                }
                if (detectives.Contains(ev.Player.Id))
                {
                    ev.Player.RankName = "Detektyw";
                    ev.Player.RankColor = "cyan";
                    ev.Player.Broadcast(10, Gamer.EventManager.EventManager.EMLB + Translations["D"]);
                    ev.Player.SendConsoleMessage(Translations["DC"], "yellow");
                }
            });
        }

        private void Player_ItemDropped(Exiled.Events.EventArgs.ItemDroppedEventArgs ev)
        {
            if (ev.Pickup.ItemId == ItemType.Flashlight)
            {
                Timing.CallDelayed(30, () =>
                {
                    foreach (var item in GameObject.FindObjectsOfType<Pickup>())
                    {
                        if (item.itemId == ItemType.Flashlight)
                        {
                            var grenadeManager = Server.Host.GameObject.GetComponent<Grenades.GrenadeManager>();
                            Grenades.GrenadeSettings settings = grenadeManager.availableGrenades[0];
                            Grenades.Grenade component = UnityEngine.Object.Instantiate<UnityEngine.GameObject>(settings.grenadeInstance).GetComponent<Grenades.Grenade>();
                            grenadeManager.transform.position = item.transform.position;
                            component.InitData(grenadeManager, UnityEngine.Vector3.zero, UnityEngine.Vector3.down);
                            NetworkServer.Spawn(component.gameObject);
                            item.Delete();
                        }
                    }
                });
            }
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.DamageType != DamageTypes.None && armor.ContainsKey(ev.Target.Id))
            {
                if (armor[ev.Target.Id] == "armor")
                {
                    ev.Amount *= 0.9F;
                }
                else if (armor[ev.Target.Id] == "harmor")
                {
                    if (detectives.Contains(ev.Target.Id))
                    {
                        ev.Amount *= 0.7F;
                    }
                    else if (traitors.Contains(ev.Target.Id))
                    {
                        ev.Amount *= 0.65F;
                    }
                }
            }
            Timing.CallDelayed(0, () =>
            {
                if (ev.Target.Health < 100 && ev.Target.Health >= 80 && ev.Target.Role != RoleType.Spectator && !detectives.Contains(ev.Target.Id))
                {
                    ev.Target.RankName = "Lekko Ranny";
                    ev.Target.RankColor = "yellow";
                }
                else if (ev.Target.Health < 80 && ev.Target.Health >= 55 && ev.Target.Role != RoleType.Spectator && !detectives.Contains(ev.Target.Id))
                {
                    ev.Target.RankName = "Ranny";
                    ev.Target.RankColor = "orange";
                }
                else if (ev.Target.Health < 55 && ev.Target.Health >= 20 && ev.Target.Role != RoleType.Spectator && !detectives.Contains(ev.Target.Id))
                {
                    ev.Target.RankName = "Poważnie Ranny";
                    ev.Target.RankColor = "red";
                }
                else if (ev.Target.Health < 20 && ev.Target.Health != 0 && ev.Target.Role != RoleType.Spectator && !detectives.Contains(ev.Target.Id))
                {
                    ev.Target.RankName = "Bliski Śmierci";
                    ev.Target.RankColor = "nickel";
                }
            });
        }

        private void Player_MedicalItemUsed(Exiled.Events.EventArgs.UsedMedicalItemEventArgs ev)
        {
            Timing.CallDelayed(0, () =>
            {
                if (ev.Player.Health == 100 && !detectives.Contains(ev.Player.Id))
                {
                    ev.Player.RankName = "";
                    ev.Player.RankColor = "Default";
                }
                else if (ev.Player.Health < 100 && ev.Player.Health >= 80 && ev.Player.Role != RoleType.Spectator && !detectives.Contains(ev.Player.Id))
                {
                    ev.Player.RankName = "Lekko Ranny";
                    ev.Player.RankColor = "yellow";
                }
                else if (ev.Player.Health < 80 && ev.Player.Health >= 55 && ev.Player.Role != RoleType.Spectator && !detectives.Contains(ev.Player.Id))
                {
                    ev.Player.RankName = "Ranny";
                    ev.Player.RankColor = "orange";
                }
                else if (ev.Player.Health < 55 && ev.Player.Health >= 20 && ev.Player.Role != RoleType.Spectator && !detectives.Contains(ev.Player.Id))
                {
                    ev.Player.RankName = "Poważnie Ranny";
                    ev.Player.RankColor = "red";
                }
                else if (ev.Player.Health < 20 && ev.Player.Health != 0 && ev.Player.Role != RoleType.Spectator && !detectives.Contains(ev.Player.Id))
                {
                    ev.Player.RankName = "Bliski Śmierci";
                    ev.Player.RankColor = "nickel";
                }
            });
        }

        private void Player_Shooting(Exiled.Events.EventArgs.ShootingEventArgs ev)
        {
            if (ev.Target != null)
            {
                if (ev.Shooter.CurrentItem.id == ItemType.GunUSP && Player.Get(ev.Target).Role != RoleType.Spectator)
                {
                    Player.Get(ev.Target).Hurt(200, ev.Shooter, DamageTypes.Usp);
                    ev.Shooter.RemoveItem(ev.Shooter.Inventory.items.First(i => i.id == ItemType.GunUSP));
                }
                else if (ev.Shooter.CurrentItem.id == ItemType.GunCOM15 && Player.Get(ev.Target).Role != RoleType.Spectator)
                    Player.Get(ev.Target).Hurt(36, ev.Shooter, DamageTypes.Com15);
            }
            else if (ev.Shooter.CurrentItem.id == ItemType.GunUSP)
                ev.Shooter.RemoveItem(ev.Shooter.Inventory.items.First(i => i.id == ItemType.GunUSP));
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            ev.Target.RankName = "";
            ev.Target.RankColor = "default";
            var players = Gamer.Utilities.RealPlayers.List;
            if (detectives.Contains(ev.Target.Id))
            {
                detectives.Remove(ev.Target.Id);
                foreach (Player player in players)
                {
                    if (traitors.Contains(player.Id))
                    {
                        pb[player.Id]++;
                        player.Broadcast(2, "Otrzymałeś 1 kredyt");
                    }
                }
            }
            else if (traitors.Contains(ev.Target.Id))
            {
                traitors.Remove(ev.Target.Id);
                foreach (Player player in players)
                {
                    if (detectives.Contains(player.Id))
                    {
                        pb[player.Id]++;
                        player.Broadcast(2, "Otrzymałeś 1 kredyt");
                    }
                }
            }
            else if (innocents.Contains(ev.Target.Id))
            {
                innocents.Remove(ev.Target.Id);
                foreach (Player player in players)
                {
                    if (traitors.Contains(player.Id))
                    {
                        pb[player.Id]++;
                        player.Broadcast(2, "Otrzymałeś 1 kredyt");
                    }
                }
            }
            if (innocents.Count() == 0 && detectives.Count() == 0 && traitors.Count() != 0)
            {
                OnEnd("<color=red>Traitors</color>");
                Round.IsLocked = false;
            }
            else if (innocents.Count() != 0 && detectives.Count() != 0 && traitors.Count() == 0)
            {
                OnEnd("<color=green>Innocents</color>");
                Round.IsLocked = false;
            }
            else if (innocents.Count() == 0 && detectives.Count() != 0 && traitors.Count() == 0)
            {
                OnEnd("<color=green>Innocents</color>");
                Round.IsLocked = false;
            }
            else if (innocents.Count() != 0 && detectives.Count() == 0 && traitors.Count() == 0)
            {
                OnEnd("<color=green>Innocents</color>");
                Round.IsLocked = false;
            }
        }

        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (ev.IsFrag)
            {
                foreach (var player in Gamer.Utilities.RealPlayers.List)
                {
                    if (Vector3.Distance(ev.Grenade.transform.position, player.Position) <= 30)
                        player.Kill(DamageTypes.Grenade);
                }
            }
        }

        private void Scp914_UpgradingItems(Exiled.Events.EventArgs.UpgradingItemsEventArgs ev)
        {
            if (ev.KnobSetting == Scp914.Scp914Knob.OneToOne && ev.Players.Count() == 1)
            {
                if (traitors.Contains(ev.Players.ToArray()[0].Id) && !ATTD.Contains(ev.Players.ToArray()[0].Id))
                    Map.Broadcast(5, $"<color=red>{ev.Players.ToArray()[0].Nickname} jest Zdrajcą</color>");
                else if (innocents.Contains(ev.Players.ToArray()[0].Id))
                    Map.Broadcast(5, $"<color=lime>{ev.Players.ToArray()[0].Nickname} jest Niewinnym</color>");
                else if (traitors.Contains(ev.Players.ToArray()[0].Id) && ATTD.Contains(ev.Players.ToArray()[0].Id))
                {
                    Map.Broadcast(5, $"<color=lime>{ev.Players.ToArray()[0].Nickname} jest Niewinnym</color>");
                    ATTD.Remove(ev.Players.ToArray()[0].Id);
                }
            }
        }

        private void Dru()
        {
            var players = Gamer.Utilities.RealPlayers.List;
            foreach (Player player in players)
            {
                if (detectives.Contains(player.Id))
                {
                    player.RankName = "Detektyw";
                    player.RankColor = "cyan";
                }
            }
            if (!Round.IsStarted) return;
            Timing.CallDelayed(5, () =>
            {
                Dru();
            });
        }
        private void Randweapon()
        {
            var rooms = Map.Rooms;
            foreach (var room in rooms)
            {
                if (room.Zone == ZoneType.LightContainment)
                {
                    var random = UnityEngine.Random.Range(0, 7);
                    switch (random)
                    {
                        case 0:
                            ItemType.GunProject90.Spawn(50, room.Position + Vector3.up * 2);
                            break;
                        case 1:
                            ItemType.GunE11SR.Spawn(40, room.Position + Vector3.up * 2);
                            break;
                        case 2:
                            ItemType.GunMP7.Spawn(35, room.Position + Vector3.up * 2);
                            break;
                        case 3:
                            ItemType.GunLogicer.Spawn(75, room.Position + Vector3.up * 2);
                            break;
                        case 4:
                            ItemType.Medkit.Spawn(2, room.Position + Vector3.up * 2);
                            break;
                        case 5:
                            ItemType.Painkillers.Spawn(5, room.Position + Vector3.up * 2);
                            break;
                        case 6:
                            ItemType.WeaponManagerTablet.Spawn(float.MaxValue, room.Position + Vector3.up * 2);
                            break;
                    }
                }
            }
        }

        private void Ranks()
        {
            var players = Gamer.Utilities.RealPlayers.List;
            foreach (Player player in players)
            {
                if (player.Health == 100 && !detectives.Contains(player.Id))
                {
                    player.RankName = "";
                    player.RankColor = "default";
                }
                else if (player.Health < 100 && player.Health >= 80 && player.Role != RoleType.Spectator && !detectives.Contains(player.Id))
                {
                    player.RankName = "Lekko Ranny";
                    player.RankColor = "yellow";
                }
                else if (player.Health < 80 && player.Health >= 55 && player.Role != RoleType.Spectator && !detectives.Contains(player.Id))
                {
                    player.RankName = "Ranny";
                    player.RankColor = "orange";
                }
                else if (player.Health < 55 && player.Health >= 20 && player.Role != RoleType.Spectator && !detectives.Contains(player.Id))
                {
                    player.RankName = "Poważnie Ranny";
                    player.RankColor = "red";
                }
                else if (player.Health < 20 && player.Health != 0 && player.Role != RoleType.Spectator && !detectives.Contains(player.Id))
                {
                    player.RankName = "Bliski Śmierci";
                    player.RankColor = "nickel";
                }
            }
            if (!Round.IsStarted) return;
            Timing.CallDelayed(5, () =>
                {
                    Ranks();
                });

        }
        public void Credits()
        {
            var players = Gamer.Utilities.RealPlayers.List;
            foreach (Player player in players)
            {
                if (traitors.Contains(player.Id) || detectives.Contains(player.Id))
                {
                    pb.Add(player.Id, 1);
                }
            }
        }

        public string TraitorEventsManager(Player sender, string args)
        {
            switch (args)
            {
                case "LOCKDOWN":
                    if (traitors.Contains(sender.Id))
                    {
                        if (pb[sender.Id] < 2)
                            return $"You need to get {2 - pb[sender.Id]} credits";
                        else
                        {
                            int a = pb[sender.Id];
                            a = a - 2;
                            pb[sender.Id] = a;
                            return "You have locked 914 and 012 for 10 seconds";
                        }
                    }
                    else return "You must be a traitor to buy this";
                case "BLACKOUT":
                    if (traitors.Contains(sender.Id))
                    {
                        if (Bcooldown.Contains(sender.Id))
                            return "You need to wait 95 seconds before using this command again";
                        else
                        {
                            Map.TurnOffAllLights(15);
                            Bcooldown.Add(sender.Id);
                            Timing.CallDelayed(95, () =>
                            {
                                Bcooldown.Remove(sender.Id);
                            });
                            return "You have turned off the lights for 15 seconds";
                        }
                    }
                    else return "You must be a traitor to buy this";
                case "DOORRESET":
                    if (traitors.Contains(sender.Id))
                    {
                        if (Dcooldown.Contains(sender.Id))
                            return "You need to wait 50 seconds before using this command again";
                        else
                        {
                            foreach (var door in Map.Doors)
                            {
                                door.NetworkTargetState = false;
                            }
                            return "You have closed all doors in LCZ";
                        }
                    }
                    else return "You must be a traitor to buy this";
                default:
                    return "Error 404";
            }
        }
    }
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class ShopCommandHandler : IBetterCommand
    {
        public override string Command => "traitorShop";

        public override string[] Aliases => new string[] { "tshop" };

        public override string Description => "Traitor Shop";

        private string Execute(Player sender, string[] args, int credits)
        {
            if (args.Length == 0)
                return GetUsage(credits);
            switch (args[0].ToUpper())
            {
                case "ATTD":
                    if (credits < 2)
                        return $"You have not enough credits, you have to get {2 - credits} more";
                    else
                    {
                        if ((Gamer.EventManager.EventManager.ActiveEvent as TSL).traitors.Contains(sender.Id))
                        {
                            if ((Gamer.EventManager.EventManager.ActiveEvent as TSL).ATTD.Contains(sender.Id))
                                return "you already bought that item";
                            int a = (Gamer.EventManager.EventManager.ActiveEvent as TSL).pb[sender.Id];
                            a = a - 2;
                            (Gamer.EventManager.EventManager.ActiveEvent as TSL).pb[sender.Id] = a;
                            (Gamer.EventManager.EventManager.ActiveEvent as TSL).ATTD.Add(sender.Id);
                            return "You have bought ATTD";
                        }
                        else return "You must be a traitor to buy this ability";
                    }
                case "ARMOR":
                    if (credits < 1)
                        return $"You have not enough credits, you have to get {1 - credits} more";
                    else
                    {
                        if ((Gamer.EventManager.EventManager.ActiveEvent as TSL).armor.ContainsKey(sender.Id))
                            if ((Gamer.EventManager.EventManager.ActiveEvent as TSL).armor[sender.Id] == "harmor" || (Gamer.EventManager.EventManager.ActiveEvent as TSL).armor[sender.Id] == "armor")
                                return "You already bought that item";
                        int a = (Gamer.EventManager.EventManager.ActiveEvent as TSL).pb[sender.Id];
                        a = a - 1;
                        (Gamer.EventManager.EventManager.ActiveEvent as TSL).pb[sender.Id] = a;
                        (Gamer.EventManager.EventManager.ActiveEvent as TSL).armor.Add(sender.Id, "armor");
                        return "You have bought Armor";
                    }
                case "HARMOR":
                    if (credits < 3)
                        return $"You have not enough credits, you have to get {3 - credits} more";
                    else
                    {
                        if ((Gamer.EventManager.EventManager.ActiveEvent as TSL).armor.ContainsKey(sender.Id))
                            if ((Gamer.EventManager.EventManager.ActiveEvent as TSL).armor[sender.Id] == "harmor" || (Gamer.EventManager.EventManager.ActiveEvent as TSL).armor[sender.Id] == "armor")
                                return "You already bought that item";
                        int a = (Gamer.EventManager.EventManager.ActiveEvent as TSL).pb[sender.Id];
                        a = a - 3;
                        (Gamer.EventManager.EventManager.ActiveEvent as TSL).pb[sender.Id] = a;
                        (Gamer.EventManager.EventManager.ActiveEvent as TSL).armor.Add(sender.Id, "harmor");
                        return "You have bought Harmor";
                    }
                case "LOCKDOWN":
                    return (Gamer.EventManager.EventManager.ActiveEvent as TSL).TraitorEventsManager(sender, "LOCKDOWN");
                case "BLACKOUT":
                    return (Gamer.EventManager.EventManager.ActiveEvent as TSL).TraitorEventsManager(sender, "BLACKOUT");
                case "DOORRESET":
                    return (Gamer.EventManager.EventManager.ActiveEvent as TSL).TraitorEventsManager(sender, "DOORRESET");
                case "C4":
                    return RequestItemT(sender, credits, ItemType.Flashlight, 2);
                case "SCP018":
                    return RequestItemT(sender, credits, ItemType.SCP018, 2);
                case "USP":
                    return RequestItemT(sender, credits, ItemType.GunCOM15, 2);
                case "GD":
                case "GOLDENDEAGLE":
                    return RequestItemD(sender, credits, ItemType.GunUSP, 1);
                case "ADRENALINE":
                    return RequestItemD(sender, credits, ItemType.Adrenaline, 1);
                case "SCP500":
                    return RequestItemD(sender, credits, ItemType.SCP500, 1);
                default:
                    return GetUsage(credits);
            }
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (!(Gamer.EventManager.EventManager.ActiveEvent is TSL)) return new string[] { "This event is not running at the moment" };
            if ((Gamer.EventManager.EventManager.ActiveEvent as TSL).detectives.Contains(player.Id) || (Gamer.EventManager.EventManager.ActiveEvent as TSL).traitors.Contains(player.Id))
            {
                player.SendConsoleMessage("TShop| " + Execute(player, args, (Gamer.EventManager.EventManager.ActiveEvent as TSL).pb[player.Id]), "green");
            }
            else return new string[] { "You are not a <color=red>Traitor</color> nor <color=#00B7EB>Detective</color> to use the shop" };
            success = true;
            return new string[] { "Done" };
        }

        private string RequestItemT(Player sender, int credits, ItemType item, int cost)
        {
            if (credits < cost)
                return $"You have not enough credits, you have to get {cost - credits} more";
            else if ((Gamer.EventManager.EventManager.ActiveEvent as TSL).traitors.Contains(sender.Id) && item != ItemType.GunCOM15)
            {
                sender.AddItem(item);
                (Gamer.EventManager.EventManager.ActiveEvent as TSL).pb[sender.Id] = credits - cost;
            }
            else if ((Gamer.EventManager.EventManager.ActiveEvent as TSL).traitors.Contains(sender.Id) && item == ItemType.GunCOM15)
            {
                sender.AddItem(new Inventory.SyncItemInfo
                {
                    id = item,
                    durability = 12,
                    modSight = 0,
                    modBarrel = 1,
                    modOther = 0
                });
                (Gamer.EventManager.EventManager.ActiveEvent as TSL).pb[sender.Id] = credits - cost;
            }
            else return "You are not a <color=red>Traitor</color> to buy this item";
            return $"You have bought {ShortNames(item)}";
        }

        private string RequestItemD(Player sender, int credits, ItemType item, int cost)
        {
            if (credits < cost)
                return $"You have not enough credits, you have to get {cost - credits} more";
            else if ((Gamer.EventManager.EventManager.ActiveEvent as TSL).detectives.Contains(sender.Id) && item != ItemType.GunUSP)
            {
                sender.AddItem(item);
                (Gamer.EventManager.EventManager.ActiveEvent as TSL).pb[sender.Id] = credits - cost;
            }
            else if ((Gamer.EventManager.EventManager.ActiveEvent as TSL).detectives.Contains(sender.Id) && item == ItemType.GunUSP)
            {
                sender.AddItem(new Inventory.SyncItemInfo
                {
                    id = item,
                    durability = 18,
                    modSight = 1,
                    modBarrel = 2,
                    modOther = 0
                });
                (Gamer.EventManager.EventManager.ActiveEvent as TSL).pb[sender.Id] = credits - cost;
            }
            else return "You are not a <color=#00B7EB>Detective</color> to buy this item";
            return $"You have bought {ShortNames(item)}";
        }

        private string ShortNames(ItemType item)
        {
            switch (item)
            {
                case ItemType.SCP018:
                    return "SCP 018";
                case ItemType.GunCOM15:
                    return "USP";
                case ItemType.GunUSP:
                    return "Golden Deagle";
                case ItemType.Adrenaline:
                    return "Adrenaline";
                case ItemType.SCP500:
                    return "SCP 500";
                case ItemType.Flashlight:
                    return "C4";
                default:
                    return "Something went wrong";
            }
        }

        private string GetUsage(int balance)
        {
            return string.Join("\n", new string[] {
                $"T Shop | Your balance: {balance} credits",
                "TD - Armor for 1 credit",
                "TD - Harmor for 3 credits",
                "T - ATTD (AntiTraitorTesterDetect) for 2 credits",
                "T - SCP018 for 2 credits",
                "T - USP for 2 credits",
                "D - GD (GoldenDeagle) for 1 credits",
                "D - Adrenaline for 1 credits",
                "D - SCP500 for 1 credits",
                "T - Lockdown for 2 credits",
                "T - Blackout for 0 credits",
                "T - Doorreset for 0 credits",
                "T - C4 for 2 credits"
            });
        }
    }
}
