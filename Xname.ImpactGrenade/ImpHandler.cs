using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Interfaces;
using Gamer.Diagnostics;
using Gamer.API.CustomItem;
using UnityEngine;
using Exiled.API.Features;
using Grenades;
using Exiled.API.Extensions;
using Gamer.RoundLoggerSystem;

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
                Gamer.Mistaken.Systems.GUI.PseudoGUIHandler.Set(player, "impact", Gamer.Mistaken.Systems.GUI.PseudoGUIHandler.Position.BOTTOM, "Trzymasz <color=yellow>Granat Uderzeniowy</color>");
            }
            /// <inheritdoc/>
            public override void OnStopHolding(Player player, Inventory.SyncItemInfo item)
            {
                Gamer.Mistaken.Systems.GUI.PseudoGUIHandler.Set(player, "impact", Gamer.Mistaken.Systems.GUI.PseudoGUIHandler.Position.BOTTOM, null);
            }
            /// <inheritdoc/>
            public override void OnForceclass(Player player)
            {
                Gamer.Mistaken.Systems.GUI.PseudoGUIHandler.Set(player, "impact", Gamer.Mistaken.Systems.GUI.PseudoGUIHandler.Position.BOTTOM, null);
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
        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (!grenades.Contains(ev.Grenade)) 
                return;
            RoundLogger.Log("IMPACT GRENADE", "EXPLODED", $"Impact grenade exploded");
            foreach (Player player in ev.TargetToDamages.Keys.ToArray())
            {
                ev.TargetToDamages[player] *= Damage_multiplayer;
                RoundLogger.Log("IMPACT GRENADE", "HURT", $"{player.PlayerToString()} was hurt by an impact grenade");
            }
        }
        private void Server_RoundStarted()
        {
            grenades.Clear();
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
                grenade.InitData(Server.Host.GrenadeManager, Vector3.zero, Vector3.zero, 0f);
                grenade.transform.position = ev.Pickup.position;
                Mirror.NetworkServer.Spawn(grenade.gameObject);
                ev.Pickup.Delete();
            }
        }
    }
    /// <summary>
    /// Handles explosion on impact.
    /// </summary>
    public class ImpComponent : MonoBehaviour
    {
        private bool used;
        void OnCollisionEnter(Collision collision)
        {
            if (!used)
                this.GetComponent<FragGrenade>().NetworkfuseTime = 0.01f;
            used = true;
        }
    }
}
