using Exiled.API.Interfaces;
using Gamer.Diagnostics;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;

namespace Xname.CE
{
    /// <inheritdoc/>
    public class CEHandler : Module
    {
        internal readonly float chance = 10f;
        internal HashSet<Player> unconsiousPlayers = new HashSet<Player>();
        public static readonly Dictionary<int, (Vector3 Pos, RoleType Role, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType)> savedInfo = new Dictionary<int, (Vector3 Pos, RoleType Role, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType)>();
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
            unconsiousPlayers.Clear();
        }
        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.DamageType.isWeapon && ev.Amount > 25)
            {
                if (UnityEngine.Random.Range(0, 100) < chance)
                {
                    ev.IsAllowed = false;
                    unconsiousPlayers.Add(ev.Target);

                }
            }
        }
    }
}
