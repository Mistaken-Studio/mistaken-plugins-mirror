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
                    Grenade grenade = UnityEngine.Object.Instantiate(player.GrenadeManager.availableGrenades[0].grenadeInstance).GetComponent<Grenade>();
                    grenade.fuseDuration = 999;
                    grenade.InitData(player.GrenadeManager, Vector3.zero, player.CameraTransform.forward, slow ? 0.75f : 1.5f);
                    grenades.Add(grenade.gameObject);
                    Mirror.NetworkServer.Spawn(grenade.gameObject);
                    player.RemoveItem(item);
                    grenade.gameObject.AddComponent<ImpComponent>();
                });
                return false;
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
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
        }

        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (!grenades.Contains(ev.Grenade)) 
                return;
            foreach (Player player in ev.TargetToDamages.Keys.ToArray())
                ev.TargetToDamages[player] *= Damage_multiplayer;
        }

        private void Server_RoundStarted()
        {
            var lockers = LockerManager.singleton.lockers.Where(i => i.chambers.Length == 9).ToArray();
            int toSpawn = 5;
            while (toSpawn > 0)
            {
                var locker = lockers[UnityEngine.Random.Range(0, lockers.Length)];
                locker.AssignPickup(ItemType.GrenadeFrag.Spawn(1.001f, locker.chambers[UnityEngine.Random.Range(0, locker.chambers.Length)].spawnpoint.position));
                toSpawn--;
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
            {
                this.GetComponent<FragGrenade>().NetworkfuseTime = 0.01f;
                Log.Debug(collision.gameObject.name);
                Log.Debug(collision.gameObject.layer);
                Log.Debug(this.gameObject.layer);
            }
            used = true;
        }
    }
}
