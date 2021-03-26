using System.Collections.Generic;

namespace Gamer.Mistaken.EVO
{
    public partial class Handler
    {
        public static List<Achievement> Achievements { get; } = new List<Achievement>()
        {
            new Achievement()
            {
                Id = 1000,
                Name = "NukeActivator",
                Description = "Activate Nuke X times",
                Levels = new (int Progress, string Name, string Color)[] {
                    (10, "Destructor Enthusiast", null),
                    (25, "Flame and Rubble Spreader", null),
                    (50, "Master of Disaster", null)
                }
            },
            new Achievement()
            {
                Id = 1001,
                Name = "SCP049",
                Description = "As 049 Kill X Players",
                Levels = new (int Progress, string Name, string Color)[] {
                    (20, "Patient 0", null),
                    (50, "Zombie Amplifier", null),
                    (100, "Plague Doctor", null)
                }
            },
            new Achievement()
            {
                Id = 1002,
                Name = "Escape",
                Description = "Escape X times",
                Levels = new (int Progress, string Name, string Color)[] {
                    (15, "Escapist", null),
                    (40, "Freedom Runner", null),
                    (70, "Ultimate Survivor", null)
                }
            },
            new Achievement()
            {
                Id = 1003,
                Name = "KillSCP",
                Description = "Kill X SCPSubjects",
                Levels = new (int Progress, string Name, string Color)[] {
                    (10, "Rookie", null),
                    (25, "Monster Hunter", null),
                    (50, "Apex Predator", null)
                }
            },
            new Achievement()
            {
                Id = 1004,
                Name = "914",
                Description = "Use 914 X times",
                Levels = new (int Progress, string Name, string Color)[] {
                    (40, "Upgrader", null),
                    (80, "Mad Scientist", null),
                    (120, "Scientific Genius", null)
                }
            },
            new Achievement()
            {
                Id = 1005,
                Name = "Commander",
                Description = "Play as MTF Commander X times",
                Levels = new (int Progress, string Name, string Color)[] {
                    (10, "Overseer", null),
                    (20, "Ascending Officer", null),
                    (40, "Commanding Officer", null)
                }
            },
            new Achievement()
            {
                Id = 1006,
                Name = "Intercom",
                Description = "Use Intercom X times",
                Levels = new (int Progress, string Name, string Color)[] {
                    (7, "Announcer", null),
                    (20, "Attention Seeker", null),
                    (35, "Celebrity", null)
                }
            },
            new Achievement()
            {
                Id = 1007,
                Name = "SCP106",
                Description = "Contain SCP 106 X times",
                Levels = new (int Progress, string Name, string Color)[] {
                    (7, "Femur Breaker", null),
                    (15, "Containmen Engineer", null),
                    (25, "O5 Council", null)
                }
            },
            new Achievement()
            {
                Id = 1008,
                Name = "Chaos",
                Description = "Spawn as Chaos Insurgency X times",
                Levels = new (int Progress, string Name, string Color)[] {
                    (30, "Chaos Soldier", null),
                    (50, "Opposing Force", null),
                    (70, "Insurgent", null)
                }
            },
            new Achievement()
            {
                Id = 1009,
                Name = "Generator",
                Description = "Activate X generators",
                Levels = new (int Progress, string Name, string Color)[] {
                    (15, "Electrician", null),
                    (30, "Facility Engineer", null),
                    (50, "Overwatch Engineer", null)
                }
            },
            new Achievement()
            {
                Id = 1010,
                Name = "Reviver",
                Description = "Revive X players",
                Levels = new (int Progress, string Name, string Color)[] {
                    (10, "Dark Arts Adept", null),
                    (20, "Apprentice of Darkness", null),
                    (40, "Necromancy Overlord", null)
                }
            },
            new Achievement()
            {
                Id = 1011,
                Name = "SCP",
                Description = "Be an SCP (excluding 049-2) X times",
                Levels = new (int Progress, string Name, string Color)[] {
                    (15, "Anomaly", null),
                    (35, "Murder Monster", null),
                    (60, "Reality Bender", null)
                }
            },
            new Achievement()
            {
                Id = 1012,
                Name = "ClassD",
                Description = "Play as Class D X times",
                Levels = new (int Progress, string Name, string Color)[] {
                    (50, "Prisoner", null),
                    (100, "Serial Killer", null),
                    (200, "Terrorist", null)
                }
            },
            new Achievement()
            {
                Id = 1013,
                Name = "SCP",
                Description = "Play as a Scientist X times",
                Levels = new (int Progress, string Name, string Color)[] {
                    (30, "Junior Researcher", null),
                    (70, "Containment Specialist", null),
                    (130, "Site Director", null)
                }
            },
            new Achievement()
            {
                Id = 1014,
                Name = "SCP",
                Description = "Die from LCZ decontamination X times",
                Levels = new (int Progress, string Name, string Color)[] {
                    (3, "Poisoned", null),
                    (10, "Melted", null),
                    (25, "Radioactive", null)
                }
            },
            new Achievement()
            {
                Id = 2000,
                Name = "Easter",
                Description = "Collect X eggs",
                Levels = new (int Progress, string Name, string Color)[] {
                    (10, "Anomaly", ""),
                    (15, "Murder Monster", ""),
                    (20, "Reality Bender", "")
                }
            }
        };
    }

    public enum Colors
    {
        BLUE_GREEN = 1,
        CYAN = 2,
        AQUA = 3,
    }

    public class Achievement
    {
        public uint Id;
        public string Name;
        public string Description;
        public (int Progress, string Name, string Color)[] Levels;
    }
}
