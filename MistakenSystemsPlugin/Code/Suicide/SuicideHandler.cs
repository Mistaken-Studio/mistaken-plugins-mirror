using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using Gamer.Utilities;
using MEC;
using UnityEngine;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.Suicide
{
    public class Handler : Module
    {
        public static HashSet<int> InSuicidalState = new HashSet<int>();

        public Handler(PluginHandler p) : base(p)
        {
            plugin.RegisterTranslation("suicide_enter", "Shoot to kill yourself");
            plugin.RegisterTranslation("suicide_exit", "You can now shoot without killing yourself");
            plugin.RegisterTranslation("suicide_force", "Executed");

            plugin.RegisterTranslation("suicide_dead_msg_U", "Ther is hole in the head but it's imposible to define what killed him");
            plugin.RegisterTranslation("suicide_dead_msg_E11", "Ther is hole in the head looks like 5.56mm caliber");
            plugin.RegisterTranslation("suicide_dead_msg_P90", "Ther is hole in the head looks like 9mm caliber");
            plugin.RegisterTranslation("suicide_dead_msg_COM15", "Ther is hole in the head looks like 9mm caliber");
            plugin.RegisterTranslation("suicide_dead_msg_USP", "Ther is hole in the head looks like 9mm caliber");
            plugin.RegisterTranslation("suicide_dead_msg_MP7", "Ther is hole in the head looks like 7.62mm caliber");
            plugin.RegisterTranslation("suicide_dead_msg_LOGICER", "Ther is hole in the head looks like 7.62mm caliber");
        }

        public override string Name => "Suicide";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Shooting += this.Handle<Exiled.Events.EventArgs.ShootingEventArgs>((ev) => Player_Shooting(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Shooting -= this.Handle<Exiled.Events.EventArgs.ShootingEventArgs>((ev) => Player_Shooting(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (InSuicidalState.Contains(ev.Player.Id))
                InSuicidalState.Remove(ev.Player.Id);
        }

        private void Player_Shooting(Exiled.Events.EventArgs.ShootingEventArgs ev)
        {
            if (InSuicidalState.Contains(ev.Shooter.Id))
            {
                ev.IsAllowed = false;
                if (ev.Shooter.CurrentItem == null)
                {
                    KillPlayer(ev.Shooter, DeathType.UNKNOWN);
                    return;
                }
                switch (ev.Shooter.CurrentItem.id)
                {
                    case ItemType.GunCOM15:
                        KillPlayer(ev.Shooter, DeathType.COM15);
                        break;
                    case ItemType.GunUSP:
                        KillPlayer(ev.Shooter, DeathType.USP);
                        break;
                    case ItemType.GunProject90:
                        KillPlayer(ev.Shooter, DeathType.P90);
                        break;
                    case ItemType.GunMP7:
                        KillPlayer(ev.Shooter, DeathType.MP7);
                        break;
                    case ItemType.GunLogicer:
                        KillPlayer(ev.Shooter, DeathType.LOGICER);
                        break;
                    case ItemType.GunE11SR:
                        KillPlayer(ev.Shooter, DeathType.E11);
                        break;
                    default:
                        KillPlayer(ev.Shooter, DeathType.UNKNOWN);
                        break;
                }
                InSuicidalState.Remove(ev.Shooter.Id);
            }
        }

        private void Server_RestartingRound()
        {
            InSuicidalState.Clear();
        }

        internal static void KillPlayer(Player player, DeathType type = DeathType.UNKNOWN)
        {
            string reason = PluginHandler.Instance.ReadTranslation("suicide_dead_msg_U");
            switch (type)
            {
                case DeathType.UNKNOWN:
                    reason = PluginHandler.Instance.ReadTranslation("suicide_dead_msg_U");
                    break;
                case DeathType.COM15:
                    reason = PluginHandler.Instance.ReadTranslation("suicide_dead_msg_COM15");
                    break;
                case DeathType.USP:
                    reason = PluginHandler.Instance.ReadTranslation("suicide_dead_msg_USP");
                    break;
                case DeathType.P90:
                    reason = PluginHandler.Instance.ReadTranslation("suicide_dead_msg_P90");
                    break;
                case DeathType.MP7:
                    reason = PluginHandler.Instance.ReadTranslation("suicide_dead_msg_MP7");
                    break;
                case DeathType.LOGICER:
                    reason = PluginHandler.Instance.ReadTranslation("suicide_dead_msg_LOGICER");
                    break;
                case DeathType.E11:
                    reason = PluginHandler.Instance.ReadTranslation("suicide_dead_msg_E11");
                    break;
            }
            CustomAchievements.RoundEventHandler.AddProggress("DeadEnd", player);
            player.Kill(reason);
        }
    }

    public enum DeathType
    {
        UNKNOWN,
        COM15,
        USP,
        LOGICER,
        E11,
        P90,
        MP7
    }
}
