using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using Grenades;
using NPCS;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Misc
{
    internal class MassTKDetectionHandler : Module
    {
        public MassTKDetectionHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "MassTKDetection";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Dying += this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Dying -= this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => Player_Dying(ev));
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
        }

        public const bool Debug = false;

        public static readonly Dictionary<string, Player[]> GreneadedPlayers = new Dictionary<string, Player[]>();
        public static readonly Dictionary<string, int> GreneadedDeadPlayers = new Dictionary<string, int>();
        public static readonly Dictionary<Player, (RoleType, Vector3)> DeathInfo = new Dictionary<Player, (RoleType, Vector3)>();
        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (!ev.IsAllowed)
            {
                RoundLogger.Log("MASS TK", "SKIP", "MTKD Skip Code: 2");
                return;
            }
            if (!ev.IsFrag)
            {
                RoundLogger.Log("MASS TK", "SKIP", "MTKD Skip Code: 3");
                return;
            }
            var frag = ev.Grenade.GetComponent<FragGrenade>();
            string userId = null;
            string name = null;
            string nick = null;
            if (frag.thrower == null)
            {
                try
                {
                    userId = frag._throwerName.Split('(')[1].Split(')')[0];
                    name = frag._throwerName;
                    nick = frag._throwerName.Split('(')[0];
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                    Log.Error(frag._throwerName);
                    return;
                }
            }
            else
            {
                if (Server.Host == ev.Thrower)
                    return;
                if (ev.Thrower == null)
                {
                    RoundLogger.Log("MASS TK", "SKIP", "MTKD Skip Code: 1");
                    return;
                }
                userId = ev.Thrower.UserId;
                name = ev.Thrower.PlayerToString();
                nick = ev.Thrower.GetDisplayName();
            }
            foreach (var item in ev.TargetToDamages.Where(i => i.Key.IsNPC()).ToArray())
                ev.TargetToDamages.Remove(item.Key);
            if (ev.TargetToDamages.Count == 0 || (ev.TargetToDamages.Count == 1 && ev.Thrower != null && ev.TargetToDamages.ContainsKey(ev.Thrower)))
            {
                RoundLogger.Log("MASS TK", "SKIP", "MTKD Skip Code: 4");
                return;
            }
            if (ev.TargetToDamages.Any(i => i.Key.Side != frag.TeamWhenThrown.GetSide() || (i.Key.Role == RoleType.ClassD && frag.TeamWhenThrown == Team.CDP)))
            {
                RoundLogger.Log("MASS TK", "SKIP", "MTKD Skip Code: 5");
                return;
            }
            if (GreneadedPlayers.ContainsKey(userId))
            {
                RoundLogger.Log("MASS TK", "SKIP", "MTKD Skip Code: 6");
                return;
            }
            RoundLogger.Log("MASS TK", "EXECUTE", "MTKD Execute Code: 1");
            GreneadedPlayers.Add(userId, ev.TargetToDamages.Keys.ToArray());
            GreneadedDeadPlayers.Add(userId, 0);
            this.CallDelayed(5, () =>
            {
                var value = GreneadedDeadPlayers[userId];
                Log.Info($"Detected Greneade TK, {value} killed");
                RoundLogger.Log("MASS TK", "DETECTED", $"{name} team killed {value} players");
                if (value > 3)
                {
                    RoundLogger.Log("MASS TK", "EXECUTE", "MTKD Execute Code: 3");
                    MapPlus.Broadcast("MassTK Detection System", 10, $"Detected Grenade Mass TeamKill ({value}) by {nick}\nRespawning team killed players", Broadcast.BroadcastFlags.AdminChat);
                    MapPlus.Broadcast("MassTK Detection System", 10, "Wykryto MassTK, respienie graczy ...", Broadcast.BroadcastFlags.Normal);
                    RoundLogger.Log("MASS TK", "RESPAWN", $"Respawning {value} players");
                    foreach (var player in GreneadedPlayers[userId])
                    {
                        if (player == null || player.IsAlive)
                        {
                            RoundLogger.Log("MASS TK", "SKIP", "MTKD Skip Code: 7");
                            return;
                        }
                        RoundLogger.Log("MASS TK", "RESPAWN", $"Spawning {player.Nickname}");
                        if (DeathInfo.TryGetValue(player, out (RoleType Role, Vector3 Position) deathInfo))
                        {
                            player.Role = deathInfo.Role;
                            this.CallDelayed(0.2f, () => player.Position = deathInfo.Position, "ExploadingGrenade.SetPos");
                        }
                    }
                }
                else
                {
                    RoundLogger.Log("MASS TK", "SKIP", "MTKD Skip Code: 8");
                }
                GreneadedDeadPlayers.Remove(userId);
                GreneadedPlayers.Remove(userId);
            }, "ExploadingGrenade");
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (!ev.IsAllowed)
            {
                RoundLogger.Log("MASS TK", "SKIP", "MTKD Skip Code: 10");
                return;
            }
            if (DeathInfo.ContainsKey(ev.Target))
                DeathInfo.Remove(ev.Target);
            DeathInfo.Add(ev.Target, (ev.Target.Role, ev.Target.Position));
            if (ev.HitInformation.GetDamageType() != DamageTypes.Grenade)
            {
                RoundLogger.Log("MASS TK", "SKIP", $"MTKD Skip Code: 11 | {ev.HitInformation.GetDamageType().name}");
                return;
            }
            if (ev.Target.Id == ev.Killer.Id && !GreneadedPlayers.Any(i => i.Value.Contains(ev.Target)))
            {
                RoundLogger.Log("MASS TK", "SKIP", "MTKD Skip Code: 12");
                return;
            }
            Player[] players;
            string userId;
            if (GreneadedPlayers.Any(i => i.Value.Contains(ev.Target)))
            {
                var tmp = GreneadedPlayers.First(i => i.Value.Contains(ev.Target));
                players = tmp.Value;
                userId = tmp.Key;
            }
            else
            {
                if (!GreneadedPlayers.TryGetValue(ev.Killer.UserId, out players))
                {
                    RoundLogger.Log("MASS TK", "SKIP", "MTKD Skip Code: 13");
                    Log.Debug($"Keys: {string.Join(", ", GreneadedPlayers.Keys)}", Debug);
                    Log.Debug($"Values: {string.Join(", ", GreneadedPlayers.Values.Select(i => i.Length))}", Debug);
                    return;
                }
                userId = ev.Killer.UserId;
            }
            if (!players.Contains(ev.Target))
            {
                RoundLogger.Log("MASS TK", "SKIP", "MTKD Skip Code: 14");
                foreach (var item in players)
                    Log.Debug($"- {item.Nickname}", Debug);
                return;
            }
            RoundLogger.Log("MASS TK", "EXECUTE", "MTKD Execute Code: 2");
            GreneadedDeadPlayers[userId]++;
        }
    }
}
