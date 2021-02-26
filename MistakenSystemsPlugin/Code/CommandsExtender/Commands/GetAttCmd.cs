


using CommandSystem;
using Gamer.Utilities;
using System.Collections.Generic;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
        [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] class GetAttCmd : IBetterCommand, IPermissionLocked
    {
        public string Permission => "ga";

        public override string Description =>
        "Get Attacker";
        public string PluginName => PluginHandler.PluginName;

        public override string Command => "getattacker";

        public override string[] Aliases => new string[] { "ga" };

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            var output = this.ForeachPlayer(args[0], out bool success, (player) => {
                List<string> tor = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
                try
                {
                    var data = Mistaken.Base.AntyTeamKillHandler.TeamAttacksList.FindAll(item => item.Victim.UserId == player.UserId);
                    var data1 = Mistaken.Base.AntyTeamKillHandler.TeamKillsList.FindAll(item => item.Victim.UserId == player.UserId);
                    if (data.Count > 0 || data1.Count > 0) tor.Add("Victim");
                    if (data.Count > 0)
                    {
                        var item = data[data.Count - 1];
                        tor.Add("=============================================================");
                        tor.Add("TeamAttack");
                        tor.Add("Killer: " + item.Killer.Nickname + "(" + item.Killer.UserId + ")");
                        tor.Add("Damage: " + item.Info.Amount);
                        tor.Add("Tool:" + item.Info.GetDamageName());
                        tor.Add("RoundsAgo:" + (Mistaken.Base.AntyTeamKillHandler.CurrentRoundId - item.RoundId));
                    }

                    if (data1.Count > 0)
                    {
                        var item = data1[data1.Count - 1];
                        tor.Add("=============================================================");
                        tor.Add("TeamKill");
                        tor.Add("Killer: " + item.Killer.Nickname + "(" + item.Killer.UserId + ")");
                        tor.Add("Tool:" + item.Info.GetDamageName());
                        tor.Add("RoundsAgo:" + (Mistaken.Base.AntyTeamKillHandler.CurrentRoundId - item.RoundId));
                    }
                }
                catch (System.Exception e)
                {
                    tor.Add(e.Message);
                    tor.Add(e.StackTrace);
                }

                var torArray = tor.ToArray();
                NorthwoodLib.Pools.ListPool<string>.Shared.Return(tor);
                return torArray;
            });
            if (!success) return new string[] { "Player not found", GetUsage() };
            _s = true;
            return output;
        }

        public string GetUsage()
        {
            return "GA (Id)";
        }
    }
}
