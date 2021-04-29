using Exiled.API.Features;
using Gamer.EventManager.EventCreator;
using MEC;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.EventManager.Events
{
    internal class HotPeanut :
        IEMEventClass,
        IInternalEvent
    {
        public override string Id => "hp";

        public override string Description { get; set; } = "Fight between SCP 173 and Class D";

        public override string Name { get; set; } = "Hot Peanut";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            //{ "", "" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died += Player_Died;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
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
            #endregion
            var isscp = false;
            var plist = Utilities.RealPlayers.List.ToList();
            plist.Shuffle();
            foreach (var player in plist)
            {
                if (!isscp)
                {
                    player.SetRole(RoleType.Scp173);
                    Timing.CallDelayed(10f, () =>
                    {
                        player.Position = Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp106);
                        player.ReferenceHub.playerStats.SetHPAmount(10);
                    });
                    isscp = true;
                }
                else
                    player.SlowChangeRole(RoleType.ClassD, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp106));
            }
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (Utilities.RealPlayers.List.Where(x => x.Role == RoleType.ClassD && x.Id != ev.Target.Id).ToArray().Length > 1)
            {
                Timing.CallDelayed(0.1f, () =>
                {
                    ev.Target.SetRole(RoleType.Scp173, true);
                    ev.Target.ReferenceHub.playerStats.SetHPAmount(10);
                });
            }
            else
                EndOnOneAliveOf();
        }
    }
}
