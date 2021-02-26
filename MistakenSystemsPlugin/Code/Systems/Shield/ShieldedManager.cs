using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace Gamer.Mistaken.Systems.Shield
{
    public class ShieldedManager : Module
    {
        public static readonly List<Shielded> Shieldeds = new List<Shielded>();

        public override string Name => "ShieldManager";
        internal ShieldedManager(PluginHandler p) : base(p)
        {
            Server_RestartingRound();
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");

            foreach (var item in Shieldeds.ToArray())
                item.Disable();
        }

        public static void Add(Shielded shielded) => Shieldeds.Add(shielded);
        public static void Remove(Player player)
        {
            foreach (var item in Shieldeds.Where(p => p.player == player).ToArray())
                item.Disable(); 
        }
        public static bool Has(Player player) => Shieldeds.Any(p => p.player.Id == player.Id);
        public static Shielded Get(Player player) => Shieldeds.Find(p => p.player.Id == player.Id);
        public static bool TryGet(Player player, out Shielded result)
        {
            result = Shieldeds.Find(p => p.player.Id == player.Id);
            return result != null;
        }

        private static void Server_RoundStarted()
        {
            Timing.RunCoroutine(ExecuteCycle());
        }
        private static void Server_RestartingRound()
        {
            foreach (var item in Shieldeds.ToArray())
                item.Disable();
            Shieldeds.Clear();
        }
        private static IEnumerator<float> ExecuteCycle()
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId; while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                foreach (var shielded in Shieldeds.ToArray())
                    shielded.DoRegenerationCicle();
                yield return Timing.WaitForSeconds(1);
            }
        }
    }

    public class Shielded
    {
        public readonly Player player;
        public int MaxShield
        {
            get
            {
                return _maxShield;
            }
            set
            {
                if(player != null)
                    player.MaxAdrenalineHealth += (value - _maxShield);
                _maxShield = value;
            }
        }
        private int _maxShield = 0;
        public float Regeneration { get; set; }
        public float SafeTime { get; }
        public float ShieldEffectivnes { get; }
        private readonly float originalShieldEffectivnes;
        public float ShieldDecay { get; set; }
        private readonly float originalShieldDecay;

        private readonly Timer safeTimer;
        private bool _isSafe = true;
        public bool IsSafe
        {
            get
            {
                return _isSafe;
            }
            private set
            {
                if (value)
                    _isSafe = true;
                else
                {
                    safeTimer.Start();
                    _isSafe = false;
                }
            }
        }

        public Shielded(Player p, int maxShield, float regeneration, float safeTime = -1, float shieldDecay = -1, float shieldEffectivnes = -1)
        {
            MaxShield = maxShield;
            Regeneration = regeneration;
            SafeTime = safeTime;
            ShieldDecay = shieldDecay;
            ShieldEffectivnes = shieldEffectivnes;
            player = p;

            safeTimer = new Timer(SafeTime * 1000);
            safeTimer.Elapsed += (_, __) =>
            {
                IsSafe = true; 
                safeTimer.Stop();
            };

            Exiled.Events.Handlers.Player.Left += Player_Left;
            Exiled.Events.Handlers.Player.ChangingRole += Player_ChangingRole;
            Exiled.Events.Handlers.Player.Hurting += Player_Hurting;

            var ps = p.ReferenceHub.playerStats;
            originalShieldDecay = ps.artificialHpDecay;
            originalShieldEffectivnes = ps.artificialNormalRatio;
            if (ShieldDecay != -1)
                ps.artificialHpDecay = ShieldDecay;
            if(ShieldEffectivnes != -1)
                ps.artificialNormalRatio = ShieldEffectivnes;
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player.Id == player.Id)
                this.Disable();
        }
        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            if(ev.Player.Id == player.Id)
                this.Disable();
        }
        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.Target.Id == player.Id)
                IsSafe = false;
        }
        internal void DoRegenerationCicle()
        {
            if (player.AdrenalineHealth > MaxShield)
            {
                if (ShieldDecay != -1)
                    player.ReferenceHub.playerStats.artificialHpDecay = originalShieldDecay;
                return;
            }
            else
            {
                if (ShieldDecay != -1)
                    player.ReferenceHub.playerStats.artificialHpDecay = ShieldDecay;
            }
            if (player.AdrenalineHealth == MaxShield)
                return;

            if (SafeTime == -1 || !IsSafe)
                return;
            player.AdrenalineHealth += Regeneration;
            if (player.AdrenalineHealth > MaxShield)
                player.AdrenalineHealth = MaxShield;
        }

        internal void Disable()
        {
            Exiled.Events.Handlers.Player.Left -= Player_Left;
            Exiled.Events.Handlers.Player.ChangingRole -= Player_ChangingRole;
            Exiled.Events.Handlers.Player.Hurting -= Player_Hurting;

            player.MaxAdrenalineHealth = 75;
            player.AdrenalineHealth = 0;
            var ps = player.ReferenceHub.playerStats;
            if (ShieldEffectivnes != -1)
                ps.artificialNormalRatio = originalShieldEffectivnes;
            if (ShieldDecay != -1)
                ps.artificialHpDecay = originalShieldDecay;

            ShieldedManager.Shieldeds.Remove(this);
        }
    }
}
