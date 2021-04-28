using CommandSystem;
using Gamer.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    class LongOverwatchCommand : IBetterCommand
    {
        public override string Description =>
            "Long Overwatch";

        public override string Command => "lovr";

        public override string[] Execute(ICommandSender sender, string[] args, out bool _s)
        {
            _s = true;
            var senderPlayer = sender.GetPlayer();
            if(!sender.CheckPermission(PlayerPermissions.Overwatch))
            {
                _s = false;
                return new string[] { "Access Denied" };
            }
            bool value = Systems.End.OverwatchHandler.InLongOverwatch.Contains(senderPlayer.UserId);
            if(value)
            {
                Systems.End.OverwatchHandler.InLongOverwatch.Remove(senderPlayer.UserId);
                senderPlayer.IsOverwatchEnabled = false;
                senderPlayer.SetSessionVar(Main.SessionVarType.LONG_OVERWATCH, false);
                Base.GUI.PseudoGUIHandler.Set(senderPlayer, "long_overwatch", Base.GUI.PseudoGUIHandler.Position.TOP, null);
                AnnonymousEvents.Call("LONG_OVERWATCH", (senderPlayer, false));
                return new string[] { "Disabled" };
            }
            else
            {
                Systems.End.OverwatchHandler.InOverwatch.Remove(senderPlayer.UserId);
                Systems.End.OverwatchHandler.InLongOverwatch.Add(senderPlayer.UserId);
                senderPlayer.IsOverwatchEnabled = true;
                senderPlayer.SetSessionVar(Main.SessionVarType.LONG_OVERWATCH, true);
                Base.GUI.PseudoGUIHandler.Set(senderPlayer, "long_overwatch", Base.GUI.PseudoGUIHandler.Position.TOP, "Active: <color=red>Long Overwatch</color>");
                AnnonymousEvents.Call("LONG_OVERWATCH", (senderPlayer, true));
                return new string[] { "Enabled" };
            }
        }
    }
}
