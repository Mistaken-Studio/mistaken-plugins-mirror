using Exiled.API.Interfaces;
using Exiled.Events.EventArgs;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.Staff;
using Gamer.RoundLoggerSystem;
using Grenades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Xname.ImpactGrenade
{
    /// <summary>
    /// Flash grenade exploding on impact.
    /// </summary>
    public class FlashImpHandler : Module
    {
        /// <inheritdoc/>
        public FlashImpHandler(IPlugin<IConfig> plugin) : base(plugin)
        {
        }
        /// <inheritdoc/>
        public override string Name => "FlashImpHandler";
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ThrowingGrenade += this.Handle<Exiled.Events.EventArgs.ThrowingGrenadeEventArgs>((ev) => Player_ThrowingGrenade(ev));
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.ThrowingGrenade -= this.Handle<Exiled.Events.EventArgs.ThrowingGrenadeEventArgs>((ev) => Player_ThrowingGrenade(ev));
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        private void Player_ThrowingGrenade(ThrowingGrenadeEventArgs ev)
        {
            if (ev.Type == Exiled.API.Enums.GrenadeType.Flashbang && ev.Player.CurrentItem.durability != 1.149f)
            {
                MEC.Timing.CallDelayed(1f, () =>
                {
                    if (ev.Player.GetEffectActive<CustomPlayerEffects.Scp268>())
                        ev.Player.DisableEffect<CustomPlayerEffects.Scp268>();
                    RoundLogger.Log("IMPACT FLASH GRENADE", "THROW", $"{ev.Player.PlayerToString()} threw an impact flash grenade");
                    Grenade grenade = UnityEngine.Object.Instantiate(ev.Player.GrenadeManager.availableGrenades[1].grenadeInstance).GetComponent<Grenade>();
                    grenade.fuseDuration = 999;
                    grenade.InitData(ev.Player.GrenadeManager, Vector3.zero, ev.Player.CameraTransform.forward, ev.IsSlow ? 0.5f : 1f);
                    Mirror.NetworkServer.Spawn(grenade.gameObject);
                    grenade.GetComponent<Rigidbody>().AddForce(new Vector3(grenade.NetworkserverVelocities.linear.x * 1.5f, grenade.NetworkserverVelocities.linear.y / 2f, grenade.NetworkserverVelocities.linear.z * 1.5f), ForceMode.VelocityChange);
                    ev.Player.RemoveItem(ev.Player.CurrentItem);
                    grenade.gameObject.AddComponent<ImpComponent>();
                });
                ev.IsAllowed = false;
            }
            else if (ev.Type == Exiled.API.Enums.GrenadeType.Scp018)
            {
                MEC.Timing.CallDelayed(1f, () =>
                {
                    if (ev.Player.GetEffectActive<CustomPlayerEffects.Scp268>())
                        ev.Player.DisableEffect<CustomPlayerEffects.Scp268>();
                    RoundLogger.Log("SCP 018 FIX", "THROW", $"{ev.Player.PlayerToString()} threw SCP-018");
                    Grenade grenade = UnityEngine.Object.Instantiate(ev.Player.GrenadeManager.availableGrenades[2].grenadeInstance).GetComponent<Grenade>();
                    grenade.fuseDuration = 3f;
                    grenade.InitData(ev.Player.GrenadeManager, Vector3.zero, ev.Player.CameraTransform.forward, ev.IsSlow ? 0.5f : 1f);
                    Mirror.NetworkServer.Spawn(grenade.gameObject);
                    ev.Player.RemoveItem(ev.Player.CurrentItem);
                    grenade.gameObject.AddComponent<Scp018Fix>();
                });
                ev.IsAllowed = false;
            }
        }
        private Dictionary<GameObject, bool> explodedBalls;
        private void Map_ExplodingGrenade(ExplodingGrenadeEventArgs ev)
        {
            if (ev.Grenade.TryGetComponent<Scp018Grenade>(out Scp018Grenade ball))
                explodedBalls[ev.Grenade] = true;
        }
        private void Server_RoundStarted()
        {
            explodedBalls.Clear();
        }
    }
    /// <summary>
    /// Fix for ball not getting disabled :/.
    /// </summary>
    public class Scp018Fix : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            foreach (var p in Gamer.Utilities.RealPlayers.List.Where(x => x.IsActiveDev()))
            {
                p.SendConsoleMessage($"{collision?.collider.name}, {collision?.gameObject}, {collision?.rigidbody}", "grey");
            }
        }
    }
}
