﻿using Exiled.API.Interfaces;
using Gamer.Diagnostics;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Enums;
using Respawning;
using Gamer.Utilities;
using MEC;
using Gamer.Mistaken.Base.Staff;

namespace Xname.CE
{
    /// <summary>
    /// SL's Combat Expansion.
    /// </summary>
    public class CEHandler : Module
    {
        internal readonly float unconsciousChance = 100f;
        internal readonly float maxBpmOnUpdate = 12f;
        internal readonly float maxBpm = 50f;
        internal readonly float WakeUpRate = 20f;
        internal static readonly Dictionary<int, (Vector3 Pos, RoleType Role, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType)> unconsciousPlayers = new Dictionary<int, (Vector3 Pos, RoleType Role, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType)>();
        internal static readonly Dictionary<int, float> playerBpm = new Dictionary<int, float>();
        internal readonly string reason = "Osoba wydaje się oddychać normalnie, ale jej tętno jest za niskie by była przytomna";
        /// <inheritdoc/>
        public CEHandler(IPlugin<IConfig> plugin) : base(plugin)
        {
        }
        /// <inheritdoc/>
        public override string Name => "CEHandler";
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Scp096.Enraging += this.Handle<Exiled.Events.EventArgs.EnragingEventArgs>((ev) => Scp096_Enraging(ev));
            Exiled.Events.Handlers.Scp096.AddingTarget += this.Handle<Exiled.Events.EventArgs.AddingTargetEventArgs>((ev) => Scp096_AddingTarget(ev));
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Scp096.Enraging -= this.Handle<Exiled.Events.EventArgs.EnragingEventArgs>((ev) => Scp096_Enraging(ev));
            Exiled.Events.Handlers.Scp096.AddingTarget -= this.Handle<Exiled.Events.EventArgs.AddingTargetEventArgs>((ev) => Scp096_AddingTarget(ev));
        }
        private void Server_RoundStarted()
        {
            unconsciousPlayers.Clear();
            playerBpm.Clear();
        }
        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.Target != null && !unconsciousPlayers.ContainsKey(ev.Target.Id))
            {
                if (ev.DamageType.isWeapon && ev.Amount > 25 && ev.Target.IsHuman)
                {
                    if (UnityEngine.Random.Range(0, 100) < unconsciousChance)
                    {
                        ev.IsAllowed = false;
                        playerBpm[ev.Target.Id] = UnityEngine.Random.Range(30f, maxBpm);
                        PutPlayerToSleep(ev.Target, ev.DamageType, reason, ev.Attacker.Id);
                        this.RunCoroutine(UpdateConsciousness(ev.Target), "UpdateConsciousness");
                    }
                }
            }
        }
        private void Scp096_Enraging(Exiled.Events.EventArgs.EnragingEventArgs ev)
        {
            if (ev.Player.IsInvisible && unconsciousPlayers.ContainsKey(ev.Player.Id))
                ev.IsAllowed = false;
        }
        private void Scp096_AddingTarget(Exiled.Events.EventArgs.AddingTargetEventArgs ev)
        {
            if (ev.Target.IsInvisible && unconsciousPlayers.ContainsKey(ev.Target.Id))
                ev.IsAllowed = false;
        }
        private IEnumerator<float> UpdateConsciousness(Player player)
        {
            if (unconsciousPlayers.TryGetValue(player.Id, out (Vector3 Pos, RoleType Role, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType) data))
            {
                while (playerBpm[player.Id] > 60)
                {
                    yield return Timing.WaitForSeconds(WakeUpRate);
                    playerBpm[player.Id] = playerBpm[player.Id] + UnityEngine.Random.Range(8f, maxBpmOnUpdate);
                }
                playerBpm.Remove(player.Id);
                WakeUpPlayer(player.Id, data.Pos, data.Role, data.Inventory, data.Ammo9, data.Ammo762, data.Ammo556, data.UnitIndex, data.UnitType);
            }
        }
        /// <summary>
        /// Makes player unconscious.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="type"></param>
        /// <param name="reason"></param>
        /// <param name="attackerid"></param>
        public static void PutPlayerToSleep(Player player, DamageTypes.DamageType type = null, string reason = "Samobójstwo", int attackerid = 0)
        {
            player.DisableAllEffects();
            unconsciousPlayers.Add(player.Id, (
                player.Position,
                player.Role,
                player.Inventory.items.ToArray(),
                player.Ammo[(int)AmmoType.Nato9],
                player.Ammo[(int)AmmoType.Nato556],
                player.Ammo[(int)AmmoType.Nato762],
                RespawnManager.Singleton.NamingManager.AllUnitNames.FindIndex(x => x.UnitName == player.ReferenceHub.characterClassManager.NetworkCurUnitName),
                player.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType));
            player.IsInvisible = true;
            player.Inventory.Clear();
            UnityEngine.Object.FindObjectOfType<RagdollManager>().SpawnRagdoll(
                player.Position, Quaternion.Euler(player.GameObject.transform.eulerAngles),
                (player.ReferenceHub.playerMovementSync == null) ? Vector3.zero : player.ReferenceHub.playerMovementSync.PlayerVelocity,
                (int)player.ReferenceHub.characterClassManager.CurClass,
                new PlayerStats.HitInfo(0f, $"*{reason}", type ?? DamageTypes.Wall, (attackerid == 0) ? player.Id : attackerid),
                (player.ReferenceHub.characterClassManager.CurRole.team > Team.SCP),
                player.GameObject.GetComponent<Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer>().PlayerId,
                player.Nickname,
                player.Id);
        }
        /// <summary>
        /// Makes player conscious.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="pos"></param>
        /// <param name="role"></param>
        /// <param name="inv"></param>
        /// <param name="ammo9mm"></param>
        /// <param name="ammo7mm"></param>
        /// <param name="ammo5mm"></param>
        /// <param name="unitIndex"></param>
        /// <param name="unitType"></param>
        public static void WakeUpPlayer(int playerId, Vector3 pos, RoleType role, Inventory.SyncItemInfo[] inv, uint ammo9mm, uint ammo7mm, uint ammo5mm, int unitIndex, byte unitType)
        {
            unconsciousPlayers.Remove(playerId);
            Player p = RealPlayers.Get(playerId);
            var old = Respawning.RespawnManager.CurrentSequence();
            Respawning.RespawnManager.Singleton._curSequence = RespawnManager.RespawnSequencePhase.SpawningSelectedTeam;
            p.Role = role;
            p.Inventory.Clear();
            Respawning.RespawnManager.Singleton._curSequence = old;
            foreach (var item in inv)
            {
                p.Inventory.items.Add(item);
            }
            p.Ammo[(int)AmmoType.Nato9] = ammo9mm;
            p.Ammo[(int)AmmoType.Nato556] = ammo5mm;
            p.Ammo[(int)AmmoType.Nato762] = ammo7mm;
            p.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType = unitType;
            if (Respawning.RespawnManager.Singleton.NamingManager.TryGetAllNamesFromGroup(unitType, out var array))
                p.UnitName = array[unitIndex];
            p.EnableEffect<CustomPlayerEffects.Blinded>(10f);
            p.EnableEffect<CustomPlayerEffects.Concussed>(20f);
            p.EnableEffect<CustomPlayerEffects.Deafened>(20f);
            p.EnableEffect<CustomPlayerEffects.Exhausted>(30f);
            p.Health = UnityEngine.Random.Range(20f, 35f);
            Timing.CallDelayed(0.5f, () => p.Position = pos);
            p.IsInvisible = false;
        }
        /// <summary>
        /// Checks if player is unconscious.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>true if player is unconscious</returns>
        public static bool IsUnconscious(Player player)
        {
            return unconsciousPlayers.ContainsKey(player.Id);
        }
    }
}
