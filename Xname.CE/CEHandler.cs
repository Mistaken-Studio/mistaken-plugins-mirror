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
using Gamer.Mistaken.Base.Staff;
using Gamer.Mistaken.Base.GUI;

namespace Xname.CE
{
    /// <summary>
    /// SL's Combat Expansion.
    /// </summary>
    public class CEHandler : Module
    {
        internal static readonly Dictionary<int, (Vector3 Pos, RoleType Role, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType)> unconsciousPlayers = new Dictionary<int, (Vector3 Pos, RoleType Role, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType)>();
        internal static readonly Dictionary<int, float> playerBpm = new Dictionary<int, float>();
        internal static readonly Dictionary<Player, Ragdoll> ragdolls = new Dictionary<Player, Ragdoll>();
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
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.Shooting += this.Handle<Exiled.Events.EventArgs.ShootingEventArgs>((ev) => Player_Shooting(ev));
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Scp096.Enraging -= this.Handle<Exiled.Events.EventArgs.EnragingEventArgs>((ev) => Scp096_Enraging(ev));
            Exiled.Events.Handlers.Scp096.AddingTarget -= this.Handle<Exiled.Events.EventArgs.AddingTargetEventArgs>((ev) => Scp096_AddingTarget(ev));
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.Shooting -= this.Handle<Exiled.Events.EventArgs.ShootingEventArgs>((ev) => Player_Shooting(ev));
        }
        private void Server_RoundStarted()
        {
            unconsciousPlayers.Clear();
            playerBpm.Clear();
            ragdolls.Clear();
        }
        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.Target != null && !unconsciousPlayers.ContainsKey(ev.Target.Id))
            {
                if (ev.DamageType.isWeapon && ev.Amount > 25 && ev.Target.IsHuman)
                {
                    if (UnityEngine.Random.Range(0, 100) < PluginHandler.Config.unconsciousChance)
                    {
                        ev.IsAllowed = false;
                        playerBpm[ev.Target.Id] = UnityEngine.Random.Range(PluginHandler.Config.minBpmOnShot, PluginHandler.Config.maxBpmOnShot);
                        Sleep(ev.Target, ev.DamageType, PluginHandler.Config.reasonOnUnconsciousRagdoll, ev.Attacker);
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
        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (ev.Player.IsInvisible && unconsciousPlayers.ContainsKey(ev.Player.Id))
                ev.IsAllowed = false;
        }
        private void Player_Shooting(Exiled.Events.EventArgs.ShootingEventArgs ev)
        {
            foreach (var component in ragdolls)
            {
                if (Vector3.Distance(ev.Position, component.Value.transform.position) <= 1)
                {
                    Role role = component.Key.ReferenceHub.characterClassManager.Classes.SafeGet((int)component.Key.ReferenceHub.characterClassManager.CurClass);
                    component.Value.Networkowner = new Ragdoll.Info(component.Key.GameObject.GetComponent<Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer>().PlayerId, component.Key.Nickname, new PlayerStats.HitInfo(0f, $"*{PluginHandler.Config.reasonOnDeadRagdoll}", DamageTypes.Wall, ev.Shooter.Id), role, component.Key.Id);
                    ragdolls.Remove(component.Key);
                }
            }
        }
        private IEnumerator<float> UpdateConsciousness(Player player)
        {
            if (unconsciousPlayers.TryGetValue(player.Id, out (Vector3 Pos, RoleType Role, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType) data))
            {
                while (playerBpm[player.Id] < PluginHandler.Config.normalBpm)
                {
                    yield return Timing.WaitForSeconds(PluginHandler.Config.wakeUpRate);
                    playerBpm[player.Id] += UnityEngine.Random.Range(PluginHandler.Config.minBpmOnUpdate, PluginHandler.Config.maxBpmOnUpdate);
                }
                playerBpm.Remove(player.Id);
                WakeUp(player);
            }
        }
        /// <summary>
        /// Makes player unconscious.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="type"></param>
        /// <param name="reason"></param>
        /// <param name="attacker"></param>
        internal static void Sleep(Player player, DamageTypes.DamageType type, string reason, Player attacker)
        {
            player.DisableAllEffects();
            if (PluginHandler.Config.unconsciousEnsnare)
                player.EnableEffect<CustomPlayerEffects.Ensnared>(float.MaxValue);
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
            Role role = player.ReferenceHub.characterClassManager.Classes.SafeGet((int)player.ReferenceHub.characterClassManager.CurClass);
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(role.model_ragdoll, player.Position + role.ragdoll_offset.position, Quaternion.Euler(player.GameObject.transform.eulerAngles + role.ragdoll_offset.rotation));
            Mirror.NetworkServer.Spawn(gameObject);
            Ragdoll component = gameObject.GetComponent<Ragdoll>();
            component.Networkowner = new Ragdoll.Info(player.GameObject.GetComponent<Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer>().PlayerId, player.Nickname, new PlayerStats.HitInfo(0f, $"*{reason}", type ?? DamageTypes.Wall, (attacker.Id == 0) ? player.Id : attacker.Id), role, player.Id);
            component.NetworkallowRecall = (player.ReferenceHub.characterClassManager.CurRole.team > Team.SCP);
            component.NetworkPlayerVelo = (player.ReferenceHub.playerMovementSync == null) ? Vector3.zero : player.ReferenceHub.playerMovementSync.PlayerVelocity;
            ragdolls.Add(player, component);
            player.SetGUI("unconciousness", Gamer.Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, $"Jesteś <color=yellow>nieprzytomny</color><br><mspace=0.5em><color=yellow>Nie możesz się ruszać. Wybudzisz się wkrótce</color></mspace>");
        }
        /// <summary>
        /// Makes player conscious.
        /// </summary>
        /// <param name="player"></param>
        internal static void WakeUp(Player player)
        {
            if (unconsciousPlayers.TryGetValue(player.Id, out (Vector3 Pos, RoleType Role, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType) data))
            {
                var old = Respawning.RespawnManager.CurrentSequence();
                Respawning.RespawnManager.Singleton._curSequence = RespawnManager.RespawnSequencePhase.SpawningSelectedTeam;
                player.Role = data.Role;
                Respawning.RespawnManager.Singleton._curSequence = old;
                player.Inventory.Clear();
                foreach (var item in data.Inventory)
                {
                    player.Inventory.items.Add(item);
                }
                player.Ammo[(int)AmmoType.Nato9] = data.Ammo9;
                player.Ammo[(int)AmmoType.Nato556] = data.Ammo556;
                player.Ammo[(int)AmmoType.Nato762] = data.Ammo762;
                player.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType = data.UnitType;
                if (Respawning.RespawnManager.Singleton.NamingManager.TryGetAllNamesFromGroup(data.UnitType, out var array))
                    player.UnitName = array[data.UnitIndex];
                player.EnableEffect<CustomPlayerEffects.Blinded>(PluginHandler.Config.blindedTime);
                player.EnableEffect<CustomPlayerEffects.Concussed>(PluginHandler.Config.concussedTime);
                player.EnableEffect<CustomPlayerEffects.Deafened>(PluginHandler.Config.deafenedTime);
                player.EnableEffect<CustomPlayerEffects.Exhausted>(PluginHandler.Config.exhaustedTime);
                player.EnableEffect<CustomPlayerEffects.Amnesia>(PluginHandler.Config.amnesiaTime);
                player.Health = UnityEngine.Random.Range(PluginHandler.Config.minHpOnWakeUp, PluginHandler.Config.maxHpOnWakeUp);
                Timing.CallDelayed(0.5f, () => player.Position = data.Pos);
                player.IsInvisible = false;
                if (ragdolls.TryGetValue(player, out Ragdoll component))
                {
                    Mirror.NetworkServer.Destroy(component.gameObject);
                    ragdolls.Remove(player);
                }
                if (player.GetEffectActive<CustomPlayerEffects.Ensnared>())
                    player.DisableEffect<CustomPlayerEffects.Ensnared>();
                player.SetGUI("unconciousness", Gamer.Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, null);
                unconsciousPlayers.Remove(player.Id);
            }
        }
        /// <summary>
        /// Checks if player is unconscious.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>true if player is unconscious</returns>
        internal static bool Unconscious(Player player)
        {
            return unconsciousPlayers.ContainsKey(player.Id);
        }
    }
    /// <summary>
    /// Extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Makes player unconscious.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="damagetype"></param>
        /// <param name="reason"></param>
        /// <param name="attacker"></param>
        public static void PutPlayerToSleep(this Player player, DamageTypes.DamageType damagetype = null, string reason = "Nieudana próba samobójcza, wciąż żyje", Player attacker = null) =>
            CEHandler.Sleep(player, damagetype, reason, attacker);
        /// <summary>
        /// Makes player conscious if he's unconscious.
        /// </summary>
        /// <param name="player"></param>
        public static void WakeUpPlayer(this Player player) =>
            CEHandler.WakeUp(player);
        /// <summary>
        /// Checks if player is unconscious.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>true if player is unconscious</returns>
        public static bool IsUnconscious(this Player player) =>
            CEHandler.Unconscious(player);
    }
}
