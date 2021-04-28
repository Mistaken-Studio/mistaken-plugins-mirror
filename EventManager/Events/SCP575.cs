using Exiled.API.Features;
using MEC;
using System.Collections.Generic;

namespace Gamer.EventManager.Events
{
    internal class SCP575 :
        EventCreator.IEMEventClass,
        EventCreator.InternalEvent
    {
        public override string Id => "575";

        public override string Description { get; set; } = "SCP-575 breach";

        public override string Name { get; set; } = "SCP-575";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            //{ "", "" }
        };

        public static bool Attack = false;

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += Server_RoundEnded;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= Server_RoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= Server_RoundEnded;
        }

        public override void Register()
        {

        }

        private void Server_RoundStarted()
        {
            Scp575AttackScript();
            Timing.CallDelayed(UnityEngine.Random.Range(30, 90), () =>
            {
                Scp575LigtsScript();
            });
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            EventManager.ActiveEvent = null;
            OnEnd(ev.LeadingTeam.ToString());
        }

        private void Scp575LigtsScript()
        {
            if (!Active)
                return;
            int time = UnityEngine.Random.Range(180, 300);
            Map.TurnOffAllLights(time, false);
            Attack = true;
            Timing.CallDelayed(time, () =>
            {
                Attack = false;
                Timing.CallDelayed(60, () =>
                {
                    Scp575LigtsScript();
                });
            });
        }

        private void Scp575AttackScript()
        {
            if (!Active)
                return;
            if (Attack)
            {
                foreach (Player player in Gamer.Utilities.RealPlayers.List)
                {
                    if (player.CurrentItem.id != ItemType.Flashlight || player.CurrentItem.id == ItemType.GunE11SR && player.CurrentItem.modOther != 4 || player.CurrentItem.id == ItemType.GunCOM15 && player.CurrentItem.modOther != 1 || player.CurrentItem.id == ItemType.GunProject90 && player.CurrentItem.modOther != 1 || player.CurrentItem.id == ItemType.GunUSP && player.CurrentItem.modOther != 1)
                        player.Health -= UnityEngine.Random.Range(10, 30);
                }
            }
            Timing.CallDelayed(UnityEngine.Random.Range(10, 20), () =>
            {
                Scp575AttackScript();
            });
        }
    }
}
