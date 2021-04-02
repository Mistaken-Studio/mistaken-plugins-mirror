using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using HarmonyLib;
using MEC;
using Mirror;
using UnityEngine;
using Gamer.Diagnostics;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using System.IO;
using Gamer.Mistaken.Systems.Logs;
using CustomPlayerEffects;

namespace Gamer.Mistaken.Systems
{
    public class Handler : Module
    {
        internal static HashSet<int> NewPlayers { get; } = new HashSet<int>();
        internal static Dictionary<string, string> DisplayNameChangend { get; } = new Dictionary<string, string>();

        public Handler(PluginHandler plugin) : base(plugin)
        { 
            Systems.Patches.SCPVoiceChatPatch.MimicedRoles.Add(RoleType.Scp049);

            if (Server.Port % 2 != 0)
            {
                Systems.Patches.SCPVoiceChatPatch.MimicedRoles.Add(RoleType.Scp0492);
                Systems.Patches.SCPVoiceChatPatch.MimicedRoles.Add(RoleType.Scp096);
                Systems.Patches.SCPVoiceChatPatch.MimicedRoles.Add(RoleType.Scp106);
                Systems.Patches.SCPVoiceChatPatch.MimicedRoles.Add(RoleType.Scp173);
            }

            new Shield.ShieldedManager(plugin);
            new InfoMessage.InfoMessageManager(plugin);
            new AntiAFK.Handler(plugin);
            new NicknameFixer.Handler(plugin);
            new Staff.StaffHandler(plugin);
            new Pets.PetsHandler(plugin);

            new GUI.SpecInfoHandler(plugin);
            new GUI.InformerHandler(plugin);

            new CustomItems.CustomItemsHandler(plugin);

            new Misc.ClassDCellsDecontaminationHandler(plugin); // OFF
            new Misc.CustomMaxHealthHandler(plugin);
            new Misc.ReusableItemsHandler(plugin);
            new Misc.RespawnPlayerHandler(plugin);
            new Misc.SpawnProtectHandler(plugin);
            new Misc.ArmorHandler(plugin);
            new Misc.BetterWarheadHandler(plugin);
            new Misc.ResurectionHandler(plugin);
            new Misc.PlayerRoundStatisticsHandler(plugin);
            new Misc.MassTKDetectionHandler(plugin);
            new Misc.DoorHandler(plugin);

            new End.RandomSizeHandler(plugin); // OFF
            new End.RandomMessagesHandler(plugin);
            new End.WaitingScreenHandler(plugin); // OFF
            new End.LeaveZombieHandler(plugin);
            new End.RoundEndHandler(plugin);
            new End.CamperEscapeHandler(plugin);
            new End.AutoUpdateHandler(plugin);
            new End.VanishHandler(plugin);
            new End.NoEndlessRoundHandler(plugin);
            new End.GrenadeLauncherHandler(plugin);
            new End.OverwatchHandler(plugin);
            new End.RandomItemSpawnsHandler(plugin);
            new End.RoundLogHandler(plugin);

            new End.FireworkManager(plugin);

            new Seasonal.EasterHandler(plugin);
            new Seasonal.PrimaAprilisHanlder(plugin);

            /*MEC.Timing.CallDelayed(2, () =>
            {
                var method = typeof(PlayerPositionManager).GetMethod(nameof(PlayerPositionManager.TransmitData), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
                Exiled.Events.Events.Instance.Harmony.Unpatch(method, HarmonyPatchType.All, Exiled.Events.Events.Instance.Harmony.Id);
            });*/

            Server_RestartingRound();
        }

