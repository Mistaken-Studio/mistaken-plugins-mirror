using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared;
using MistakenSocket.Shared.Achievements;
using MistakenSocket.Shared.API;
using MistakenSocket.Shared.EVO;
using UnityEngine;

namespace Gamer.Mistaken.EVO
{
    public partial class Handler : Module
    {
        public Handler(PluginHandler p) : base(p)
        {
            plugin.RegisterTranslation("evo_unlock", "You have unlocked $achiev");
        }

        public override string Name => "EVO";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Player.Dying += this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Escaping += this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
            Exiled.Events.Handlers.Scp914.Activating += this.Handle<Exiled.Events.EventArgs.ActivatingEventArgs>((ev) => Scp914_Activating(ev));
            Exiled.Events.Handlers.Warhead.Starting += this.Handle<Exiled.Events.EventArgs.StartingEventArgs>((ev) => Warhead_Starting(ev));
            Exiled.Events.Handlers.Player.IntercomSpeaking += this.Handle<Exiled.Events.EventArgs.IntercomSpeakingEventArgs>((ev) => Player_IntercomSpeaking(ev));
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet += this.Handle<Exiled.Events.EventArgs.InsertingGeneratorTabletEventArgs>((ev) => Player_InsertingGeneratorTablet(ev));
            Exiled.Events.Handlers.Map.GeneratorActivated += this.Handle<Exiled.Events.EventArgs.GeneratorActivatedEventArgs>((ev) => Map_GeneratorActivated(ev));
            Exiled.Events.Handlers.Scp106.Containing += this.Handle<Exiled.Events.EventArgs.ContainingEventArgs>((ev) => Scp106_Containing(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Player.Dying -= this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Escaping -= this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
            Exiled.Events.Handlers.Scp914.Activating -= this.Handle<Exiled.Events.EventArgs.ActivatingEventArgs>((ev) => Scp914_Activating(ev));
            Exiled.Events.Handlers.Warhead.Starting -= this.Handle<Exiled.Events.EventArgs.StartingEventArgs>((ev) => Warhead_Starting(ev));
            Exiled.Events.Handlers.Player.IntercomSpeaking -= this.Handle<Exiled.Events.EventArgs.IntercomSpeakingEventArgs>((ev) => Player_IntercomSpeaking(ev));
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet -= this.Handle<Exiled.Events.EventArgs.InsertingGeneratorTabletEventArgs>((ev) => Player_InsertingGeneratorTablet(ev));
            Exiled.Events.Handlers.Map.GeneratorActivated -= this.Handle<Exiled.Events.EventArgs.GeneratorActivatedEventArgs>((ev) => Map_GeneratorActivated(ev));
            Exiled.Events.Handlers.Scp106.Containing -= this.Handle<Exiled.Events.EventArgs.ContainingEventArgs>((ev) => Scp106_Containing(ev));
        }

        private readonly Dictionary<Generator079, string> GeneratorActivators = new Dictionary<Generator079, string>();
        private void Map_GeneratorActivated(Exiled.Events.EventArgs.GeneratorActivatedEventArgs ev)
        {
            if(GeneratorActivators.TryGetValue(ev.Generator, out string userId))
                AddProgres(1009, userId);
        }

        private void Player_InsertingGeneratorTablet(Exiled.Events.EventArgs.InsertingGeneratorTabletEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            GeneratorActivators.Remove(ev.Generator);
            GeneratorActivators.Add(ev.Generator, ev.Player.UserId);
        }

        public static void RefreshRank(string UserId)
        {
            SSL.Client.Send(MessageType.EVO_REQUEST_RANK, new EVORequestRankData
            {
                UserId = UserId
            }).GetResponseDataCallback((result) => {
                if (result.Type != MistakenSocket.Shared.API.ResponseType.OK)
                    return;
                var data = result.Payload.Deserialize<EVOResponseRankData>(0, 0, out _, false);
                SSL_OnEVOReponseRank(data);
            });
        }

        public static void GetAllRanks(string UserId)
        {
            SSL.Client.Send(MessageType.EVO_REQUEST_ALL_RANKS, new EVORequestRankData
            {
                UserId = UserId
            }).GetResponseDataCallback((result) =>
            {
                if (result.Type != MistakenSocket.Shared.API.ResponseType.OK)
                    return;
                var data = result.Payload.Deserialize<EVOResponseAllRanksData>(0, 0, out _, false);
                var player = RealPlayers.Get(data.UserId);
                if (player == null)
                    return;

                var tmp = data.Ranks.ToList();
                tmp.RemoveAll(i => i.Name == "null");
                tmp.Insert(0, new Rank
                {
                    Name = "None",
                    Color = ""
                });

                if (CommandHandler.SetRequests.TryGetValue(player.UserId, out int prefId))
                {
                    CommandHandler.SetRequests.Remove(player.UserId);
                    try
                    {
                        SSL.Client.Send(MessageType.EVO_CHANGE_PREF_RANK, new EVOChangePrefRank
                        {
                            UserId = player.UserId,
                            Pref = data.Ranks[prefId]
                        });
                    }
                    catch { }
                }
                else
                {
                    string message = "EVO Ranks: ";
                    for (int i = 0; i < data.Ranks.Length; i++)
                        message += "\n" + (data.PrefRank.Name == data.Ranks[i].Name ? "<b>" : "") + $"Index: #{i}, <color={data.Ranks[i].Color.Replace("BLUE_GREEN", "GREEN")}>{data.Ranks[i].Name}</color>" + (data.PrefRank.Name == data.Ranks[i].Name ? "</b>" : "");
                    player.SendConsoleMessage(message, "green");
                }
            });
        }

        private static void SSL_OnEVOReponseRank(EVOResponseRankData data)
        {
            if (data.HasRank)
            {
                Log.Info("[EVO] Rank detected");
                var player = RealPlayers.List.ToList().FirstOrDefault(p => p.UserId == data.UserId);
                if (player == null)
                    return;
                player.BadgeHidden = false;
                player.RankColor = data.Rank.Color.ToLower();
                player.RankName = data.Rank.Name;
            }
            else
            {
                Log.Info("[EVO] No rank detected");
            }
        }
        private static void SSL_OnAchievementInfoResponse(uint Id, string UserId, uint Progress, uint Level)
        {
            var achiev = Achievements.Find(a => a.Id == Id);
            if (achiev == null) return;
            if (Level >= 3) return;
            var nextLevel = achiev.Levels[Level];
            if (nextLevel.Progress <= Progress)
            {
                SSL.Client.Send(MessageType.ACHIEVEMENT_ADD_LEVEL, new AchievementAddLevel(Id, UserId));
                SSL.Client.Send(MessageType.EVO_UNLOCK_RANK, new EVOUnlockRank
                {
                    Rank = new Rank
                    {
                        Color = nextLevel.Color ?? ((Colors)Level + 1).ToString(),
                        Name = nextLevel.Name
                    },
                    UserId = UserId
                });

                RealPlayers.Get(UserId)?.Broadcast("EVO", 10, PluginHandler.Instance.ReadTranslation("evo_unlock").Replace("$achiev", $"<color={nextLevel.Color ?? ((Colors)Level + 1).ToString().Replace("BLUE_GREEN", "GREEN")}>{nextLevel.Name}</color>"));
            }
        }

        private void Scp106_Containing(Exiled.Events.EventArgs.ContainingEventArgs ev)
        {
            if (ev.IsAllowed && ev.ButtonPresser != null && GameObject.FindObjectOfType<LureSubjectContainer>().NetworkallowContain && !OneOhSixContainer.used)
            {
                if (ev.ButtonPresser.DoNotTrack) 
                    return;
                AddProgres(1007, ev.ButtonPresser.UserId);
            }
        }

        private void Player_IntercomSpeaking(Exiled.Events.EventArgs.IntercomSpeakingEventArgs ev)
        {
            if (ev.Player?.DoNotTrack ?? true) return;
            if (ev.IsAllowed && Intercom.host.remainingCooldown <= 0 && Intercom.host.speaking == false)
                AddProgres(1006, ev.Player.UserId);
        }

        private void Warhead_Starting(Exiled.Events.EventArgs.StartingEventArgs ev)
        {
            if (ev.Player == null) return;
            if (ev.Player.DoNotTrack) return;
            if (!ev.IsAllowed) return;
            AddProgres(1000, ev.Player.UserId);
        }

        private void Scp914_Activating(Exiled.Events.EventArgs.ActivatingEventArgs ev)
        {
            if (ev.Player?.DoNotTrack ?? true) return;
            if (ev.Player != null)
                AddProgres(1004, ev.Player.UserId);
        }

        private void Player_Escaping(Exiled.Events.EventArgs.EscapingEventArgs ev)
        {
            if (ev.Player.DoNotTrack) return;
            AddProgres(1002, ev.Player.UserId);
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player.DoNotTrack) 
                return;
            if (ev.NewRole == RoleType.NtfCommander)
                AddProgres(1005, ev.Player.UserId);
            else if (ev.NewRole == RoleType.ChaosInsurgency)
                AddProgres(1008, ev.Player.UserId);
            else if (ev.NewRole.GetTeam() == Team.SCP && ev.NewRole != RoleType.Scp0492)
                AddProgres(1011, ev.Player.UserId);
            else  if (ev.NewRole == RoleType.ClassD)
                AddProgres(1012, ev.Player.UserId);
            else if (ev.NewRole == RoleType.Scientist)
                AddProgres(1013, ev.Player.UserId);
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (ev.Killer.Role == RoleType.Scp049 && !ev.Killer.DoNotTrack)
                AddProgres(1001, ev.Killer.UserId);

            if (ev.Target.Team == Team.SCP && ev.Target.Role != RoleType.Scp0492 && ev.Killer != null && !ev.Killer.DoNotTrack && ev.Target.Id != ev.Killer.Id)
                AddProgres(1003, ev.Killer.UserId);
            if (!ev.Target.DoNotTrack && ev.HitInformation.GetDamageType() == DamageTypes.Decont)
                AddProgres(1014, ev.Target.UserId);
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            RefreshRank(ev.Player.UserId);
            if (ev.Player.DoNotTrack)
                ev.Player.Broadcast(5, "You have active DoNotTrack flag, will not be tracked by server\nYour statistics/achievements were removed");
        }

        public static void AddProgres(uint Id, string UserId)
        {
            if (PluginHandler.Config.IsHardRP())
                return;
            if (UserId == null)
                return;
            SSL.Client.Send(MessageType.ACHIEVEMENT_ADD_PROGGRES, new AchievementAddProggres(Id, UserId));
            SSL.Client.Send(MessageType.ACHIEVEMENT_REQUEST_INFO, new AchievementRequestProggres(Id, UserId)).GetResponseDataCallback((result) =>
            {
                if (result.Type != ResponseType.OK)
                    return;
                var data = result.Payload.Deserialize<AchievementResponseProggres>(0, 0, out _, false);
                SSL_OnAchievementInfoResponse(data.Id, data.UserId, data.Proggres, data.CurrentLevel);
            });           
        }
    }
}
