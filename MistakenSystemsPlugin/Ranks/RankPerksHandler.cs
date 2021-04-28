using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Systems.Misc;

namespace Gamer.Mistaken.Ranks
{
    public static class Extensions
    {
        public static bool IsVIP(this Player player, out RanksHandler.VipLevel vipLevel)
        {
            vipLevel = RanksHandler.VipLevel.NONE;
            if (!RanksHandler.VipList.TryGetValue(player.UserId, out RanksHandler.PlayerInfo role))
                return false;
            vipLevel = role.VipLevel;
            return true;
        }
    }
    public class RankPerksHandler : Module
    {
        public override string Name => "Rank perks";
        public RankPerksHandler(PluginHandler plugin) : base(plugin)
        {

        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (!RanksHandler.VipList.TryGetValue(ev.Player?.UserId, out RanksHandler.PlayerInfo role))
                return;
            if (role.VipLevel != RanksHandler.VipLevel.NONE)
            {
                if (ev.NewRole.GetTeam() == Team.SCP || ev.NewRole == global::RoleType.Spectator)
                {
                    Log.Debug("Player is SCP or Spectator. Can't give VIP items");
                    return;
                }
                var num1 = UnityEngine.Random.Range(1, 101);
                var num2 = UnityEngine.Random.Range(1, 101);
                var num3 = UnityEngine.Random.Range(1, 101);
                Log.Debug(ev.Player + " got N1: " + num1);
                Log.Debug(ev.Player + " got N2: " + num2);
                Log.Debug(ev.Player + " got N3: " + num3);
                string debugMessage = $"Items | N1: {num1} | N2: {num2} | N3: {num3}";
                switch (role.VipLevel)
                {
                    case RanksHandler.VipLevel.EUCLID:
                        {
                            ev.Player.SendConsoleMessage($"EUCLID {debugMessage}", "yellow");
                            if (ev.NewRole == global::RoleType.ClassD)
                            {
                                ev.Items.Add(ItemType.Flashlight);
                                if (30 >= num1)
                                    ev.Items.Add(ItemType.KeycardJanitor);
                            }
                            if (ev.NewRole == global::RoleType.Scientist)
                            {
                                if (30 >= num1)
                                    ev.Items.Add(ItemType.Radio);
                            }
                            if (ev.NewRole == global::RoleType.ClassD || ev.NewRole == global::RoleType.Scientist)
                            {
                                if (15 >= num2)
                                    ArmorHandler.Armor.GiveDelayed(ev.Player, 20);
                            }
                            if (30 >= num3)
                                ev.Items.Add(ItemType.Medkit);
                            break;
                        }
                    case RanksHandler.VipLevel.KETER:
                        {
                            ev.Player.SendConsoleMessage($"KETER {debugMessage}", "yellow");
                            if (ev.NewRole == global::RoleType.ClassD)
                            {
                                ev.Items.Add(ItemType.Flashlight);
                                if (15 >= num1)
                                    ev.Items.Add(ItemType.KeycardScientist);
                                else if (65 >= num1)
                                    ev.Items.Add(ItemType.KeycardJanitor);
                            }
                            if (ev.NewRole == global::RoleType.Scientist)
                            {
                                if (50 >= num1)
                                    ev.Items.Add(ItemType.Radio);
                            }
                            if (ev.NewRole == global::RoleType.ClassD || ev.NewRole == global::RoleType.Scientist)
                            {
                                if (30 >= num2)
                                    ArmorHandler.Armor.GiveDelayed(ev.Player, 25);
                            }
                            if (ev.NewRole == global::RoleType.FacilityGuard)
                            {
                                if (15 >= num1)
                                {
                                    ev.Items.Add(ItemType.GunProject90);
                                    ev.Items.Remove(ItemType.GunMP7);
                                    MEC.Timing.CallDelayed(5, () =>
                                    {
                                        ev.Player.Ammo.amount[(int)Exiled.API.Enums.AmmoType.Nato9] = 50;
                                    });
                                }
                                if (15 >= num2)
                                {
                                    ev.Items.Add(ItemType.KeycardSeniorGuard);
                                    ev.Items.Remove(ItemType.KeycardGuard);
                                }
                            }
                            if (30 >= num3)
                                ev.Items.Add(ItemType.Adrenaline);
                            else if (60 >= num3)
                                ev.Items.Add(ItemType.Medkit);
                            break;
                        }
                    case RanksHandler.VipLevel.APOLLYON:
                        {
                            ev.Player.SendConsoleMessage($"APOLLYON {debugMessage}", "yellow");
                            if (ev.NewRole == global::RoleType.ClassD)
                            {
                                ev.Items.Add(ItemType.Flashlight);
                                if (65 >= num1)
                                    ev.Items.Add(ItemType.KeycardScientist);
                                else
                                    ev.Items.Add(ItemType.KeycardJanitor);
                            }
                            if (ev.NewRole == global::RoleType.Scientist)
                            {
                                ev.Items.Add(ItemType.Radio);
                                if (30 >= num1)
                                {
                                    ev.Items.Add(ItemType.KeycardScientistMajor);
                                    ev.Items.Remove(ItemType.KeycardScientist);
                                }
                            }
                            if (ev.NewRole == global::RoleType.ClassD || ev.NewRole == global::RoleType.Scientist)
                            {
                                if (80 >= num2)
                                    ArmorHandler.Armor.GiveDelayed(ev.Player, 30);
                            }
                            if (ev.NewRole == global::RoleType.FacilityGuard)
                            {
                                if (15 >= num1)
                                {
                                    ev.Items.Add(ItemType.GunE11SR);
                                    ev.Items.Remove(ItemType.GunMP7);
                                    MEC.Timing.CallDelayed(5, () =>
                                    {
                                        ev.Player.Ammo.amount[(int)Exiled.API.Enums.AmmoType.Nato556] = 80;
                                    });
                                }
                                else if (45 >= num1)
                                {
                                    ev.Items.Add(ItemType.GunProject90);
                                    ev.Items.Remove(ItemType.GunMP7);
                                    MEC.Timing.CallDelayed(5, () =>
                                    {
                                        ev.Player.Ammo.amount[(int)Exiled.API.Enums.AmmoType.Nato9] = 100;
                                    });
                                }
                                if (30 >= num2)
                                {
                                    ev.Items.Add(ItemType.KeycardSeniorGuard);
                                    ev.Items.Remove(ItemType.KeycardGuard);
                                }
                            }
                            if (50 >= num3)
                                ev.Items.Add(ItemType.Adrenaline);
                            else
                                ev.Items.Add(ItemType.Medkit);
                            break;
                        }
                }
            }
        }

    }
}
