using Exiled.API.Features;
using System;
using System.Linq;

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
            Exiled.Events.Handlers.Server.WaitingForPlayers += Server_WaitingForPlayers;
            Exiled.Events.Handlers.Player.Verified += Player_Verified;
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (EventManager.EventActive())
                ev.Player.Broadcast(10, $"{EventManager.EMLB} Na serwerze obecnie trwa: <color=#6B9ADF>{EventManager.ActiveEvent.Name}</color>");
        }

        private void Server_WaitingForPlayers()
        {
            if (!EventManager.EventActive() && plugin.EventQueue.TryDequeue(out var eventClass))
            {
                Log.Debug(eventClass.Id);
                try
                {
                    eventClass.Initiate();
                }
                catch (Exception eeee)
                {
                    Log.Debug(eeee);
                }
            }
            else
            {
                Log.Debug(plugin.EventQueue.Count);
                Log.Debug(EventManager.EventActive().ToString());
                Log.Debug("zesrało się nitka");
            }
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
            {
                Map.ClearBroadcasts();
                Map.Broadcast(10, EventManager.EMLB + EventManager.T_Event_NUM_ALIVE.Replace("$players", players.Count.ToString()));
            }
        }

        private void Server_EndingRound(Exiled.Events.EventArgs.EndingRoundEventArgs ev)
        {
            if (EventManager.ForceEnd)
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
