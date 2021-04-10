using System;
using System.Reflection;
using UnityEngine;
using RemoteAdmin;
using Cryptography;
using Gamer.Mistaken.LOFH;
using System.Collections.Generic;
using GameCore;
using System.Text;
using Mirror;
using Exiled.API.Features;
using Gamer.Utilities;
using HarmonyLib;
using System.Linq;
using Gamer.Mistaken.Systems.Misc;

namespace RemoteAdmin
{
    [HarmonyPatch(typeof(CommandProcessor))]
    [HarmonyPatch("ProcessQuery")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(CommandSender) })]
    internal static class LOFHPatch
    {
        public static readonly HashSet<string> DisabledFor = new HashSet<string>();

        public static string RoleToColor(RoleType role, bool ovrm)
        {
            if (ovrm) 
                return "#008080";
            switch(role)
            {
                case RoleType.ChaosInsurgency:
                    return "#1d6f00";
                case RoleType.ClassD:
                    return "#ff8400";
                case RoleType.FacilityGuard:
                    return "#7795a9";
                case RoleType.None:
                    return "#afafaf";
                case RoleType.NtfCadet:
                    return "#61beff";
                case RoleType.NtfLieutenant:
                    return "#0096ff";
                case RoleType.NtfCommander:
                    return "#1200ff";
                case RoleType.NtfScientist:
                    return "#4e63ff";
                case RoleType.Scientist:
                    return "#f1e96e";
                case RoleType.Scp93989:
                case RoleType.Scp93953:
                case RoleType.Scp173:
                case RoleType.Scp106:
                case RoleType.Scp096:
                case RoleType.Scp079:
                case RoleType.Scp049:
                    return "#FF0000";
                case RoleType.Scp0492:
                    return "#800000";
                case RoleType.Spectator:
                    return "#FFFFFF";
                case RoleType.Tutorial:
                    return "#00FF00";

                default:
                    return "#FF00FF";
            }
        }

        public static bool Prefix(string q, CommandSender sender)
        {
            try
            {
                if (sender == null)
                    return true;
                if (!sender.IsPlayer())
                    return true;
                var senderPlayer = sender.GetPlayer();
                if (DisabledFor.Contains(senderPlayer.UserId))
                    return true;
                GameObject senderGameObject = senderPlayer.GameObject;

                foreach (GameObject player2 in PlayerManager.players)
                {
                    if (player2 == null) 
                        continue;
                    if (ReferenceHub.GetHub(player2).characterClassManager.UserId == sender.SenderId)
                        senderGameObject = player2;
                }
                //PlayerCommandSender playerCommandSender1 = sender as PlayerCommandSender;
                string[] query = q.Split(' ');

                //QueryProcessor processor = playerCommandSender1?.Processor;
                //if (playerCommandSender1 != null)
                //str1 = str1 + " (" + playerCommandSender1.CCM.UserId + ")";
                switch (query[0].ToUpper())
                {
                    case "BAN":
                        {
                            if (query[1].Contains("."))
                            {
                                var tmp = query[1].Split('.');
                                query[1] = tmp[0];
                                if (tmp.Length > 1)
                                {
                                    if (!string.IsNullOrWhiteSpace(tmp[1]))
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Anty Missclick: \nYou can't execute this command on more than one player, please select only one", false, true, "");
                                        return false;
                                    }
                                }
                            }
                            if(!int.TryParse(query[1], out int playerId))
                            {
                                sender.RaReply(query[0].ToUpper() + "#Invalid Player: " + query[1], false, true, "");
                                return false;
                            }
                            string reason = string.Empty;
                            if (query.Length > 3)
                            {
                                reason = query.Skip(3).Aggregate((string current, string n) => current + " " + n);
                            }
                            int duration = 0;
                            try
                            {
                                duration = Misc.RelativeTimeToSeconds(query[2], 60);
                            }
                            catch
                            {
                                sender.RaReply(query[0].ToUpper() + "#Invalid time: " + query[2], false, true, "");
                                return false;
                            }
                            if (duration < 0)
                            {
                                duration = 0;
                                query[2] = "0";
                            }
                            sender.RaReply($"{query[0].ToUpper()}#<size=100%>{MenuSystem.ProccessPress(senderPlayer, playerId, duration, reason)}</size>", true, true, "");
                            return false;
                        }
                    case "REQUEST_DATA":
                        if (query.Length >= 2)
                        {
                            string upper = query[1].ToUpper();
                            if (!(upper == "PLAYER_LIST"))
                            {
                                if (!(upper == "PLAYER") && !(upper == "SHORT-PLAYER") && MenuSystem.CurrentMenus[senderPlayer.Id] == 0)
                                    return true;
                                else if (query.Length >= 3)
                                {
                                    try
                                    {
                                        string rawQuery = query[2];
                                        if (query[2].Contains("."))
                                            query[2] = query[2].Split('.')[0];
                                        if((int.TryParse(query[2], out int playerId) && playerId == 8000) || MenuSystem.CurrentMenus[senderPlayer.Id] != 0)
                                        {
                                            sender.RaReply($"{query[0].ToUpper()}:PLAYER#<size=100%>{MenuSystem.ProccessPress(senderPlayer, playerId, rawQuery, (upper == "SHORT-PLAYER" ? 0 : (upper == "PLAYER" ? 1 : 2)))}</size>", true, true, "PlayerInfo");
                                            sender.RaReply("PlayerInfoQR#ModifiedRA", true, false, "PlayerInfo");
                                        }
                                        else
                                        {
                                            GameObject gameObject5 = null;
                                            NetworkConnection networkConnection = null;
                                            foreach (NetworkConnection networkConnection2 in NetworkServer.connections.Values)
                                            {
                                                GameObject gameObject6 = GameCore.Console.FindConnectedRoot(networkConnection2);
                                                if (!(gameObject6 == null) && !(ReferenceHub.GetHub(gameObject6).queryProcessor.PlayerId.ToString() != query[2]))
                                                {
                                                    gameObject5 = gameObject6;
                                                    networkConnection = networkConnection2;
                                                }
                                            }
                                            if (gameObject5 == null)
                                            {
                                                sender.RaReply(query[0].ToUpper() + ":PLAYER#Player with id " + (string.IsNullOrEmpty(query[2]) ? "[null]" : query[2]) + " not found!", false, true, "");
                                            }
                                            else
                                            {
                                                bool gameplayData = global::PermissionsHandler.IsPermitted(sender.Permissions, global::PlayerPermissions.GameplayData);
                                                bool flag6 = global::PermissionsHandler.IsPermitted(sender.Permissions, 18007046UL);// || (sender.IsPlayer() && sender.GetPlayer().UserId.IsDevUserId());

                                                senderPlayer.ReferenceHub.queryProcessor.GameplayData = gameplayData;
                                                if ((senderPlayer.ReferenceHub.serverRoles.Staff || senderPlayer.ReferenceHub.serverRoles.RaEverywhere))
                                                {
                                                    flag6 = true;
                                                }
                                                global::ReferenceHub hub = global::ReferenceHub.GetHub(gameObject5);
                                                global::CharacterClassManager characterClassManager = hub.characterClassManager;
                                                global::ServerRoles serverRoles = hub.serverRoles;
                                                var checkedPlayer = Player.Get(gameObject5);
                                                if (query[1].ToUpper() == "PLAYER")
                                                {
                                                    global::ServerLogs.AddLog(global::ServerLogs.Modules.DataAccess, string.Concat(new object[]
                                                    {
                                                        sender.LogName,
                                                        " accessed IP address of player ",
                                                            gameObject5.GetComponent<QueryProcessor>().PlayerId,
                                                        " (",
                                                            gameObject5.GetComponent<global::NicknameSync>().MyNick,
                                                        ")."
                                                    }), global::ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
                                                }
                                                StringBuilder stringBuilder = NorthwoodLib.Pools.StringBuilderPool.Shared.Rent();
                                                stringBuilder.Append("<color=white>");
                                                float textSize = 95;
                                                stringBuilder.Append("Nickname: " + hub.nicknameSync.CombinedName);
                                                stringBuilder.Append("\nPlayer ID: " + hub.queryProcessor.PlayerId);
                                                stringBuilder.Append("\nIP: " + ((networkConnection != null) ? ((query[1].ToUpper() == "PLAYER") ? networkConnection.address : "[REDACTED]") : "null"));
                                                stringBuilder.Append("\nUser ID: " + (flag6 ? (string.IsNullOrEmpty(characterClassManager.UserId) ? "(none)" : characterClassManager.UserId) : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>"));
                                                if (flag6)
                                                {
                                                    if (characterClassManager.SaltedUserId != null && characterClassManager.SaltedUserId.Contains("$"))
                                                    {
                                                        stringBuilder.Append("\nSalted User ID: " + characterClassManager.SaltedUserId);
                                                    }
                                                    if (!string.IsNullOrEmpty(characterClassManager.UserId2))
                                                    {
                                                        stringBuilder.Append("\nUser ID 2: " + characterClassManager.UserId2);
                                                    }
                                                }
                                                string serverRole = serverRoles.GetColoredRoleString(false);
                                                if (serverRole != "") stringBuilder.Append("\nServer role: " + serverRole);
                                                bool hidden = PermissionsHandler.IsPermitted(sender.Permissions, global::PlayerPermissions.ViewHiddenBadges);
                                                bool hiddenGlobal = PermissionsHandler.IsPermitted(sender.Permissions, global::PlayerPermissions.ViewHiddenGlobalBadges);
                                                if (senderPlayer.ReferenceHub.serverRoles.Staff)
                                                {
                                                    hidden = true;
                                                    hiddenGlobal = true;
                                                }
                                                bool flag9 = !string.IsNullOrEmpty(serverRoles.HiddenBadge);
                                                bool flag10 = !flag9 || (serverRoles.GlobalHidden && hiddenGlobal) || (!serverRoles.GlobalHidden && hidden);
                                                if (flag10)
                                                {
                                                    if (flag9)
                                                    {
                                                        stringBuilder.Append("\n<color=#DC143C>Hidden role: </color>" + serverRoles.HiddenBadge);
                                                        stringBuilder.Append("\n<color=#DC143C>Hidden role type: </color>" + (serverRoles.GlobalHidden ? "GLOBAL" : "LOCAL"));
                                                    }
                                                    if (serverRoles.RaEverywhere)
                                                    {
                                                        stringBuilder.Append("\nActive flag: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>");
                                                    }
                                                    else if (serverRoles.Staff)
                                                    {
                                                        stringBuilder.Append("\nActive flag: Studio Staff");
                                                    }
                                                }
                                                if (characterClassManager.Muted)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#F70D1A>SERVER MUTED</color>");
                                                }
                                                else if (characterClassManager.IntercomMuted)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#F70D1A>INTERCOM MUTED</color>");
                                                }
                                                if (characterClassManager.GodMode)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#659EC7>GOD MODE</color>");
                                                }
                                                if (characterClassManager.NoclipEnabled)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#DC143C>NOCLIP ENABLED</color>");
                                                }
                                                if (serverRoles.DoNotTrack)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#BFFF00>DO NOT TRACK</color>");
                                                }
                                                if (serverRoles.BypassMode)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#BFFF00>BYPASS MODE</color>");
                                                }

                                                if (!string.IsNullOrWhiteSpace(checkedPlayer.GroupName))
                                                {
                                                    stringBuilder.Append($"\nGroup: {checkedPlayer.GroupName}");
                                                }
                                                string[] flags = LOFH.GetFlags(characterClassManager.UserId);

                                                if (LOFH.ThreatLevelDatas.TryGetValue(characterClassManager.UserId, out Gamer.Mistaken.Systems.ThreatLevel.ThreadLevelData threatData) && threatData.Response.Length != 0)
                                                {
                                                    var color = (threatData.Level == 4 ? "black" : (threatData.Level == 3 ? "red" : (threatData.Level == 2 ? "orange" : (threatData.Level == 1 ? "yellow" : "white"))));
                                                    stringBuilder.Append($"\n<color={color}>Threat Level is {threatData.Level} {(threatData.Response.Length == 0 ? "" : $"| Reasons ({threatData.Num}):")}</color>");
                                                    foreach (var item in threatData.Response)
                                                        stringBuilder.Append("\n - " + item);
                                                }
                                                for (int i = 0; i < flags.Length; i++)
                                                    stringBuilder.Append("\n" + flags[i]);


                                                if (LOFH.Country.TryGetValue(characterClassManager.UserId, out string countryCode) && countryCode != "PL")
                                                    stringBuilder.Append("\nCountry Code: " + countryCode);
                                                if (flag10 && serverRoles.RemoteAdmin)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#43C6DB>REMOTE ADMIN AUTHENTICATED</color>");
                                                }
                                                if (checkedPlayer.IsOverwatchEnabled)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#008080>OVERWATCH MODE</color>");
                                                }
                                                else
                                                {
                                                    if (!gameplayData)
                                                    {
                                                        stringBuilder.Append("\nClass: " + "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>");
                                                        if (hub.characterClassManager.NetworkCurClass != RoleType.Spectator)
                                                        {
                                                            stringBuilder.Append("\nHP: " + "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>");
                                                            stringBuilder.Append("\nAHP: " + "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>");
                                                            stringBuilder.Append("\nPosition: " + "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>");
                                                            stringBuilder.Append("\nEffects: " + "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>");
                                                        }
                                                        stringBuilder.Append("\n<color=#D4AF37>* GameplayData permission required</color>");
                                                    }
                                                    else
                                                    {
                                                        stringBuilder.Append("\nClass: " + (characterClassManager.Classes.CheckBounds(characterClassManager.CurClass) ? characterClassManager.CurRole.fullName : "None"));
                                                        if (hub.characterClassManager.NetworkCurClass != RoleType.Spectator)
                                                        {
                                                            stringBuilder.Append("\nHP: " + hub.playerStats.HealthToString());
                                                            stringBuilder.Append($"\nAHP: {hub.playerStats.NetworksyncArtificialHealth}/{hub.playerStats.NetworkmaxArtificialHealth}");
                                                            stringBuilder.Append("\nPosition: " + string.Format("[{0}; {1}; {2}]", hub.playerMovementSync.RealModelPosition.x, hub.playerMovementSync.RealModelPosition.y, hub.playerMovementSync.RealModelPosition.z));
                                                            List<string> Effects = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
                                                            foreach (var item in hub.playerEffectsController.AllEffects)
                                                            {
                                                                if (item.Value.Enabled)
                                                                    Effects.Add($"{item.Key.Name} ({item.Value.Intensity})" + (item.Value.Duration > 0 ? $" {item.Value.Duration}s left" : ""));
                                                            }
                                                            if (Effects.Count > 0)
                                                            {
                                                                stringBuilder.Append("\nEffects:");
                                                                foreach (var effect in Effects)
                                                                {
                                                                    stringBuilder.Append($"\n- {effect}");
                                                                }
                                                                textSize = 75;
                                                            }
                                                            NorthwoodLib.Pools.ListPool<string>.Shared.Return(Effects);
                                                            if (Gamer.Mistaken.Systems.Shield.ShieldedManager.Has(checkedPlayer))
                                                            {
                                                                var data = Gamer.Mistaken.Systems.Shield.ShieldedManager.Get(checkedPlayer);
                                                                stringBuilder.Append($"\nShield: Max: {data.MaxShield} | Regeneration: {data.Regeneration}");
                                                            }
                                                        }
                                                    }
                                                }
                                                stringBuilder.Append("</color>");
                                                sender.RaReply($"{query[0].ToUpper()}:PLAYER#<size={textSize}%>{stringBuilder}</size>", true, true, "PlayerInfo");
                                                sender.RaReply("PlayerInfoQR#" + (string.IsNullOrEmpty(characterClassManager.UserId) ? "(no User ID)" : characterClassManager.UserId), true, false, "PlayerInfo");
                                                NorthwoodLib.Pools.StringBuilderPool.Shared.Return(stringBuilder);
                                            }
                                        }
                                        return false;
                                    }
                                    catch (Exception ex3)
                                    {
                                        sender.RaReply(string.Concat(new string[]
                                        {
                                        query[0].ToUpper(),
                                        "#An unexpected problem has occurred!\nMessage: ",
                                        ex3.Message,
                                        "\nStackTrace: ",
                                        ex3.StackTrace,
                                        "\nAt: ",
                                        ex3.Source
                                        }), false, true, "PlayerInfo");
                                        throw;
                                    }
                                }
                                else
                                {
                                    sender.RaReply(query[0].ToUpper() + ":PLAYER#Please specify the PlayerId!", false, true, "");
                                    return false;
                                }
                            }
                            else
                            {
                                try
                                {
                                    senderPlayer.ReferenceHub.queryProcessor.GameplayData = true;
                                    if (q.Contains("STAFF"))
                                        return true;
                                    bool hiddenBages = PermissionsHandler.IsPermitted(sender.Permissions, global::PlayerPermissions.ViewHiddenBadges);
                                    bool hiddenGlobalBages = PermissionsHandler.IsPermitted(sender.Permissions, global::PlayerPermissions.ViewHiddenGlobalBadges);
                                    if (senderPlayer.ReferenceHub.serverRoles.Staff)
                                    {
                                        hiddenBages = true;
                                        hiddenGlobalBages = true;
                                    }
                                    string text3 = $"\n{MenuSystem.GetMenu(senderPlayer, out bool GeneratePlayerList)}";
                                    if (GeneratePlayerList)
                                    {
                                        foreach (Player player in RealPlayers.List)
                                        {
                                            if (player == null)
                                                continue;
                                            QueryProcessor queryProcessor = player.ReferenceHub.queryProcessor;
                                            string text4 = string.Empty;
                                            bool ovrm = false;

                                            string lofhData = LOFH.GetConstructedPrefix(player.UserId);
                                            ServerRoles serverRoles = player.ReferenceHub.serverRoles;
                                            if (LOFH.Hidden.Contains(player.UserId))
                                            {
                                                if (player.Id == senderPlayer.Id || serverRoles.KickPower <= senderPlayer.ReferenceHub.serverRoles.KickPower)
                                                    text3 += "<color=red>[<color=white><b>HIDDEN</b></color>]</color>";
                                                else
                                                    continue;
                                            }
                                            try
                                            {
                                                if (string.IsNullOrEmpty(serverRoles.HiddenBadge) || (serverRoles.GlobalHidden && hiddenGlobalBages) || (!serverRoles.GlobalHidden && hiddenBages))
                                                    text4 = (serverRoles.RaEverywhere ? "[~] " : (serverRoles.Staff ? "[@] " : lofhData != "" ? lofhData + " " : (serverRoles.RemoteAdmin ? "[RA] " : string.Empty)));
                                                else
                                                    text4 = lofhData != "" ? $"{lofhData} " : string.Empty;
                                                ovrm = player.IsOverwatchEnabled;
                                            }
                                            catch
                                            {
                                            }
                                            text3 = string.Concat(new object[]
                                            {
                                                text3,
                                                text4,
                                                $"<color={RoleToColor(player.Role, ovrm)}>",
                                                "(",
                                                queryProcessor.PlayerId,
                                                ") ",
                                                player.ReferenceHub.nicknameSync.CombinedName.Replace("\n", string.Empty),
                                                ovrm ? "<OVRM>" : string.Empty,
                                                "</color>"
                                            });
                                            text3 += "\n";
                                        }
                                    }
                                    sender.RaReply(query[0].ToUpper() + ":PLAYER_LIST#" + text3, true, query.Length < 3 || query[2].ToUpper() != "SILENT", "");
                                    return false;
                                }
                                catch (Exception ex2)
                                {
                                    sender.RaReply(string.Concat(new string[]
                                    {
                                        query[0].ToUpper(),
                                        ":PLAYER_LIST#An unexpected problem has occurred!\nMessage: ",
                                        ex2.Message,
                                        "\nStackTrace: ",
                                        ex2.StackTrace,
                                        "\nAt: ",
                                        ex2.Source
                                    }), false, true, "");
                                    throw;
                                }
                            }
                        }
                        else
                            return true;
                    default:
                        return true;
                }
            }
            catch (System.Exception e)
            {
                Gamer.Utilities.Logger.Error("LOFHBase Late Error Catch", e.Message);
                Gamer.Utilities.Logger.Error("LOFHBase Late Error Catch", e.StackTrace);
                return true;
            }
        }
    }
}

