using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.BetterSCP.SCP914
{
    class SCP914Handler : Module
    {
        public override string Name => nameof(SCP914Handler);
        public SCP914Handler(PluginHandler p) : base(p)
        {
            p.RegisterTranslation("scp914_rough", "Killed by extreme heat");
            p.RegisterTranslation("scp914_coarse", "Killed by high heat");
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Scp914.Activating += this.Handle<Exiled.Events.EventArgs.ActivatingEventArgs>((ev) => Scp914_Activating(ev));
            Exiled.Events.Handlers.Scp914.UpgradingItems += this.Handle<Exiled.Events.EventArgs.UpgradingItemsEventArgs>((ev) => Scp914_UpgradingItems(ev));
            Exiled.Events.Handlers.Scp914.ChangingKnobSetting += this.Handle<Exiled.Events.EventArgs.ChangingKnobSettingEventArgs>((ev) => Scp914_ChangingKnobSetting(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Scp914.Activating -= this.Handle<Exiled.Events.EventArgs.ActivatingEventArgs>((ev) => Scp914_Activating(ev));
            Exiled.Events.Handlers.Scp914.UpgradingItems -= this.Handle<Exiled.Events.EventArgs.UpgradingItemsEventArgs>((ev) => Scp914_UpgradingItems(ev));
            Exiled.Events.Handlers.Scp914.ChangingKnobSetting -= this.Handle<Exiled.Events.EventArgs.ChangingKnobSettingEventArgs>((ev) => Scp914_ChangingKnobSetting(ev));
        }

        private void Scp914_ChangingKnobSetting(Exiled.Events.EventArgs.ChangingKnobSettingEventArgs ev)
        {
            switch (ev.KnobSetting)
            {
                case Scp914.Scp914Knob.Rough:
                    Log(ev.Player, Systems.Logs.SCP914Action.CHANGE_ROUGH);
                    break;
                case Scp914.Scp914Knob.Coarse:
                    Log(ev.Player, Systems.Logs.SCP914Action.CHANGE_COARSE);
                    break;
                case Scp914.Scp914Knob.OneToOne:
                    Log(ev.Player, Systems.Logs.SCP914Action.CHANGE_1TO1);
                    break;
                case Scp914.Scp914Knob.Fine:
                    Log(ev.Player, Systems.Logs.SCP914Action.CHANGE_FINE);
                    break;
                case Scp914.Scp914Knob.VeryFine:
                    Log(ev.Player, Systems.Logs.SCP914Action.CHANGE_VERY_FINE);
                    break;
            }
        }

        private void Scp914_UpgradingItems(Exiled.Events.EventArgs.UpgradingItemsEventArgs ev)
        {
            foreach (var player in ev.Players.Where(p => p.IsReadyPlayer() && p.IsAlive))
            {
                PlayerStats ps = player.ReferenceHub.playerStats;

                switch (ev.KnobSetting)
                {
                    case Scp914.Scp914Knob.Rough:
                        {
                            int damage = 500;
                            var hitInfo = new PlayerStats.HitInfo(damage, "*" + plugin.ReadTranslation("scp914_rough"), new DamageTypes.DamageType("914"), 0);
                            if (player.Health <= damage)
                            {
                                //CustomAchievements.RoundEventHandler.AddProggress("914Killer", player);
                                if (ATK.AntyTeamKillHandler.IsTeamkill(Last914User, player))
                                    ATK.AntyTeamKillHandler.TeamKill.Create(new Exiled.Events.EventArgs.DyingEventArgs(Last914User, player, hitInfo), null);
                                if (player.UserId == Last914User.UserId && player.Role == RoleType.Scp0492)
                                    MapPlus.Broadcast("Better 914", 10, $"{Last914User.Nickname} has commited suicide in 914 as Zombie", Broadcast.BroadcastFlags.AdminChat);
                            }
                            ps.HurtPlayer(hitInfo, player.GameObject);
                            Log(player, Systems.Logs.SCP914Action.HURT_ROUGH);
                            break;
                        }
                    case Scp914.Scp914Knob.Coarse:
                        {
                            int damage = 250;
                            var hitInfo = new PlayerStats.HitInfo(damage, "*" + plugin.ReadTranslation("scp914_coarse"), new DamageTypes.DamageType("914"), 0);
                            if (player.Health <= damage)
                            {
                                //CustomAchievements.RoundEventHandler.AddProggress("914Killer", player);
                                if (ATK.AntyTeamKillHandler.IsTeamkill(Last914User, player))
                                    ATK.AntyTeamKillHandler.TeamKill.Create(new Exiled.Events.EventArgs.DyingEventArgs(Last914User, player, hitInfo), null);
                                if (player.UserId == Last914User.UserId && player.Role == RoleType.Scp0492)
                                    MapPlus.Broadcast("Better 914", 10, $"{Last914User.Nickname} has commited suicide in 914 as Zombie", Broadcast.BroadcastFlags.AdminChat);
                            }
                            ps.HurtPlayer(hitInfo, player.GameObject);
                            Log(player, Systems.Logs.SCP914Action.HURT_COARSE);
                            if (player.Team != Team.SCP)
                            {
                                Inventory inv = PlayerManager.localPlayer.GetComponent<Inventory>();
                                GameObject gameObject = GameObject.Instantiate(inv.pickupPrefab);
                                NetworkServer.Spawn(gameObject);
                                gameObject.GetComponent<Pickup>().SetupPickup(
                                    ItemType.Medkit,
                                    float.MaxValue,
                                    player.GameObject,
                                    new Pickup.WeaponModifiers(),
                                    new Vector3(ev.Scp914.output.position.x, ev.Scp914.output.position.y, ev.Scp914.output.position.z) + new Vector3(0, 1, 0),
                                    Quaternion.identity
                                );
                            }
                            break;
                        }
                    case Scp914.Scp914Knob.VeryFine:
                        {
                            if (player.Team == Team.SCP)
                                return;
                            var num = UnityEngine.Random.Range(-10, 5);
                            if (num == 1)
                                Log(player, Systems.Logs.SCP914Action.RECIVE_SCP207_1);
                            else if (num == 2)
                                Log(player, Systems.Logs.SCP914Action.RECIVE_SCP207_2);
                            else if (num == 3)
                                Log(player, Systems.Logs.SCP914Action.RECIVE_SCP207_3);
                            else if (num == 4)
                                Log(player, Systems.Logs.SCP914Action.RECIVE_SCP207_4);
                            var min = player.ReferenceHub.playerEffectsController.GetEffect<CustomPlayerEffects.Scp207>().Intensity;
                            player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<CustomPlayerEffects.Scp207>((byte)Mathf.Max(num < 0 ? 0 : num, min));
                            break;
                        }
                }
            }

            foreach (var player in RealPlayers.List.Where(p => p.IsReadyPlayer() && p.IsAlive && Vector3.Distance(p.Position, ev.Scp914.output.position) < 2))
                Timing.RunCoroutine(PunishOutput(player));
        }

        private IEnumerator<float> PunishOutput(Player player)
        {
            Log(player, Systems.Logs.SCP914Action.HURT_OUTPUT);
            for (int i = 0; i < 10 * 3; i++)
            {
                player.Hurt(player.Health / 50, new DamageTypes.DamageType("SCP 914"), "*SCP 914");
                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        private Player Last914User;
        private void Scp914_Activating(Exiled.Events.EventArgs.ActivatingEventArgs ev)
        {
            Last914User = ev.Player;
            Log(ev.Player, Systems.Logs.SCP914Action.ACTIVATE);
        }

        private new void Log(Player p, Systems.Logs.SCP914Action type)
        {
            if (!Systems.Logs.LogManager.SCP914Logs.ContainsKey(RoundPlus.RoundId))
                Systems.Logs.LogManager.SCP914Logs.Add(RoundPlus.RoundId, new List<Systems.Logs.SCP914Log>());
            Systems.Logs.LogManager.SCP914Logs[RoundPlus.RoundId].Add(new Systems.Logs.SCP914Log(p, type));
        }
    }
}
