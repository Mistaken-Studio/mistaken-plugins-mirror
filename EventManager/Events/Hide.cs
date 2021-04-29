using Exiled.API.Enums;
using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.EventManager.Events
{
    internal class Hide :
        EventCreator.IEMEventClass,
        EventCreator.IInternalEvent
    {
        public override string Id => "hide";

        public override string Description { get; set; } = "Hide event";

        public override string Name { get; set; } = "Hide";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            //{ "", "" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
        }

        public override void Register()
        {
        }

        private void Server_RoundStarted()
        {
            #region ini
            Mistaken.Systems.Utilities.API.Map.RespawnLock = true;
            Round.IsLocked = true;
            #endregion
            foreach (var player in Gamer.Utilities.RealPlayers.List)
            {
                if (player.Side == Side.Scp)
                    player.SlowChangeRole(RoleType.Scp93953, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp173));
                else
                    player.SlowChangeRole(RoleType.ClassD);
            }
            Winner();
        }

        private void Winner()
        {
            if (!Active) return;
            var players = Gamer.Utilities.RealPlayers.List;
            List<Player> winners = new List<Player>();
            foreach (var player in players)
            {
                if (player.CurrentRoom.Zone == ZoneType.HeavyContainment) winners.Add(player);
            }
            if (winners.Count() > 1)
                OnEnd($"Class D Won ({winners.Count()} Class D Escaped)");
            else if (winners.Count() != 0)
                OnEnd($"Class D Won ({winners[0].Nickname} Escaped)");
            else if (!Gamer.Utilities.RealPlayers.Any(Team.CDP))
                OnEnd("Scp Won");
            else WaitAndExecute(5, Winner);
        }
    }
}
