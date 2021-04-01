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

namespace Xname.ImpactGrenade
{
    public class ImpHandler : Module
    {
        internal static readonly Vector3 Size = new Vector3(1f, .40f, 1f);
        internal static readonly float Damage_multiplayer = 0.14f;
        internal static HashSet<GameObject> grenades = new HashSet<GameObject>();
        public class ImpItem : CustomItem
        {
            public ImpItem() => base.Register();
            public override string ItemName => "Impact Grenade";
            public override ItemType Item => ItemType.GrenadeFrag;
            public override int Durability => 001;
            public override Vector3 Size => ImpHandler.Size;
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
            public override bool OnThrow(Player player, Inventory.SyncItemInfo item, bool slow)
            {
                MEC.Timing.CallDelayed(1f, () =>
                {
                    Grenade grenade = UnityEngine.Object.Instantiate<GameObject>(player.GrenadeManager.availableGrenades[0].grenadeInstance).GetComponent<Grenade>();
                    grenade.InitData(player.GrenadeManager, Vector3.zero, player.CameraTransform.forward, slow ? 0.75f : 1.5f);
                    grenade.NetworkfuseTime = 999;
                    ImpHandler.grenades.Add(grenade.gameObject);
                    Mirror.NetworkServer.Spawn(grenade.gameObject);
                    player.RemoveItem(item);
                    grenade.gameObject.AddComponent<ImpComponent>();
                    /*MEC.Timing.CallDelayed(0.1f, () =>
                    {
                        grenade.gameObject.AddComponent<ImpComponent>();
                    });*/
                });
                return false;
            }
        }

        public ImpHandler(IPlugin<IConfig> plugin) : base(plugin)
        {
            new ImpItem();
        }

        public override string Name => "ImpHandler";

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
        }

        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (grenades.Contains(ev.Grenade))
            {
                foreach (Player player in ev.TargetToDamages.Keys.ToArray())
                {
                    ev.TargetToDamages[player] *= Damage_multiplayer;
                }
            }
        }
    }

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
