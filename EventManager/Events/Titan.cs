using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.EventManager.Events
{
    internal class Titan :
        EventCreator.IEMEventClass,
        EventCreator.IInternalEvent
    {
        public override string Id => "titan";

        public override string Description { get; set; } = "Kill the powerfull titan";

        public override string Name { get; set; } = "Titan";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "T", "Jesteś <color=green>Tytanem</color>. Twoim zadaniem jest rozprawienie się z <color=blue>MFO</color> atakujących Ciebie." },
            { "M", "Waszym zadaniem (<color=blue>MFO</color>) jest zabicie <color=green>Tytana</color>, który znajduje się na waszym spawnie. Uważajcie!" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.Hurting += Player_Hurting;
            Exiled.Events.Handlers.Player.Died += Player_Died;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
            Exiled.Events.Handlers.Player.Hurting -= Player_Hurting;
            Exiled.Events.Handlers.Player.Died -= Player_Died;
        }

        public override void Register()
        {
        }

        private void Server_RoundStarted()
        {
            #region ini
            Mistaken.Systems.Utilities.API.Map.RespawnLock = true;
            Round.IsLocked = true;
            foreach (var e in Map.Lifts)
            {
                if (e.elevatorName.ToUpper().StartsWith("GATE"))
                    e.Network_locked = true;
            }
            #endregion
            var players = Gamer.Utilities.RealPlayers.List.ToList();
            var titan = players[UnityEngine.Random.Range(0, players.Count())];
            players.Remove(titan);
            titan.SlowChangeRole(RoleType.ChaosInsurgency);
            foreach (var player in players)
            {
                switch (UnityEngine.Random.Range(0, 3))
                {
                    case 0:
                        player.SlowChangeRole(RoleType.NtfCadet, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.NtfCommander));
                        break;
                    case 1:
                        player.SlowChangeRole(RoleType.NtfLieutenant, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.NtfCommander));
                        break;
                    case 2:
                        player.SlowChangeRole(RoleType.NtfCommander, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.NtfCommander));
                        break;
                }
                player.Broadcast(10, EventManager.EMLB + Translations["M"]);
            }
            WaitAndExecute(2, () =>
            {
                titan.Broadcast(8, EventManager.EMLB + Translations["T"]);
                titan.Health *= players.Count() + 1;
                titan.ArtificialHealth = (players.Count() + 1) * 30;
            });
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            var players = Gamer.Utilities.RealPlayers.List.Where(x => x != ev.Target);
            if (players.Count(x => x.Team == Team.MTF) == 0) OnEnd($"<color=green>Tytan {ev.Killer.Nickname}</color> wygrał!", true);
            else if (players.Count(x => x.Role == RoleType.ChaosInsurgency) == 0) OnEnd("<color=blue>MFO</color> wygrywa!", true);
            else if (ev.Target.Team == Team.MTF) players.First(x => x.Role == RoleType.ChaosInsurgency).ArtificialHealth += 8 * players.Count();
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.DamageType == DamageTypes.Logicer && ev.Amount > 30) ev.Amount = 150;
            else if (ev.DamageType == DamageTypes.Logicer) ev.Amount = 51;
        }
    }
}
