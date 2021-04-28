using Exiled.API.Enums;
using Exiled.API.Features;
using System;
using System.Collections.Generic;

namespace Gamer.Mistaken.Systems.Logs
{
    public static class LogManager
    {
        public static Dictionary<ElevatorType, List<ElevatorLog>> ElevatorLogs { get; } = new Dictionary<ElevatorType, List<ElevatorLog>>();
        public static Dictionary<Interactables.Interobjects.DoorUtils.DoorVariant, List<DoorLog>> DoorLogs { get; } = new Dictionary<Interactables.Interobjects.DoorUtils.DoorVariant, List<DoorLog>>();
        public static Dictionary<int, List<PlayerInfo>> PlayerLogs { get; } = new Dictionary<int, List<PlayerInfo>>();
        public static Dictionary<int, List<SCP914Log>> SCP914Logs { get; } = new Dictionary<int, List<SCP914Log>>();
        public static Dictionary<int, DateTime> RoundStartTime { get; } = new Dictionary<int, DateTime>();
        public static Dictionary<int, Player> FlashLog { get; } = new Dictionary<int, Player>();
    }

    public struct ElevatorLog
    {
        public Player Player;
        public DateTime Time;
        public Lift.Status Status;

        public ElevatorLog(Exiled.Events.EventArgs.InteractingElevatorEventArgs ev)
        {
            this.Player = ev.Player;
            this.Time = DateTime.Now;
            this.Status = ev.Status;
        }
    }
    public struct DoorLog
    {
        public Player Player;
        public DateTime Time;
        public bool Open;

        public DoorLog(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            this.Player = ev.Player;
            this.Time = DateTime.Now;
            this.Open = !ev.Door.NetworkTargetState;
        }
    }
    public struct PlayerInfo
    {
        public string Name;
        public string UserId;
        public string IP;
        public int ID;
        public bool IMute;
        public bool Mute;

        public PlayerInfo(Player p)
        {
            this.ID = p.Id;
            this.UserId = p.UserId;
            this.IP = p.IPAddress;
            this.IMute = p.IsIntercomMuted;
            this.Mute = p.IsMuted;
            this.Name = p.Nickname;
        }
    }

    public struct SCP914Log
    {
        public string Name;
        public string UserId;
        public int ID;
        public SCP914Action Action;
        public DateTime Time;

        public SCP914Log(Player p, SCP914Action action)
        {
            this.ID = p.Id;
            this.UserId = p.UserId;
            this.Name = p.Nickname;
            this.Action = action;
            this.Time = DateTime.Now;
        }
    }
    public enum SCP914Action
    {
        CHANGE_ROUGH,
        CHANGE_COARSE,
        CHANGE_1TO1,
        CHANGE_FINE,
        CHANGE_VERY_FINE,
        ACTIVATE,
        HURT_ROUGH,
        HURT_COARSE,
        HURT_OUTPUT,
        RECIVE_SCP207_1,
        RECIVE_SCP207_2,
        RECIVE_SCP207_3,
        RECIVE_SCP207_4,
    }
}
