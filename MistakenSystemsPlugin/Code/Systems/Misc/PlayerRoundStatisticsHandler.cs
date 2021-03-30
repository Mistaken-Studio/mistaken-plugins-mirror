using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Utilities;
using Grenades;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using Exiled.API.Extensions;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.Systems.Misc
{
    public class PlayerRoundStatisticsHandler : Module
    {
        public PlayerRoundStatisticsHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "PlayerRoundStatistics";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundEnded += this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundEnded -= this.Handle<Exiled.Events.EventArgs.RoundEndedEventArgs>((ev) => Server_RoundEnded(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            foreach (var player in RealPlayers.List.Where(p => p.IsAlive).ToArray())
                DisplayStats(player);
        }

        private void Server_RestartingRound() =>
            LastStats.Clear();

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if(ev.Target.IsReadyPlayer())
                DisplayStats(ev.Target);
        }


        internal static readonly Dictionary<int, (uint Kills, uint TkKills, uint TkDeaths, uint Escapes, float DmgToSCP)> LastStats = new Dictionary<int, (uint Kills, uint TkKills, uint TkDeaths, uint Escapes, float DmgToSCP)>();
        private void DisplayStats(Player player)
        {
            if (Base.PlayerStatsHandler.Stats.ContainsKey(player.UserId))
            {
                var stats = Base.PlayerStatsHandler.Stats[player.UserId];
                var kills = stats.Kills;
                var tk_kills = stats.Tk_kills;
                var tk_deaths = stats.Tk_deaths;
                var escapes = stats.Escapes;
                var scpDmg = 0f;
                if (LastStats.TryGetValue(player.Id, out (uint Kills, uint TkKills, uint TkDeaths, uint Escapes, float DmgToSCP) oldStats))
                {
                    kills -= oldStats.Kills;
                    tk_kills -= oldStats.TkKills;
                    tk_deaths -= oldStats.TkDeaths;
                    escapes -= oldStats.Escapes;
                    scpDmg -= oldStats.DmgToSCP;
                }    
                string message = "";
                if (kills > 0)
                    message += $"Kills: <color=yellow>{kills}</color><br>";
                if (scpDmg > 0)
                    message += $"Dmg done to SCP: <color=yellow>{scpDmg}</color><br>";
                if (escapes > 0)
                    message += $"You have <color=yellow>Escaped</color><br>";
                else if (tk_deaths > 0)
                    message += $"You have been <color=yellow>team killed</color><br>";
                if (tk_kills > 0)
                    message += $"TeamKill Kills: <color=yellow>{tk_kills}</color><br>";
                if (LastStats.ContainsKey(player.Id))
                    LastStats.Remove(player.Id);
                LastStats.Add(player.Id, (kills, tk_kills, tk_deaths, escapes, scpDmg));
                GUI.SpecInfoHandler.AddDeathMessage(player, message);
            }
        }
    }
}
