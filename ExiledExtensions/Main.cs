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

namespace Gamer.Utilities
{
    public static class Main
    {
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
                //Lamda
                case "76561198796704471":
                case "450669501952819200":
                    return true;
                default:
                    return false;
            }
        }

        public static Player GetPlayer(this CommandSender me) => Player.Get(me.SenderId);
        public static Player GetPlayer(this ICommandSender me) => Player.Get(((CommandSender)me).SenderId);

        public static bool IsPlayer(this CommandSender me) => GetPlayer(me) != null;
        public static bool IsPlayer(this ICommandSender me) => GetPlayer(me) != null;
        
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

        public static List<Player> GetPlayers(this IBetterCommand _, string arg)
        {
            List<Player> tor = new List<Player>();
            foreach (var item in arg.Split('.'))
            {
                if (int.TryParse(item, out int pid))
                {
                    var p = Player.Get(pid);
                    if (p != null)
                        tor.Add(p);
                }
            }
            return tor;
        }

        public static bool ForeachPlayer(this IBetterCommand me, string arg, out Player[] players, Action<Player> toExecute)
        {
            players = GetPlayers(me, arg).ToArray();
            if (players.Length == 0) return false;
            foreach (var item in players)
            {
                toExecute?.Invoke(item);
            }
            return true;
        }

        public static bool ForeachPlayer(this IBetterCommand me, string arg, Action<Player> toExecute)
        {
            return ForeachPlayer(me, arg, out Player[] _, toExecute);
        }

        public static string[] ForeachPlayer(this IBetterCommand me, string arg, out Player[] players, out bool success, Func<Player, string[]> toExecute)
        {
            List<string> tor = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
            players = GetPlayers(me, arg).ToArray();
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

        public static string[] ForeachPlayer(this IBetterCommand me, string arg, out bool success, Func<Player, string[]> toExecute)
        {
            var tor = ForeachPlayer(me, arg, out Player[] _, out success, toExecute);
            return tor;
        }

        public static void PrivateCassie(this Player me, string message, bool isHold = false, bool isNoisy = true)
        {
            if (me?.Connection == null || me == Server.Host)
                return;
            foreach (var controller in Respawning.RespawnEffectsController.AllControllers)
            {
                if(controller.connectionToClient == me.Connection)
                    controller?.CallTargetRpc(nameof(controller.RpcCassieAnnouncement), me.Connection, message, isHold, isNoisy);
            }
        }

        public static void CallTargetRpc<T>(this T me, string name, NetworkConnection conn, params object[] args) where T : NetworkBehaviour
        {
            if(!Player.Dictionary.ContainsKey(conn?.identity?.gameObject))
            {
                Log.Error("Tried to call CallTargetRpc on not real player !");
                return;
            }
            NetworkWriter writer = NetworkWriterPool.GetWriter();
            foreach (var arg in args)
            {
                if (arg is string)
                    writer.WriteString((string)arg);
                else if (arg is bool)
                    writer.WriteBoolean((bool)arg);
                else if (arg is short)
                    writer.WriteInt16((short)arg);
                else if (arg is int)
                    writer.WriteInt32((int)arg);
                else if (arg is long)
                    writer.WriteInt64((long)arg);
                else if (arg is float)
                    writer.WriteSingle((float)arg);
                else if (arg is double)
                    writer.WriteDouble((double)arg);
                else
                    throw new System.ArgumentException("Args is unsupported type | " + arg.GetType().FullName, nameof(args));
            }
            System.Reflection.MethodInfo dynMethod = me.GetType().GetMethod("SendTargetRPCInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dynMethod.Invoke(me, new object[] { conn, typeof(T), name, writer, 0 });
            NetworkWriterPool.Recycle(writer);
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

        public static bool IsHintFree(this Player me) => !LockedHints.Contains(me.Id);
        private readonly static HashSet<int> LockedHints = new HashSet<int>();
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

        private static SHA1 SHA = SHA1.Create();
        public static string ToString(this Player me, bool userId)
        {
            return me.ToString(userId, true);
        }
        public static string ToString(this Player me, bool userId, bool autoHideUserId = true)
        {
            if (autoHideUserId)
                return $"({me.Id}) {me.Nickname}";
            return $"({me.Id}) {me.Nickname}" + (userId ? $" | {me.UserId}" : "");
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
    }
}
