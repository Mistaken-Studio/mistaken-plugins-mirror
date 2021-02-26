using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;

namespace Gamer.Utilities
{
    public static class RealPlayers
    {
        public static IEnumerable<Player> List => Player.List.Where(p => !NPCS.Npc.List.Any(npc => npc.NPCPlayer.Id == p.Id) && p.IsConnected && p.IsVerified);
        public static IEnumerable<Player> Get(Team value) => List.Where(p => p.Team == value && p.IsConnected && p.IsVerified);
        public static IEnumerable<Player> Get(RoleType value) => List.Where(p => p.Role == value && p.IsConnected && p.IsVerified);
        public static Player Get(int value) => List.FirstOrDefault(p => p.Id == value && p.IsConnected && p.IsVerified);
        public static Player Get(string value) => (Player.UserIdsCache.ContainsKey(value) ? Player.UserIdsCache[value] : null);

        public static bool Any(Team value) => List.Any(p => p.Team == value && p?.GameObject != null);
        public static bool Any(RoleType value) => List.Any(p => p.Role == value && p?.GameObject != null);
    }
}