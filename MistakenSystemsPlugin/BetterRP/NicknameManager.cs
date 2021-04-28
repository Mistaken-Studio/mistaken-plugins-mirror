using Exiled.API.Features;
using System.Collections.Generic;

namespace Gamer.Mistaken.BetterRP
{
    public static class NicknameManager
    {
        public static readonly string[] NatoAlphabet = new string[]
        {
            "Alfa",
            "Bravo",
            "Charlie",
            "Delta",
            "Echo",
            "Foxtrot",
            "Golf",
            "Hotel",
            "India",
            "Juliett",
            "Kilo",
            "Lima",
            "Mike",
            "November",
            "Oscar",
            "Papa",
            "Quebec",
            "Romeo",
            "Sierra",
            "Tango",
            "Uniform",
            "Victor",
            "Whiskey",
            "X-ray",
            "Yankee",
            "Zulu",
        };

        public static readonly string[] Names = new string[] {
            "Aaron",
            "Adam",
            "Alex",
            "Bo",
            "Benjamin",
            "Jack",
            "Frank",
            "Gary",
            "Adrian",
            "Robert",
            "Michael",
            "Ashley",
            "Charles",
            "Ed",
            "Martin",
            "Luther",
            "Tyler",
            "Timothy",
            "Max",
            "Mike",
            "Maurice",
            "Jerry",
            "Tom",
            "Rick",
            "Morty",
            "Nick",
            "Josh",
            "Chris",
            "Oliver",
            "Dustin",
            "Oswald",
            "Peter",
            "Jamie",
            "Grant",
            "Red",
            "Hank",
            "Connor",
            "Jack",
        };

        public static readonly string[] LastNames = new string[] {
            "Adams",
            "Anderson",
            "Bailey",
            "Brown",
            "Clarke",
            "Fisher",
            "Fletcher",
            "Fox",
            "Green",
            "Black",
            "Hill",
            "Howard",
            "Kelly",
            "Lee",
            "Lewis",
            "Mason",
            "Miller",
            "Moore",
            "Owen",
            "Pearce",
            "Power",
            "Reynolds",
            "Roberts",
            "Ross",
            "Spencer",
            "Turner",
            "Walker",
            "White",
            "Wood",
            "Wright",
            "Bright",
            "Young",
            "Jamie",
            "Harvey",
            "Gray",
            "Eliott",
            "Able",
        };

        private static readonly List<string> UsedNames = new List<string>();

        public static void ClearUsedNames() => UsedNames.Clear();

        private static string _generateNickname(RoleType role, string unit = "", string subTeam = "")
        {
            switch (role)
            {
                case RoleType.ClassD:
                    return UnityEngine.Random.Range(1000, 9999).ToString();
                case RoleType.NtfScientist:
                case RoleType.FacilityGuard:
                case RoleType.Scientist:
                    return Names[UnityEngine.Random.Range(0, Names.Length)] + " " + LastNames[UnityEngine.Random.Range(0, LastNames.Length)];
                case RoleType.ChaosInsurgency:
                    return $"{unit}-{subTeam}";
                case RoleType.NtfCadet:
                case RoleType.NtfLieutenant:
                case RoleType.NtfCommander:
                    return $"{unit}-{subTeam}";
                default:
                    return "";
            }
        }

        public static string GenerateNickname(RoleType role, string unit = "", string subTeam = "")
        {
            string result = _generateNickname(role, unit, subTeam);
            if (role == RoleType.ClassD || role == RoleType.Scientist || role == RoleType.FacilityGuard || role == RoleType.NtfScientist)
            {
                int i = 0;
                while (UsedNames.Contains(result))
                {
                    if (i > 1000)
                    {
                        Log.Debug(result);
                        result = "FAILED TO GENERATE NICKNAME";
                        break;
                    }
                    result = _generateNickname(role, unit, subTeam);
                    i++;
                }

                UsedNames.Add(result);
            }

            switch (role)
            {
                case RoleType.ClassD:
                    return $"D-{result}";
                case RoleType.ChaosInsurgency:
                    return $"Chaos-{result}";
                case RoleType.FacilityGuard:
                case RoleType.Scientist:
                case RoleType.NtfScientist:
                    return $"{result}";
                case RoleType.NtfCadet:
                case RoleType.NtfLieutenant:
                case RoleType.NtfCommander:
                    return $"{result}";
                case RoleType.Tutorial:
                case RoleType.Spectator:
                default:
                    return "default";
                    //default:
                    //    return $"{role.ToString().Replace("_", "-").ToUpper()}";
            }
        }
    }
}
