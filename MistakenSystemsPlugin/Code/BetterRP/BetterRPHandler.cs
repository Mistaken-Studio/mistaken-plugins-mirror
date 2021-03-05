using System.Collections.Generic;
using UnityEngine;
using MEC;
using System;
using System.Linq;
using Gamer.Mistaken.BetterRP.Ambients;
using Exiled.API.Features;
using Exiled.API.Extensions;
using Mirror;
using Gamer.Utilities;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.BetterRP
{
    class Handler : Module
    {
        public Handler(PluginHandler plugin) : base(plugin)
        {
            

            new NicknameHandler(plugin);
        }

        public override string Name => "BetterRP";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundEnded += this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Warhead.Detonated += this.Handle(() => Warhead_Detonated(), "WarheadDetonated");
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.MedicalItemUsed += this.Handle<Exiled.Events.EventArgs.UsedMedicalItemEventArgs>((ev) => Player_MedicalItemUsed(ev));
            Exiled.Events.Handlers.Player.PreAuthenticating += this.Handle<Exiled.Events.EventArgs.PreAuthenticatingEventArgs>((ev) => Player_PreAuthenticating(ev));
            Exiled.Events.Handlers.Player.Escaping += this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
            if (PluginHandler.Config.Type == MSP_ServerType.HARD_RP)
                Exiled.Events.Handlers.CustomEvents.OnTransmitPositionData += this.Handle<Exiled.Events.EventArgs.TransmitPositionEventArgs>((ev) => CustomEvents_OnTransmitPositionData(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundEnded -= this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Warhead.Detonated -= this.Handle(() => Warhead_Detonated(), "WarheadDetonated");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Player.MedicalItemUsed -= this.Handle<Exiled.Events.EventArgs.UsedMedicalItemEventArgs>((ev) => Player_MedicalItemUsed(ev));
            Exiled.Events.Handlers.Player.PreAuthenticating -= this.Handle<Exiled.Events.EventArgs.PreAuthenticatingEventArgs>((ev) => Player_PreAuthenticating(ev));
            Exiled.Events.Handlers.Player.Escaping -= this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
            if (PluginHandler.Config.Type == MSP_ServerType.HARD_RP)
                Exiled.Events.Handlers.CustomEvents.OnTransmitPositionData -= this.Handle<Exiled.Events.EventArgs.TransmitPositionEventArgs>((ev) => CustomEvents_OnTransmitPositionData(ev));
        }

        private readonly Dictionary<int, Camera079> CamerasToPlayers = new Dictionary<int, Camera079>();
        private readonly Dictionary<Camera079, float> Cameras = new Dictionary<Camera079, float>();
        private void CustomEvents_OnTransmitPositionData(Exiled.Events.EventArgs.TransmitPositionEventArgs ev)
        {
            if (ev.Player.Role == RoleType.None)
                return;
            if(ev.Player.Team == Team.RIP && !ev.Player.IsOverwatchEnabled)
            {
                for (int i = 0; i < ev.PositionMessages.Length; i++)
                {
                    var data = ev.PositionMessages[i];
                    if (!CamerasToPlayers.TryGetValue(data.playerID, out Camera079 camera)) 
                    {
                        camera = Map.Cameras[UnityEngine.Random.Range(0, Map.Cameras.Count)];
                        CamerasToPlayers.Add(data.playerID, camera);
                    }
                    int Cycles = Mathf.FloorToInt(((float)NetworkTime.time * 10) / 90);
                    float defRot = Cameras[camera];
                    float tor = defRot + (Cycles % 2 == 0 ? -45 : 45);
                    if(Cycles % 2 == 0) 
                        tor += ((float)NetworkTime.time * 10) % 90;
                    else
                        tor -= ((float)NetworkTime.time * 10) % 90;

                    ev.PositionMessages[i] = new PlayerPositionData(camera.targetPosition.position + Vector3.down, tor, data.playerID);
                }
            }
        }

        private void Player_Escaping(Exiled.Events.EventArgs.EscapingEventArgs ev)
        {
            if (ev.IsAllowed)
            {
                List<Inventory.SyncItemInfo> Items = NorthwoodLib.Pools.ListPool<Inventory.SyncItemInfo>.Shared.Rent(ev.Player.Inventory.items);
                ev.Player.ClearInventory();
                MEC.Timing.CallDelayed(1, () => {
                    foreach (var item in Items)
                    {
                        if (ev.Player.Inventory.items.Count >= 8 || 
                        (item.id.IsWeapon() && ev.Player.Inventory.items.Where(i => i.id.IsWeapon()).Count() >= 2) ||
                        (item.id.IsMedical() && ev.Player.Inventory.items.Where(i => i.id.IsMedical()).Count() >= 3)
                        )
                            item.id.Spawn(item.durability, ev.Player.Position, default, item.modSight, item.modBarrel, item.modOther);
                        else
                            ev.Player.AddItem(item);
                    }
                    NorthwoodLib.Pools.ListPool<Inventory.SyncItemInfo>.Shared.Return(Items);
                });
            }
        }

        private void Player_PreAuthenticating(Exiled.Events.EventArgs.PreAuthenticatingEventArgs ev)
        {
            if (!PluginHandler.Config.IsRP())
                return;
            if (ev.UserId.IsDevUserId())
                return;
            if (MuteHandler.QueryPersistentMute(ev.UserId))
            {
                var writer = new LiteNetLib.Utils.NetDataWriter();
                writer.Put((byte)10);
                var mute = BetterMutes.MuteHandler.GetMute(ev.UserId);
                if (mute == null || mute.Intercom)
                {
                    writer.Put("You are muted so you can't play on RolePlay servers.. Go Play on our other server or ask Admin to unmute you.");
                }
                else
                {
                    string reason = mute.Reason == "removeme" ? "No reason provided" : mute.Reason;
                    if (mute.EndTime == -1)
                        writer.Put($"You are muted so you can't play on RolePlay servers.. You are muted for \"{reason}\", mute has no end date, ask Admin to unmute you.");
                    else
                        writer.Put($"You are muted so you can't play on RolePlay servers.. You are muted for \"{reason}\" until {new DateTime(mute.EndTime):dd.MM.yyyy} UTC");
                }
                ev.Request.Reject(writer);
                ev.Disallow();
            }
        }


        public readonly List<int> UsedPills = new List<int>();
        private void Player_MedicalItemUsed(Exiled.Events.EventArgs.UsedMedicalItemEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (ev.Item == ItemType.Medkit || ev.Item == ItemType.SCP500)
            {
                var pec = ev.Player.ReferenceHub.playerEffectsController;
                pec.DisableEffect<CustomPlayerEffects.Disabled>();
                pec.DisableEffect<CustomPlayerEffects.Poisoned>();
                pec.DisableEffect<CustomPlayerEffects.Bleeding>();
                if (AdrenalineNotReady.Contains(ev.Player.Id)) 
                    AdrenalineNotReady.Remove(ev.Player.Id);
            }
            else if(ev.Item == ItemType.Painkillers)
            {
                UsedPills.Add(ev.Player.Id);
                Timing.CallDelayed(30, () => UsedPills.Remove(ev.Player.Id));
                if(UsedPills.Where(i => i == ev.Player.Id).Count() > 3)
                    ev.Player.EnableEffect<CustomPlayerEffects.Poisoned>(30);
            }
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (!ev.Target.IsHuman)
                return;
            if (UnityEngine.Random.Range(0, 100) < ev.Amount / 5)
            {
                if (ev.DamageType == DamageTypes.Scp0492)
                {
                    CustomAchievements.RoundEventHandler.AddProggress("Plague", ev.Attacker);
                    if(ev.Amount < ev.Target.Health + ev.Target.ArtificialHealth)
                        ev.Target.EnableEffect<CustomPlayerEffects.Poisoned>();
                }
            }
            if (ev.HitInformations.GetDamageType() == DamageTypes.Bleeding)
                ev.Amount *= ev.Target.UserId == "76561198861875204@steam" ? 0.5f : 0.45f;
            if (ev.Amount >= ev.Target.Health + ev.Target.ArtificialHealth)
                return;
            if (!PluginHandler.Config.IsRP()) 
                return;
            if (
                ev.DamageType == DamageTypes.Com15            ||
                ev.DamageType == DamageTypes.E11StandardRifle ||
                ev.DamageType == DamageTypes.Grenade          ||
                ev.DamageType == DamageTypes.Logicer          ||
                ev.DamageType == DamageTypes.MicroHid         ||
                ev.DamageType == DamageTypes.Mp7              ||
                ev.DamageType == DamageTypes.P90              ||
                ev.DamageType == DamageTypes.Scp0492          ||
                ev.DamageType == DamageTypes.Scp939           ||
                ev.DamageType == DamageTypes.Usp
            )
            {
                if (!AdrenalineNotReady.Contains(ev.Target.Id) && ev.Attacker?.Team != ev.Target?.Team)
                {
                    Timing.CallDelayed(0.1f, () =>
                    {
                        ev.Target.ShowHint("You feel <color=yellow>adrenaline</color> hitting", 5);
                        var pec = ev.Target.ReferenceHub.playerEffectsController;
                        var invigorated = pec.GetEffect<CustomPlayerEffects.Invigorated>();
                        var oldInvigoratedIntensityValue = invigorated.Intensity;
                        var oldInvigoratedDurationValue = invigorated.Duration;
                        pec.EnableEffect<CustomPlayerEffects.Invigorated>(5, true);
                        var cola = pec.GetEffect<CustomPlayerEffects.Scp207>();
                        var oldColaIntensityValue = cola.Intensity;
                        var oldColaDurationValue = cola.Duration;
                        pec.EnableEffect<CustomPlayerEffects.Scp207>(5, true);
                        ev.Target.ArtificialHealth += 7;
                        if (cola.Intensity < 1)
                            cola.Intensity = 1;
                        MEC.Timing.CallDelayed(6, () => {
                            if (!ev.Target.IsConnected)
                                return;
                            if(oldInvigoratedIntensityValue > 0)
                            {
                                ev.Target.EnableEffect<CustomPlayerEffects.Invigorated>(oldInvigoratedDurationValue);
                                pec.ChangeEffectIntensity<CustomPlayerEffects.Invigorated>(oldInvigoratedIntensityValue);
                            }
                            if (oldColaIntensityValue > 0)
                            {
                                ev.Target.EnableEffect<CustomPlayerEffects.Scp207>(oldColaDurationValue);
                                pec.ChangeEffectIntensity<CustomPlayerEffects.Scp207>(oldColaIntensityValue);
                            }
                        });

                    });
                    AdrenalineNotReady.Add(ev.Target.Id);
                    Timing.CallDelayed(90, () => {
                        AdrenalineNotReady.Remove(ev.Target.Id);
                    });
                }          
            }
            if(
                ev.DamageType == DamageTypes.Com15 ||
                ev.DamageType == DamageTypes.E11StandardRifle ||
                ev.DamageType == DamageTypes.Grenade ||
                ev.DamageType == DamageTypes.Logicer ||
                ev.DamageType == DamageTypes.Mp7 ||
                ev.DamageType == DamageTypes.P90 ||
                ev.DamageType == DamageTypes.Usp ||
                ev.DamageType == DamageTypes.Scp939
                )
            {
                if (UnityEngine.Random.Range(0, 101) < ev.Amount / 5)
                    ev.Target.EnableEffect<CustomPlayerEffects.Bleeding>();
            }
            else if(ev.DamageType == DamageTypes.Falldown)
            {
                var pec = ev.Target.ReferenceHub.playerEffectsController;
                var rand = UnityEngine.Random.Range(0, 101);
                if (rand < (ev.Amount - 50) / 5)
                {
                    ev.Target.EnableEffect<CustomPlayerEffects.Bleeding>();
                    ev.Target.EnableEffect<CustomPlayerEffects.Ensnared>();
                    ev.Target.ShowHintPulsating("Złamałeś obie nogi i <color=yellow>nie</color> możesz chodzić");
                }
                else if (rand < ev.Amount / 5)
                {
                    ev.Target.EnableEffect<CustomPlayerEffects.Bleeding>();
                    ev.Target.EnableEffect<CustomPlayerEffects.Disabled>();
                }
            }
        }

        public Dictionary<int, List<string>> HealthEffects { get; } = new Dictionary<int, List<string>>();
        public List<int> AdrenalineNotReady { get; } = new List<int>();

        private IEnumerator<float> DoHeathEffects()
        {
            yield return Timing.WaitForSeconds(1);
            while(Round.IsStarted)
            {
                foreach (var player in RealPlayers.List.ToArray())
                {
                    if (HealthEffects.TryGetValue(player.Id, out List<string> effects))
                    {
                        foreach (var effect in effects.ToArray())
                        {
                            switch (effect)
                            {
                                case "Disabled":
                                    player.ReferenceHub.playerEffectsController.DisableEffect<CustomPlayerEffects.Disabled>();
                                    break;
                                case "Deafened":
                                    player.ReferenceHub.playerEffectsController.DisableEffect<CustomPlayerEffects.Deafened>();
                                    break;
                                case "Concussed":
                                    player.ReferenceHub.playerEffectsController.DisableEffect<CustomPlayerEffects.Concussed>();
                                    break;
                                case "Blinded":
                                    player.ReferenceHub.playerEffectsController.DisableEffect<CustomPlayerEffects.Blinded>();
                                    break;
                                default:
                                    Log.Debug("Unknown Effect | "+effect);
                                    continue;
                            }
                            HealthEffects[player.Id].Remove(effect);
                        }
                    }
                    else
                        HealthEffects.Add(player.Id, new List<string>());
                    if (!player.IsHuman || !player.IsConnected) 
                        continue;
                    if (player.Health < 20)
                    {
                        player.EnableEffect<CustomPlayerEffects.Deafened>();
                        HealthEffects[player.Id].Add("Deafened");

                        if (player.Health < 15)
                        {
                            player.EnableEffect<CustomPlayerEffects.Concussed>();
                            HealthEffects[player.Id].Add("Concussed");

                            if (player.Health < 10)
                            {
                                player.EnableEffect<CustomPlayerEffects.Disabled>();
                                HealthEffects[player.Id].Add("Disabled");

                                if (player.Health < 5)
                                {
                                    player.EnableEffect<CustomPlayerEffects.Blinded>();
                                    HealthEffects[player.Id].Add("Blinded");
                                }
                            }
                        }
                    }
                }
                yield return Timing.WaitForSeconds(5);
            }
            Log.Info("DoHeathEffects END");
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player == null) 
                return;
            if (!PluginHandler.Config.IsRP()) 
                return;
            if (ev.NewRole == RoleType.ChaosInsurgency && !ev.Items.Contains(ItemType.Disarmer))
            {
                if (UnityEngine.Random.Range(1, 101) > 75)
                {
                    ev.Items.Add(ItemType.Disarmer);
                }
            }
            else if (ev.NewRole == RoleType.Spectator)
            {
                MEC.Timing.CallDelayed(0.2f, () => {
                    var effectsManager = ev.Player.ReferenceHub.playerEffectsController;
                    foreach (var item in effectsManager.AllEffects)
                    {
                        item.Value.ServerDisable();
                    }
                });
            }
            if (ev.Items.Contains(ItemType.KeycardO5))
            {
                ev.Items.RemoveAll(item => item == ItemType.KeycardNTFLieutenant || item == ItemType.KeycardChaosInsurgency);
            }
            if(PluginHandler.Config.Type == MSP_ServerType.HARD_RP)
                CamerasToPlayers.Remove(ev.Player.Id);
        }

        private void Warhead_Detonated()
        {
            var tmp = Map.Doors.First(d => d.Type() == Exiled.API.Enums.DoorType.SurfaceGate);
            tmp.NetworkTargetState = true;
        }

        private void Server_WaitingForPlayers()
        {
            RoundModifiersManager.SetInstance();
            if (UnityEngine.Random.Range(1, 101) < 2)
            {
                RoundModifiersManager.Instance.SetActiveEvents();
                Log.Debug("Activating random events");
            }
        }

        private void Server_RoundStarted()
        {
            if (!PluginHandler.Config.IsRP()) return;
            HealthEffects.Clear();
            Timing.RunCoroutine(DoAmbients());
            Timing.RunCoroutine(DoHeathEffects());
            RoundModifiersManager.Instance.ExecuteFags();
            CamerasToPlayers.Clear();
            Cameras.Clear();
            foreach (var item in Map.Cameras)
                Cameras.Add(item, item.curRot);
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            if (!PluginHandler.Config.IsRP()) 
                return;
            foreach (var item in Pickup.Instances.ToArray())
                item.Delete();

            foreach (var player in RealPlayers.List.ToArray())
                player.ClearInventory();
        }


        public static readonly Ambient[] Ambients = new Ambient[]
        {
            new Spotted035(),
            new ClassEtoCheckpoint(),
            new MajorSciJuly(),
            new MajorSciDark(),
            new FacilityManager(),
            new Intruders(),
            new DoctorRJ(),
            new SCP008(),
            new SCP131A(),
            new SCP066(),
            new SCP538(),
            //new CASSIECIvsMTF(),
            new CassieIni(),
            new CassieStart1(),
            new CassieStart2(),
            new CassieStart3(),
            new CassieStart4(),
            new CassieStart5(),
            new NinetailedfoxWait(),
            new NinetailedfoxTerminateChaos(),


            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
            new RandomAmbient(),
        };

        public static bool AmbientLock = false;
        public const float DefaultChance = 10;
        public static float Chance { get; private set; } = DefaultChance;
        IEnumerator<float> DoAmbients()
        {
            UsedAmbients.Clear();
            yield return Timing.WaitForSeconds(5);
            while(Round.IsStarted)
            {
                if (UnityEngine.Random.Range(1, 101) <= Chance && !AmbientLock)
                {
                    var msg = GetAmbient(out bool jammed);
                    if (msg != null)
                    {
                        while (Cassie.IsSpeaking)
                            yield return Timing.WaitForOneFrame;
                        if (jammed)
                            NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(msg, 0.1f, 0.07f);
                        else
                            Cassie.Message(msg, false, false);
                    }
                    Chance -= 5;
                } 
                else 
                    Chance = DefaultChance;
                yield return Timing.WaitForSeconds(UnityEngine.Random.Range(120, 300));
            }
        }

        public static readonly List<int> UsedAmbients = new List<int>();
        internal static string GetAmbient(out bool jammed, int id = -1)
        {
            return GetAmbient(0, out jammed, id);
        }
        internal static string GetAmbient(int overflowId, out bool jammed, int id = -1)
        {
            jammed = true;
            if (overflowId > 100) return "CASSIE CRITICAL ERROR DETECTED";
            Ambient ambient = null;
            if(id == -1)
            {
                int random = UnityEngine.Random.Range(0, Ambients.Length);
                ambient = Ambients[random];
            }
            else
            {
                ambient = Ambients.First(item => item.Id == id);
                if (ambient == null) return GetAmbient(overflowId + 1, out jammed);
            }
            
            if (UsedAmbients.Contains(ambient.Id)) return GetAmbient(overflowId + 1, out jammed);
            if (!ambient.CanPlay()) return GetAmbient(overflowId + 1, out jammed);
            //else if (id == 100) return null;
            if(!ambient.IsReusable) UsedAmbients.Add(ambient.Id);
            jammed = ambient.IsJammed;
            return ambient.Message
                .Replace("$classd", RealPlayers.List.Where(p => p.Team == Team.CDP).Count().ToString())
                .Replace("$mtf", RealPlayers.List.Where(p => p.Team == Team.MTF).Count().ToString())
                .Replace("$ci", RealPlayers.List.Where(p => p.Team == Team.CHI).Count().ToString());
        }
    }
}
