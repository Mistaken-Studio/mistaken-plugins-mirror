using System;
using System.Linq;
using System.Net;
using System.Text;
using Exiled.API.Features;
using Gamer.Utilities;
using System.Threading;
using System.Threading.Tasks;
using Gamer.Diagnostics;
using System.Collections.Generic;
using Exiled.API.Enums;
using Gamer.Mistaken.Systems.Misc;
using Gamer.Mistaken.Systems;
using Gamer.RoundLoggerSystem;

namespace Gamer.CustomClasses
{
    public class GuardCommanderHandler : Module
    {
        public GuardCommanderHandler(PluginHandler plugin) : base(plugin)
        {
        }

        public override string Name => "GuardCommander";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        public static Player GuardCommander;
        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (GuardCommander?.Id == ev.Target.Id)
            {
                CustomInfoHandler.Set(ev.Target, "Guard_Commander", null, false);
                GuardCommander = null;
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (GuardCommander?.Id == ev.Player.Id && ev.NewRole != RoleType.NtfCommander)
            {
                CustomInfoHandler.Set(ev.Player, "Guard_Commander", null, false);
                GuardCommander = null;
            }
        }

        private void Server_RoundStarted()
        {
            GuardCommander = null;
            MEC.Timing.CallDelayed(1.2f, () =>
            {
                try
                {
                    var guards = RealPlayers.Get(RoleType.FacilityGuard).ToArray();
                    if (guards.Length < 3)
                        return;
                    GuardCommander = guards[UnityEngine.Random.Range(0, guards.Length)];
                    GuardCommander.SetRole(RoleType.NtfCommander, true, false);
                    GuardCommander.ClearInventory();
                    GuardCommander.AddItem(ItemType.GunE11SR);
                    GuardCommander.AddItem(ItemType.KeycardNTFLieutenant);
                    GuardCommander.AddItem(ItemType.Disarmer);
                    GuardCommander.AddItem(ItemType.Radio);
                    ArmorHandler.LiteArmor.Give(GuardCommander, 15);
                    Taser.TaserHandler.TaserItem.Give(GuardCommander);
                    Xname.ImpactGrenade.ImpHandler.ImpItem.Give(GuardCommander);
                    GuardCommander.AddItem(new Inventory.SyncItemInfo
                    {
                        id = ItemType.WeaponManagerTablet,
                        durability = 1.301f
                    });
                    GuardCommander.Ammo[(int)AmmoType.Nato556] = 120;
                    CustomInfoHandler.Set(GuardCommander, "Guard_Commander", "<color=blue><b>Dowódca Ochrony</b></color>", false);
                    RoundLoggerSystem.RoundLogger.Log("CUSTOM CLASSES", "GUARD COMMANDER", $"Spawned {GuardCommander.PlayerToString()} as Guard Commander");
                }
                catch(System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
            });
        }
    }
}
