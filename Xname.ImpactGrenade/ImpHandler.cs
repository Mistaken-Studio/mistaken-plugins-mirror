#pragma warning disable IDE0079
#pragma warning disable IDE0060
#pragma warning disable IDE0051

using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Gamer.API.CustomItem;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Mistaken.Base.Staff;
using Gamer.RoundLoggerSystem;
using Grenades;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xname.ImpactGrenade
{
    /// <inheritdoc/>
    public class ImpHandler : Module
    {
        internal static readonly Vector3 Size = new Vector3(1f, .40f, 1f);
        internal static readonly float Damage_multiplayer = 0.14f;
        internal static HashSet<GameObject> grenades = new HashSet<GameObject>();
        /// <summary>
        /// Grenade that explodes on impact.
        /// </summary>
        public class ImpItem : CustomItem
        {
            /// <inheritdoc/>
            public ImpItem() => base.Register();
            /// <inheritdoc/>
            public override string ItemName => "Impact Grenade";
            /// <inheritdoc/>
            public override ItemType Item => ItemType.GrenadeFrag;
            /// <inheritdoc/>
            public override int Durability => 001;
            /// <inheritdoc/>
            public override Vector3 Size => ImpHandler.Size;
            /// <inheritdoc/>
            public override Upgrade[] Upgrades => new Upgrade[]
            {
                new Upgrade
                {
                    Chance = 100,
                    Durability = null,
                    Input = ItemType.GrenadeFrag,
                    KnobSetting = Scp914.Scp914Knob.Fine
                }
            };
            /// <inheritdoc/>
            public override bool OnThrow(Player player, Inventory.SyncItemInfo item, bool slow)
            {
                MEC.Timing.CallDelayed(1f, () =>
                {
                    RoundLogger.Log("IMPACT GRENADE", "THROW", $"{player.PlayerToString()} threw an impact grenade");
                    if (player.GetEffectActive<CustomPlayerEffects.Scp268>())
                        player.DisableEffect<CustomPlayerEffects.Scp268>();
                    Grenade grenade = UnityEngine.Object.Instantiate(player.GrenadeManager.availableGrenades[0].grenadeInstance).GetComponent<Grenade>();
                    grenade.fuseDuration = 999;
                    grenade.InitData(player.GrenadeManager, Vector3.zero, player.CameraTransform.forward, slow ? 0.5f : 1f);
                    grenades.Add(grenade.gameObject);
                    Mirror.NetworkServer.Spawn(grenade.gameObject);
                    grenade.GetComponent<Rigidbody>().AddForce(new Vector3(grenade.NetworkserverVelocities.linear.x * 1.5f, grenade.NetworkserverVelocities.linear.y / 2f, grenade.NetworkserverVelocities.linear.z * 1.5f), ForceMode.VelocityChange);
                    player.RemoveItem(item);
                    grenade.gameObject.AddComponent<ImpComponent>();
                    OnStopHolding(player, item);
                });
                return false;
            }
            /// <inheritdoc/>
            public override void OnStartHolding(Player player, Inventory.SyncItemInfo item)
            {
                player.SetGUI("impact", Gamer.Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, "Trzymasz <color=yellow>Granat Uderzeniowy</color>");
            }
            /// <inheritdoc/>
            public override void OnStopHolding(Player player, Inventory.SyncItemInfo item)
            {
                player.SetGUI("impact", Gamer.Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
            }
            /// <inheritdoc/>
            public override void OnForceclass(Player player)
            {
                player.SetGUI("impact", Gamer.Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
            }
            /// <summary>
            /// Gives Impact Grenade to <paramref name="player"/>
            /// </summary>
            /// <param name="player">Player that Impact Grenade should be given to</param>
            public static void Give(Player player)
            {
                player.AddItem(new Inventory.SyncItemInfo
                {
                    durability = 1.001f,
                    id = ItemType.GrenadeFrag,
                });
            }
        }
        /// <inheritdoc/>
        public ImpHandler(IPlugin<IConfig> plugin) : base(plugin)
        {
            new ImpItem();
        }
        /// <inheritdoc/>
        public override string Name => "ImpHandler";
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Map.ChangingIntoGrenade += this.Handle<Exiled.Events.EventArgs.ChangingIntoGrenadeEventArgs>((ev) => Map_ChangingIntoGrenade(ev));
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Map.ChangingIntoGrenade -= this.Handle<Exiled.Events.EventArgs.ChangingIntoGrenadeEventArgs>((ev) => Map_ChangingIntoGrenade(ev));
        }
        private GrenadeManager lastImpactThrower;
        private HashSet<GameObject> ballgo;
        private IEnumerator<float> Checkball()
        {
            if (ballgo.Count > 0)
            {
                foreach (var go in ballgo)
                {
                    if (go != null)
                    {
                        foreach (var p in Gamer.Utilities.RealPlayers.List.Where(x => x.IsActiveDev()))
                        {
                            p.SendConsoleMessage($"{go.activeSelf}, {go.transform.position}", "green");
                        }
                        if (go.TryGetComponent<Scp018Grenade>(out Scp018Grenade ball))
                        {
                            ball.enabled = false;
                            NetworkServer.Destroy(ball.gameObject);
                            NetworkServer.Destroy(go);
                        }
                    }
                }
            }
            yield return MEC.Timing.WaitForSeconds(1f);
        }
        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (ev.Grenade.TryGetComponent<Scp018Grenade>(out Scp018Grenade ball))
            {
                ballgo.Add(ev.Grenade);
                foreach (var p in Gamer.Utilities.RealPlayers.List.Where(x => x.IsActiveDev()))
                {
                    p.SendConsoleMessage($"{ev.Grenade.name}, {ball.NetworkfuseTime}", "grey");
                }
            }
            else
            {
                foreach (var p in Gamer.Utilities.RealPlayers.List.Where(x => x.IsActiveDev()))
                {
                    p.SendConsoleMessage($"{ev.Grenade.name}", "green");
                }
            }
            if (!grenades.Contains(ev.Grenade))
                return;
            RoundLogger.Log("IMPACT GRENADE", "EXPLODED", $"Impact grenade exploded");
            var tmp = (ev.Grenade.GetComponent<FragGrenade>()).thrower;
            lastImpactThrower = tmp;
            MEC.Timing.CallDelayed(1, () =>
            {
                if (lastImpactThrower == tmp)
                    lastImpactThrower = null;
            });
            foreach (Player player in ev.TargetToDamages.Keys.ToArray())
            {
                ev.TargetToDamages[player] *= Damage_multiplayer;
                RoundLogger.Log("IMPACT GRENADE", "HURT", $"{player.PlayerToString()} was hurt by an impact grenade");
            }
        }
        private void Server_RoundStarted()
        {
            ballgo.Clear();
            grenades.Clear();
            MEC.Timing.RunCoroutine(Checkball());
            var lockers = LockerManager.singleton.lockers.Where(i => i.chambers.Length == 9).ToArray();
            int toSpawn = 8;
            while (toSpawn > 0)
            {
                var locker = lockers[UnityEngine.Random.Range(0, lockers.Length)];
                locker.AssignPickup(ItemType.GrenadeFrag.Spawn(1.001f, locker.chambers[UnityEngine.Random.Range(0, locker.chambers.Length)].spawnpoint.position));
                RoundLogger.Log("IMPACT GRENADE", "SPAWN", $"Impact grenade spawned");
                toSpawn--;
            }
        }
        private void Map_ChangingIntoGrenade(Exiled.Events.EventArgs.ChangingIntoGrenadeEventArgs ev)
        {
            if (ev.Pickup.durability == 1.001f)
            {
                ev.IsAllowed = false;
                Grenade grenade = UnityEngine.Object.Instantiate(Server.Host.GrenadeManager.availableGrenades[0].grenadeInstance).GetComponent<Grenade>();
                grenades.Add(grenade.gameObject);
                grenade.fuseDuration = 0.01f;
                grenade.InitData(lastImpactThrower ?? Server.Host.GrenadeManager, Vector3.zero, Vector3.zero, 0f);
                grenade.transform.position = ev.Pickup.position;
                Mirror.NetworkServer.Spawn(grenade.gameObject);
                ev.Pickup.Delete();
                RoundLogger.Log("IMPACT GRENADE", "CHAINED", $"Impact grenade chained");
            }
        }
    }
    /// <summary>
    /// Handles explosion on impact.
    /// </summary>
    public class ImpComponent : MonoBehaviour
    {
        private bool used;
        private void OnCollisionEnter(Collision collision)
        {
            if (!used && TryGetComponent<FragGrenade>(out FragGrenade frag))
                frag.NetworkfuseTime = 0.01f;
            else if (!used && TryGetComponent<FlashGrenade>(out FlashGrenade flash))
                flash.NetworkfuseTime = 0.01f;
            used = true;
        }
    }
}
