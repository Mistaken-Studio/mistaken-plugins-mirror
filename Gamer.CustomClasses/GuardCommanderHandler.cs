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
using UnityEngine;
using Exiled.API.Extensions;

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
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (ev.IsAllowed)
                return;
            if (GuardCommander != ev.Player)
                return;
            if (ev.Player.CurrentItem.id != ItemType.KeycardSeniorGuard)
                return;
            var type = ev.Door.Type();
            if(type == DoorType.NukeSurface)
            {
                foreach (var player in RealPlayers.List.Where(p => p.Id != ev.Player.Id && (p.Role != RoleType.FacilityGuard && p.Team == Team.MTF)))
                {
                    if (Vector3.Distance(player.Position, ev.Player.Position) < 10)
                    {
                        ev.IsAllowed = true;
                        return;
                    }
                }
            }
            else if (type == DoorType.Scp106Primary || type == DoorType.Scp106Secondary || type == DoorType.Scp106Bottom)
            {
                if (!Map.IsLCZDecontaminated)
                    return;
                bool tmp = false;
                foreach (var player in RealPlayers.List.Where(p => p.Id != ev.Player.Id && (p.Role == RoleType.NtfCommander || p.Role == RoleType.NtfScientist)))
                {
                    if (Vector3.Distance(player.Position, ev.Player.Position) < 10)
                    {
                        if (!tmp)
                            tmp = true;
                        else
                        {
                            ev.IsAllowed = true;
                        }
                    }
                }
            }
            else if (type == DoorType.GateA || type == DoorType.GateB)
            {
                foreach (var player in RealPlayers.List.Where(p => p.Id != ev.Player.Id && p.Role == RoleType.Scientist || ((p.Role == RoleType.ClassD || p.Role == RoleType.ChaosInsurgency) && p.IsCuffed)))
                {
                    if (Vector3.Distance(player.Position, ev.Player.Position) < 10)
                    {
                        ev.IsAllowed = true;
                        return;
                    }
                }
            }
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
                    var guards = RealPlayers.Get(/*RoleType.FacilityGuard*/RoleType.ClassD).ToArray();
                    if (guards.Length < 3 && false)
                        return;
                    GuardCommander = guards[UnityEngine.Random.Range(0, guards.Length)];
                    GuardCommander.SetRole(RoleType.NtfCommander, true, false);
                    GuardCommander.ClearInventory();
                    GuardCommander.AddItem(ItemType.GunProject90);
                    GuardCommander.AddItem(ItemType.KeycardSeniorGuard);
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
                    GuardCommander.Ammo[(int)AmmoType.Nato9] = 150;
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
