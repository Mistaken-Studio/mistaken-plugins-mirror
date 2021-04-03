using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Systems.Staff;
using Gamer.Utilities;
using MEC;
using UnityEngine;

namespace Gamer.Mistaken.Systems
{
    public class CustomInfoHandler : Module
    {
        public CustomInfoHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "CustomInfo";
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        private static readonly Dictionary<Player, Dictionary<string, string>> CustomInfo = new Dictionary<Player, Dictionary<string, string>>();
        private static readonly Dictionary<Player, Dictionary<string, string>> CustomInfoStaffOnly = new Dictionary<Player, Dictionary<string, string>>();
        private static readonly List<Player> ToUpdate = new List<Player>();

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(DoRoundLoop());
        }
        private IEnumerator<float> DoRoundLoop()
        {
            yield return MEC.Timing.WaitForSeconds(1);
            while(Round.IsStarted)
            {
                yield return MEC.Timing.WaitForSeconds(1);
                if (ToUpdate.Count == 0)
                    continue;
                foreach (var item in ToUpdate.ToArray())
                {
                    if(item.IsConnected)
                        Update(item);
                    ToUpdate.Remove(item);
                }
            }
        }

        private void Update(Player player)
        {
            if (!CustomInfo.ContainsKey(player))
                CustomInfo[player] = new Dictionary<string, string>();
            if (!CustomInfoStaffOnly.ContainsKey(player))
                CustomInfoStaffOnly[player] = new Dictionary<string, string>();
            string toSet = string.Join("<br>", CustomInfo[player].Values);
            player.CustomInfo = string.IsNullOrWhiteSpace(toSet) ? null : toSet;
            {
                var tmp = CustomInfoStaffOnly[player].Values.ToList();
                tmp.AddRange(CustomInfo[player].Values);
                toSet = string.Join("<br>", tmp);
                foreach (var staff in RealPlayers.List.Where(p => p.IsStaff()))
                    staff.SetPlayerInfoForTargetOnly(player, toSet);
            }
        }

        public static void Set(Player player, string key, string value, bool staffOnly)
        {   
            if(staffOnly)
            {
                if (!CustomInfoStaffOnly.ContainsKey(player))
                    CustomInfoStaffOnly[player] = new Dictionary<string, string>();
                if (string.IsNullOrWhiteSpace(value))
                    CustomInfoStaffOnly[player].Remove(key);
                else if (!CustomInfoStaffOnly[player].TryGetValue(key, out string oldValue) || oldValue != value)
                    CustomInfoStaffOnly[player][key] = value;
                else
                    return;
            }
            else
            {
                if (!CustomInfo.ContainsKey(player))
                    CustomInfo[player] = new Dictionary<string, string>();
                if (string.IsNullOrWhiteSpace(value))
                    CustomInfo[player].Remove(key);
                else if (!CustomInfo[player].TryGetValue(key, out string oldValue) || oldValue != value)
                    CustomInfo[player][key] = value;
                else
                    return;
            }
            ToUpdate.Add(player);
        }
    }
}