        public override string Name => "Systems";

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Left += this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
            Exiled.Events.Handlers.Player.InteractingElevator += this.Handle<Exiled.Events.EventArgs.InteractingElevatorEventArgs>((ev) => Player_InteractingElevator(ev));
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.TriggeringTesla += this.Handle<Exiled.Events.EventArgs.TriggeringTeslaEventArgs>((ev) => Player_TriggeringTesla(ev));
            Exiled.Events.Handlers.Player.Dying += this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet += this.Handle<Exiled.Events.EventArgs.InsertingGeneratorTabletEventArgs>((ev) => Player_InsertingGeneratorTablet(ev));
            Exiled.Events.Handlers.Player.EjectingGeneratorTablet += this.Handle<Exiled.Events.EventArgs.EjectingGeneratorTabletEventArgs>((ev) => Player_EjectingGeneratorTablet(ev));
            Exiled.Events.Handlers.Player.IntercomSpeaking += this.Handle<Exiled.Events.EventArgs.IntercomSpeakingEventArgs>((ev) => Player_IntercomSpeaking(ev));
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += this.Handle<Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs>((ev) => Server_SendingRemoteAdminCommand(ev));
            Exiled.Events.Handlers.Map.Decontaminating += this.Handle<Exiled.Events.EventArgs.DecontaminatingEventArgs>((ev) => Map_Decontaminating(ev));
            Exiled.Events.Handlers.Server.RoundEnded += this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Player.Destroying += this.Handle<Exiled.Events.EventArgs.DestroyingEventArgs>((ev) => Player_Destroying(ev));
            Exiled.Events.Handlers.CustomEvents.OnFirstTimeJoined += this.Handle<Exiled.Events.EventArgs.FirstTimeJoinedEventArgs>((ev) => CustomEvents_OnFirstTimeJoined(ev));
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance += this.Handle<Exiled.Events.EventArgs.AnnouncingNtfEntranceEventArgs>((ev) => Map_AnnouncingNtfEntrance(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Left -= this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
            Exiled.Events.Handlers.Player.InteractingElevator -= this.Handle<Exiled.Events.EventArgs.InteractingElevatorEventArgs>((ev) => Player_InteractingElevator(ev));
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.TriggeringTesla -= this.Handle<Exiled.Events.EventArgs.TriggeringTeslaEventArgs>((ev) => Player_TriggeringTesla(ev));
            Exiled.Events.Handlers.Player.Dying -= this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet -= this.Handle<Exiled.Events.EventArgs.InsertingGeneratorTabletEventArgs>((ev) => Player_InsertingGeneratorTablet(ev));
            Exiled.Events.Handlers.Player.EjectingGeneratorTablet -= this.Handle<Exiled.Events.EventArgs.EjectingGeneratorTabletEventArgs>((ev) => Player_EjectingGeneratorTablet(ev));
            Exiled.Events.Handlers.Player.IntercomSpeaking -= this.Handle<Exiled.Events.EventArgs.IntercomSpeakingEventArgs>((ev) => Player_IntercomSpeaking(ev));
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= this.Handle<Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs>((ev) => Server_SendingRemoteAdminCommand(ev));
            Exiled.Events.Handlers.Map.Decontaminating -= this.Handle<Exiled.Events.EventArgs.DecontaminatingEventArgs>((ev) => Map_Decontaminating(ev));
            Exiled.Events.Handlers.Server.RoundEnded -= this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Player.Destroying -= this.Handle<Exiled.Events.EventArgs.DestroyingEventArgs>((ev) => Player_Destroying(ev));
            Exiled.Events.Handlers.CustomEvents.OnFirstTimeJoined -= this.Handle<Exiled.Events.EventArgs.FirstTimeJoinedEventArgs>((ev) => CustomEvents_OnFirstTimeJoined(ev));
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance -= this.Handle<Exiled.Events.EventArgs.AnnouncingNtfEntranceEventArgs>((ev) => Map_AnnouncingNtfEntrance(ev));
        }

        private void Map_AnnouncingNtfEntrance(Exiled.Events.EventArgs.AnnouncingNtfEntranceEventArgs ev)
        {
            Map.ChangeUnitColor(Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames.Count - 1, "#00F");
        }

        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (ev.IsAllowed && !ev.IsFrag)
            {
                foreach (var target in ev.TargetToDamages.Keys)
                    LogManager.FlashLog[target.Id] = ev.Thrower;
            }
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            foreach (var player in RealPlayers.List)
            {
                if (player.DisplayNickname != null && !DisplayNameChangend.ContainsKey(player.UserId))
                    DisplayNameChangend.Add(player.UserId, player.DisplayNickname);
            }
        }

