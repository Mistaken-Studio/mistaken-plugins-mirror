using CommandSystem;
using Exiled.API.Features;
using Gamer.Mistaken.Systems.Misc;
using Gamer.Utilities;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class WarheadCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "basic";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "warhead";

        public override string[] Aliases => new string[] { };

        public override string Description => "Controll Alpha Warhead";

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0)
                return new string[] { GetUsage() };
            var admin = sender.GetPlayer();

            switch (args[0])
            {
                case "start":
                    {
                        if (!admin.CheckPermission($"{PluginName}.lock")) return new string[] { "No Permissions." };
                        Warhead.Start();
                        BetterWarheadHandler.Warhead.CountingDown = true;
                        success = true;
                        return new string[] { "Alpha Warhead engaged" };
                    }
                case "stop":
                    {
                        Warhead.Stop();
                        BetterWarheadHandler.Warhead.CountingDown = false;
                        success = true;
                        return new string[] { "Alpha Warhead cancled" };
                    }
                case "on":
                    {
                        if (!admin.CheckPermission($"{PluginName}.lock")) return new string[] { "No Permissions." };
                        Warhead.LeverStatus = true;
                        BetterWarheadHandler.Warhead.Enabled = true;
                        success = true;
                        return new string[] { "Alpha Warhead turned on" };
                    }
                case "off":
                    {
                        if (!admin.CheckPermission($"{PluginName}.lock")) return new string[] { "No Permissions." };
                        Warhead.LeverStatus = false;
                        BetterWarheadHandler.Warhead.Enabled = false;
                        success = true;
                        return new string[] { "Alpha Warhead turned off" };
                    }
                case "open":
                    {
                        if (!admin.CheckPermission($"{PluginName}.lock")) return new string[] { "No Permissions." };
                        Warhead.IsKeycardActivated = true;
                        BetterWarheadHandler.Warhead.ButtonOpen = true;
                        success = true;
                        return new string[] { "Alpha Warhead button opened" };
                    }
                case "close":
                    {
                        if (!admin.CheckPermission($"{PluginName}.lock")) return new string[] { "No Permissions." };
                        Warhead.IsKeycardActivated = false;
                        BetterWarheadHandler.Warhead.ButtonOpen = false;
                        success = true;
                        return new string[] { "Alpha Warhead button closed" };
                    }
                case "lockstart":
                    {
                        if (!admin.CheckPermission($"{PluginName}.lock")) return new string[] { "No Permissions." };
                        if (args.Length == 1) return new string[] { "Wrong arguments", "warhead lockstart true/false" };
                        if (args[1] == "true")
                        {
                            BetterWarheadHandler.Warhead.StartLock = true;
                            success = true;
                            return new string[] { "Alpha Warhead start lock turned on" };
                        }
                        else if (args[1] == "false")
                        {
                            BetterWarheadHandler.Warhead.StartLock = false;
                            success = true;
                            return new string[] { "Alpha Warhead start lock turned off" };
                        }
                        else
                        {
                            return new string[] { "Wrong arguments", "warhead lockstart true/false" };
                        }
                    }
                case "lockstop":
                    {
                        if (!admin.CheckPermission($"{PluginName}.lock")) return new string[] { "No Permissions." };
                        if (args.Length == 1) return new string[] { "Wrong arguments", "warhead lockstop true/false" };
                        if (args[1] == "true")
                        {
                            BetterWarheadHandler.Warhead.StopLock = true;
                            success = true;
                            //plugin.CommandManager.CallCommand(sender, "nuke", new string[] { "lock" });
                            return new string[] { "Alpha Warhead stop lock turned on" };
                        }
                        else if (args[1] == "false")
                        {
                            BetterWarheadHandler.Warhead.StopLock = false;
                            success = true;
                            //plugin.CommandManager.CallCommand(sender, "nuke", new string[] { "unlock" });
                            return new string[] { "Alpha Warhead stop lock turned off" };
                        }
                        else
                        {
                            return new string[] { "Wrong arguments", "warhead lockstop true/false" };
                        }
                    }
                case "lockbutton":
                    {
                        if (!admin.CheckPermission($"{PluginName}.lock")) return new string[] { "No Permissions." };
                        if (args.Length == 1) return new string[] { "Wrong arguments", "warhead lockbutton true/false" };
                        if (args[1] == "true")
                        {
                            BetterWarheadHandler.Warhead.ButtonLock = true;
                            success = true;
                            return new string[] { "Alpha Warhead button lock turned on" };
                        }
                        else if (args[2] == "false")
                        {
                            BetterWarheadHandler.Warhead.ButtonLock = false;
                            success = true;
                            return new string[] { "Alpha Warhead button lock turned off" };
                        }
                        else
                        {
                            return new string[] { "Wrong arguments", "warhead lockbutton true/false" };
                        }
                    }
                case "locklever":
                    {
                        if (!admin.CheckPermission($"{PluginName}.lock")) return new string[] { "No Permissions." };
                        if (args.Length == 1) return new string[] { "Wrong arguments", "warhead locklever true/false" };
                        if (args[1] == "true")
                        {
                            BetterWarheadHandler.Warhead.LeverLock = true;
                            success = true;
                            return new string[] { "Alpha Warhead level locked" };
                        }
                        else if (args[1] == "false")
                        {
                            BetterWarheadHandler.Warhead.LeverLock = false;
                            success = true;
                            return new string[] { "Alpha Warhead level locked" };
                        }
                        else
                        {
                            return new string[] { "Wrong arguments", "warhead locklever true/false" };
                        }
                    }
                case "getlast":
                    {
                        if (!admin.CheckPermission($"{PluginName}.data")) return new string[] { "No Permissions." };
                        success = true;
                        return new string[] {
                            "Last Warhead Start User: " + (BetterWarheadHandler.Warhead.LastStartUser == null ? "Unknown" : $"{BetterWarheadHandler.Warhead.LastStartUser.Nickname} ({BetterWarheadHandler.Warhead.LastStartUser.Id}) | {BetterWarheadHandler.Warhead.LastStartUser.UserId}"),
                            "Last Warhead Stop User: " + (BetterWarheadHandler.Warhead.LastStopUser == null ? "Unknown" : $"{BetterWarheadHandler.Warhead.LastStopUser.Nickname} ({BetterWarheadHandler.Warhead.LastStopUser.Id}) | {BetterWarheadHandler.Warhead.LastStopUser.UserId}") };
                    }
                case "stats":
                    {
                        if (!admin.CheckPermission($"{PluginName}.data")) return new string[] { "No Permissions." };
                        success = true;
                        return new string[] { "Alpha Warhead stats:",
                            "Detonated: " +Warhead.IsDetonated,
                            "Counting Down: " +BetterWarheadHandler.Warhead.CountingDown,
                            "Enabled: " +Warhead.LeverStatus,
                            "Time Left: " +BetterWarheadHandler.Warhead.TimeLeft,
                            "StartLock: "+BetterWarheadHandler.Warhead.StartLock,
                            "StopLock: "+BetterWarheadHandler.Warhead.StopLock,
                            "LeverLock: "+BetterWarheadHandler.Warhead.LeverLock,
                            "ButtonLock: "+BetterWarheadHandler.Warhead.ButtonLock,
                            "Button Open: "+Warhead.IsLocked,
                            "Last Start User: "+(BetterWarheadHandler.Warhead.LastStartUser == null ? "Unknown":$"{BetterWarheadHandler.Warhead.LastStartUser.Nickname} ({BetterWarheadHandler.Warhead.LastStartUser.Id}) | {BetterWarheadHandler.Warhead.LastStartUser.UserId}"),
                            "Last Stop User: "+(BetterWarheadHandler.Warhead.LastStopUser == null ? "Unknown":$"{BetterWarheadHandler.Warhead.LastStopUser.Nickname} ({BetterWarheadHandler.Warhead.LastStopUser.Id}) | {BetterWarheadHandler.Warhead.LastStopUser.UserId}")
                        };
                    }
                default:
                    {
                        return new string[] { "Wrong arguments", GetUsage() };
                    }
            }
        }

        public string GetUsage()
        {
            return "warhead start/stop/on/off/lockstart/lockstop/lockbutton/locklever/getlast/stats";
        }
    }
}
