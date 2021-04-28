using CommandSystem;
using Exiled.API.Features;
using Gamer.Utilities;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared;
using MistakenSocket.Shared.API;
using MistakenSocket.Shared.ClientToCentral;
using System;


namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.GameConsoleCommandHandler))]
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    class CheckIPCommand : IBetterCommand, IPermissionLocked
    {
        public override string Description =>
        "Checks IPs";

        public string GetUsage()
        {
            return "checkIP [Id]";
        }

        public string Permission => "check_ip";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "checkIp";

        public override string[] Aliases => new string[] { "cIp" };

        public static void GetIPs(string UserId, Action<string[]> callBack)
        {
            SSL.Client.Send(MessageType.CMD_REQUEST_DATA, new RequestData
            {
                Type = DataType.SL_CONNECTED_IPS,
                argument = UserId.Serialize(false)
            }).GetResponseDataCallback((data) =>
            {
                if (data.Type != ResponseType.OK)
                {
                    Log.Error($"{(int)data.Type} {data.Type}: {data.Response}");
                    callBack(new string[0]);
                    return;
                }
                callBack((string[])data.Payload.Deserialize(0, 0, out _, false, typeof(string[])));
            });
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var admin = sender.GetPlayer();
            if (args.Length == 0) return new string[] { GetUsage() };
            if (int.TryParse(args[0], out int pid))
            {
                var player = RealPlayers.Get(pid);

                if (player != null)
                {
                    GetIPs(player.UserId, (data) =>
                    {
                        admin.SendConsoleMessage($"{player.UserId} IPs:", "green");
                        foreach (var item in data)
                            admin.SendConsoleMessage(item, "green");
                    });

                    success = true;
                    return new string[] { "Printed in console '~'" };
                }
                else
                    return new string[] { "Player not found" };
            }
            else if (args[0].Contains("@steam") || args[0].Contains("@discord"))
            {
                GetIPs(args[0], (data) =>
                {
                    admin.SendConsoleMessage($"{args[0]} IPs:", "green");
                    foreach (var item in data)
                        admin.SendConsoleMessage(item, "green");
                });

                success = true;
                return new string[] { "Printed in console '~'" };
            }
            else return new string[] { GetUsage() };
        }
    }
}
