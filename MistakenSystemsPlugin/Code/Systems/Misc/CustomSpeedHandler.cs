using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Utilities;
using Grenades;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using Exiled.API.Extensions;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.Systems.Misc
{
    public class CustomSpeedHandler : Module
    {
        public static readonly Dictionary<int, (float, float)> Speeds = new Dictionary<int, (float, float)>();
        public static readonly Dictionary<RoleType, (float, float)> ClassBasedSpeeds = new Dictionary<RoleType, (float, float)>();

        public CustomSpeedHandler(PluginHandler p) : base(p)
        {
            Timing.RunCoroutine(AutoUpdate());
        }
        public override string Name => "CustomSpeed";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }

        private static bool Initiated = false;

        private void Server_RoundStarted()
        {
            Initiated = false;
        }

        private IEnumerator<float> AutoUpdate()
        {
            while(true)
            {
                ForceUpdate();
                yield return Timing.WaitForSeconds(5);
            }
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            ForceUpdate(ev.Player);
        }

        private void Server_RestartingRound()
        {
            Speeds.Clear();
        } 

        public static void ForceUpdate()
        {
            foreach (var player in RealPlayers.List.ToArray())
                ForceUpdate(player);
        }

        public static void ForceUpdate(Player player)
        {
            var result = GetSpeeds(player);
            if (!result.HasValue)
                return;
            player.SendCustomSync(ServerConfigSynchronizer.Singleton.netIdentity, typeof(ServerConfigSynchronizer), null, (writer) =>
            {
                writer.WritePackedUInt64(6UL);
                writer.WriteSingle(result.Value.Item1);
                writer.WriteSingle(result.Value.Item2);
            });
        }

        public static void SetSpeeds(Player player, float? walkSpeed, float? runSpeed)
        {
            if(!Initiated)
            {
                ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier = 10;
                ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier = 10;
                Initiated = true;
            }
            if (!walkSpeed.HasValue)
                walkSpeed = 1.2f;
            else if (10 < walkSpeed)
                walkSpeed = 10;
            if (!runSpeed.HasValue)
                runSpeed = 1.05f;
            else if (ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier < runSpeed)
                runSpeed = 10f;
            if (!Speeds.ContainsKey(player.Id))
                Speeds.Add(player.Id, (walkSpeed.Value, runSpeed.Value));
            else
                Speeds[player.Id] = (walkSpeed.Value, runSpeed.Value);
            ForceUpdate(player);
        }

        private static (float, float)? GetSpeeds(Player player)
        {
            if (Speeds.TryGetValue(player.Id, out (float, float) value))
                return value;
            if (ClassBasedSpeeds.TryGetValue(player.Role, out value))
                return value;
            return null;
        }
    }
}
