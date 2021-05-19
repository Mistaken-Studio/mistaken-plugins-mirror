using Exiled.API.Features;
using Gamer.Utilities;
using MEC;
using System.Collections.Generic;

namespace Gamer.EventManager.Events
{
    internal class Achtung :
        EventCreator.IEMEventClass,
        EventCreator.IInternalEvent,
        EventCreator.IWinOnLastAlive,
        EventCreator.IAnnouncPlayersAlive
    {
        public override string Id => "achtung";

        public override string Description { get; set; } = "Achtung :)";

        public override string Name { get; set; } = "Achtung";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D", "Granaty pojawią się pod Tobą. Ostatni żywy wygrywa!" }
        };

        public bool ClearPrevious => true;

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
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Gamer.Mistaken.Base.Utilities.API.Map.RespawnLock = true;
            Round.IsLocked = true;
            #endregion
            foreach (var player in Gamer.Utilities.RealPlayers.List)
                player.SlowChangeRole(RoleType.ClassD, Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp106));
            SpawnGrenades(25);
            Map.Broadcast(10, $"{EventManager.EMLB} {Translations["D"]}");
        }

        private void SpawnGrenades(float time)
        {
            if (!Active)
                return;
            WaitAndExecute(time, () =>
            {
                time--;
                if (time < 2)
                    time = 2;
                SpawnGrenades(time);
                if (!Active)
                    return;
                foreach (var player in Gamer.Utilities.RealPlayers.List)
                {
                    player.DropGrenadeUnder(0, 1);
                    if (time < 7)
                    {
                        Timing.CallDelayed(1, () =>
                        {
                            player.DropGrenadeUnder(0, UnityEngine.Random.Range(0, 3));
                        });
                    }
                }
            });
        }
    }
}
