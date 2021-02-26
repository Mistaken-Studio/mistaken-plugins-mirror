using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Diagnostics;
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

        public readonly static Dictionary<Player, Player[]> GreneadedPlayers = new Dictionary<Player, Player[]>();
        public readonly static Dictionary<Player, int> GreneadedDeadPlayers = new Dictionary<Player, int>();
        public readonly static Dictionary<Player, (RoleType, Vector3)> DeathInfo = new Dictionary<Player, (RoleType, Vector3)>();
        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (Server.Host == ev.Thrower)
                return;
            if (ev.Thrower == null)
            {
                //Log.Debug("MTKD Skip Code: 1");
                return;
            }
            if (!ev.IsAllowed)
            {
                //Log.Debug("MTKD Skip Code: 2");
                return;
            }
            if (!ev.IsFrag)
            {
                //Log.Debug("MTKD Skip Code: 3");
                return;
            }
            if (ev.TargetToDamages.Count == 0 || (ev.TargetToDamages.Count == 1 && ev.TargetToDamages.ContainsKey(ev.Thrower)))
            {
                //Log.Debug("MTKD Skip Code: 4");
                return;
            }
            if (ev.TargetToDamages.Any(i => i.Key.Side != ev.Thrower.Side || (i.Key.Role == RoleType.ClassD && ev.Thrower.Role == RoleType.ClassD)))
            {
                //Log.Debug("MTKD Skip Code: 5");
                return;
            }
            if (GreneadedPlayers.ContainsKey(ev.Thrower))
            {
                //Log.Debug("MTKD Skip Code: 6");
                return;
            }
            //Log.Debug("MTKD Execute Code: 1");
            GreneadedPlayers.Add(ev.Thrower, ev.TargetToDamages.Keys.ToArray());
            GreneadedDeadPlayers.Add(ev.Thrower, 0);
            MEC.Timing.CallDelayed(5, () =>
            {
                var value = GreneadedDeadPlayers[ev.Thrower];
                Log.Info($"Detected Greneade TK, {value} killed");
                if(value > 3)
                {
                    //Log.Debug("MTKD Execute Code: 3");
                    MapPlus.Broadcast("MassTK Detection System", 10, $"Detected Grenade Mass TeamKill ({value}) by {ev.Thrower.Nickname}\nRespawning team killed players", Broadcast.BroadcastFlags.AdminChat);
                    MapPlus.Broadcast("MassTK Detection System", 10, "Wykryto MassTK, respienie graczy ...", Broadcast.BroadcastFlags.Normal);
                    foreach (var player in GreneadedPlayers[ev.Thrower])
                    {
                        if (player == null || player.IsAlive)
                        {
                            //Log.Debug("MTKD Skip Code: 7");
                            return;
                        }
                        Log.Debug($"MTKD Spawning {player.Nickname}");
                        if (DeathInfo.TryGetValue(player, out (RoleType Role, Vector3 Position) deathInfo))
                        {
                            player.Role = deathInfo.Role;
                            MEC.Timing.CallDelayed(0.2f, () => player.Position = deathInfo.Position);
                        }
                    }
                }
                else
                {
                    //Log.Debug("MTKD Skip Code: 8");
                }
                GreneadedDeadPlayers.Remove(ev.Thrower);
                GreneadedPlayers.Remove(ev.Thrower);
            });
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (!ev.IsAllowed)
            {
                //Log.Debug("MTKD Skip Code: 10");
                return;
            }
            if (DeathInfo.ContainsKey(ev.Target))
                DeathInfo.Remove(ev.Target);
            DeathInfo.Add(ev.Target, (ev.Target.Role, ev.Target.Position));
            if (ev.HitInformation.GetDamageType() != DamageTypes.Grenade)
            {
                //Log.Debug($"MTKD Skip Code: 11 | {ev.HitInformation.GetDamageName()}");
                return;
            }
            if (ev.Target.Id == ev.Killer.Id)
            {
                //Log.Debug("MTKD Skip Code: 12");
                return;
            }
            if (!GreneadedPlayers.TryGetValue(ev.Killer, out Player[] players))
            {
                //Log.Debug("MTKD Skip Code: 13");
                //Log.Debug($"Keys: {string.Join(", ", GreneadedPlayers.Keys)}");
                //Log.Debug($"Values: {string.Join(", ", GreneadedPlayers.Values.Select(i => i.Length))}");
                return;
            }
            if (!players.Contains(ev.Target))
            {
                //Log.Debug("MTKD Skip Code: 14");
                foreach (var item in players)
                {
                    Log.Debug($"- {item.Nickname}");
                }
                return;
            }
            //Log.Debug("MTKD Execute Code: 2");
            GreneadedDeadPlayers[ev.Killer]++;
        }
    }
}