        private void Map_Decontaminating(Exiled.Events.EventArgs.DecontaminatingEventArgs ev)
        {
            MEC.Timing.CallDelayed(5, () =>
            {
                var door = Map.Doors.FirstOrDefault(d => d.Type() == DoorType.SurfaceGate);
                door.NetworkActiveLocks = 0;
                door.NetworkTargetState = true;
            });
        }

        private void Server_SendingRemoteAdminCommand(Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs ev)
        {
            string nameLower = ev.Name.ToLower();
            if(nameLower == "imute" || nameLower == "mute" || nameLower == "enrage")
            {
                if (ev.Sender.UserId.IsDevUserId())
                    return;
                ev.IsAllowed = false;
                ev.ReplyMessage = "Command Removed";
            } 
            else if(ev.Name.ToLower().StartsWith("cassie"))
            {
                string cassie = string.Join(" ", ev.Arguments);
                float dur = Cassie.CalculateDuration(cassie);
                Log.Debug($"[CASSIE] Long: {dur}s | Message: {cassie}");
                if (dur < 15)
                    return;
                if (ev.Sender.UserId.IsDevUserId())
                    return;
                if (ev.Sender.CheckPermission($"{PluginHandler.PluginName}.long_cassie"))
                    return;
                ev.IsAllowed = false;
                ev.Success = false;
                ev.ReplyMessage = $"Missing permission: {PluginHandler.PluginName}.long_cassie\nTo use cassie longer than 15 seconds this permission is requied(cassie was {dur}s long)";
            }
        }

        public static Player LastIntercomUser;
        private static bool IntercomInfoTimeout = false;
        private void Player_IntercomSpeaking(Exiled.Events.EventArgs.IntercomSpeakingEventArgs ev)
        {
            if(ev.IsAllowed)
                LastIntercomUser = ev.Player;
            if (IntercomInfoTimeout)
                return;
            if(Round.IsStarted && Round.ElapsedTime.TotalSeconds > 5 && Intercom.host.IntercomState == Intercom.State.Ready)
                MapPlus.Broadcast("INTERCOM", 5, $"({ev.Player.Id}) {ev.Player.Nickname} started using intercom", Broadcast.BroadcastFlags.AdminChat);
            IntercomInfoTimeout = true;
            Timing.RunCoroutine(OffCooldown());
        }

        private IEnumerator<float> OffCooldown()
        {
            yield return Timing.WaitForSeconds(1);
            while (Intercom.host.speaking)
                yield return Timing.WaitForSeconds(1);
            yield return Timing.WaitForSeconds(1);
            IntercomInfoTimeout = false;
        }

        private void Player_EjectingGeneratorTablet(Exiled.Events.EventArgs.EjectingGeneratorTabletEventArgs ev)
        {
            if (Generators.TryGetValue(ev.Generator, out Side inserterSide) && ev.Player.Side == inserterSide)
                ev.IsAllowed = false;
        }

        private readonly Dictionary<Generator079, Side> Generators = new Dictionary<Generator079, Side>();
        private void Player_InsertingGeneratorTablet(Exiled.Events.EventArgs.InsertingGeneratorTabletEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if (Generators.ContainsKey(ev.Generator))
                Generators.Remove(ev.Generator);
            Generators.Add(ev.Generator, ev.Player.Side);
        }

