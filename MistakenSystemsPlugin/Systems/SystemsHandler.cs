#pragma warning disable IDE0079
#pragma warning disable IDE0060

using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.API;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.Staff;
using Gamer.Mistaken.Systems.Logs;
using Gamer.Utilities;
using Grenades;
using MEC;
using Mirror;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared;
using MistakenSocket.Shared.ClientToCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            new Pets.PetsHandler(plugin);

            new GUI.SpecInfoHandler(plugin);
            new GUI.InformerHandler(plugin);

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
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Scp049.FinishingRecall += this.Handle<Exiled.Events.EventArgs.FinishingRecallEventArgs>((ev) => Scp049_FinishingRecall(ev));
            Exiled.Events.Handlers.Player.PreAuthenticating += this.Handle<Exiled.Events.EventArgs.PreAuthenticatingEventArgs>((ev) => Player_PreAuthenticating(ev));
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
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Scp049.FinishingRecall -= this.Handle<Exiled.Events.EventArgs.FinishingRecallEventArgs>((ev) => Scp049_FinishingRecall(ev));
            Exiled.Events.Handlers.Player.PreAuthenticating -= this.Handle<Exiled.Events.EventArgs.PreAuthenticatingEventArgs>((ev) => Player_PreAuthenticating(ev));
        }
        public static readonly Dictionary<string, PlayerPreferences> PlayerPreferencesDict = new Dictionary<string, PlayerPreferences>();
        private void Player_PreAuthenticating(Exiled.Events.EventArgs.PreAuthenticatingEventArgs ev)
        {
            if (string.IsNullOrWhiteSpace(ev.UserId))
                return;
            PlayerPreferencesDict[ev.UserId] = PlayerPreferences.NONE;
            MistakenSocket.Client.SL.SSL.Client.Send(MistakenSocket.Shared.API.MessageType.CMD_REQUEST_DATA, new RequestData
            {
                Type = MistakenSocket.Shared.API.DataType.SL_PLAYER_PREFERENCES,
                argument = ev.UserId.Serialize(false)
            }).GetResponseDataCallback((data) =>
            {
                if (data.Type == MistakenSocket.Shared.API.ResponseType.OK)
                    PlayerPreferencesDict[ev.UserId] = data.Payload.Deserialize<PlayerPreferences>(false);
            });
        }

        private void Scp049_FinishingRecall(Exiled.Events.EventArgs.FinishingRecallEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            this.CallDelayed(1, () =>
            {
                if (!ev.Target.IsConnected)
                    return;
                if (ev.Target.Role != RoleType.Scp0492)
                    return;
                Exiled.Events.Handlers.Player.OnChangingRole(new Exiled.Events.EventArgs.ChangingRoleEventArgs(ev.Target, ev.Target.Role, new List<ItemType>(), true, false));
            }, "FinishingRecall");
        }

        private void Server_WaitingForPlayers()
        {
            SpawnKeycard(ItemType.KeycardSeniorGuard, new Vector3(18, 40, 0.05f), new Vector3(90, 90, 0), new Vector3(181f, 992.460f, -58.3f + 0.1f));
            SpawnKeycard(ItemType.KeycardSeniorGuard, new Vector3(30, 40, 0.05f), new Vector3(90, 0, 0), new Vector3(181f, 992.461f, -58.3f - 2f + 0.1f));
            SpawnKeycard(ItemType.KeycardSeniorGuard, new Vector3(30, 40, 0.05f), new Vector3(90, 0, 0), new Vector3(181f, 992.461f, -58.3f + 2f + 0.1f));
            SpawnKeycard(ItemType.KeycardGuard, new Vector3(26.25f, 1600, 0.05f), new Vector3(90, 0, 0), new Vector3(181f, 992.45f, -58.3f + 0.1f));
            SpawnKeycard(ItemType.KeycardGuard, new Vector3(26.25f, 1600, 0.05f), new Vector3(90, 45, 0), new Vector3(181f, 992.451f, -58.3f + 0.1f));
            SpawnKeycard(ItemType.KeycardGuard, new Vector3(26.25f, 1600, 0.05f), new Vector3(90, 90, 0), new Vector3(181f, 992.452f, -58.3f + 0.1f));
            SpawnKeycard(ItemType.KeycardGuard, new Vector3(26.25f, 1600, 0.05f), new Vector3(90, 135, 0), new Vector3(181f, 992.453f, -58.3f + 0.1f));

            SpawnKeycard(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, 90, 0), new Vector3(187.446f, 992.453f, -58.3f + 0.1f));
            SpawnKeycard(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, 90, 0), new Vector3(174.56f, 992.453f, -58.3f + 0.1f));
            SpawnKeycard(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, 0, 0), new Vector3(181f, 992.453f, -58.3f + 0.1f - 6.45f));
            SpawnKeycard(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, 0, 0), new Vector3(181f, 992.453f, -58.3f + 0.1f + 6.45f));
            SpawnKeycard(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, 45, 0), new Vector3(187.446f - 1.86f, 992.453f, -58.3f + 0.1f + 4.5f));
            SpawnKeycard(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, 45, 0), new Vector3(174.56f + 1.86f, 992.453f, -58.3f + 0.1f - 4.5f));
            SpawnKeycard(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, -45, 0), new Vector3(187.446f - 1.86f, 992.453f, -58.3f + 0.1f - 4.5f));
            SpawnKeycard(ItemType.KeycardFacilityManager, new Vector3(24, 35, 0.05f), new Vector3(90, -45, 0), new Vector3(174.56f + 1.86f, 992.453f, -58.3f + 0.1f + 4.5f));

        }
        public void SpawnKeycard(ItemType keycardType, Vector3 size, Vector3 rotation, Vector3 position)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
            gameObject.transform.position = position;
            gameObject.transform.localScale = size;
            gameObject.transform.rotation = Quaternion.Euler(rotation);
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Mirror.NetworkServer.Spawn(gameObject);
            var keycard = gameObject.GetComponent<Pickup>();
            keycard.SetupPickup(keycardType, 200f, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
            keycard.Locked = true;
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
            this.CallDelayed(5, () =>
             {
                 var door = Map.Doors.FirstOrDefault(d => d.Type() == DoorType.SurfaceGate);
                 door.NetworkActiveLocks = 0;
                 door.NetworkTargetState = true;
             }, "Decontaminating1");
            if (!Round.IsStarted)
                return;
            this.CallDelayed(30, () =>
            {
                if (Round.ElapsedTime.TotalMinutes < 4)
                    return;
                foreach (var item in RealPlayers.List.Where(p => p.Position.y > -100 && p.Position.y < 100))
                    item.EnableEffect<CustomPlayerEffects.Decontaminating>();
                this.CallDelayed(30, () =>
                {
                    if (Round.ElapsedTime.TotalMinutes < 4)
                        return;
                    foreach (var item in RealPlayers.List.Where(p => p.Position.y > -100 && p.Position.y < 100))
                        item.EnableEffect<CustomPlayerEffects.Decontaminating>();
                }, "Decontaminating3");
            }, "Decontaminating2");
        }

        private void Server_SendingRemoteAdminCommand(Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs ev)
        {
            string nameLower = ev.Name.ToLower();
            if (nameLower == "imute" || nameLower == "mute" || nameLower == "enrage")
            {
                if (ev.Sender.IsActiveDev())
                    return;
                ev.IsAllowed = false;
                ev.ReplyMessage = "Command Removed";
            }
            else if (ev.Name.ToLower().StartsWith("cassie"))
            {
                string cassie = string.Join(" ", ev.Arguments);
                float dur = Cassie.CalculateDuration(cassie);
                Log.Debug($"[CASSIE] Long: {dur}s | Message: {cassie}");
                if (dur < 15)
                    return;
                if (ev.Sender.IsActiveDev())
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
            if (ev.IsAllowed)
                LastIntercomUser = ev.Player;
            if (IntercomInfoTimeout)
                return;
            if (Round.IsStarted && Round.ElapsedTime.TotalSeconds > 5 && Intercom.host.IntercomState == Intercom.State.Ready)
                MapPlus.Broadcast("INTERCOM", 5, $"({ev.Player.Id}) {ev.Player.Nickname} started using intercom", Broadcast.BroadcastFlags.AdminChat);
            IntercomInfoTimeout = true;
            this.RunCoroutine(OffCooldown(), "OffCooldown");
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
            Server.Host.GrenadeManager.availableGrenades[0].grenadeInstance.GetComponent<FragGrenade>()
                .hurtLayerMask.value = LayerMask.GetMask("Ignore Raycast", "Player", "Pickup", "Hitbox", "DestroyedDoor", "Door");
            LeftOnStart.Clear();
            LastIntercomUser = null;
            Generators.Clear();
            this.RunCoroutine(InformCommanderDeath(), "InformCommanderDeath");
            this.RunCoroutine(NoVoidFailling(), "NoVoidFailling");
            if (Server.Port == 7791)
                this.RunCoroutine(DoRoundLoop(), "DoRoundLoop");
            //System.IO.File.WriteAllLines(Paths.Configs + "/cassie_words.txt", NineTailedFoxAnnouncer.singleton.voiceLines.Select(i => $"\"{i.apiName}\","));

            Map.ChangeUnitColor(0, "#888");
        }
        private IEnumerator<float> DoRoundLoop()
        {
            yield return Timing.WaitForSeconds(1);
            bool wasCuffed = false;
            while (Round.IsStarted)
            {
                foreach (var item in RealPlayers.List)
                {
                    if (item.Role != RoleType.Scp049)
                    {
                        Base.CustomInfoHandler.Set(item, "cuff", null, false);
                        break;
                    }
                    if (wasCuffed != item.IsCuffed)
                        break;
                    Base.CustomInfoHandler.Set(item, "cuff", item.IsCuffed ? "<color=red><b>CUFFED</b></color>" : null, false);
                    //Dodać aby Spekci widzieli że jest skuty, np ranga z new linem i w new linie wiadomość, ale jeszcze gdy gracz wejdzie, umrze, zrespi
                    wasCuffed = item.IsCuffed;
                }
                yield return Timing.WaitForSeconds(1);
            }
        }
        private IEnumerator<float> InformCommanderDeath()
        {
            yield return Timing.WaitForSeconds(1);
            bool wasAlive = false;
            int rid = RoundPlus.RoundId;
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                if (!wasAlive)
                    wasAlive = RealPlayers.Any(RoleType.NtfCommander);
                else
                {
                    var tmp = RealPlayers.Any(RoleType.NtfCommander);
                    if (!tmp)
                        this.CallDelayed(60, () =>
                        {
                            if (rid == RoundPlus.RoundId)
                                Cassie.GlitchyMessage("WARNING . . . NINETAILEDFOX COMMANDER IS DEAD", 0.3f, 0.15f);
                        }, "InformCommanderDeath");
                    wasAlive = tmp;
                }
                yield return Timing.WaitForSeconds(60);
            }
        }

        private IEnumerator<float> NoVoidFailling()
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId;
            while (Round.IsStarted && rid == RoundPlus.RoundId)
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
            if (ev.NewRole == RoleType.Spectator && !(ev.Player.Team == Team.CHI || ev.Player.Team == Team.SCP || ev.Player.Team == Team.TUT || ev.Player.Team == Team.RIP || ev.Player.Role == RoleType.None))
            {
                if (RealPlayers.List.Where(p => p.Id != ev.Player.Id && p.IsHuman && p.Team != Team.CHI).Count() == 1)
                    Cassie.DelayedGlitchyMessage("Spotted only 1 alive . There could be ChaosInsurgency", 25, 0.5f, 0.1f);
            }
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.Attacker?.IsActiveDev() ?? false)
            {
                if (ev.Attacker.Side == ev.Target.Side && !(ev.Target.Role == RoleType.ClassD && ev.Attacker.Role == RoleType.ClassD) && ev.Target.Id != ev.Attacker.Id)
                {
                    ev.Amount = Math.Min(ev.Target.Health - 1, ev.Amount);
                    this.CallDelayed(0.2f, () => ev.Target.DisableEffect<Bleeding>(), "DevBleading1");
                    return;
                }
            }
            if (ev.Target?.IsActiveDev() ?? false)
            {
                if (ev.Attacker.Side == ev.Target.Side && !(ev.Target.Role == RoleType.ClassD && ev.Attacker.Role == RoleType.ClassD) && ev.Target.Id != ev.Attacker.Id)
                {
                    ev.Amount = Math.Min(ev.Target.Health - 1, ev.Amount);
                    this.CallDelayed(0.2f, () => ev.Target.DisableEffect<Bleeding>(), "DevBleading2");
                    return;
                }
            }
            if (ev.Attacker != null && RealPlayers.Get(ev.Attacker.Id) == null)
                ev.IsAllowed = false;
            if (!ev.IsAllowed)
                return;
            if (ev.DamageType == DamageTypes.Tesla)
            {
                if (ev.Target.Team == Team.SCP && ev.Amount < ev.Target.Health)
                {
                    ev.Target.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Burned>(30);
                    ev.Target.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Concussed>(30);
                    ev.Target.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Deafened>(30);
                    ev.Target.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Disabled>(30);
                }
                else if (ev.Target.ReferenceHub.characterClassManager.AliveTime < 5)
                    ev.IsAllowed = false;
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

            if (ev.Target.Side != ev.Killer?.Side)
                DropBallAndGrenades(ev.Target);

            if (ev.Target.Team == Team.SCP && ev.Target.Role != RoleType.Scp079)
            {
                this.CallDelayed(5, () =>
                {
                    var scps = RealPlayers.Get(Team.SCP).ToArray();
                    if (scps.Length == 1 && scps[0].Role == RoleType.Scp079 && !Generator079.mainGenerator.forcedOvercharge)
                    {
                        Generator079.mainGenerator.forcedOvercharge = true;
                        Recontainer079.BeginContainment(true);
                        Cassie.Message("ALLSECURED . SCP 0 7 9 RECONTAINMENT SEQUENCE COMMENCING . FORCEOVERCHARGE");
                    }
                }, "Dying");
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
            this.CallDelayed(20, () =>
            {
                NewPlayers.Remove(ev.Player.Id);
            }, "FirstTimeJoined");
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
            if (ev.Door.Type() == DoorType.Scp079Second)
                this.RunCoroutine(SpawnPainKillers(), "SpawnPainKillers");
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
