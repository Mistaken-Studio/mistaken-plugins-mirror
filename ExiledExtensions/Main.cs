using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Features;
using Mirror;
using UnityEngine;
using MEC;
using System.Security.Cryptography;
using Exiled.API.Extensions;
using NPCS;

namespace Gamer.Utilities
{
    public static class AnnonymousEvents
    {
        private static readonly Dictionary<string, List<Action<object>>> Subscribers = new Dictionary<string, List<Action<object>>>();

        public static void Call(string name, object arg)
        {
            Log.Debug("Running " + name);
            if(Subscribers.TryGetValue(name, out List<Action<object>> handlers))
            {
                foreach (var item in handlers)
                    item(arg);
            }
        }
        public static void Subscribe(string name, Action<object> handler)
        {
            Log.Debug("Subscribing to " + name);
            if (!Subscribers.ContainsKey(name))
                Subscribers[name] = new List<Action<object>>();
            Subscribers[name].Add(handler);
        }
        public static void UnSubscribe(string name, Action<object> handler)
        {
            Log.Debug("UnSubscribing to " + name);
            if (Subscribers.ContainsKey(name))
                Subscribers[name].Remove(handler);
        }
    }

    public static class Main
    {
        public static Vector3 GetByRoomOffset(this Room me, Vector3 offset)
        {
            var basePos = me.Position;
            offset = me.transform.forward * -offset.x + me.transform.right * -offset.z + Vector3.up * offset.y;
            basePos += offset;
            return basePos;
        }
        public static void Broadcast(this Player me, string tag, ushort duration, string message, Broadcast.BroadcastFlags flags = global::Broadcast.BroadcastFlags.Normal)
        {
            me.Broadcast(duration, $"<color=orange>[<color=green>{tag}</color>]</color> {message}", flags);
        }

