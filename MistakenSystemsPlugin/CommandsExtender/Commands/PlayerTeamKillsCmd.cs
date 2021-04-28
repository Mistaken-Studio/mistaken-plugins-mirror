using CommandSystem;
using Gamer.Utilities;
using System.Collections.Generic;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class PlayerTeamKillsCmd : IBetterCommand, IPermissionLocked
    {
        public string Permission => "ptkd";

        public override string Description =>
         "Return Player TK Data";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "playerTKData";

        public override string[] Aliases => new string[] { "ptkd" };

        public string GetUsage()
        {
            return "PTKD [Id]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            var output = this.ForeachPlayer(args[0], out bool success, (player) =>
            {
                List<string> tor = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
                if (args.Length > 1 && args[1].ToLower() == "vic")
                {

                    var data = ATK.AntyTeamKillHandler.TeamAttacksList.FindAll(item => item.Victim.UserId == player.UserId);
                    var data1 = ATK.AntyTeamKillHandler.TeamKillsList.FindAll(item => item.Victim.UserId == player.UserId);
                    if (data.Count > 0 || data1.Count > 0) tor.Add("Victim");
                    if (data.Count > 0)
                    {
                        foreach (var item in data)
                        {
                            tor.Add("=============================================================");
                            tor.Add("TeamAttack");
                            tor.Add("Killer: " + item.Victim.Nickname + "(" + item.Victim.UserId + ")");
                            tor.Add("Damage: " + item.Info.Amount);
                            tor.Add("Tool:" + item.Info.GetDamageName());
                            tor.Add("RoundsAgo:" + (ATK.AntyTeamKillHandler.CurrentRoundId - item.RoundId));
                        }
                    }

                    if (data1.Count > 0)
                    {
                        foreach (var item in data1)
                        {
                            tor.Add("=============================================================");
                            tor.Add("TeamKill");
                            tor.Add("Killer: " + item.Victim.Nickname + "(" + item.Victim.UserId + ")");
                            tor.Add("Tool:" + item.Info.GetDamageName());
                            tor.Add("RoundsAgo:" + (ATK.AntyTeamKillHandler.CurrentRoundId - item.RoundId));
                        }
                    }
                }
                else
                {
                    var data = ATK.AntyTeamKillHandler.TeamAttacksList.FindAll(item => item.Killer.UserId == player.UserId);
                    var data1 = ATK.AntyTeamKillHandler.TeamKillsList.FindAll(item => item.Killer.UserId == player.UserId);
                    if (data.Count > 0 || data1.Count > 0) tor.Add("Attacker");
                    if (data.Count > 0)
                    {
                        foreach (var item in data)
                        {
                            tor.Add("=============================================================");
                            tor.Add("TeamAttack");
                            tor.Add("Victim: " + item.Victim.Nickname + "(" + item.Victim.UserId + ")");
                            tor.Add("Damage: " + item.Info.Amount);
                            tor.Add("Tool:" + item.Info.GetDamageName());
                            tor.Add("RoundsAgo:" + (ATK.AntyTeamKillHandler.CurrentRoundId - item.RoundId));
                        }
                    }

                    if (data1.Count > 0)
                    {
                        foreach (var item in data1)
                        {
                            tor.Add("=============================================================");
                            tor.Add("TeamKill");
                            tor.Add("Victim: " + item.Victim.Nickname + "(" + item.Victim.UserId + ")");
                            tor.Add("Tool:" + item.Info.GetDamageName());
                            tor.Add("RoundsAgo:" + (ATK.AntyTeamKillHandler.CurrentRoundId - item.RoundId));
                        }
                    }
                }
                var torArray = tor.ToArray();
                NorthwoodLib.Pools.ListPool<string>.Shared.Return(tor);
                return torArray;
            });
            if (!success) return new string[] { "Player not found", GetUsage() };
            _s = true;
            return output;
        }
    }
}
