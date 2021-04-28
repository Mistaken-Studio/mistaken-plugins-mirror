using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using System;
using System.Collections.Generic;

namespace Gamer.Mistaken.BetterRP
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class ForceAmbient : IBetterCommand, IPermissionLocked
    {
        public string Permission => "force_ambient";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "ambient";

        public override string[] Aliases => new string[] { };

        public override string Description => "Plays Random Ambient or one with supplied Id";

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            string msg = "ERROR";
            bool jammed = true;
            if (args.Length == 0)
            {
                msg = Handler.GetAmbient(out jammed);
            }
            else if (int.TryParse(args[0], out int ambientId))
            {
                msg = Handler.GetAmbient(out jammed, ambientId);
            }

            if (msg != null)
            {
                if (jammed)
                    NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(msg, 0.1f, 0.07f);
                else
                    Cassie.Message(msg, false, false);
            }

            success = true;

            return new string[] { "Done" };
        }

        public string GetUsage() =>
            "ambient (Id)";

    }

    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class ForceRPEvents : IBetterCommand, IPermissionLocked
    {
        public string Permission => "rp_events_force";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "rpe";

        public override string[] Aliases => new string[] { };

        public override string Description => "Adds rp event";

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            if (!RoundModifiersManager.Instance.SetActiveEvents(int.Parse(args[0])))
            {
                success = false;
                return new string[] { "Failed to generate random events" };
            }
            else
            {
                List<string> tor = new List<string>();
                success = true;
                for (int i = 0; i < RoundModifiersManager.RandomEventsLength; i++)
                {
                    RoundModifiersManager.RandomEvents re = (RoundModifiersManager.RandomEvents)Math.Pow(2, i);
                    if (RoundModifiersManager.Instance.ActiveEvents.HasFlag(re))
                    {
                        tor.Add(re.ToString());
                    }
                }

                return new string[] { "Activated:", string.Join("\n", tor) };
            }
        }

        public string GetUsage() =>
            "rpevents (Id)";

    }

    /*[CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    class IdCommand : IBetterCommand
    {
        public override string Command => "id";

        public override string[] Aliases => new string[] { "name", "nickname" };

        public override string Description => "Displays player's nickname";

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = true;
            return new string[] { $"Your nickname: {sender.GetPlayer().DisplayNickname}" };
        }
    }*/
}
