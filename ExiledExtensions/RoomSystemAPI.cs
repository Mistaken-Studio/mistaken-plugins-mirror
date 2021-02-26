using Exiled.API.Enums;
using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Utilities.RoomSystemAPI
{
    public static class RoomSystem
    {
        public static RoomType NearestRoom(this Player player)
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

        public static RoomType CheckNear(this Player player)
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

        public enum RoomType
        {
            UNKNOWN = 0,
            SURFACE = 1,
            EZ = 2,
            HCZ = 3,
            LCZ = 4,
            NUKE = 5,
            CCH049 = 6,
            POCKET = 7,
        }
    }

    public class ScanOutput
    {
        public int EZ = 0;
        public int HCZ = 0;
        public int LCZ = 0;
        public int SURFACE = 0;
        public int NUKE = 0;
        public int CCH049 = 0;
        public int POCKET = 0;

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
