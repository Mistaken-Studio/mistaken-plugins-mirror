using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamer.Mistaken.CustomAchievements
{
    public static partial class CustomAchievements
    {
        public static Achievement GetAchievement(string Name)
        {
            foreach (Achievement item in Achievements.ToArray())
            {
                if (item.Name.ToLower() == Name.ToLower())
                    return item;
            }
            return null;
        }

        public static Achievement GetAchievement(uint Id)
        {
            foreach (Achievement item in Achievements.ToArray())
            {
                if (item.Id == Id)
                    return item;
            }
            return null;
        }

        public static readonly Achievement[] Achievements = new Achievement[]
        {
            new Achievement(0, "BumBum", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 15 },
                { Level.EXPERT, 20 },
            }),
            new Achievement(1, "Nope", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 5 },
                { Level.SILVER, 10 },
                { Level.GOLD, 25 },
                { Level.DIAMOND, 50 },
                { Level.EXPERT, 100 },
            }),
            new Achievement(2, "Use1499", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 25 },
                { Level.EXPERT, 50 },
            }),
            new Achievement(3, "IsItSafe", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 25 },
                { Level.EXPERT, 50 },
            }),
            new Achievement(4, "Really?", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 25 },
                { Level.EXPERT, 50 },
            }),
            new Achievement(5, "Welcome!", new Dictionary<Level, uint>
            {
                { Level.EXPERT, 1 },
            }),
            new Achievement(6, "Manipulator", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 15 },
                { Level.EXPERT, 20 },
            }),
            new Achievement(7, "SneakyFox", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 15 },
                { Level.EXPERT, 20 },
            }),
            new Achievement(8, "Informant", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 15 },
                { Level.EXPERT, 20 },
            }),
            new Achievement(9, "Plague", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 15 },
                { Level.EXPERT, 20 },
            }),
            new Achievement(10, "WeComeInPeace", new Dictionary<Level, uint>
            {
                    { Level.EXPERT, 1 },
            }),
            new Achievement(11, "ZombieTwice", new Dictionary<Level, uint>
            {
                    { Level.EXPERT, 1 },
            }),
            new Achievement(12, "DeadEnd", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 3 },
                { Level.GOLD, 5 },
                { Level.DIAMOND, 10 },
                { Level.EXPERT, 15 },
            }),
            new Achievement(13, "KaboomAndGo", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 2 },
                { Level.GOLD, 4 },
                { Level.DIAMOND, 7 },
                { Level.EXPERT, 10 },
            }),
            new Achievement(14, "Gravity", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 15 },
                { Level.EXPERT, 30 },
            }),
            new Achievement(15, "ScrewFundation", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 5 },
                { Level.SILVER, 10 },
                { Level.GOLD, 25 },
                { Level.DIAMOND, 50 },
                { Level.EXPERT, 100 },
            }),
            new Achievement(16, "ForGratherGood", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 3 },
                { Level.GOLD, 5 },
                { Level.DIAMOND, 10 },
                { Level.EXPERT, 15 },
            }),
            new Achievement(17, "DejaVu", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 3 },
                { Level.GOLD, 5 },
                { Level.DIAMOND, 10 },
                { Level.EXPERT, 15 },
            }),
            new Achievement(18, "Crazy?", new Dictionary<Level, uint>
            {
                    { Level.EXPERT, 1 },
            }),
            new Achievement(19, "N-wordPass", new Dictionary<Level, uint>
            {
                    { Level.EXPERT, 50 },
            }),
            new Achievement(20, "SCP-173-D", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 3 },
                { Level.GOLD, 5 },
                { Level.DIAMOND, 10 },
                { Level.EXPERT, 15 },
            }),
            new Achievement(21, "Senior_Resarcher", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 10 },
                { Level.SILVER, 25 },
                { Level.GOLD, 50 },
                { Level.DIAMOND, 100 },
                { Level.EXPERT, 150 },
            }),
            new Achievement(22, "Senced", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 25 },
                { Level.SILVER, 50 },
                { Level.GOLD, 100 },
                { Level.DIAMOND, 150 },
                { Level.EXPERT, 200 },
            }),
            new Achievement(23, "Honored", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 5 },
                { Level.SILVER, 10 },
                { Level.GOLD, 25 },
                { Level.DIAMOND, 50 },
                { Level.EXPERT, 80 },
            }),
            new Achievement(24, "Survivor", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 25 },
                { Level.EXPERT, 50 },
            }),
            new Achievement(25, "BigBoom", new Dictionary<Level, uint>
            {
                { Level.GOLD, 1 },
                { Level.DIAMOND, 2 },
                { Level.EXPERT, 3 },
            }),
            new Achievement(26, "GetCrazy", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 10 },
                { Level.SILVER, 20 },
                { Level.GOLD, 40 },
                { Level.DIAMOND, 70 },
                { Level.EXPERT, 100 },
            }),
            new Achievement(27, "SoSad", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 3 },
                { Level.GOLD, 5 },
                { Level.DIAMOND, 10 },
                { Level.EXPERT, 20 },
            }),
            new Achievement(28, "914Killer", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 2 },
                { Level.GOLD, 4 },
                { Level.DIAMOND, 7 },
                { Level.EXPERT, 10 },
            }),
            new Achievement(29, "GoToPocket", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 3 },
                { Level.SILVER, 7 },
                { Level.GOLD, 12 },
                { Level.DIAMOND, 20 },
                { Level.EXPERT, 40 },
            }),
            new Achievement(30, "GiveStain", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 2 },
                { Level.SILVER, 6 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 14 },
                { Level.EXPERT, 20 },
            }),
            new Achievement(31, "Daddy", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 10 },
                { Level.SILVER, 20 },
                { Level.GOLD, 40 },
                { Level.DIAMOND, 70 },
                { Level.EXPERT, 100 },
            }),
            new Achievement(32, "Toasted", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 25 },
                { Level.EXPERT, 50 },
            }),
            new Achievement(33, "SauronsPride_" + Level.BRONZE, new Dictionary<Level, uint>
            {
                { Level.BRONZE, 10 },
            }),
            new Achievement(34, "SauronsPride_" + Level.SILVER, new Dictionary<Level, uint>
            {
                { Level.SILVER, 10 },
            }),
            new Achievement(35, "SauronsPride_" + Level.GOLD, new Dictionary<Level, uint>
            {
                { Level.GOLD, 10 },
            }),
            new Achievement(36, "SauronsPride_" + Level.DIAMOND, new Dictionary<Level, uint>
            {
                { Level.DIAMOND, 10 },
            }),
            new Achievement(37, "SauronsPride_" + Level.EXPERT, new Dictionary<Level, uint>
            {
                { Level.EXPERT, 10 },
            }),
            new Achievement(38, "ZbeszekToxicRage", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 5 },
                { Level.SILVER, 10 },
                { Level.GOLD, 25 },
                { Level.DIAMOND, 35 },
                { Level.EXPERT, 50 },
            }),
            new Achievement(39, "Traitor", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 15 },
                { Level.DIAMOND, 25 },
                { Level.EXPERT, 40 },
            }),
            new Achievement(40, "RedRightHand", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 10 },
                { Level.SILVER, 30 },
                { Level.GOLD, 50 },
                { Level.DIAMOND, 80 },
                { Level.EXPERT, 100 },
            }),
            new Achievement(41, "IneedDoctor", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 25 },
                { Level.EXPERT, 50 },
            }),
            new Achievement(42, "PacifistEscape", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 20 },
                { Level.EXPERT, 50 },
            }),
            new Achievement(43, "DevilsMemorial", new Dictionary<Level, uint>
            {
                { Level.EXPERT, 1 }
            }),
            new Achievement(44, "VibeCheck", new Dictionary<Level, uint>
            {
                { Level.EXPERT, 1 }
            }),
            new Achievement(45, "GoWW_2", new Dictionary<Level, uint>
            {
                { Level.EXPERT, 1 }
            }),
            new Achievement(46, "GoWW", new Dictionary<Level, uint>
            {
                { Level.EXPERT, 1 }
            }),
            new Achievement(47, "Addicted", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 25 },
                { Level.EXPERT, 50 }
            }),
            new Achievement(48, "Pills", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 10 },
                { Level.SILVER, 25 },
                { Level.GOLD, 50 },
                { Level.DIAMOND, 100 },
                { Level.EXPERT, 200 }
            }),
            new Achievement(49, "Ping", new Dictionary<Level, uint>
            {
                { Level.EXPERT, 1 }
            }),
            new Achievement(50, "ImNotHere", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 15 },
                { Level.EXPERT, 20 }
            }),
            new Achievement(51, "likeMorfigo", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 2 },
                { Level.SILVER, 3 },
                { Level.GOLD, 4 },
                { Level.DIAMOND, 5 },
                { Level.EXPERT, 6 }
            }),
            new Achievement(52, "Cola", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 10 },
                { Level.SILVER, 20 },
                { Level.GOLD, 30 },
                { Level.DIAMOND, 45 },
                { Level.EXPERT, 60 }
            }),
            new Achievement(53, "Tutorial", new Dictionary<Level, uint>
            {
                { Level.EXPERT, 1 }
            }),
            new Achievement(54, "ClassDSafe", new Dictionary<Level, uint>
            {
                { Level.EXPERT, 1 }
            }),
            new Achievement(55, "LightsOut", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 1 },
                { Level.SILVER, 5 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 25 },
                { Level.EXPERT, 50 }
            }),
            new Achievement(56, "Darkness", new Dictionary<Level, uint>
            {
                { Level.EXPERT, 1 }
            }),
            new Achievement(57, "First", new Dictionary<Level, uint>
            {
                { Level.EXPERT, 1 }
            }),
            new Achievement(58, "Medkit", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 5 },
                { Level.SILVER, 10 },
                { Level.GOLD, 25 },
                { Level.DIAMOND, 50 },
                { Level.EXPERT, 100 }
            }),
            new Achievement(59, "NotPlanned", new Dictionary<Level, uint>
            {
                { Level.EXPERT, 1 }
            }),
            new Achievement(60, "AFK", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 5 },
                { Level.SILVER, 7 },
                { Level.GOLD, 10 },
                { Level.DIAMOND, 15 },
                { Level.EXPERT, 20 },
            }),
            new Achievement(61, "OldMan", new Dictionary<Level, uint>
            {
                { Level.BRONZE, 5 },
                { Level.SILVER, 10 },
                { Level.GOLD, 25 },
                { Level.DIAMOND, 50 },
                { Level.EXPERT, 100 },
            }),
        };
    }
}
