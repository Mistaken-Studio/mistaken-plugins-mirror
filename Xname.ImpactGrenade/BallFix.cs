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
    /// Fix for ball that's not getting deleted/distabled.
    /// </summary>
    public class BallFix : Module
    {
        /// <inheritdoc/>
        public BallFix(IPlugin<IConfig> plugin) : base(plugin)
        {

        }
        /// <inheritdoc/>
        public override string Name => "BallFix";
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Player.ThrowingGrenade += this.Handle<Exiled.Events.EventArgs.ThrowingGrenadeEventArgs>((ev) => Player_ThrowingGrenade(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Player.ThrowingGrenade -= this.Handle<Exiled.Events.EventArgs.ThrowingGrenadeEventArgs>((ev) => Player_ThrowingGrenade(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        private void Player_ThrowingGrenade(ThrowingGrenadeEventArgs ev)
        {
            if (ev.Type == Exiled.API.Enums.GrenadeType.Scp018)
            {
                MEC.Timing.CallDelayed(1f, () =>
                {
                    if (ev.Player.GetEffectActive<CustomPlayerEffects.Scp268>())
                        ev.Player.DisableEffect<CustomPlayerEffects.Scp268>();
                    RoundLogger.Log("SCP 018 FIX", "THROW", $"{ev.Player.PlayerToString()} threw SCP-018");
                    Grenade grenade = UnityEngine.Object.Instantiate(ev.Player.GrenadeManager.availableGrenades[2].grenadeInstance).GetComponent<Grenade>();
                    grenade.fuseDuration = 900f;
                    grenade.InitData(ev.Player.GrenadeManager, Vector3.zero, ev.Player.CameraTransform.forward, ev.IsSlow ? 0.5f : 1f);
                    Mirror.NetworkServer.Spawn(grenade.gameObject);
                    ev.Player.RemoveItem(ev.Player.CurrentItem);
                    grenade.gameObject.AddComponent<Scp018Fix>();
                });
                ev.IsAllowed = false;
            }
        }
        /// <inheritdoc/>
        public static HashSet<GameObject> explodedBalls = new HashSet<GameObject>();
        private void Map_ExplodingGrenade(ExplodingGrenadeEventArgs ev)
        {
            if (ev.Grenade.TryGetComponent<Scp018Grenade>(out Scp018Grenade ball))
                explodedBalls.Add(ev.Grenade);
        }
        private void Server_RoundStarted()
        {
            explodedBalls.Clear();
        }
    }
    /// <summary>
    /// Collision after ball explosion handling.
    /// </summary>
    public class Scp018Fix : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (this == null)
                RoundLogger.Log("SCP 018 FIX", "COLLISION", "Ball is null :/");
            foreach (var p in Gamer.Utilities.RealPlayers.List.Where(x => x.IsActiveDev()))
            {
                p.SendConsoleMessage($"{this?.name}, {collision?.gameObject}", "grey");
            }
            if (BallFix.explodedBalls.Contains(this?.gameObject))
            {
                foreach (var p in Gamer.Utilities.RealPlayers.List.Where(x => x.IsActiveDev()))
                {
                    p.SendConsoleMessage($"works", "green");
                }
                RoundLogger.Log("SCP 018 FIX", "DESTROY", "Tried to destroy a ball, but did it?");
                Mirror.NetworkServer.Destroy(this?.gameObject);
            }
        }
    }
}
