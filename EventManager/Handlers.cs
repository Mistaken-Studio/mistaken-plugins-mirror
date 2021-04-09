using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamer.EventManager
{
    class SystemsHandler
    {
        private readonly EventManager plugin;

        public SystemsHandler(EventManager p)
        {
            plugin = p;
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.Escaping += Player_Escaping;
            Exiled.Events.Handlers.Server.EndingRound += Server_EndingRound;
            Exiled.Events.Handlers.Player.Died += Player_Died;
            Exiled.Events.Handlers.Server.RestartingRound += Server_RestartingRound;
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (!EventManager.EventActive())
                return;
            var players = Gamer.Utilities.RealPlayers.List.Where(p => p.IsAlive && p.Id != ev.Target.Id).ToList();
            if (players.Count == 1 && EventManager.ActiveEvent is EventCreator.IWinOnLastAlive)
                EventManager.ActiveEvent.OnEnd(players[0].Nickname);
            else if (players.Count == 0 && EventManager.ActiveEvent is EventCreator.IEndOnNoAlive)
                EventManager.ActiveEvent.OnEnd();
            if (EventManager.ActiveEvent is EventCreator.IAnnouncPlayersAlive && players.Count > 1)
                Map.Broadcast(10, EventManager.EMLB + EventManager.T_Event_NUM_ALIVE.Replace("$players", players.Count.ToString()));
        }

        private void Server_EndingRound(Exiled.Events.EventArgs.EndingRoundEventArgs ev)
        {
            if(EventManager.ForceEnd)
            {
                ev.IsAllowed = true;
                ev.IsRoundEnded = true;
                EventManager.ForceEnd = false;
            }
        }

        private void Player_Escaping(Exiled.Events.EventArgs.EscapingEventArgs ev)
        {
            if (!EventManager.EventActive())
                return;
            if (EventManager.ActiveEvent is EventCreator.IWinOnEscape)
                EventManager.ActiveEvent.OnEnd(ev.Player.Nickname);
        }

        private void Server_RoundStarted()
        {
            EventManager.rounds_without_event++;
            EventManager.ForceEnd = false;
        }

        private void Server_RestartingRound()
        {
            if (EventManager.ActiveEvent?.Active ?? false) 
                EventManager.ActiveEvent.DeInitiate();
        }
    }
}
