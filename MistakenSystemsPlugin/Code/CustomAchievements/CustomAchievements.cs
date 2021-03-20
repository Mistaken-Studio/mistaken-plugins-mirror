using MEC;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Mirror;
using MistakenSocket.Client.SL;
using System;
using Gamer.Utilities;
using Exiled.API.Enums;
using MistakenSocket.Shared.API;
using MistakenSocket.Shared;
using MistakenSocket.Shared.Achievements;

namespace Gamer.Mistaken.CustomAchievements
{
    public static partial class CustomAchievements
    {
        static internal bool DisableForRound { get; set; } = false;

        public static void OnEnabled(PluginHandler plugin)
        {
            if(PluginHandler.Config.IsHardRP())
                return;
            plugin.RegisterTranslation("achiv_get", T_achiv_get_def);

            new RoundEventHandler(plugin);
        }


        private static string T_achiv_get_def { get; } =    "<color=#dddddd>Achievement unlocked: </color><color={lvlcolor}>{name}</color>" + "|_n" +
                                                            "<color=#696969>Check list of your achievements on </color><color={lvlcolor}>discord.mistaken.pl</color>";

        [System.Obsolete("Achievements are disabled")]
        public static void ForceLevel(string userId, uint Id, Level level)
        {
            ForceLevelRequests.Add(new KeyValuePair<string, KeyValuePair<uint, Level>>(userId, new KeyValuePair<uint, Level>(Id, level)));
            HandleAchievementData(SSL.Client.Send(MessageType.ACHIEVEMENT_REQUEST_INFO, new AchievementRequestProggres(Id, userId)));
        }

        private static readonly List<KeyValuePair<string, KeyValuePair<uint, Level>>> ForceLevelRequests = new List<KeyValuePair<string, KeyValuePair<uint, Level>>>();

        private static readonly HashSet<(string, uint)> OnCooldown = new HashSet<(string, uint)>();
        [System.Obsolete("Achievements are disabled")]
        private static void OnAchievementInfoResponse(uint Id, string UserId, uint Proggres, uint CurrentLevel)
        {
            Achievement achievement = GetAchievement(Id);
            if (achievement == null) return;

            foreach (var item in ForceLevelRequests.ToArray())
            {
                if (item.Key == UserId)
                {
                    var data = item.Value;
                    if (data.Key == Id)
                    {
                        if ((uint)data.Value > CurrentLevel)
                        {
                            var player = RealPlayers.List.FirstOrDefault(p => p.UserId == UserId);
                            if (player != null)
                            {
                                achievement.Achive(player, data.Value);
                                RoundEventHandler.AddProggress("SauronsPride_" + (data.Value).ToString(), player);
                                SaveAchievementProggres(UserId, Id, Proggres, (uint)data.Value);
                                ForceLevelRequests.Remove(item);
                            }
                            return;
                        }
                    }
                }
            }
            for (int i = (int)CurrentLevel + 1; i < 6; i++)
            {
                if (achievement.ProggresLevel.TryGetValue((Level)i, out uint value))
                {
                    if (value <= Proggres && value != 0)
                    {
                        var player = RealPlayers.List.FirstOrDefault(p => p.UserId == UserId);
                        if(player != null)
                        {
                            if (OnCooldown.Contains((UserId, Id)))
                                break;
                            OnCooldown.Add((UserId, Id));
                            MEC.Timing.CallDelayed(5, () => OnCooldown.Remove((UserId, Id)));
                            achievement.Achive(player, (Level)i);
                            RoundEventHandler.AddProggress("SauronsPride_" + ((Level)i).ToString(), player);
                            AddAchievementLevel(UserId, Id);
                            return;
                            //SaveAchievementProggres(UserId, Id, Proggres, (uint)i);
                        }
                    }
                }
            }
        }
        [System.Obsolete("Achievements are disabled")]
        private static void HandleAchievementData(MessageIdentificator? messageId)
        {
            messageId.GetResponseDataCallback((response) => {
                if (response.Type != ResponseType.OK)
                    return;
                var data = response.Payload.Deserialize<AchievementResponseProggres>(0, 0, out _, false);
                OnAchievementInfoResponse(data.Id, data.UserId, data.Proggres, data.CurrentLevel);
            });
        }

        [System.Obsolete("Achievements are disabled")]
        public static void SaveAchievementProggres(string UserId, uint Id, uint Proggres, uint CurrentLevel)
        {
            SSL.Client.Send(MessageType.ACHIEVEMENT_SAVE_PROGGRES, new AchievementSaveProggres(Id, UserId, Proggres, CurrentLevel));
            HandleAchievementData(SSL.Client.Send(MessageType.ACHIEVEMENT_REQUEST_INFO, new AchievementRequestProggres(Id, UserId)));
        }

        [System.Obsolete("Achievements are disabled")]
        public static void AddAchievementProggres(string UserId, uint Id)
        {
            SSL.Client.Send(MessageType.ACHIEVEMENT_ADD_PROGGRES, new AchievementAddProggres(Id, UserId));
            HandleAchievementData(SSL.Client.Send(MessageType.ACHIEVEMENT_REQUEST_INFO, new AchievementRequestProggres(Id, UserId)));
        }

        [System.Obsolete("Achievements are disabled")]
        internal static void AddAchievementLevel(string UserId, uint Id)
        {
            SSL.Client.Send(MessageType.ACHIEVEMENT_ADD_LEVEL, new AchievementAddLevel(Id, UserId));
            HandleAchievementData(SSL.Client.Send(MessageType.ACHIEVEMENT_REQUEST_INFO, new AchievementRequestProggres(Id, UserId)));
        }

        public enum Level
        {
            NONE = 0,
            BRONZE = 1,
            SILVER = 2,
            GOLD = 3,
            DIAMOND = 4,
            EXPERT = 5,
        }
    }
}