        public static bool CheckPermissions(this Player me, PlayerPermissions perms)
        {
            return PermissionsHandler.IsPermitted(me.ReferenceHub.serverRoles.Permissions, perms);
        }
        public static bool IsDev(this Player me)
        {
            if (me == null)
                return false;
            return me.UserId.IsDevUserId();
        }

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
                //Lambda
                //case "76561198796704471":
                //case "450669501952819200":
                    return true;
                default:
                    return false;
            }
        }

        public static Player GetPlayer(this CommandSender me) => Player.Get(me.SenderId);
        public static Player GetPlayer(this ICommandSender me) => Player.Get(((CommandSender)me).SenderId);

        public static bool IsPlayer(this CommandSender me) => GetPlayer(me) != null;
        public static bool IsPlayer(this ICommandSender me) => GetPlayer(me) != null;

        public static bool IsDNT(this Player me)
        {
            switch (me.RawUserId.Split('@')[0])
            {
                //vorenus aka kgbp
                case "vorenus":
                    return false;
                default:
                    return me.DoNotTrack;
            }
        }

        public enum SessionVarType
        {
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
            CI_IMPACT
        }
        public static T GetSessionVar<T>(this Player me, SessionVarType type, T defaultValue = default) => me.GetSessionVar<T>(type.ToString(), defaultValue);
        public static T GetSessionVar<T>(this Player me, string name, T defaultValue = default)
        {
            if (me.SessionVariables.TryGetValue(name, out object value))
            {
                if (value is T)
                    return (T)value;
                else
                    throw new ArgumentException($"Expected to see SessionVariable of type {typeof(T).FullName} but got {value.GetType().FullName}");
            }
            else
                return defaultValue;
        }
        public static void SetSessionVar(this Player me, SessionVarType type, object value) => me.SetSessionVar(type.ToString(), value);
        public static void SetSessionVar(this Player me, string name, object value) => me.SessionVariables[name] = value;
        

        public static bool CheckPermission(this ICommandSender me, string permission) => CheckPermission(me as CommandSender, permission);      
        public static bool CheckPermission(this CommandSender cs, string permission) => CheckPermission(cs.GetPlayer(), permission);
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
        public static bool CheckPermission(this string userId, string permission)
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

        public static List<Player> GetPlayers(this IBetterCommand _, string arg, bool allowPets = false)
        {
            List<Player> tor = new List<Player>();
            foreach (var item in arg.Split('.'))
            {
                if (int.TryParse(item, out int pid))
                {
                    var p = allowPets ? Player.Get(pid) : RealPlayers.Get(pid);
                    if (p != null)
                        tor.Add(p);
                }
            }
            return tor;
        }

        public static bool ForeachPlayer(this IBetterCommand me, string arg, out Player[] players, Action<Player> toExecute, bool allowPets = false)
        {
            players = GetPlayers(me, arg, allowPets).ToArray();
            if (players.Length == 0) return false;
            foreach (var item in players)
            {
                toExecute?.Invoke(item);
            }
            return true;
        }

        public static bool ForeachPlayer(this IBetterCommand me, string arg, Action<Player> toExecute, bool allowPets = false)
        {
            return ForeachPlayer(me, arg, out Player[] _, toExecute, allowPets);
        }

        public static string[] ForeachPlayer(this IBetterCommand me, string arg, out Player[] players, out bool success, Func<Player, string[]> toExecute, bool allowPets = false)
        {
            List<string> tor = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
            players = GetPlayers(me, arg, allowPets).ToArray();
            if (players.Length == 0) success = false;
            else success = true;
            foreach (var item in players)
            {
                tor.AddRange(toExecute?.Invoke(item).Select(i => $"{item.Nickname} | {i}"));
            }
            var torArray = tor.ToArray();
            NorthwoodLib.Pools.ListPool<string>.Shared.Rent(tor);
            return torArray;
        }

        public static string[] ForeachPlayer(this IBetterCommand me, string arg, out bool success, Func<Player, string[]> toExecute, bool allowPets = false)
        {
            var tor = ForeachPlayer(me, arg, out Player[] _, out success, toExecute, allowPets);
            return tor;
        }

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

        public static void Kill(this Player me, string reason)
        {
            me.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(float.MaxValue, $"*{reason}", DamageTypes.None, -1), me.GameObject);
        }
        [System.Obsolete("Use PseudoGUI", true)]
        public static bool IsHintFree(this Player me) => !LockedHints.Contains(me.Id);
        private readonly static HashSet<int> LockedHints = new HashSet<int>();
        [System.Obsolete("Use PseudoGUI", true)]
        public static void ShowHintPulsating(this Player me, string message, float duraiton = 3f, bool lockHints = false, bool overrideLock = false)
        {
            if (LockedHints.Contains(me.Id) && !overrideLock)
                return;
            message = message.Replace("\n", "<br>");
            me.HintDisplay.Show(new Hints.TextHint(message, new Hints.HintParameter[]
            {
                new Hints.StringHintParameter(message)
            }, new Hints.HintEffect[]
            {
                Hints.HintEffectPresets.TrailingPulseAlpha(0.5f, 1f, 0.5f, 2f, 0f, 3)
            }, duraiton));
            if (lockHints)
            {
                LockedHints.Add(me.Id);
                MEC.Timing.CallDelayed(duraiton, () => {
                    LockedHints.Remove(me.Id);
                });
            }
        }
        [System.Obsolete("Use PseudoGUI")]
        public static void ShowHint(this Player me, string message, bool lockHints, float duraiton = 3f, bool overrideLock = false)
        {
            if (LockedHints.Contains(me.Id) && !overrideLock)
                return;
            message = message.Replace("\n", "<br>");
            me.ShowHint(message, duraiton);
            if (lockHints)
            {
                LockedHints.Add(me.Id);
                MEC.Timing.CallDelayed(duraiton, () => {
                    LockedHints.Remove(me.Id);
                });
            }
        }

        public static string ToString(this Player me, bool userId)
        {
            return me.ToString(userId, true);
        }
        public static string ToString(this Player me, bool userId, bool autoHideUserId = true)
        {
            if (autoHideUserId)
                return $"({me.Id}) {me.GetDisplayName()}";
            return $"({me.Id}) {me.GetDisplayName()}" + (userId ? $" | {me.UserId}" : "");
        }

        public static bool IsLCZDecontaminated(this LightContainmentZoneDecontamination.DecontaminationController _, float minTimeLeft = 0)
        {
            return MapPlus.IsLCZDecontaminated(minTimeLeft);
        }

        public static void Do106Teleport(this Player me, Vector3 target)
        {
            me.ReferenceHub.scp106PlayerScript.NetworkportalPosition = target;
            Timing.RunCoroutine(me.ReferenceHub.scp106PlayerScript._DoTeleportAnimation(), 0);

            Timing.CallDelayed(5f, () => {
                me.ReferenceHub.scp106PlayerScript.NetworkportalPosition = new Vector3(0, 6000, 0);
            });
        }

        public static bool IsReadyPlayer(this Player me) => me.IsConnected && me.IsVerified && !me.IsNPC() && me.UserId != null;
    }
}
