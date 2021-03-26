using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using NPCS;

namespace Gamer.Utilities
{
    public static class RealPlayers
    {
        public static IEnumerable<Player> List
        {
            get
            {
                lock (Player.List)
                {
                    return Player.List.Where(p => p.IsReadyPlayer());
                }
            }
        }
        public static IEnumerable<Player> Get(Team value) => List.Where(p => p.Team == value);
        public static IEnumerable<Player> Get(RoleType value) => List.Where(p => p.Role == value);
        public static Player Get(int value) => List.FirstOrDefault(p => p.Id == value);
        public static Player Get(string value) => List.FirstOrDefault(p => p.UserId == value);

        public static bool Any(Team value) => List.Any(p => p.Team == value);
        public static bool Any(RoleType value) => List.Any(p => p.Role == value);
    }
}