        private void Server_RoundStarted()
        {
            LeftOnStart.Clear();
            LastIntercomUser = null;
            Generators.Clear();
            Timing.RunCoroutine(InformCommanderDeath());
            Timing.RunCoroutine(NoVoidFailling());
            //System.IO.File.WriteAllLines(Paths.Configs + "/cassie_words.txt", NineTailedFoxAnnouncer.singleton.voiceLines.Select(i => $"\"{i.apiName}\","));

            Map.ChangeUnitColor(0, "#888");
        }
        private IEnumerator<float> InformCommanderDeath()
        {
            yield return Timing.WaitForSeconds(1);
            bool wasAlive = false;
            int rid = RoundPlus.RoundId; 
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                if(!wasAlive)
                    wasAlive = RealPlayers.Any(RoleType.NtfCommander);
                else
                {
                    var tmp = RealPlayers.Any(RoleType.NtfCommander);
                    if (!tmp)
                        MEC.Timing.CallDelayed(60, () => 
                        {
                            if (rid == RoundPlus.RoundId)
                                Cassie.GlitchyMessage("WARNING . . . NINETAILEDFOX COMMANDER IS DEAD", 0.3f, 0.15f);
                        });
                    wasAlive = tmp;
                }
                yield return Timing.WaitForSeconds(60);
            }
        }

