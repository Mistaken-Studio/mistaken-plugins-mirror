using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using MistakenSocket.Shared.API;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.Mistaken.CustomAchievements
{
    public class RoundEventHandler : Module
    {
        public override bool Enabled => false;


        private Player LastDetonator = null;
        private readonly HashSet<string> Curred = new HashSet<string>();
        private readonly HashSet<string> AliveFromStart = new HashSet<string>();
        private readonly List<Player> NoKills = new List<Player>();
        private readonly List<KeyValuePair<Player, string>> GrenadeKills = new List<KeyValuePair<Player, string>>();
        private readonly List<Player> Targets = new List<Player>();
        private Player goww_killer = null;
        private bool goww_killer_s = true;
        private Player Killer = null;
        private bool Killer_s = true;
        private bool All173Kills = true;
        private int KilledScpByWarhead = 0;
        private int Portals = 0;
        private Player First;

        public RoundEventHandler(PluginHandler _plugin) : base(_plugin)
        {

        }
        public override string Name => "CustomAchievements";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Warhead.Detonated += this.Handle(() => Warhead_Detonated(), "WarheadDetonated");
            Exiled.Events.Handlers.Warhead.Starting += this.Handle<Exiled.Events.EventArgs.StartingEventArgs>((ev) => Warhead_Starting(ev));
            Exiled.Events.Handlers.Warhead.Stopping += this.Handle<Exiled.Events.EventArgs.StoppingEventArgs>((ev) => Warhead_Stopping(ev));

            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Dying += this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.ItemDropped += this.Handle<Exiled.Events.EventArgs.ItemDroppedEventArgs>((ev) => Player_ItemDropped(ev));
            Exiled.Events.Handlers.Player.Handcuffing += this.Handle<Exiled.Events.EventArgs.HandcuffingEventArgs>((ev) => Player_Handcuffing(ev));
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.Escaping += this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
            Exiled.Events.Handlers.Player.MedicalItemUsed += this.Handle<Exiled.Events.EventArgs.UsedMedicalItemEventArgs>((ev) => Player_MedicalItemUsed(ev));

            Exiled.Events.Handlers.Scp914.UpgradingItems += this.Handle<Exiled.Events.EventArgs.UpgradingItemsEventArgs>((ev) => Scp914_UpgradingItems(ev));
            Exiled.Events.Handlers.Scp914.Activating += this.Handle<Exiled.Events.EventArgs.ActivatingEventArgs>((ev) => Scp914_Activating(ev));

            Exiled.Events.Handlers.Scp096.Enraging += this.Handle<Exiled.Events.EventArgs.EnragingEventArgs>((ev) => Scp096_Enraging(ev));

            Exiled.Events.Handlers.Scp106.CreatingPortal += this.Handle<Exiled.Events.EventArgs.CreatingPortalEventArgs>((ev) => Scp106_CreatingPortal(ev));

            Exiled.Events.Handlers.Scp049.FinishingRecall += this.Handle<Exiled.Events.EventArgs.FinishingRecallEventArgs>((ev) => Scp049_FinishingRecall(ev));

            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RoundEnded += this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Server.RespawningTeam += this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Warhead.Detonated -= this.Handle(() => Warhead_Detonated(), "WarheadDetonated");
            Exiled.Events.Handlers.Warhead.Starting -= this.Handle<Exiled.Events.EventArgs.StartingEventArgs>((ev) => Warhead_Starting(ev));
            Exiled.Events.Handlers.Warhead.Stopping -= this.Handle<Exiled.Events.EventArgs.StoppingEventArgs>((ev) => Warhead_Stopping(ev));

            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Dying -= this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.ItemDropped -= this.Handle<Exiled.Events.EventArgs.ItemDroppedEventArgs>((ev) => Player_ItemDropped(ev));
            Exiled.Events.Handlers.Player.Handcuffing -= this.Handle<Exiled.Events.EventArgs.HandcuffingEventArgs>((ev) => Player_Handcuffing(ev));
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.Escaping -= this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
            Exiled.Events.Handlers.Player.MedicalItemUsed -= this.Handle<Exiled.Events.EventArgs.UsedMedicalItemEventArgs>((ev) => Player_MedicalItemUsed(ev));

            Exiled.Events.Handlers.Scp914.UpgradingItems -= this.Handle<Exiled.Events.EventArgs.UpgradingItemsEventArgs>((ev) => Scp914_UpgradingItems(ev));
            Exiled.Events.Handlers.Scp914.Activating -= this.Handle<Exiled.Events.EventArgs.ActivatingEventArgs>((ev) => Scp914_Activating(ev));

            Exiled.Events.Handlers.Scp096.Enraging -= this.Handle<Exiled.Events.EventArgs.EnragingEventArgs>((ev) => Scp096_Enraging(ev));

            Exiled.Events.Handlers.Scp106.CreatingPortal -= this.Handle<Exiled.Events.EventArgs.CreatingPortalEventArgs>((ev) => Scp106_CreatingPortal(ev));

            Exiled.Events.Handlers.Scp049.FinishingRecall -= this.Handle<Exiled.Events.EventArgs.FinishingRecallEventArgs>((ev) => Scp049_FinishingRecall(ev));

            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RoundEnded -= this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Server.RespawningTeam -= this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => Server_RespawningTeam(ev));
        }

        private void Scp049_FinishingRecall(Exiled.Events.EventArgs.FinishingRecallEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) 
                return;
            AddProggress("Daddy", ev.Scp049);
            AddProggress("IneedDoctor", ev.Target);
            if (Curred.Contains(ev.Target.UserId))
                AddProggress("ZombieTwice", ev.Target);
            else 
                Curred.Add(ev.Target.UserId);
        }

        private void Scp106_CreatingPortal(Exiled.Events.EventArgs.CreatingPortalEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) 
                return;
            Portals++;
            if (Portals == 2) 
                ForceLevel("GiveStain", ev.Player, CustomAchievements.Level.BRONZE);
            else if (Portals == 6) 
                ForceLevel("GiveStain", ev.Player, CustomAchievements.Level.SILVER);
            else if (Portals == 10) 
                ForceLevel("GiveStain", ev.Player, CustomAchievements.Level.GOLD);
            else if (Portals == 14) 
                ForceLevel("GiveStain", ev.Player, CustomAchievements.Level.DIAMOND);
            else if (Portals == 20) 
                ForceLevel("GiveStain", ev.Player, CustomAchievements.Level.EXPERT);
        }

        private void Server_RespawningTeam(Exiled.Events.EventArgs.RespawningTeamEventArgs ev)
        {
            Targets.AddRange(ev.Players);
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) 
                return;
            Player player;
            foreach (string item in AliveFromStart.ToArray())
            {
                player = RealPlayers.List.FirstOrDefault(p => p.UserId == item);
                if (player != null) 
                    AddProggress("Survivor", player);
            }
            Player[] tmp = RealPlayers.List.Where(p => p.Role == RoleType.Scp173).ToArray();
            if (tmp.Length > 0)
            {
                if (All173Kills)
                    ForceLevel("DevilsMemorial", tmp[0], CustomAchievements.Level.EXPERT);
            }
        }

        private void Server_RoundStarted()
        {
            if (CustomAchievements.DisableForRound) 
                return;
            AliveFromStart.Clear();
            foreach (Player item in RealPlayers.List.ToArray())
            {
                NoKills.Add(item);
                AliveFromStart.Add(item.UserId);
            }
        }

        private void Scp914_Activating(Exiled.Events.EventArgs.ActivatingEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) 
                return;
            last914User = ev.Player;
        }

        private void Scp914_UpgradingItems(Exiled.Events.EventArgs.UpgradingItemsEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) 
                return;
            if (Scp914.Scp914Machine.singleton.knobState == Scp914.Scp914Knob.VeryFine || Scp914.Scp914Machine.singleton.knobState == Scp914.Scp914Knob.Fine)
            {
                foreach (var pickup in ev.Items)
                {
                    if (pickup.NetworkitemId == ItemType.KeycardO5)
                    {
                        if (last914User != null) 
                            AddProggress("SoSad", last914User);
                    }
                }
            }
        }

        private void Player_MedicalItemUsed(Exiled.Events.EventArgs.UsedMedicalItemEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) return;
            switch (ev.Item)
            {
                case ItemType.Adrenaline:
                    {
                        AddProggress("Addicted", ev.Player);
                        break;
                    }
                case ItemType.Painkillers:
                    {
                        AddProggress("Pills", ev.Player);
                        break;
                    }
                case ItemType.Medkit:
                    {
                        AddProggress("Medkit", ev.Player);
                        break;
                    }
                case ItemType.SCP268:
                    {
                        AddProggress("ImNotHere", ev.Player);
                        break;
                    }
                case ItemType.SCP207:
                    {
                        AddProggress("Cola", ev.Player);
                        break;
                    }
            }
        }

        private void Scp096_Enraging(Exiled.Events.EventArgs.EnragingEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) return;
            if (!ev.IsAllowed) return;
            AddProggress("GetCrazy", ev.Player);
        }

        private void Player_Escaping(Exiled.Events.EventArgs.EscapingEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) return;
            if (!ev.IsAllowed) 
                return;
            if (ev.Player.Inventory.availableItems.Any(i => i.id == ItemType.SCP268))
            {
                foreach (var item in ev.Player.ReferenceHub.playerEffectsController?.AllEffects)
                {
                    if (item.Key == typeof(CustomPlayerEffects.Scp268) && item.Value.Enabled) 
                        AddProggress("likeMorfigo", ev.Player);
                }
            }
            if (LastDetonator != null)
            {
                if (ev.Player.Id == LastDetonator.Id)
                {
                    if (ev.Player.Team == Team.CDP || ev.Player.Team == Team.RSC)
                    {
                        AddProggress("KaboomAndGo", ev.Player);
                    }
                }
            }
            if (ev.Player.IsCuffed)
            {
                if (ev.Player.Team == Team.CHI || ev.Player.Team == Team.MTF)
                {
                    AddProggress("Traitor", ev.Player);
                }
            }
            if (NoKills.Contains(ev.Player))
            {
                AddProggress("PacifistEscape", ev.Player);
            }
            if (goww_killer?.UserId == ev.Player.UserId && false)
            {
                AddProggress("GoWW", ev.Player);
            }
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) return;
            if (ev.Target.DoNotTrack) return;
            if (ev.HitInformations.GetDamageType() == DamageTypes.Scp106)
            {
                if (ev.Target.Health - ev.Amount > 0)
                {
                    AddProggress("GoToPocket", ev.Target);
                }
            }
        }

        private void Player_Handcuffing(Exiled.Events.EventArgs.HandcuffingEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) return;
            if (ev.IsAllowed)
            {
                if (ev.Cuffer.DoNotTrack) return;
                if (ev.Cuffer.Team == Team.CHI)
                {
                    if (ev.Target.Team == Team.SCP)
                    {
                        AddProggress("WeComeInPeace", ev.Cuffer);
                    }
                }
            }
        }

        private void Player_ItemDropped(Exiled.Events.EventArgs.ItemDroppedEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) return;
            if (ev.Player.DoNotTrack) return;
            if (ev.Pickup.ItemId ==ItemType.KeycardO5)
            {
                AddProggress("Crazy?", ev.Player);
            }
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) return;
            if (ev.Player.DoNotTrack) return;
            if (ev.IsAllowed)
            {
                if (ev.Door.Type() == Exiled.API.Enums.DoorType.Scp106Primary || ev.Door.Type() == Exiled.API.Enums.DoorType.Scp106Secondary || ev.Door.Type() == Exiled.API.Enums.DoorType.Scp106Bottom) AddProggress("N-wordPass", ev.Player);
            }
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) return;
            if (AliveFromStart.Contains(ev.Target.UserId)) 
                AliveFromStart.Remove(ev.Target.UserId);
            if (NoKills.Contains(ev.Killer)) 
                NoKills.Remove(ev.Killer);
            var killsValue = kills.FindAll(item => item.Key.UserId == ev.Killer.UserId);
            if (killsValue.Count > 0)
            {
                AddProggress("NotPlanned", ev.Killer);
                AddProggress("NotPlanned", ev.Target);
                kills.Remove(killsValue[0]);
            }
            else
            {
                var tmp = new KeyValuePair<Player, Player>(ev.Target, ev.Killer);
                kills.Add(tmp);
                Timing.RunCoroutine(RemoveKill(tmp));
            }

            if (ev.Target.Team == Team.SCP)
            {
                if (ev.Target.Role == RoleType.Scp173)
                {
                    if (ev.HitInformation.GetDamageType() == DamageTypes.Grenade)
                    {
                        AddProggress("SCP-173-D", ev.Killer);
                    }
                }
                if (ev.HitInformation.GetDamageType() == DamageTypes.MicroHid)
                {
                    if (ev.Killer != null)
                    {
                        AddProggress("Toasted", ev.Killer);
                    }
                }
            }
            if (ev.HitInformation.GetDamageType() == DamageTypes.Decont)
            {
                AddProggress("Really?", ev.Target);
            }
            if (ev.HitInformation.GetDamageType() == DamageTypes.Nuke)
            {
                if (LastDetonator != null && ev.Target.Team == Team.SCP)
                {
                    KilledScpByWarhead++;
                    if (AddingBigBoom)
                        return;
                    AddingBigBoom = true;
                    MEC.Timing.CallDelayed(1, () =>
                    {
                        if (KilledScpByWarhead == 1) 
                            ForceLevel("BigBoom", LastDetonator, CustomAchievements.Level.GOLD);
                        else if (KilledScpByWarhead == 2) 
                            ForceLevel("BigBoom", LastDetonator, CustomAchievements.Level.DIAMOND);
                        else if (KilledScpByWarhead >= 3) 
                            ForceLevel("BigBoom", LastDetonator, CustomAchievements.Level.EXPERT);
                    });
                }
            }
            if (ev.HitInformation.GetDamageType() == DamageTypes.Wall && ev.Killer == null)
            {
                AddProggress("DejaVu", ev.Target);
            }
            if (ev.HitInformation.GetDamageType() == DamageTypes.Falldown)
            {
                AddProggress("Gravity", ev.Target);
            }
            if (ev.HitInformation.GetDamageType() == DamageTypes.Lure)
            {
                
                    AddProggress("ForGratherGood", ev.Target);
            }
            if (ev.HitInformation.GetDamageType() == DamageTypes.Pocket || ev.HitInformation.GetDamageType() == DamageTypes.Scp106)
            {
                AddProggress("OldMan", ev.Killer);
            }
            if (ev.Target.Role == RoleType.ChaosInsurgency && ev.Target != ev.Killer)
            {
                AddProggress("ScrewFundation", ev.Killer);
            }
            if (ev.Target.Team == Team.CDP)
            {
                if (ev.Killer.Role == RoleType.FacilityGuard)
                {
                    if (ev.HitInformation.GetDamageType() == DamageTypes.Com15 ||
                        ev.HitInformation.GetDamageType() == DamageTypes.E11StandardRifle ||
                        ev.HitInformation.GetDamageType() == DamageTypes.Logicer ||
                        ev.HitInformation.GetDamageType() == DamageTypes.Mp7 ||
                        ev.HitInformation.GetDamageType() == DamageTypes.P90 ||
                        ev.HitInformation.GetDamageType() == DamageTypes.Usp)
                    {
                        AddProggress("ZbeszekToxicRage", ev.Target);
                    }
                }
            }
            if (ev.Target.Role == RoleType.Scp173)
            {
                if (All173Kills && !ev.Target.DoNotTrack) 
                    ForceLevel("DevilsMemorial", ev.Target, CustomAchievements.Level.EXPERT);
            }
            if (ev.HitInformation.GetDamageType() != DamageTypes.Scp173)
            {
                if (ev.Target.Team == Team.CDP || ev.Target.Team == Team.RSC)
                {
                    All173Kills = false;
                }
            }
            if (ev.HitInformation.GetDamageType() == DamageTypes.Grenade)
            {
                if (ev.Killer.Team == Team.CDP || ev.Target.Team == Team.MTF)
                {
                    KeyValuePair<Player, string> value = new KeyValuePair<Player, string>(ev.Killer, ev.Target.UserId);
                    GrenadeKills.Add(value);
                    MEC.Timing.RunCoroutine(GKillRemove(value));
                }
            }
            if (ev.Target.Team == Team.CDP || ev.Target.Team == Team.RSC)
            {
                if (ev.Killer.Team == Team.CDP)
                {
                    if (Killer_s)
                    {
                        Killer = ev.Killer;
                        Killer_s = false;
                    }
                    else if (Killer != null && Killer.UserId != ev.Killer.UserId) Killer = null;

                    if (Killer != null)
                    {
                        MEC.Timing.RunCoroutine(SlowCheck_GOWW2());
                    }
                }
                else Killer = null;
            }
            if (ev.HitInformation.GetDamageType() == DamageTypes.Wall && ev.Killer.UserId == ev.Target.UserId && !ev.Target.DoNotTrack) AddProggress("Ping", ev.Target);
            if (ev.Target.Role == RoleType.Tutorial && ev.Target.UserId != ev.Killer.UserId && !ev.Killer.DoNotTrack) AddProggress("Tutorial", ev.Killer);
            tget = Targets.Find(item => item.UserId == ev.Target.UserId);
            if (tget != null)
            {
                if (ev.Killer.Team == Team.CDP || ev.Killer.Team == Team.RSC)
                {
                    if (goww_killer_s)
                    {
                        goww_killer = ev.Killer;
                        goww_killer_s = false;
                    }
                    else if (goww_killer != null)
                    {
                        if (goww_killer.UserId != ev.Killer.UserId) goww_killer = null;
                    }
                }
            }
        }

        private bool AddingBigBoom = false;

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) return;
            if (ev.Player.DoNotTrack) return;
            if (ev.NewRole == RoleType.Scientist)
            {
                AddProggress("Senior_Resarcher", ev.Player);
            }
            else if (ev.NewRole == RoleType.ClassD)
            {
                AddProggress("Senced", ev.Player);
            }
            else if (ev.NewRole == RoleType.NtfCommander)
            {
                AddProggress("Honored", ev.Player);
            }
            else if (ev.NewRole == RoleType.ChaosInsurgency)
            {
                AddProggress("RedRightHand", ev.Player);
            }
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) return;
            if (ev.Player.DoNotTrack)
            {
                MistakenSocket.Client.SL.SSL.Client.Send(MessageType.ACHIEVEMENT_CLEAR, new MistakenSocket.Shared.Achievements.ClearAchievements
                {
                    UserId = ev.Player.UserId
                });
                return;
            }
            AddProggress("Welcome!", ev.Player);
            NoKills.Add(ev.Player);
            if (First == null)
            {
                AddProggress("First", ev.Player);
                First = ev.Player;
            }
            //MEC.Timing.RunCoroutine(SlowJoin(ev));
        }

        private void Server_RestartingRound()
        {
            CustomAchievements.DisableForRound = false;
            //ProgressData.Clear();
            Curred.Clear();
            NoKills.Clear();
            GrenadeKills.Clear();
            LastDetonator = null;
            KilledScpByWarhead = 0;
            Portals = 0;
            All173Kills = true;
            Killer_s = true;
            Killer = null;
            goww_killer = null;
            goww_killer_s = true;
            First = null;
            AddingBigBoom = false;
        }

        private void Warhead_Stopping(Exiled.Events.EventArgs.StoppingEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) return;
            if (ev.Player == null) return;
            if (ev.Player.DoNotTrack) return;
            AddProggress("Nope", ev.Player);
        }

        private void Warhead_Starting(Exiled.Events.EventArgs.StartingEventArgs ev)
        {
            if (CustomAchievements.DisableForRound) return;
            if (ev.Player.DoNotTrack) return;
            if (ev.IsAllowed) LastDetonator = ev.Player;
        }

        private void Warhead_Detonated()
        {
            if (CustomAchievements.DisableForRound) return;
            if (LastDetonator != null)
            {
                AddProggress("BumBum", LastDetonator);
            }
        }

        public static void AddProggress(string Name, Player player)
        {
            if (player.DoNotTrack)
                return;
            CustomAchievements.AddAchievementProggres(player.UserId, CustomAchievements.GetAchievement(Name).Id);
        }
        public static void ForceLevel(string Name, Player player, CustomAchievements.Level level)
        {
            if (player.DoNotTrack)
                return;
            CustomAchievements.ForceLevel(player.UserId, CustomAchievements.GetAchievement(Name).Id, level);
        }

        Player tget;

        private List<KeyValuePair<Player, Player>> kills = new List<KeyValuePair<Player, Player>>();

        IEnumerator<float> RemoveKill(KeyValuePair<Player, Player> value)
        {
            yield return Timing.WaitForSeconds(10);
            kills.Remove(value);
        }

        IEnumerator<float> GKillRemove(KeyValuePair<Player, string> value)
        {
            int ammount = 0;
            foreach (KeyValuePair<Player, string> item in GrenadeKills.ToArray())
            {
                if (item.Key.UserId == value.Key.UserId)
                {
                    ammount++;
                }
            }
            if (ammount == 5)
            {
                if (!value.Key.DoNotTrack) 
                    AddProggress("VibeCheck", value.Key);
            }
            yield return MEC.Timing.WaitForSeconds(10);
            GrenadeKills.Remove(value);
        }

        IEnumerator<float> SlowCheck_GOWW2()
        {
            yield return MEC.Timing.WaitForSeconds(1);
            if (RealPlayers.List.Where(p => p.Team == Team.CDP).Count() == 1 && RealPlayers.List.Where(p => p.Team == Team.RSC).Count() == 0)
            {
                if (!Killer.DoNotTrack)
                    AddProggress("GoWW_2", Killer);
            }
        }

        private Player last914User;
    }
}