using Exiled.API.Features;
using Gamer.Mistaken.Base.Staff;
using Gamer.Utilities;
using System.Collections.Generic;

namespace Gamer.Mistaken.LOFH
{
    public static class LOFH
    {
        internal static Dictionary<Player, string> LastSelectedPlayer = new Dictionary<Player, string>();

        internal static Dictionary<string, Flag> Flags { get; } = new Dictionary<string, Flag>();
        internal static Dictionary<string, Prefix> Prefixes { get; } = new Dictionary<string, Prefix>();

        public static Dictionary<string, string> Country { get; } = new Dictionary<string, string>();

        public static Dictionary<string, Systems.ThreatLevel.ThreadLevelData> ThreatLevelDatas { get; } = new Dictionary<string, Systems.ThreatLevel.ThreadLevelData>();

        public static HashSet<string> Hidden { get; } = new HashSet<string>();
        public static Dictionary<string, MistakenSocket.Shared.Blacklist.BlacklistEntry> WarningFlags { get; } = new Dictionary<string, MistakenSocket.Shared.Blacklist.BlacklistEntry>();
        private static Dictionary<string, int> InVanish { get; } = new Dictionary<string, int>();
        public static void AddVanish(string userId, int level)
        {
            if (InVanish.ContainsKey(userId))
                RemoveVanish(userId);
            InVanish.Add(userId, level);
        }
        public static void RemoveVanish(string userId) => InVanish.Remove(userId);
        public static void ClearVanish() => InVanish.Clear();

        public static bool HasStaffFlag(string userId) => userId.IsStaff();

        public static string[] GetFlags(string userId)
        {
            List<string> flags = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
            if (InVanish.TryGetValue(userId, out int vanishLevel))
                flags.Add($"Active flag: <color=#808080>Vanish: <{vanishLevel}></color>");
            if (Flags.TryGetValue(userId, out Flag flag))
                flags.Add((flag.AddSM ? "Active flag: " : "") + flag.Value);

            flags.Add($"<color=#FF0000>Bans</color>: {Systems.Bans.BansManager.GetBans(userId).Length}");
            var tor = flags.ToArray();
            NorthwoodLib.Pools.ListPool<string>.Shared.Return(flags);
            return tor;
        }

        public static string GetBans(string userId) =>
            $"<color=#FF0000>|</color><color=white>{Systems.Bans.BansManager.GetBans(userId).Length}</color><color=#FF0000>|</color>";

        public static string GetPrefixes(string userId)
        {
            string tor = "";
            if (MenuSystem.Reported.Contains(userId))
                tor = $"[<color=#FFFF00>R</color>] ";
            if (InVanish.TryGetValue(userId, out int vanishLevel))
                tor = $"[<color=#808080>V <{vanishLevel}></color>] ";
            if (userId.IsDevUserId())
            {
                if (userId.IsStaff())
                    return $"{tor}[<color=#FF0000>|</color><b><color=#FFD700><b>DEV</b></color></b><color=#FF0000>|</color>] ";
                else
                    return $"{tor}[<color=#FFFF00>|</color><b><color=#FF0000><b>R-DEV</b></color></b><color=#FFFF00>|</color>] ";
            }
            if (userId.IsStaff())
                return $"{tor}[<color=blue>M</color> <color=white>STAFF</color>] ";
            return tor;
        }

        public static string GetConstructedPrefix(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return "";
            string tor = $"{GetPrefixes(userId)}{GetMutesPrefix(userId)}{GetBans(userId)}";

            if (!HasStaffFlag(userId))
                return $"{tor}{GetThreatPrefix(userId)}";
            else
                return tor;
        }

        public static string GetThreatPrefix(string userId)
        {
            if (!ThreatLevelDatas.TryGetValue(userId, out Systems.ThreatLevel.ThreadLevelData threatData) || threatData.Level == 0)
                return "";
            var color = (threatData.Level == 4 ? "black" : (threatData.Level == 3 ? "red" : (threatData.Level == 2 ? "orange" : (threatData.Level == 1 ? "yellow" : "white"))));
            return $"<color=blue><<color={color}>{threatData.Level}</color>></color>";
        }

        public static string GetMutesPrefix(string userId)
        {
            if (!Player.UserIdsCache.TryGetValue(userId, out Player player))
                return "";
            if (player.IsMuted)
                return $"[<color=red>MUTED</color>]";
            if (player.IsIntercomMuted)
                return $"[<color=red><color=#FFC0CB>I</color>MUTED</color>]";
            return "";
        }

        public class Prefix
        {
            public bool AddSE = true;
            public string Value;
            public string userId;
        }
        public class Flag
        {
            public bool AddSM = true;
            public string Value;
            public string userId;
            public string Nickname;
        }
    }
}
