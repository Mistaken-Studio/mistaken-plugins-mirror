using Exiled.API.Interfaces;
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

namespace Xname.CE
{
    /// <inheritdoc/>
    public class CEHandler : Module
    {
        internal readonly float unconsciousChance = 100f;
        internal readonly float maxWakeUpRate = 15f;
        internal readonly float maxBpm = 150f;
        public static readonly Dictionary<int, (Vector3 Pos, RoleType Role, Inventory.SyncItemInfo[] Inventory, float Bpm, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType)> unconsciousPlayers = new Dictionary<int, (Vector3 Pos, RoleType Role, Inventory.SyncItemInfo[] Inventory, float Bpm, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType)>();
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
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        private void Server_RoundStarted()
        {
            unconsciousPlayers.Clear();
        }
        private IEnumerator<float> UpdateConsciousness(Player player)
        {
            if (unconsciousPlayers.TryGetValue(player.Id, out (Vector3 Pos, RoleType Role, Inventory.SyncItemInfo[] Inventory, float Bpm, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType) data))
            {
                while (data.Bpm > 80)
                {
                    yield return Timing.WaitForSeconds(20f);
                    data.Bpm = data.Bpm - UnityEngine.Random.Range(10, maxWakeUpRate);
                }
                WakeUpPlayer(player.Id, data.Pos, data.Role, data.Inventory, data.Ammo9, data.Ammo762, data.Ammo556, data.UnitIndex, data.UnitType);
            }
        }
        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.Target != null)
            {
                //if (ev.DamageType.isWeapon && ev.Amount > 25 && ev.Target.IsHuman)
                //{
                    if (UnityEngine.Random.Range(0, 100) < unconsciousChance)
                    {
                        ev.IsAllowed = false;
                        unconsciousPlayers.Add(ev.Target.Id, (
                            ev.Target.Position,
                            ev.Target.Role,
                            ev.Target.Inventory.items.ToArray(),
                            UnityEngine.Random.Range(120f, maxBpm),
                            ev.Target.Ammo[(int)AmmoType.Nato9],
                            ev.Target.Ammo[(int)AmmoType.Nato556],
                            ev.Target.Ammo[(int)AmmoType.Nato762],
                            RespawnManager.Singleton.NamingManager.AllUnitNames.FindIndex(x => x.UnitName == ev.Target.ReferenceHub.characterClassManager.NetworkCurUnitName),
                            ev.Target.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType
                            )
                        );
                        ev.Target.IsInvisible = true;
                        ev.Target.Inventory.Clear();
                        UnityEngine.Object.FindObjectOfType<RagdollManager>().SpawnRagdoll(
                            ev.Target.Position, Quaternion.Euler(ev.Target.GameObject.transform.eulerAngles),
                            (ev.Target.ReferenceHub.playerMovementSync == null) ? Vector3.zero : ev.Target.ReferenceHub.playerMovementSync.PlayerVelocity,
                            (int)ev.Target.ReferenceHub.characterClassManager.CurClass,
                            new PlayerStats.HitInfo(ev.Amount, $"*{reason}", ev.DamageType, ev.Attacker.Id),
                            (ev.Target.ReferenceHub.characterClassManager.CurRole.team > Team.SCP),
                            ev.Target.GameObject.GetComponent<Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer>().PlayerId,
                            ev.Target.Nickname,
                            ev.Target.Id);
                        this.RunCoroutine(UpdateConsciousness(ev.Target), "UpdateConsciousness");
                    }
                //}
            }
        }
        private void WakeUpPlayer(int playerId, Vector3 pos, RoleType role, Inventory.SyncItemInfo[] inv, uint ammo9mm, uint ammo7mm, uint ammo5mm, int unitIndex, byte unitType)
        {
            Player p = RealPlayers.Get(playerId);
            p.Role = role;
            foreach (var item in inv)
            {
                p.Inventory.items.Add(item);
            }
            p.Ammo[(int)AmmoType.Nato9] = ammo9mm;
            p.Ammo[(int)AmmoType.Nato556] = ammo5mm;
            p.Ammo[(int)AmmoType.Nato762] = ammo7mm;
            var old = Respawning.RespawnManager.CurrentSequence();
            Respawning.RespawnManager.Singleton._curSequence = RespawnManager.RespawnSequencePhase.SpawningSelectedTeam;
            p.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType = unitType;
            if (Respawning.RespawnManager.Singleton.NamingManager.TryGetAllNamesFromGroup(unitType, out var array))
                p.UnitName = array[unitIndex];
            Respawning.RespawnManager.Singleton._curSequence = old;
            p.Position = pos;
            unconsciousPlayers.Remove(playerId);
        }
    }
}
