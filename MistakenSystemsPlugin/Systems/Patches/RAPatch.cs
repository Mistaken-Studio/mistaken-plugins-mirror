#pragma warning disable

using System;
using RemoteAdmin;
using System.Collections.Generic;
using Mirror;
using Exiled.API.Features;
using Gamer.Utilities;
using HarmonyLib;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared.API;

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(CommandProcessor))]
    [HarmonyPatch("ProcessQuery")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(CommandSender) })]
    public static class RAPatch
    {
        internal static readonly Dictionary<NetworkConnection, string> Responses = new Dictionary<NetworkConnection, string>();
        public static void Postfix(string q, CommandSender sender)
        {
            DateTime start = DateTime.Now;
            if (!sender.IsPlayer())
                return;
            var conn = sender.GetPlayer().Connection;
            if (q.ToUpper().StartsWith("REQUEST_DATA"))
            {
                Responses.Remove(conn);
                return;
            }
            string result = "";
            if(Responses.ContainsKey(conn))
            {
                result = Responses[conn];
                Responses.Remove(conn);
            }
            SSL.Client.Send(MessageType.CMD_STORE_COMMAND, new MistakenSocket.Shared.CommandHistory.Command
            {
                Query = q,
                Server = Server.Port - 7776,
                Time = DateTime.Now,
                UserId = (sender.IsPlayer() ? sender.GetPlayer()?.UserId : null) ?? "CONSOLE",
                Result = result.Replace("\n", "|___n")
            });
            Diagnostics.MasterHandler.LogTime("Patch", "RAPatch", start, DateTime.Now);
        }

        public static void LogCommand(CommandSender sender, string query, string response)
        {
            SSL.Client.Send(MessageType.CMD_STORE_COMMAND, new MistakenSocket.Shared.CommandHistory.Command
            {
                Query = query,
                Server = Server.Port - 7776,
                Time = DateTime.Now,
                UserId = (sender.IsPlayer() ? sender.GetPlayer()?.UserId : null) ?? "CONSOLE",
                Result = response.Replace("\n", "|___n")
            });
        }
    }

    [HarmonyPatch(typeof(QueryProcessor), "TargetReply")]
    //[HarmonyPatch(new Type[] { typeof(NetworkConnection), typeof(string), typeof(string), typeof(string), typeof(string) })]
    internal static class TargetReplyPatch
    {
        public static void Prefix(NetworkConnection conn, string content, bool isSuccess, bool logInConsole, string overrideDisplay)
        {
            if (!RAPatch.Responses.ContainsKey(conn))
                RAPatch.Responses.Add(conn, content);
            else
                RAPatch.Responses[conn] += "\n" + content;
        }
    }
}

