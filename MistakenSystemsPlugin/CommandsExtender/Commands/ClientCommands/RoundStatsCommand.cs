using CommandSystem;
using Gamer.Utilities;
using System.Collections.Generic;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    class RoundStatsCommand : IBetterCommand
    {
        public override string Description => "Display Round Stats";
        public override string Command => "roundstats";
        public override string[] Aliases => new string[] { "rstart", "stats" };
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (PStats.PlayerStatsHandler.Stats.ContainsKey(player.UserId))
            {
                var stats = PStats.PlayerStatsHandler.Stats[player.UserId];
                var kills = stats.Kills;
                var tk_kills = stats.Tk_kills;
                var tk_deaths = stats.Tk_deaths;
                var escapes = stats.Escapes;
                var scpDmg = 0f;
                if (Systems.Misc.PlayerRoundStatisticsHandler.LastStats.TryGetValue(player.Id, out (uint Kills, uint TkKills, uint TkDeaths, uint Escapes, float DmgToSCP) oldStats))
                {
                    kills -= oldStats.Kills;
                    tk_kills -= oldStats.TkKills;
                    tk_deaths -= oldStats.TkDeaths;
                    escapes -= oldStats.Escapes;
                    scpDmg -= oldStats.DmgToSCP;
                }
                List<string> message = new List<string>(); ;
                if (kills > 0)
                    message.Add($"Kills: <color=yellow>{kills}</color>");
                if (scpDmg > 0)
                    message.Add($"Dmg done to SCP: <color=yellow>{scpDmg}</color>");
                if (escapes > 0)
                    message.Add($"You have <color=yellow>Escaped</color>");
                else if (tk_deaths > 0)
                    message.Add($"You have been <color=yellow>team killed</color>");
                if (tk_kills > 0)
                    message.Add($"TeamKill Kills: <color=yellow>{tk_kills}</color>");

                success = true;
                return message.ToArray();
            }
            return new string[] { "Not found" };
        }
    }
}
