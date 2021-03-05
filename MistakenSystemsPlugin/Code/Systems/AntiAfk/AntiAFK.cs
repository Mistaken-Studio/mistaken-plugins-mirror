using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using Gamer.Utilities;
using MEC;
using UnityEngine;
using Gamer.Diagnostics;
using Gamer.RoundLoggerSystem;

namespace Gamer.Mistaken.Systems.AntiAFK
{
    public class Handler : Module
    {
        public Handler(PluginHandler p) : base(p)
        {  
        }
        public override string Name => "AntiAFK";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }

        private static void Server_RestartingRound()
        {
            AfkPosition.Clear();
        }

        private static void Server_RoundStarted()
        {
            Timing.RunCoroutine(AfkDetector());
        }

        internal static readonly Dictionary<int, KeyValuePair<int, Vector3>> AfkPosition = new Dictionary<int, KeyValuePair<int, Vector3>>();

        private static IEnumerator<float> AfkDetector()
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId; 
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                CheckForAfk();
                yield return Timing.WaitForSeconds(10);
            }
        }

        private const string AfkMessage =
            @"<size=40><voffset=20em>
            <color=red><b><size=200>WARNING</size></b></color>
            <br><br><br><br><br>
            You have <color=yellow>{sLeft} seconds</color> to move or type '<color=yellow>.notafk</color>' in console in '<color=yellow>~</color>'
            </voffset></size>";

        private static void CheckForAfk()
        {
            foreach (var player in RealPlayers.List.Where(p => p.IsAlive && p.Role != RoleType.Scp079 && !p.GetEffectActive<CustomPlayerEffects.Ensnared>()).ToArray())
            {
                var ppos = player.Position;
                if (AfkPosition.TryGetValue(player.Id, out KeyValuePair<int, Vector3> value))
                {
                    int level = value.Key;
                    Vector3 pos = value.Value;
                    if (pos.x == ppos.x && pos.y == ppos.y && pos.z == ppos.z)
                    {
                        AfkPosition[player.Id] = new KeyValuePair<int, Vector3>(level + 1, ppos);
                        switch (level + 1)
                        {
                            case 12:
                                {
                                    if (!player.CheckPermission(PluginHandler.Instance.Name + ".anti_afk_kick_proof"))
                                    {
                                        RoundLogger.Log("ANTY AFK", "WARN", $"{player.PlayerToString()} was warned for being afk");
                                        Timing.RunCoroutine(InformAFK(player));
                                    }
                                    break;
                                }
                            case 18:
                                {
                                    CustomAchievements.RoundEventHandler.AddProggress("AFK", player);
                                    if (!player.CheckPermission(PluginHandler.Instance.Name + ".anti_afk_kick_proof"))
                                    {
                                        player.Disconnect("Anti AFK: You were AFK");
                                        RoundLogger.Log("ANTY AFK", "DISCONNECT", $"{player.PlayerToString()} was disconnected for being afk");
                                        MapPlus.Broadcast("Anti AFK", 10, $"{player.Nickname} was disconnected for being AFK", Broadcast.BroadcastFlags.AdminChat);
                                    }
                                    break;
                                }
                        }
                    }
                    else
                        AfkPosition[player.Id] = new KeyValuePair<int, Vector3>(0, ppos);
                }
                else
                    AfkPosition.Add(player.Id, new KeyValuePair<int, Vector3>(0, ppos));
            }
        }

        private static IEnumerator<float> InformAFK(Player p)
        {
            for (int i = 60; i > -1; i--)
            {
                if (AfkPosition.TryGetValue(p.Id, out KeyValuePair<int, Vector3> value))
                {
                    if (value.Key >= 12)
                        p.ShowHint(AfkMessage.Replace("{sLeft}", i.ToString("00")), 2);
                    else
                        yield break;
                }
                else
                    yield break;
                yield return Timing.WaitForSeconds(1);
            }
        }
    }

    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class CommandHandler : IBetterCommand
    {
        public override string Command => "notafk";

        public override string[] Aliases => new string[] { };

        public override string Description => "I'm not afk";

        public override string[] Execute(global::CommandSystem.ICommandSender sender, string[] args, out bool success)
        {
            var player = sender.GetPlayer();
            if (Handler.AfkPosition.ContainsKey(player.Id))
                Handler.AfkPosition.Remove(player.Id);
            success = true;
            return new string[] { "Done" };
        }
    }
}
