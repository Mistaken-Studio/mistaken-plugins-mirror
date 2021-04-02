using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Gamer.Diagnostics;
using Gamer.Mistaken.Utilities.APILib;
using Gamer.Utilities;

namespace Gamer.Mistaken.Base
{
    public class LoggerHandler : Diagnostics.Module
    {
        public override string Name => nameof(LoggerHandler);
        public LoggerHandler(IPlugin<IConfig> plugin) : base(plugin)
        {
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += this.Handle<Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs>((ev) => Server_SendingRemoteAdminCommand(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= this.Handle<Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs>((ev) => Server_SendingRemoteAdminCommand(ev));
        }

        private void Server_SendingRemoteAdminCommand(Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs ev)
        {
            if (string.IsNullOrWhiteSpace(ev.Name)) 
                return;
            if(ev.Name.StartsWith("@"))
            {
                SendCommand("AdminChat", ev.Name.Substring(1) + string.Join(" ", ev.Arguments), ev, "0");
                return;
            }

            Log.Info($"Admin:{ev.Sender?.Nickname ?? "Console"} Command: {ev.Name} {string.Join(" ", ev.Arguments)}");
            switch (ev.Name.ToLower())
            {
                case "request_data":
                case "ban2":
                case "ban":
                case "confirm":
                    break;
                case "server_event":
                    {
                        if (ev.Arguments.Count == 0)
                            SendCommand(ev.Name.ToLower(), "", ev, "0");
                        else
                            SendCommand(ev.Name.ToLower(), ev.Arguments[0], ev, "0");
                        break;
                    }
                case "roundrestart":
                case "reconnectrs":
                case "lockdown":
                case "forcestart":
                    {
                        SendCommand(ev.Name.ToLower(), "None", ev, "0");
                        break;
                    }
                case "goto":
                    {
                        if (ev.Arguments.Count == 0)
                            SendCommand(ev.Name.ToLower(), "", ev, "0");
                        else
                        {
                            Player player = Player.Get(ev.Arguments[0]);
                            SendCommand(ev.Name.ToLower(), "None", ev, player?.UserId);
                        }
                        break;
                    }
                case "ball":
                case "canadel":
                case "flash":
                case "grenade":
                    {
                        if (ev.Arguments.Count == 0)
                            SendCommand(ev.Name.ToLower(), "", ev, "0");
                        else
                        {
                            if(ev.Arguments[0].Split('.').Length > 2)
                            {
                                Player player = RealPlayers.Get(ev.Arguments[0].Split('.')[0]);
                                SendCommand(ev.Name.ToLower(), ev.Arguments[0], ev, player?.UserId);
                            }
                            else
                            {
                                Player player = RealPlayers.Get(ev.Arguments[0]);
                                SendCommand(ev.Name.ToLower(), "NONE", ev, player?.UserId);
                            }
                        }
                        break;
                    }
                case "tpall":
                case "bring":
                case "heal":
                case "iunmute":
                case "unmute":
                case "imute":
                case "mute":
                    {
                        if (ev.Arguments.Count == 0)
                            break;
                        string playerstring = ev.Arguments[0]?.Split('.')?[0];
                        Player player = RealPlayers.Get(playerstring);

                        SendCommand(ev.Name.ToLower(), "NONE", ev, player?.UserId);
                        break;
                    }
                case "destroy":
                case "unlock":
                case "lock":
                case "close":
                case "open":
                    {
                        if (ev.Arguments.Count == 0)
                            SendCommand(ev.Name.ToLower(), "", ev, "0");
                        else
                            SendCommand(ev.Name.ToLower(), ev.Arguments[0], ev, "0");
                        break;
                    }
                case "doortp":
                    {
                        if (ev.Arguments.Count == 0)
                            SendCommand(ev.Name.ToLower(), "", ev, "0");
                        else
                        {
                            Player player = RealPlayers.Get(ev.Arguments[0]?.Split('.')?[0]);
                            SendCommand(ev.Name.ToLower(), ev.Arguments[0] + ev.Arguments[1], ev, player?.UserId);
                        }
                        break;
                    }
                case "dropall":
                case "clean":
                    {
                        if (ev.Arguments.Count < 0)
                            SendCommand(ev.Name.ToLower(), "None", ev, "0");
                        else if (ev.Arguments.Count < 1)
                            SendCommand(ev.Name.ToLower(), ev.Arguments[0], ev, "0");
                        else
                            SendCommand(ev.Name.ToLower(), ev.Arguments[0] + " " + ev.Arguments[1], ev, "0");
                        break;
                    }
                case "utag":
                case "rtag":
                case "updatetag":
                case "requesttag":
                case "refreshtag":
                    {
                        SendCommand(ev.Name.ToLower(), string.Join(" ", ev.Arguments), ev, "0");
                        break;
                    }
                case "em":
                case "eventmanager":
                    {
                        if (ev.Arguments.Count == 0)
                            SendCommand(ev.Name.ToLower(), "NONE", ev, "0");
                        else
                        {
                            switch (ev.Arguments[0].ToLower())
                            {
                                case "f":
                                case "force":
                                    {
                                        SendCommand(ev.Name.ToLower(), "force " + (ev.Arguments.Count == 1 ? "" : ev.Arguments[1]), ev, "0");
                                        break;
                                    }
                                case "l":
                                case "list":
                                    {
                                        SendCommand(ev.Name.ToLower(), "list", ev, "0");
                                        break;
                                    }
                                default:
                                    {
                                        SendCommand(ev.Name.ToLower(), string.Join(" ", ev.Arguments), ev, "0");
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case "roundlock":
                    {
                        SendCommand(ev.Name.ToLower(), (!Round.IsLocked).ToString(), ev, "0");
                        break;
                    }
                case "lobbylock":
                    {
                        SendCommand(ev.Name.ToLower(), (!Round.IsLobbyLocked).ToString(), ev, "0");
                        break;
                    }
                case "warhead":
                    {
                        if (ev.Arguments.Count == 0)
                            SendCommand(ev.Name.ToLower(), "NONE", ev, "0");
                        else
                            SendCommand(ev.Name.ToLower(), string.Join(" ", ev.Arguments), ev, "0");
                        break;
                    }
                case "pbc":
                    {
                        if (ev.Arguments.Count == 0)
                            SendCommand(ev.Name.ToLower(), "", ev, "0");
                        else
                        {
                            Player player = RealPlayers.Get(ev.Arguments[0]?.Split('.')?[0]);
                            SendCommand(ev.Name.ToLower(), string.Join(" ", ev.Arguments.Skip(1)), ev, player?.UserId ?? "0");
                        }
                        break;
                    }
                case "bc":
                case "cassie_silent":
                case "cassie_sl":
                case "cassie":
                    {
                        SendCommand(ev.Name.ToLower(), string.Join(" ", ev.Arguments), ev, "0");
                        break;
                    }
                default:
                    {
                        if (ev.Arguments.Count == 0)
                            SendCommand(ev.Name.ToLower(), "NONE", ev, "0");
                        else
                        {
                            string playerstring = "";
                            if (ev.Arguments.Count >= 1)
                                playerstring = ev.Arguments[0]?.Split('.')?[0];
                            Player player = null;
                            if (playerstring != "")
                                player = RealPlayers.Get(playerstring);
                            string argString = string.Join(" ", ev.Arguments);
                            SendCommand(ev.Name.ToLower(), string.IsNullOrWhiteSpace(argString) ? "NONE" : argString, ev, player?.UserId ?? "0");
                        }
                        break;
                    }
            }
        }

        public static void SendCommand(string command, string arg, Exiled.Events.EventArgs.SendingRemoteAdminCommandEventArgs ev, string userId)
        {
            if (ev.Sender == null) 
                return;
            if (arg.Contains("&"))
                arg.Replace("&", "");
            if (arg.Contains("?"))
                arg.Replace("?", "");

            if (command.Contains("&"))
                command.Replace("&", "");
            if (command.Contains("?"))
                command.Replace("?", "");

            if (!Utilities.APILib.API.GetUrl(APIType.SEND_LOGS, out string url, arg, command, userId, ev.Sender.UserId, ServerConsole.Ip, Server.Port.ToString()))
                return;
            using (var client = new WebClient())
            {
                if (url.Contains("#"))
                    url = url.Replace("#", "");
                //Log.Debug(url);
                client.DownloadDataAsync(new Uri(url));
            }
        }

        public static void SendRemoteCommand(string command, string AdminUId, string userId = "0")
        {
            if (command.Contains("&"))
                command.Replace("&", "");
            if (command.Contains("?"))
                command.Replace("?", "");

            if (!Utilities.APILib.API.GetUrl(APIType.SEND_LOGS, out string url, command, "RemoteCommand", userId, AdminUId, ServerConsole.Ip, Server.Port.ToString())) 
                return;
            using (var client = new WebClient())
            {
                if(url.Contains("#"))
                    url = url.Replace("#", "");
                //Log.Debug(url);
                client.DownloadStringAsync(new Uri(url));
            }
        }
    }
}
