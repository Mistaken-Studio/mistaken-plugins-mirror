using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.Systems.End
{
    internal class LeaveZombieHandler : Module
    {
        public LeaveZombieHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "LeaveZombie";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.CustomEvents.OnFirstTimeJoined += this.Handle<Exiled.Events.EventArgs.FirstTimeJoinedEventArgs>((ev) => CustomEvents_OnFirstTimeJoined(ev));
            Exiled.Events.Handlers.Player.Left += this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Dying += this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.CustomEvents.OnFirstTimeJoined -= this.Handle<Exiled.Events.EventArgs.FirstTimeJoinedEventArgs>((ev) => CustomEvents_OnFirstTimeJoined(ev));
            Exiled.Events.Handlers.Player.Left -= this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Dying -= this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (ev.Target.Role == RoleType.Scp0492)
            {
                if (ev.HitInformation.GetDamageType() == DamageTypes.Wall)
                    MapPlus.Broadcast("SCP049-2", 10, $"({ev.Target.Id}) {ev.Target.Nickname} was killed by \"WALL\" | Room: {ev.Target.CurrentRoom?.Type.ToString() ?? "Unknown"} | Pos: {ev.Target.Position}", Broadcast.BroadcastFlags.AdminChat);
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player == null) return;
            if (ev.NewRole == RoleType.Scp0492 && ZombiesThatLeft.Any(item => item.UserId == ev.Player.UserId))
            {
                var info = ZombiesThatLeft.Find(item => item.UserId == ev.Player.UserId);
                ev.ShouldPreservePosition = true;
                ev.Player.Position = info.Position;
                ev.Player.Health = info.HP;
                ZombiesThatLeft.Remove(info);
            }
        }

        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            if (ev.Player.Role == RoleType.Scp0492 && Round.IsStarted)
                ZombiesThatLeft.Add(new ZombieInfo(ev.Player));
        }

        private void CustomEvents_OnFirstTimeJoined(Exiled.Events.EventArgs.FirstTimeJoinedEventArgs ev)
        {
            if (ev.Player == null)
            {
                Log.Warn("Joined player is null");
                return;
            }
            this.CallDelayed(5, () =>
            {
                if (ZombiesThatLeft.Any(item => item.UserId == ev.Player.UserId))
                {
                    var info = ZombiesThatLeft.Find(item => item.UserId == ev.Player.UserId);
                    ZombiesThatLeft.Remove(info);
                    ev.Player.Role = RoleType.Scp0492;
                    this.CallDelayed(1, () =>
                    {
                        ev.Player.Health = info.HP;
                        ev.Player.Position = info.Position;
                    }, "ZombiesThatLeftLate");
                }
            }, "ZombiesThatLeft");
        }

        private void Server_RestartingRound()
        {
            ZombiesThatLeft.Clear();
        }


        private readonly List<ZombieInfo> ZombiesThatLeft = new List<ZombieInfo>();

        private class ZombieInfo
        {
            public string UserId;
            public float HP;
            public Vector3 Position;

            public ZombieInfo(Player p)
            {
                UserId = p.UserId;
                HP = p.Health;
                Position = p.Position;
            }
        }
    }
}
