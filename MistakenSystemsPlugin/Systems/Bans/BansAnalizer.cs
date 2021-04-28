using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamer.Mistaken.Systems.Bans
{
    public static class BansAnalizer
    {
        public static readonly Dictionary<BanCategory, string[]> CategoryDefiners = new Dictionary<BanCategory, string[]>()
        {
            { 
                BanCategory.TK, 
                new string[] 
                {
                    "TK",
                    "TeamKill",
                    "Team Kill",
                    "TKill",
                } 
            },
            {
                BanCategory.TEAMING,
                new string[]
                {
                    "Team",
                    "Sojusz",
                }
            },
            {
                BanCategory.FAILRP,
                new string[]
                {
                    "FailRP",
                    "Fail RP",
                }
            },
            {
                BanCategory.MICSPAM,
                new string[]
                {
                    "MicSpam",
                    "Wydawanie",
                    "Muzyka",
                }
            }
        };
        public enum BanCategory
        {
            OTHER,
            TK,
            TEAMING,
            FAILRP,
            MICSPAM,
        }
        public enum BanType
        {
            AUTO,
            NORMAL
        }
        public enum DurationCategory
        {
            KICK = 0,
            MINUTE = 1,
            HOUR = 60,
            DAY = 1440,
            WEEK = 10080,
            MONTH = 43200,
            YEAR = 518400,
            PERM = 51840000
        }

        public static BanType GetBanType((string AdminId, string Reason, int Duration, DateTime Time) ban)
        {
            if (ban.Reason.StartsWith("TK:") || ban.Reason.StartsWith("AutoBan:"))
                return BanType.AUTO;
            return BanType.NORMAL;
        }

        public static BanCategory GetBanCategory((string AdminId, string Reason, int Duration, DateTime Time) ban)
        {
            return GetBanCategory(ban.Reason);
        }

        public static BanCategory GetBanCategory(string reason)
        {
            reason = reason.ToLower();
            foreach (var category in CategoryDefiners)
            {
                foreach (var item in category.Value)
                {
                    if (reason.Contains(item.ToLower()))
                        return category.Key;
                }
            }
            return BanCategory.OTHER;
        }

        public static int GetMaxDur((string AdminId, string Reason, int Duration, DateTime Time)[] bans) => bans.Max(i => i.Duration / 60);
        public static DurationCategory GetDurationCategory(int duration)
        {
            switch(duration)
            {
                case 0:
                    return DurationCategory.KICK;
                case int i when i < 60: //HOUR
                    return DurationCategory.MINUTE;
                case int i when i < 1440: //DAY
                    return DurationCategory.HOUR;
                case int i when i < 10080: //WEEK
                    return DurationCategory.DAY;
                case int i when i < 43200: //MONTH
                    return DurationCategory.WEEK;
                case int i when i < 518400: //YEAR
                    return DurationCategory.MONTH;
                case int i when i < 5184000: //PERM (10y)
                    return DurationCategory.YEAR;
                case int i when i >= 5184000: //PERM (10y)
                    return DurationCategory.PERM;
                default:
                    return DurationCategory.KICK;
            }
        }

        public static int GuessBanDuration((string AdminId, string Reason, int Duration, DateTime Time)[] bans, BanCategory banCategory)
        {
            DateTime latestBan = DateTime.MinValue;
            Dictionary<BanCategory, List<(string AdminId, string Reason, int Duration, DateTime Time)>> CategorizedBans = new Dictionary<BanCategory, List<(string AdminId, string Reason, int Duration, DateTime Time)>>();
            foreach (var ban in bans)
            {
                var thisBanCategory = GetBanCategory(ban);
                if (!CategorizedBans.ContainsKey(thisBanCategory))
                    CategorizedBans.Add(thisBanCategory, new List<(string AdminId, string Reason, int Duration, DateTime Time)>());
                CategorizedBans[thisBanCategory].Add(ban);
                if (latestBan < ban.Time)
                    latestBan = ban.Time;
            }
            if (!CategorizedBans.ContainsKey(banCategory))
            {
                switch(banCategory)
                {
                    case BanCategory.TEAMING:
                        return 30;
                    case BanCategory.TK:
                        return 60;
                    case BanCategory.MICSPAM:
                        return 15;
                    case BanCategory.FAILRP:
                        return 30;
                    default:
                        return 0;
                }
            }
            else
            {
                var selectedBans = CategorizedBans[banCategory].ToArray();
                int maxDur = GetMaxDur(selectedBans);
                DurationCategory durationCategory = GetDurationCategory(maxDur);
                Exiled.API.Features.Log.Debug($"[BAN ANALIZER] {maxDur} => {durationCategory}");
                if (durationCategory == DurationCategory.PERM)
                    return (int)DurationCategory.PERM;
                else
                    return Enum.GetValues(typeof(DurationCategory)).ToArray<int>().First(i => i > (int)durationCategory);
            }
        }
    }
}
