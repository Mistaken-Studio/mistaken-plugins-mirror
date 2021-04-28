using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.EventManager.Events
{
    internal class Morbus :
        EventCreator.IEMEventClass,
        EventCreator.InternalEvent,
        EventCreator.ISpawnRandomItems
    {
        public override string Id => "morbus";

        public override string Description { get; set; } = "Morbus event";

        public override string Name { get; set; } = "Morbus";

        public override EventCreator.Version Version => new EventCreator.Version(4, 0, 0);

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "Mother", "" },
            { "D", "" }
        };

        private Player Mother;
        private List<int> MorbusesFirst = new List<int>();
        private List<int> MorbusesSecond = new List<int>();

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died += Player_Died;
            Exiled.Events.Handlers.Map.GeneratorActivated += Map_GeneratorActivated;
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet += Player_InsertingGeneratorTablet;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died -= Player_Died;
            Exiled.Events.Handlers.Map.GeneratorActivated -= Map_GeneratorActivated;
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet -= Player_InsertingGeneratorTablet;
        }

        public override void Register()
        {
        }

        private void Server_RoundStarted()
        {
            #region ini
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Mistaken.Systems.Utilities.API.Map.RespawnLock = true;
            Round.IsLocked = true;
            #endregion
            Map.TurnOffAllLights(float.MaxValue);
            var players = Gamer.Utilities.RealPlayers.List.ToList();
            Mother = players[UnityEngine.Random.Range(0, players.Count)];
            Mother.SlowChangeRole(RoleType.ClassD);
            Mother.AddItem(ItemType.Coin);
            players.Remove(Mother);
            Mother.Broadcast(10, EventManager.EMLB + Translations["Mother"]);
            foreach (var item in players)
            {
                item.Role = RoleType.ClassD;
                item.Broadcast(10, EventManager.EMLB + Translations["D"]);
            }
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (ev.Target.Id == Mother.Id)
                OnEndMorbus();
        }

        private void Map_GeneratorActivated(Exiled.Events.EventArgs.GeneratorActivatedEventArgs ev)
        {
            if (Map.ActivatedGenerators > 4)
                OnEndMorbus();
        }

        private void Player_InsertingGeneratorTablet(Exiled.Events.EventArgs.InsertingGeneratorTabletEventArgs ev)
        {
            ev.Generator.NetworkremainingPowerup = 180;
        }

        private void OnEndMorbus()
        {
            WaitAndExecute(60, () =>
            {
                foreach (var player in Gamer.Utilities.RealPlayers.List)
                {
                    if (MorbusesFirst.Contains(player.Id)) player.Kill(DamageTypes.Decont);
                    if (MorbusesSecond.Contains(player.Id)) player.Kill(DamageTypes.Decont);
                    if (player.Id == Mother.Id) player.Kill(DamageTypes.Decont);
                }
            });
        }
    }
}
