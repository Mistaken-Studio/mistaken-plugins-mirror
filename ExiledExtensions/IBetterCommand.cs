using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace Gamer.Utilities
{
    public abstract class IBetterCommand : ICommand
    {
        public abstract string Command { get; }

        public virtual string[] Aliases { get; } = new string[0];

        public virtual string Description { get; } = "";

        public string FullPermission
        {
            get
            {
                if (this is IPermissionLocked pl)
                    return $"{pl.PluginName}.{pl.Permission}";
                return "";
            }
        }

        private DateTime _start;
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            _start = DateTime.Now;
            if (sender.IsPlayer())
            {
                if (this is IPermissionLocked && !((CommandSender)sender).SenderId.CheckPermission(this.FullPermission))
                {
                    response = $"<b>Access Denied</b>\nMissing {this.FullPermission}";
                    Diagnostics.MasterHandler.LogTime("Command", this.Command, _start, DateTime.Now);
                    return false;
                }
            }
            bool bc = false;
            string argsString = string.Join(" ", arguments.Array);
            int playerId = sender.IsPlayer() ? sender.GetPlayer().Id : 1;
            if (argsString.Contains("@me"))
                argsString = argsString.Replace("@me", playerId.ToString());

            List<string> args = NorthwoodLib.Pools.ListPool<string>.Shared.Rent(arguments.Array);
            foreach (var item in args.ToArray())
            {
                if (item == "@-cbc")
                {
                    args.Remove(item);
                    sender.GetPlayer().ClearBroadcasts();
                }
                if (item == "@-bc")
                {
                    args.Remove(item);
                    bc = true;
                }
            }

            var newQuery = argsString;
            if (argsString.Contains("@!me"))
                newQuery = argsString.Replace("@!me", string.Join(".", RealPlayers.List.Where(p => p.Id != playerId).Select(p => p.Id)));
            if (argsString.Contains("@all"))
                newQuery = argsString.Replace("@all", string.Join(".", RealPlayers.List.Select(p => p.Id)));
            if (argsString.Contains("@team:"))
            {
                foreach (var item in args.Where(arg => arg.StartsWith("@team:")))
                {
                    var values = item.Split(':');
                    if (values.Length > 0)
                    {
                        var value = values[1];
                        if (int.TryParse(value, out int teamId))
                        {
                            if (teamId > -1 && teamId < 7)
                            {
                                newQuery = argsString.Replace("@team:" + value, string.Join(".", RealPlayers.Get((Team)teamId).Select(p => p.Id)));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 7; i++)
                            {
                                if (((Team)i).ToString().ToLower() == value.ToLower())
                                    newQuery = argsString.Replace("@team:" + value, string.Join(".", RealPlayers.Get((Team)i).Select(p => p.Id)));
                            }
                        }
                    }
                }
            }
            if (argsString.Contains("@!team:"))
            {
                foreach (var item in args.Where(arg => arg.StartsWith("@!team:")))
                {
                    var values = item.Split(':');
                    if (values.Length > 0)
                    {
                        var value = values[1];
                        if (int.TryParse(value, out int teamId))
                        {
                            if (teamId > -1 && teamId < 7)
                            {
                                newQuery = argsString.Replace("@!team:" + value, string.Join(".", RealPlayers.Get((Team)teamId).Select(p => p.Id)));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 7; i++)
                            {
                                if (((Team)i).ToString().ToLower() == value.ToLower())
                                    newQuery = argsString.Replace("@!team:" + value, string.Join(".", RealPlayers.Get((Team)i).Select(p => p.Id)));
                            }
                        }
                    }
                }
            }

            if (argsString.Contains("@role:"))
            {
                foreach (var item in args.Where(arg => arg.StartsWith("@role:")))
                {
                    var values = item.Split(':');
                    if (values.Length > 0)
                    {
                        var value = values[1];
                        if (int.TryParse(value, out int roleId))
                        {
                            if (roleId > -1 && roleId < 18)
                            {
                                newQuery = argsString.Replace("@role:" + value, string.Join(".", RealPlayers.Get((RoleType)roleId).Select(p => p.Id)));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 18; i++)
                            {
                                if (((RoleType)i).ToString().ToLower() == value.ToLower())
                                    newQuery = argsString.Replace("@role:" + value, string.Join(".", RealPlayers.Get((RoleType)i).Select(p => p.Id)));
                            }
                        }
                    }
                }
            }
            if (argsString.Contains("@!role:"))
            {
                foreach (var item in args.Where(arg => arg.StartsWith("@!role:")))
                {
                    var values = item.Split(':');
                    if (values.Length > 0)
                    {
                        var value = values[1];
                        if (int.TryParse(value, out int roleId))
                        {
                            if (roleId > -1 && roleId < 18)
                            {
                                newQuery = argsString.Replace("@!role:" + value, string.Join(".", RealPlayers.List.Where(p => p.Role != (RoleType)roleId).Select(p => p.Id)));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 18; i++)
                            {
                                if (((RoleType)i).ToString().ToLower() == value.ToLower())
                                    newQuery = argsString.Replace("@!role:" + value, string.Join(".", RealPlayers.List.Where(p => p.Role != (RoleType)i).Select(p => p.Id)));
                            }
                        }
                    }
                }
            }

            if (argsString.Contains("@zone:"))
            {
                foreach (var item in args.Where(arg => arg.StartsWith("@zone:")))
                {
                    var values = item.Split(':');
                    if (values.Length > 0)
                    {
                        var value = values[1];
                        for (int i = 0; i < 4; i++)
                        {
                            if (((ZoneType)i).ToString().ToLower() == value.ToLower())
                                newQuery = argsString.Replace("@zone:" + value, string.Join(".", RealPlayers.List.Where(p => p.CurrentRoom.Zone == (ZoneType)i).Select(p => p.Id)));
                        }
                    }
                }
            }
            if (argsString.Contains("@!zone:"))
            {
                foreach (var item in args.Where(arg => arg.StartsWith("@!zone:")))
                {
                    var values = item.Split(':');
                    if (values.Length > 0)
                    {
                        var value = values[1];
                        for (int i = 0; i < 4; i++)
                        {
                            if (((ZoneType)i).ToString().ToLower() == value.ToLower())
                                newQuery = argsString.Replace("@!zone:" + value, string.Join(".", RealPlayers.List.Where(p => p.CurrentRoom.Zone != (ZoneType)i).Select(p => p.Id)));
                        }
                    }
                }
            }


            response = string.Join("\n", Execute(sender, newQuery.Split(' ').Skip(1).ToArray(), out bool successfull));
            if(bc)
                sender.GetPlayer().Broadcast(Command, 10, string.Join("\n", response));
            NorthwoodLib.Pools.ListPool<string>.Shared.Return(args);
            Diagnostics.MasterHandler.LogTime("Command", this.Command, _start, DateTime.Now);
            return successfull;
        }

        public abstract string[] Execute(ICommandSender sender, string[] args, out bool success);
    }
}
