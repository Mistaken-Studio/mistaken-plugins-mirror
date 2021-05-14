using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.Staff;
using Gamer.Utilities;
using MEC;
using System.Collections.Generic;
using System.Linq;

namespace Gamer.Mistaken.Base
{
    /// <inheritdoc/>
    public class CustomInfoHandler : Module
    {
        /// <inheritdoc/>
        public override bool IsBasic => true;
        /// <inheritdoc/>
        public CustomInfoHandler(PluginHandler p) : base(p)
        {
        }

        /// <inheritdoc/>
        public override string Name => "CustomInfo";
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }

        private void Server_RestartingRound()
        {
            CustomInfo.Clear();
            CustomInfoStaffOnly.Clear();
            ToUpdate.Clear();
        }

        private static readonly Dictionary<Player, Dictionary<string, string>> CustomInfo = new Dictionary<Player, Dictionary<string, string>>();
        private static readonly Dictionary<Player, Dictionary<string, string>> CustomInfoStaffOnly = new Dictionary<Player, Dictionary<string, string>>();
        private static readonly Dictionary<Player, Dictionary<Player, Dictionary<string, string>>> CustomInfoTargeted = new Dictionary<Player, Dictionary<Player, Dictionary<string, string>>>();
        private static readonly List<Player> ToUpdate = new List<Player>();

        private void Server_RoundStarted()
        {
            this.RunCoroutine(DoRoundLoop(), "DoRoundLoop");
        }
        private IEnumerator<float> DoRoundLoop()
        {
            yield return MEC.Timing.WaitForSeconds(1);
            while (Round.IsStarted)
            {
                yield return MEC.Timing.WaitForSeconds(2);
                if (ToUpdate.Count == 0)
                    continue;
                foreach (var item in ToUpdate.ToArray())
                {
                    try
                    {
                        if (item.IsConnected)
                            Update(item);
                        ToUpdate.Remove(item);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                    }
                }
            }
        }

        private void Update(Player player)
        {
            if (!CustomInfo.ContainsKey(player))
                CustomInfo[player] = new Dictionary<string, string>();
            if (!CustomInfoStaffOnly.ContainsKey(player))
                CustomInfoStaffOnly[player] = new Dictionary<string, string>();
            if (!CustomInfoTargeted.ContainsKey(player))
                CustomInfoTargeted[player] = new Dictionary<Player, Dictionary<string, string>>();

            string for_players = string.Join("\n", CustomInfo[player].Values);
            player.CustomInfo = string.IsNullOrWhiteSpace(for_players) ? null : for_players;

            if (CustomInfoStaffOnly[player].Count > 0)
            {
                foreach (var item in RealPlayers.List.Where(p => p.IsStaff()))
                {
                    if (!CustomInfoTargeted[player].ContainsKey(item))
                        CustomInfoTargeted[player][item] = new Dictionary<string, string>();
                    foreach (var item2 in CustomInfoStaffOnly[player])
                        CustomInfoTargeted[player][item][item2.Key] = item2.Value;
                }
            }
            if (CustomInfoTargeted[player].Count > 0)
            {
                if (player?.Connection?.identity == null)
                    return;
                foreach (var item in CustomInfoTargeted[player])
                {
                    if (item.Value.Count == 0)
                        continue;
                    if (!(player?.IsConnected ?? false))
                        continue;
                    if (item.Key?.Connection?.identity == null)
                        continue;
                    var tmp = item.Value.Values.ToList();
                    tmp.AddRange(CustomInfo[player].Values);
                    this.CallDelayed(1, () => item.Key.SetPlayerInfoForTargetOnly(player, string.Join("\n", tmp)), "Update");
                }
            }
        }
        /// <summary>
        /// Sets CustomInfo
        /// </summary>
        /// <param name="player">Player</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public static void Set(Player player, string key, string value)
        {
            if (!CustomInfo.ContainsKey(player))
                CustomInfo[player] = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(value))
                CustomInfo[player].Remove(key);
            else if (!CustomInfo[player].TryGetValue(key, out string oldValue) || oldValue != value)
                CustomInfo[player][key] = value;
            else
                return;
            ToUpdate.Add(player);
        }

        public static void SetStaff(Player player, string key, string value)
        {
            if (!CustomInfoStaffOnly.ContainsKey(player))
                CustomInfoStaffOnly[player] = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(value))
                CustomInfoStaffOnly[player].Remove(key);
            else if (!CustomInfoStaffOnly[player].TryGetValue(key, out string oldValue) || oldValue != value)
                CustomInfoStaffOnly[player][key] = value;
            else
                return;
            ToUpdate.Add(player);
        }

        public static void SetTarget(Player player, string key, string value, Player target)
        {
            if (!CustomInfoTargeted.ContainsKey(player))
                CustomInfoTargeted[player] = new Dictionary<Player, Dictionary<string, string>>();
            if (!CustomInfoTargeted[player].ContainsKey(target))
                CustomInfoTargeted[player][target] = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(value))
                CustomInfoTargeted[player][target].Remove(key);
            else if (!CustomInfoTargeted[player][target].TryGetValue(key, out string oldValue) || oldValue != value)
                CustomInfoTargeted[player][target][key] = value;
            else
                return;
            ToUpdate.Add(player);
        }
    }
}
