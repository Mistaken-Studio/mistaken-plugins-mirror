using CommandSystem;
using Gamer.Utilities;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class SpawnSamsaraCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "samsara";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "spawn_samsara";

        public override string[] Aliases => new string[] { "samsara" };

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            Systems.End.NoEndlessRoundHandler.Spawn(new System.Collections.Generic.List<Exiled.API.Features.Player>
            {
                sender.GetPlayer()
            });
            _s = true;
            return new string[] { "Done" };
        }
    }
}
