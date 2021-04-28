using CommandSystem;
using Gamer.Utilities;
using System.Linq;

namespace Gamer.Mistaken.BetterMutes
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    public class MuteCommandHandler : IBetterCommand, IPermissionLocked
    {
        public override string Command => "mute2";

        public override string[] Aliases => new string[] { "m2" };

        public override string Description => "Mute option that allows to mute with reason and for limited time";

        public string Permission => "mute";

        public string PluginName => PluginHandler.PluginName;

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length < 3) return new string[] { GetUsage() };
            var target = RealPlayers.Get(args[0]);
            if (target == null) return new string[] { "Player not found", GetUsage() };
            if (!MuteHandler.GetDuration(args[1], out int duration))
                return new string[] { "Wrong duration, Too bad" };
            string reason = string.Join(" ", args.Skip(2));
            success = true;
            if (MuteHandler.Mute(target, false, reason, duration))
                return new string[] { $"Muted ({target.Id}) {target.Nickname} for {duration} minutes with reason \"{reason}\"" };
            success = false;
            return new string[] { "User is already muted" };
        }

        public string GetUsage() =>
            "mute2 [playerId] [duration (-1 = perm)] [reason]";
    }

    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    public class UnMuteCommandHandler : IBetterCommand, IPermissionLocked
    {
        public override string Command => "unmute2";

        public override string[] Aliases => new string[] { "unm2" };

        public override string Description => "UnMute for Mute2";

        public string Permission => "mute";

        public string PluginName => PluginHandler.PluginName;

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            var target = RealPlayers.Get(args[0]);
            if (target == null && args[0].Length != 17) return new string[] { "Player not found", GetUsage() };
            var uId = target?.UserId ?? args[0];
            if (!uId.Contains("@"))
                uId += "@steam";
            success = true;
            if (MuteHandler.RemoveMute(uId, false))
                return new string[] { $"UnMuted {uId}" };
            success = false;
            return new string[] { "User was not muted" };
        }

        public string GetUsage() =>
            "unmute2 [playerId/userId]";
    }

    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    public class IMuteCommandHandler : IBetterCommand, IPermissionLocked
    {
        public override string Command => "imute2";

        public override string[] Aliases => new string[] { "im2" };

        public override string Description => "Intercom Mute option that allows to mute with reason and for limited time";

        public string Permission => "mute";

        public string PluginName => PluginHandler.PluginName;

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length < 3) return new string[] { GetUsage() };
            var target = RealPlayers.Get(args[0]);
            if (target == null) return new string[] { "Player not found", GetUsage() };
            if (!MuteHandler.GetDuration(args[1], out int duration))
                return new string[] { "Wrong duration, Too bad" };
            string reason = string.Join(" ", args.Skip(2));
            success = true;
            if (MuteHandler.Mute(target, true, reason, duration))
                return new string[] { $"Intercom Muted ({target.Id}) {target.Nickname} for {duration} minutes with reason \"{reason}\"" };
            success = false;
            return new string[] { "User is already intercom muted" };
        }

        public string GetUsage() =>
            "imute2 [playerId] [duration (-1 = perm)] [reason]";
    }

    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    public class IUnMuteCommandHandler : IBetterCommand, IPermissionLocked
    {
        public override string Command => "iunmute2";

        public override string[] Aliases => new string[] { "iunm2" };

        public override string Description => "UnMute for IMute2";

        public string Permission => "mute";

        public string PluginName => PluginHandler.PluginName;

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0) return new string[] { GetUsage() };
            var target = RealPlayers.Get(args[0]);
            if (target == null && args[0].Length != 17) return new string[] { "Player not found", GetUsage() };
            var uId = target?.UserId ?? args[0];
            if (!uId.Contains("@"))
                uId += "@steam";
            success = true;
            if (MuteHandler.RemoveMute(uId, true))
                return new string[] { $"Intercom UnMuted {uId}" };
            success = false;
            return new string[] { "User was not intercom muted" };
        }

        public string GetUsage() =>
            "iunmute2 [playerId/userId]";
    }
}