        private IEnumerator<float> NoVoidFailling()
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId; while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                var start = DateTime.Now;
                foreach (var player in RealPlayers.List.Where(p => p.Role != RoleType.Tutorial))
                {
                    if (player.Position.y < -2100)
                    {
                        bool hadGodMode = player.IsGodModeEnabled;
                        if (!hadGodMode) 
                            player.IsGodModeEnabled = true;
                        player.Position = new Vector3(0, 1004, 0);
                        yield return Timing.WaitForSeconds(0.5f);
                        if (!hadGodMode) 
                            player.IsGodModeEnabled = false;
                    }
                }
                Diagnostics.MasterHandler.LogTime("SystemsHandler", "NoVoidFalling", start, DateTime.Now);
                yield return Timing.WaitForSeconds(30);
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (UnityEngine.Random.Range(0, 2) == 1 && (ev.NewRole == RoleType.NtfCadet || ev.NewRole == RoleType.FacilityGuard))
                ev.Items.Add(ItemType.GunUSP);
            if(ev.NewRole == RoleType.Spectator && !(ev.Player.Team == Team.CHI || ev.Player.Team == Team.SCP || ev.Player.Team == Team.TUT || ev.Player.Team == Team.RIP || ev.Player.Role == RoleType.None))
            {
                if(RealPlayers.List.Where(p => p.Id != ev.Player.Id && p.IsHuman && p.Team != Team.CHI).Count() == 1)
                    Cassie.DelayedGlitchyMessage("Spotted only 1 alive . There could be ChaosInsurgency", 25, 0.5f, 0.1f);
            }
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.Attacker?.IsDev() ?? false)
            {
                if (ev.Attacker.Side == ev.Target.Side && !(ev.Target.Role == RoleType.ClassD && ev.Attacker.Role == RoleType.ClassD) && ev.Target.Id != ev.Attacker.Id)
                {
                    ev.Amount = Math.Min(ev.Target.Health - 1, ev.Amount);
                    MEC.Timing.CallDelayed(0.2f, () => ev.Target.DisableEffect<Bleeding>());
                    return;
                }
            }
            if (ev.Target?.IsDev() ?? false)
            {
                if (ev.Attacker.Side == ev.Target.Side && !(ev.Target.Role == RoleType.ClassD && ev.Attacker.Role == RoleType.ClassD) && ev.Target.Id != ev.Attacker.Id)
                {
                    ev.Amount = Math.Min(ev.Target.Health - 1, ev.Amount);
                    MEC.Timing.CallDelayed(0.2f, () => ev.Target.DisableEffect<Bleeding>());
                    return;
                }
            }
            if (ev.Attacker != null && RealPlayers.Get(ev.Attacker.Id) == null)
                ev.IsAllowed = false;
            if (!ev.IsAllowed) return;
            if (ev.DamageType == DamageTypes.Tesla && ev.Target.Team == Team.SCP && ev.Amount < ev.Target.Health)
            {
                ev.Target.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Burned>(30);
                ev.Target.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Concussed>(30);
                ev.Target.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Deafened>(30);
                ev.Target.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Disabled>(30);
            }
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (!ev.IsAllowed) 
                return;
            if (ev.HitInformation.GetDamageType() == DamageTypes.Tesla)
            {
                if (ev.Target.Team == Team.SCP)
                    MapPlus.Broadcast("SYSTEM", 10, $"({ev.Target.Id}) {ev.Target.Nickname} died on tesla as {ev.Target.Role}", Broadcast.BroadcastFlags.AdminChat);
                else
                {
                    var hid = ev.Target.Inventory.items.FirstOrDefault(i => i.id == ItemType.MicroHID);
                    ev.Target.Inventory.items.SetWeaponAmmo(hid, 1);
                }
            }

            if (ev.Target.UserId == ev.Killer.UserId && ev.HitInformation.GetDamageType().name == "Client has failed to spawn.")
            {
                ev.IsAllowed = false;
                ev.Target.Role = ev.Target.Role;
            }

            if(ev.Target.Side != ev.Killer?.Side)
                DropBallAndGrenades(ev.Target);

            if (ev.Target.Team == Team.SCP && ev.Target.Role != RoleType.Scp079)
            {
                Timing.CallDelayed(5, () =>
                {
                    var scps = RealPlayers.Get(Team.SCP).ToArray();
                    if (scps.Length == 1 && scps[0].Role == RoleType.Scp079 && !Generator079.mainGenerator.forcedOvercharge)
                    {
                        Generator079.mainGenerator.forcedOvercharge = true;
                        Recontainer079.BeginContainment(true);
                        Cassie.Message("ALLSECURED . SCP 0 7 9 RECONTAINMENT SEQUENCE COMMENCING . FORCEOVERCHARGE");
                    }
                });
            }
        }

        private void DropBallAndGrenades(Player player)
        {
            var manager = player.GameObject.GetComponent<Grenades.GrenadeManager>();
            foreach (var item in player.Inventory.items.Where(i => i.id == ItemType.SCP018).ToArray())
            {
                Log.Debug("Detected ball in inventory");
                SpawnGrenade(player, manager.availableGrenades[2], manager);
                player.RemoveItem(item);
            }
            if (player.CurrentItem.id == ItemType.GrenadeFrag)
            {
                Log.Debug("Detected grenade in hand");
                SpawnGrenade(player, manager.availableGrenades[0], manager);
                player.RemoveItem(player.CurrentItem);
            }
        }

        private void SpawnGrenade(Player player, Grenades.GrenadeSettings settings, Grenades.GrenadeManager manager)
        {
            Grenades.Grenade component = GameObject.Instantiate(settings.grenadeInstance).GetComponent<Grenades.Grenade>();
            if (component == null)
            {
                Log.Error("Grenade component is null");
                return;
            }
            component.InitData(manager, player.ReferenceHub?.playerMovementSync?.PlayerVelocity ?? default, Vector3.down, 1);
            NetworkServer.Spawn(component.gameObject);
        }

        private void Player_Destroying(Exiled.Events.EventArgs.DestroyingEventArgs ev)
        {
            if (!ev.Player.IsReadyPlayer())
                return;
            if (ev.Player.DisplayNickname != null)
                DisplayNameChangend[ev.Player.UserId] = ev.Player.DisplayNickname;
            else
                DisplayNameChangend.Remove(ev.Player.UserId);
        }
        private readonly Dictionary<string, RoleType> LeftOnStart = new Dictionary<string, RoleType>();
        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            AutoRoundRestart();
            JoinedButNotLeft.Remove(ev.Player.UserId);
            if (Round.ElapsedTime.TotalSeconds < 30)
                LeftOnStart[ev.Player.UserId] = ev.Player.Role;
        }

        private void AutoRoundRestart()
        {

            /*if (Server.Port == 7791)
            {
                IdleMode.PauseIdleMode = true;
                return;
            }*/
            int players = RealPlayers.List.Count() - 1;
            if (players == 0 && Round.IsStarted)
            {
                IdleMode.SetIdleMode(false, true);
                Log.Info("Server is empty... Restarting!");
                Round.Restart(true);
            }
            else if (Round.IsStarted)
                Log.Debug($"Server is not empty({players})");
            else
                Log.Debug($"Server may or may be not empty but round is not started");
        }

        private void Player_TriggeringTesla(Exiled.Events.EventArgs.TriggeringTeslaEventArgs ev)
        {
            if (ev.Player.Role == RoleType.Tutorial)
                ev.IsTriggerable = false;
        }

        public static readonly HashSet<string> JoinedButNotLeft = new HashSet<string>();

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (!ev.Player.IsVerified)
                Log.Error("Verified but not verified");
            if (ev.Player.UserId == null)
            {
                Log.Error("UserId is null");
                return;
            }
            if (!JoinedButNotLeft.Contains(ev.Player.UserId))
            {
                JoinedButNotLeft.Add(ev.Player.UserId);
                Exiled.Events.Handlers.CustomEvents.InvokeOnFirstTimeJoined(new Exiled.Events.EventArgs.FirstTimeJoinedEventArgs(ev.Player));
            }

            if (Round.ElapsedTime.TotalSeconds < 30 && LeftOnStart.ContainsKey(ev.Player.UserId))
                ev.Player.Role = LeftOnStart[ev.Player.UserId];

            Bans.BansManager.GetBans(ev.Player.UserId);
            if (!Logs.LogManager.PlayerLogs.ContainsKey(RoundPlus.RoundId))
                Logs.LogManager.PlayerLogs[RoundPlus.RoundId] = NorthwoodLib.Pools.ListPool<Logs.PlayerInfo>.Shared.Rent();
            Logs.LogManager.PlayerLogs[RoundPlus.RoundId].Add(new Logs.PlayerInfo(ev.Player));

            if (DisplayNameChangend.TryGetValue(ev.Player.UserId, out string nick))
            {
                ev.Player.DisplayNickname = nick;
                DisplayNameChangend.Remove(ev.Player.UserId);
            }

            if (ev.Player.CheckPermission($"{PluginHandler.PluginName}.SpeakAsSCPToHuman"))
                Patches.SCPVoiceChatPatch.HasAccessToSCPAlt.Add(ev.Player.UserId);
        }
        private void CustomEvents_OnFirstTimeJoined(Exiled.Events.EventArgs.FirstTimeJoinedEventArgs ev)
        {
            NewPlayers.Add(ev.Player.Id);
            MEC.Timing.CallDelayed(20, () =>
            {
                NewPlayers.Remove(ev.Player.Id);
            });
        }

        private void Player_InteractingElevator(Exiled.Events.EventArgs.InteractingElevatorEventArgs ev)
        {
            if (Logs.Commands.ElevatorLogCommand.Active.Contains(ev.Player.Id))
            {
                if (Logs.LogManager.ElevatorLogs.TryGetValue(ev.Lift.Type(), out List<Logs.ElevatorLog> value))
                    Logs.Commands.ElevatorLogCommand.Execute(ev.Player, ev.Lift.Type(), value);
                else
                    ev.Player.Broadcast("ELEVATOR LOG", 10, "Elevator data not found", Broadcast.BroadcastFlags.AdminChat);
                ev.IsAllowed = false;
            }
            if (!ev.IsAllowed)
                return;
            if (!Logs.LogManager.ElevatorLogs.ContainsKey(ev.Lift.Type()))
                Logs.LogManager.ElevatorLogs.Add(ev.Lift.Type(), NorthwoodLib.Pools.ListPool<Logs.ElevatorLog>.Shared.Rent());
            Logs.LogManager.ElevatorLogs[ev.Lift.Type()].Add(new Logs.ElevatorLog(ev));
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (Logs.Commands.DoorLogCommand.Active.Contains(ev.Player.Id))
            {
                if (Logs.LogManager.DoorLogs.TryGetValue(ev.Door, out List<Logs.DoorLog> value))
                    Logs.Commands.DoorLogCommand.Execute(ev.Player, ev.Door, value);
                else 
                    ev.Player.Broadcast("DOOR LOG", 10, "Door data not found");
                ev.IsAllowed = false;
            }
            if (!ev.IsAllowed) 
                return;
            if (!Logs.LogManager.DoorLogs.ContainsKey(ev.Door))
                Logs.LogManager.DoorLogs[ev.Door] = NorthwoodLib.Pools.ListPool<Logs.DoorLog>.Shared.Rent();
            Logs.LogManager.DoorLogs[ev.Door].Add(new Logs.DoorLog(ev));
            if(ev.Door.Type() == DoorType.Scp079Second) 
                Timing.RunCoroutine(SpawnPainKillers());
        }

        private void Server_RestartingRound()
        {
            RoundPlus.IncRoundId();
            Bans.BansManager.ClearCache();

            Logs.LogManager.RoundStartTime[RoundPlus.RoundId] = DateTime.Now;
            foreach (var item in Logs.LogManager.DoorLogs)
            {
                NorthwoodLib.Pools.ListPool<Logs.DoorLog>.Shared.Return(item.Value);
            }
            Logs.LogManager.DoorLogs.Clear();
            foreach (var item in Logs.LogManager.ElevatorLogs)
            {
                NorthwoodLib.Pools.ListPool<Logs.ElevatorLog>.Shared.Return(item.Value);
            }
            Logs.LogManager.ElevatorLogs.Clear();
            Logs.LogManager.FlashLog.Clear();

            Patches.SCP079RecontainPatch.Restart();
            Patches.SCPVoiceChatPatch.HasAccessToSCPAlt.Clear();
            Patches.RagdollManagerPatch.Ragdolls.Clear();

            LOFH.LOFH.ClearVanish();

            spawnedPainKillers = false;
        }

        private bool spawnedPainKillers = false;
        public IEnumerator<float> SpawnPainKillers()
        {
            if (spawnedPainKillers)
                yield break;
            spawnedPainKillers = true;
            var door = Map.Doors.First(d => d.Type() == DoorType.Scp079Second);
            var pos = door.transform.position;
            pos += door.transform.forward * 4.5f;
            pos += door.transform.right * 4.2f;
            pos += door.transform.up * 0.1f;
            for (int x = 0; x < 4; x++)
            {
                for (int z = 0; z < 5; z++)
                {
                    ItemType.Painkillers.Spawn(5, pos + new Vector3(x * 0.2f, 0, z * 0.2f));
                    yield return Timing.WaitForSeconds(0.05f);
                }
            }
            pos += door.transform.forward * 1f;
            for (int x = 0; x < 2; x++)
            {
                for (int z = 0; z < 2; z++)
                {
                    ItemType.Adrenaline.Spawn(1, pos + new Vector3(x * 0.4f, 0, z * 0.4f));
                    yield return Timing.WaitForSeconds(0.05f);
                }
            }
            pos -= door.transform.forward * 1f;
            pos += door.transform.right * 1f;
            for (int x = 0; x < 5; x++)
            {
                for (int z = 0; z < 2; z++)
                {
                    ItemType.Medkit.Spawn(2, pos + new Vector3(x * 0.5f, 0, z * 0.5f));
                    yield return Timing.WaitForSeconds(0.05f);
                }
            }
            pos += door.transform.forward * 1f;
            for (int x = 0; x < 1; x++)
            {
                for (int z = 0; z < 2; z++)
                {
                    ItemType.SCP500.Spawn(2, pos + new Vector3(x * 0.4f, 0, z * 0.4f));
                    yield return Timing.WaitForSeconds(0.05f);
                }
            }
        }
    }
}
