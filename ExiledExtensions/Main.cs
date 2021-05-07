using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Features;
using Mirror;
using NPCS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Utilities
{
    /// <summary>
    /// Announymous Events
    /// </summary>
    public static class AnnonymousEvents
    {
        private static readonly Dictionary<string, List<Action<object>>> Subscribers = new Dictionary<string, List<Action<object>>>();

        /// <summary>
        /// Calls Event
        /// </summary>
        /// <param name="name">Event Name</param>
        /// <param name="arg">Event args</param>
        public static void Call(string name, object arg)
        {
            Log.Debug("Running " + name);
            if (Subscribers.TryGetValue(name, out List<Action<object>> handlers))
            {
                foreach (var item in handlers)
                    item(arg);
            }
        }
        /// <summary>
        /// Subscribes to event
        /// </summary>
        /// <param name="name">Event name</param>
        /// <param name="handler">Event handler</param>
        public static void Subscribe(string name, Action<object> handler)
        {
            Log.Debug("Subscribing to " + name);
            if (!Subscribers.ContainsKey(name))
                Subscribers[name] = new List<Action<object>>();
            Subscribers[name].Add(handler);
        }
        /// <summary>
        /// UnSubscribes to event
        /// </summary>
        /// <param name="name">Event name</param>
        /// <param name="handler">Event handler</param>
        public static void UnSubscribe(string name, Action<object> handler)
        {
            Log.Debug("UnSubscribing to " + name);
            if (Subscribers.ContainsKey(name))
                Subscribers[name].Remove(handler);
        }
    }
    /// <summary>
    /// Main Utils
    /// </summary>
    public static class Main
    {
        /// <summary>
        /// List of staff that allowed to ignore DNT
        /// </summary>
        public static string[] IgnoredUIDs = new string[] { };
        /// <summary>
        /// Returns room offseted position
        /// </summary>
        /// <param name="me">Room</param>
        /// <param name="offset">Offset</param>
        /// <returns>Position</returns>
        public static Vector3 GetByRoomOffset(this Room me, Vector3 offset)
        {
            var basePos = me.Position;
            offset = me.transform.forward * -offset.x + me.transform.right * -offset.z + Vector3.up * offset.y;
            basePos += offset;
            return basePos;
        }
        /// <inheritdoc cref="MapPlus.Broadcast(string, ushort, string, global::Broadcast.BroadcastFlags)"/>
        public static void Broadcast(this Player me, string tag, ushort duration, string message, Broadcast.BroadcastFlags flags = global::Broadcast.BroadcastFlags.Normal)
        {
            me.Broadcast(duration, $"<color=orange>[<color=green>{tag}</color>]</color> {message}", flags);
        }

        /// <summary>
        /// Checks if player has base game permission
        /// </summary>
        /// <param name="me">Player</param>
        /// <param name="perms">Permission</param>
        /// <returns>If has permission</returns>
        public static bool CheckPermissions(this Player me, PlayerPermissions perms)
        {
            return PermissionsHandler.IsPermitted(me.ReferenceHub.serverRoles.Permissions, perms);
        }
        /// <summary>
        /// If player is Dev
        /// </summary>
        /// <param name="me">Player</param>
        /// <returns>Is Dev</returns>
        public static bool IsDev(this Player me)
        {
            if (me == null)
                return false;
            return me.UserId.IsDevUserId();
        }
        /// <summary>
        /// Returns if UserId is Dev's userId
        /// </summary>
        /// <param name="me">UserId</param>
        /// <returns>If belongs to dev</returns>
        public static bool IsDevUserId(this string me)
        {
            if (me == null)
                return false;
            switch (me.Split('@')[0])
            {
                //WW
                case "76561198134629649":
                case "356174382655209483":
                //Barwa
                case "76561198035545880":
                case "373551302292013069":
                case "barwa":
                //Xname
                case "76561198123437513":
                case "373911388575236096":
                //Hyper
                case "76561198215262787":
                case "365499281215586326":
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Returns player
        /// </summary>
        /// <param name="me">Potentialy player</param>
        /// <returns>Player</returns>
        public static Player GetPlayer(this CommandSender me) => Player.Get(me.SenderId);
        /// <summary>
        /// Returns player
        /// </summary>
        /// <param name="me">Potentialy player</param>
        /// <returns>Player</returns>
        public static Player GetPlayer(this ICommandSender me) => Player.Get(((CommandSender)me).SenderId);
        /// <summary>
        /// Returns if <paramref name="me"/> is Player or Server
        /// </summary>
        /// <param name="me">To Check</param>
        /// <returns>Result</returns>
        public static bool IsPlayer(this CommandSender me) => GetPlayer(me) != null;
        /// <summary>
        /// Returns if <paramref name="me"/> is Player or Server
        /// </summary>
        /// <param name="me">To Check</param>
        /// <returns>Result</returns>
        public static bool IsPlayer(this ICommandSender me) => GetPlayer(me) != null;

        /// <summary>
        /// If player has DNT and if it should be effective
        /// </summary>
        /// <param name="me">Player</param>
        /// <returns>if has DNT</returns>
        public static bool IsDNT(this Player me)
        {
            if (IgnoredUIDs.Contains(me.UserId))
                return false;
            return me.DoNotTrack;
        }
        /// <summary>
        /// Session Vars
        /// </summary>
        public enum SessionVarType
        {
#pragma warning disable CS1591 // Brak komentarza XML dla widocznego publicznie typu lub składowej
            RUN_SPEED,
            WALK_SPEED,
            TALK,
            ITEM_LESS_CLSSS_CHANGE,
            HIDDEN,
            LONG_OVERWATCH,
            NO_SPAWN_PROTECT,
            VANISH,
            CI_ARMOR,
            CI_LIGHT_ARMOR,
            CI_HEAVY_ARMOR,
            CI_TASER,
            CI_SNAV,
            CI_IMPACT,
            CI_SCP1499,
            CI_GRENADE_LAUNCHER,
            CI_GUARD_COMMANDER_KEYCARD,
            CC_ZONE_MANAGER_KEYCARD,
            CC_DEPUTY_FACILITY_MANAGER_KEYCARD,
            CC_GUARD_COMMANDER,
            CC_ZONE_MANAGER,
            CC_DEPUTY_FACILITY_MANAGER,
            CC_IGNORE_CHANGE_ROLE
#pragma warning restore CS1591 // Brak komentarza XML dla widocznego publicznie typu lub składowej
        }
        /// <summary>
        /// Returns SessionVarValue or <paramref name="defaultValue"/> if was not found
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="me">Player</param>
        /// <param name="type">Session Var</param>
        /// <param name="defaultValue">Default Value</param>
        /// <returns>Value</returns>
        public static T GetSessionVar<T>(this Player me, SessionVarType type, T defaultValue = default) => me.GetSessionVar<T>(type.ToString(), defaultValue);
        /// <summary>
        /// Returns SessionVarValue or <paramref name="defaultValue"/> if was not found
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="me">Player</param>
        /// <param name="name">Session Var</param>
        /// <param name="defaultValue">Default Value</param>
        /// <returns>Value</returns>
        public static T GetSessionVar<T>(this Player me, string name, T defaultValue = default)
        {
            if (me.SessionVariables.TryGetValue(name, out object value))
            {
                if (value is T Tvalue)
                    return Tvalue;
                else
                    throw new ArgumentException($"Expected to see SessionVariable of type {typeof(T).FullName} but got {value.GetType().FullName}");
            }
            else
                return defaultValue;
        }
        /// <summary>
        /// Sets SessionVarValue
        /// </summary>
        /// <param name="me">Player</param>
        /// <param name="type">Session Var</param>
        /// <param name="value">Value</param>
        public static void SetSessionVar(this Player me, SessionVarType type, object value) => me.SetSessionVar(type.ToString(), value);
        /// <summary>
        /// Sets SessionVarValue
        /// </summary>
        /// <param name="me">Player</param>
        /// <param name="name">Session Var</param>
        /// <param name="value">Value</param>
        public static void SetSessionVar(this Player me, string name, object value) => me.SessionVariables[name] = value;


        /// <summary>
        /// Returns if player has permission
        /// </summary>
        /// <param name="cs">Player</param>
        /// <param name="permission">Permission</param>
        /// <returns>If has permisison</returns>
        public static bool CheckPermission(this CommandSender cs, string permission) => CheckPermission(cs.GetPlayer(), permission);
        /// <summary>
        /// Returns if player has permission
        /// </summary>
        /// <param name="player">Player</param>
        /// <param name="permission">Permission</param>
        /// <returns>If has permisison</returns>
        public static bool CheckPermission(this Player player, string permission)
        {
            if (player.IsDev())
                return true;
            else
            {
                if (Exiled.Permissions.Extensions.Permissions.CheckPermission(player, permission))
                    return true;
                else
                {
                    UserGroup userGroup = ServerStatic.GetPermissionsHandler().GetUserGroup(player.UserId);
                    Group group = null;
                    if (userGroup != null)
                    {
                        string key = ServerStatic.GetPermissionsHandler()._groups.FirstOrDefault((KeyValuePair<string, UserGroup> g) => g.Value == player.Group).Key ?? "";
                        Log.Debug("Group " + key, true);
                        if (Exiled.Permissions.Extensions.Permissions.Groups == null)
                        {
                            Log.Error("Permissions config is null.");
                            return false;
                        }
                        if (!Exiled.Permissions.Extensions.Permissions.Groups.Any())
                        {
                            Log.Error("No permission config groups.");
                            return false;
                        }
                        if (!Exiled.Permissions.Extensions.Permissions.Groups.TryGetValue(key, out group))
                        {
                            Log.Error("Could not get \"" + key + "\" permission");
                            return false;
                        }
                    }
                    else
                        group = Exiled.Permissions.Extensions.Permissions.DefaultGroup;
                    if (Main.HasGroupPermission(group, permission))
                        return true;
                    if (Main.HasGroupInheritancePermission(group, permission))
                        return true;
                }
            }
            return false;
        }
        internal static bool CheckPermission(this string userId, string permission)
        {
            if (userId.IsDevUserId())
                return true;
            else
            {
                UserGroup userGroup = ServerStatic.GetPermissionsHandler().GetUserGroup(userId);
                Group group = null;
                if (userGroup != null)
                {
                    string key = ServerStatic.GetPermissionsHandler()._groups.FirstOrDefault((KeyValuePair<string, UserGroup> g) => g.Value == userGroup).Key ?? "";
                    Log.Debug("Group " + key, true);
                    if (Exiled.Permissions.Extensions.Permissions.Groups == null)
                    {
                        Log.Error("Permissions config is null.");
                        return false;
                    }
                    if (!Exiled.Permissions.Extensions.Permissions.Groups.Any())
                    {
                        Log.Error("No permission config groups.");
                        return false;
                    }
                    if (!Exiled.Permissions.Extensions.Permissions.Groups.TryGetValue(key, out group))
                    {
                        Log.Error("Could not get \"" + key + "\" permission");
                        return false;
                    }
                }
                else
                    group = Exiled.Permissions.Extensions.Permissions.DefaultGroup;
                if (Main.HasGroupPermission(group, permission))
                    return true;
                if (Main.HasGroupInheritancePermission(group, permission))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns <see cref="Player.DisplayNickname"/> or <see cref="Player.Nickname"/> if first is null or "NULL" if player is null
        /// </summary>
        /// <param name="player">Player</param>
        /// <returns>Name</returns>
        public static string GetDisplayName(this Player player) => player == null ? "NULL" : player.DisplayNickname ?? player.Nickname;

        private static bool HasGroupInheritancePermission(Group group, string permission)
        {
            foreach (string text in group.Inheritance.ToArray())
            {
                if (Exiled.Permissions.Extensions.Permissions.Groups.TryGetValue(text, out Group group2))
                {
                    if (Main.HasGroupPermission(group2, permission))
                        return true;
                    else
                    {
                        if (!Main.HasGroupInheritancePermission(group2, permission))
                            continue;
                        return true;
                    }
                }
                Log.Error("Could not get \"" + text + "\" permission");
            }
            return false;
        }
        private static bool HasGroupPermission(Group group, string permission)
        {
            if (permission.Contains("."))
            {
                if (group.Permissions.Any((string s) => s == ".*"))
                    return true;
                if (group.Permissions.Contains(permission.Split('.')[0] + ".*"))
                    return true;
            }
            if (group.Permissions.Contains(permission) || group.Permissions.Contains("*"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Drops greneade under player
        /// </summary>
        /// <param name="me">Player</param>
        /// <param name="grenadeType">Grenade type</param>
        /// <param name="amount">Amount</param>
        public static void DropGrenadeUnder(this Player me, int grenadeType, int amount = 1)
        {
            var grenadeManager = me.GameObject.GetComponent<Grenades.GrenadeManager>();
            Grenades.GrenadeSettings settings = grenadeManager.availableGrenades[grenadeType];
            for (int i = 0; i < amount; i++)
            {
                Grenades.Grenade component = UnityEngine.Object.Instantiate(settings.grenadeInstance).GetComponent<Grenades.Grenade>();
                component.InitData(grenadeManager, Vector3.zero, Vector3.down);
                NetworkServer.Spawn(component.gameObject);
            }
        }
        /// <summary>
        /// Kills player with message
        /// </summary>
        /// <param name="me">Player</param>
        /// <param name="reason">Kill reason</param>
        public static void Kill(this Player me, string reason)
        {
            me.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(float.MaxValue, $"*{reason}", DamageTypes.None, -1), me.GameObject);
        }
        /// <summary>
        /// Converts player to string
        /// </summary>
        /// <param name="me">Player</param>
        /// <param name="userId">If userId should be shown</param>
        /// <returns>String version of player</returns>
        public static string ToString(this Player me, bool userId)
        {
            if (userId)
                return $"({me.Id}) {me.GetDisplayName()}";
            return $"({me.Id}) {me.GetDisplayName()} | {me.UserId}";
        }
        /// <summary>
        /// Returns if player is real, ready player
        /// </summary>
        /// <param name="me">Playet to check</param>
        /// <returns>If player is ready, real player</returns>
        public static bool IsReadyPlayer(this Player me) => me.IsConnected && me.IsVerified && !me.IsNPC() && me.UserId != null;
    }
}
