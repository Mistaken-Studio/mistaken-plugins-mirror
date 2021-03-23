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
                Levels = new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(10, "Destructor Enthusiast"),
                    new KeyValuePair<int, string>(25, "Flame and Rubble Spreader"),
                    new KeyValuePair<int, string>(50, "Master of Disaster")
                }
            },
            new Achievement()
            {
                Id = 1001,
                Name = "SCP049",
                Description = "As 049 Kill X Players",
                Levels = new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(20, "Patient 0"),
                    new KeyValuePair<int, string>(50, "Zombie Amplifier"),
                    new KeyValuePair<int, string>(100, "Plague Doctor")
                }
            },
            new Achievement()
            {
                Id = 1002,
                Name = "Escape",
                Description = "Escape X times",
                Levels = new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(15, "Escapist"),
                    new KeyValuePair<int, string>(40, "Freedom Runner"),
                    new KeyValuePair<int, string>(70, "Ultimate Survivor")
                }
            },
            new Achievement()
            {
                Id = 1003,
                Name = "KillSCP",
                Description = "Kill X SCPSubjects",
                Levels = new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(10, "Rookie"),
                    new KeyValuePair<int, string>(25, "Monster Hunter"),
                    new KeyValuePair<int, string>(50, "Apex Predator")
                }
            },
            new Achievement()
            {
                Id = 1004,
                Name = "914",
                Description = "Use 914 X times",
                Levels = new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(40, "Upgrader"),
                    new KeyValuePair<int, string>(80, "Mad Scientist"),
                    new KeyValuePair<int, string>(120, "Scientific Genius")
                }
            },
            new Achievement()
            {
                Id = 1005,
                Name = "Commander",
                Description = "Play as MTF Commander X times",
                Levels = new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(10, "Overseer"),
                    new KeyValuePair<int, string>(20, "Ascending Officer"),
                    new KeyValuePair<int, string>(40, "Commanding Officer")
                }
            },
            new Achievement()
            {
                Id = 1006,
                Name = "Intercom",
                Description = "Use Intercom X times",
                Levels = new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(7, "Announcer"),
                    new KeyValuePair<int, string>(20, "Attention Seeker"),
                    new KeyValuePair<int, string>(35, "Celebrity")
                }
            },
            new Achievement()
            {
                Id = 1007,
                Name = "SCP106",
                Description = "Contain SCP 106 X times",
                Levels = new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(7, "Femur Breaker"),
                    new KeyValuePair<int, string>(15, "Containmen Engineer"),
                    new KeyValuePair<int, string>(25, "O5 Council")
                }
            },
            new Achievement()
            {
                Id = 1008,
                Name = "Chaos",
                Description = "Spawn as Chaos Insurgency X times",
                Levels = new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(30, "Chaos Soldier"),
                    new KeyValuePair<int, string>(50, "Opposing Force"),
                    new KeyValuePair<int, string>(70, "Insurgent")
                }
            },
            new Achievement()
            {
                Id = 1009,
                Name = "Generator",
                Description = "Activate X generators",
                Levels = new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(15, "Electrician"),
                    new KeyValuePair<int, string>(30, "Facility Engineer"),
                    new KeyValuePair<int, string>(50, "Overwatch Engineer")
                }
            },
            new Achievement()
            {
                Id = 1010,
                Name = "Reviver",
                Description = "Revive X players",
                Levels = new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(10, "Dark Arts Adept"),
                    new KeyValuePair<int, string>(20, "Apprentice of Darkness"),
                    new KeyValuePair<int, string>(40, "Necromancy Overlord")
                }
            },
            new Achievement()
            {
                Id = 1011,
                Name = "SCP",
                Description = "Be an SCP (excluding 049-2) X times",
                Levels = new KeyValuePair<int, string>[] {
                    new KeyValuePair<int, string>(15, "Anomaly"),
                    new KeyValuePair<int, string>(35, "Murder Monster"),
                    new KeyValuePair<int, string>(60, "Reality Bender")
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
        public KeyValuePair<int, string>[] Levels;
    }
}
