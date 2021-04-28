using Exiled.API.Enums;
using Exiled.API.Features;
using System.Linq;
using UnityEngine;

namespace Gamer.Utilities.RoomSystemAPI
{
    /// <summary>
    /// Room System
    /// </summary>
    public static class RoomSystem
    {
        internal static RoomType NearestRoom(this Player player)
        {
            try
            {
                if (player == null)
                    return RoomType.UNKNOWN;
                if (player.Role == RoleType.Scp079)
                    return RoomType.HCZ;
                Vector3 pos = player.Position;

                if (pos.y > 900)
                    return RoomType.SURFACE;
                else if (pos.y > 0)
                    return RoomType.LCZ;
                else if (pos.y < -1900)
                    return RoomType.POCKET;
                else if (pos.y < -900)
                    return CheckNear(player);
                else if (pos.y < -700)
                    return RoomType.CCH049;
                else if (pos.y < -500)
                    return RoomType.NUKE;
                else
                    return RoomType.UNKNOWN;
            }
            catch
            {
                return RoomType.UNKNOWN;
            }
        }
        private static RoomType CheckNear(this Player player)
        {
            if (player == null)
                return RoomType.UNKNOWN;
            switch (player.CurrentRoom?.Zone)
            {
                case ZoneType.Entrance:
                    return RoomType.EZ;
                case ZoneType.HeavyContainment:
                    return RoomType.HCZ;
                case ZoneType.LightContainment:
                    return RoomType.LCZ;
                case ZoneType.Surface:
                    return RoomType.SURFACE;
                case ZoneType.Unspecified:
                    return RoomType.UNKNOWN;
                default:
                    return RoomType.UNKNOWN;
            }
        }

        /// <summary>
        /// Zone types
        /// </summary>
        public enum RoomType
        {
#pragma warning disable CS1591 // Brak komentarza XML dla widocznego publicznie typu lub składowej
            UNKNOWN = 0,
            SURFACE = 1,
            EZ = 2,
            HCZ = 3,
            LCZ = 4,
            NUKE = 5,
            CCH049 = 6,
            POCKET = 7,
#pragma warning restore CS1591 // Brak komentarza XML dla widocznego publicznie typu lub składowej
        }
    }
    /// <summary>
    /// Scan output
    /// </summary>
    public class ScanOutput
    {
        /// <summary>
        /// Amount of players in EZ
        /// </summary>
        public int EZ = 0;
        /// <summary>
        /// Amount of players in HCZ
        /// </summary>
        public int HCZ = 0;
        /// <summary>
        /// Amount of players in LCZ
        /// </summary>
        public int LCZ = 0;
        /// <summary>
        /// Amount of players on Surface
        /// </summary>
        public int SURFACE = 0;
        /// <summary>
        /// Amount of players in Nuke
        /// </summary>
        public int NUKE = 0;
        /// <summary>
        /// Amount of players in SCP 049 Containment Chamber
        /// </summary>
        public int CCH049 = 0;
        /// <summary>
        /// Amount of players in Pocket Dimmension
        /// </summary>
        public int POCKET = 0;

        /// <summary>
        /// Scans
        /// </summary>
        /// <param name="team">Looked team</param>
        public ScanOutput(Team team = Team.RIP)
        {
            foreach (var player in team == Team.RIP ? RealPlayers.List.Where(p => p.Team != Team.RIP) : RealPlayers.List.Where(p => p.Team == team))
            {
                RoomSystem.RoomType type = RoomSystem.NearestRoom(player);
                switch (type)
                {
                    case RoomSystem.RoomType.EZ:
                        {
                            EZ++;
                            break;
                        }
                    case RoomSystem.RoomType.HCZ:
                        {
                            HCZ++;
                            break;
                        }
                    case RoomSystem.RoomType.LCZ:
                        {
                            LCZ++;
                            break;
                        }
                    case RoomSystem.RoomType.SURFACE:
                        {
                            SURFACE++;
                            break;
                        }
                    case RoomSystem.RoomType.NUKE:
                        {
                            NUKE++;
                            break;
                        }
                    case RoomSystem.RoomType.CCH049:
                        {
                            CCH049++;
                            break;
                        }
                    case RoomSystem.RoomType.POCKET:
                        {
                            POCKET++;
                            break;
                        }
                    case RoomSystem.RoomType.UNKNOWN:
                        {
                            Log.Debug("Failed to find player");
                            break;
                        }
                }
            }
        }
    }
}
