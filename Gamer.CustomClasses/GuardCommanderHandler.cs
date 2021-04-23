﻿using System;
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
using Gamer.API.CustomClass;
using Gamer.Mistaken.Systems.GUI;
using Gamer.Mistaken.Ranks;
using Gamer.Mistaken.Systems.Staff;

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
            if (Server.Port % 2 == 1 && Server.Port < 7790)
                return;
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        public class GuardCommander : CustomClass
        {
            public static GuardCommander Instance = new GuardCommander();
            public override Main.SessionVarType ClassSessionVarType => Main.SessionVarType.CC_GUARD_COMMANDER;
            public override string ClassName => "Dowódca Ochrony";
            public override string ClassDescription => "Twoim zadaniem jest dowodzenie <color=#7795a9>ochroną placówki</color>.<br>Twoja karta <color=yellow>pozwala</color> ci otworzyć Gate A i Gate B, ale tylko gdy:<br>- Obok jest <color=#f1e96e>Naukowiec</color><br>- Obok jest skuta <color=#ff8400>Klasa D</color><br>- Obok jest skuty <color=#1d6f00>Rebeliant Chaosu</color>";
            public override RoleType Role => RoleType.NtfCommander;

            public override void Spawn(Player player)
            {
                PlayingAsClass.Add(player);
                player.SetSessionVar(ClassSessionVarType, true);
                player.SetRole(RoleType.NtfCommander, true, false);
                player.ClearInventory();
                if(player.IsVIP(out var vipLevel))
                {
                    if(vipLevel != RanksHandler.VipLevel.SAFE && UnityEngine.Random.Range(0, 100) < 15 + ((uint)vipLevel * 15))
                    {
                        player.AddItem(ItemType.GunE11SR);
                        player.Ammo[(int)AmmoType.Nato556] = 120;
                    }
                    else
                    {
                        player.AddItem(ItemType.GunProject90);
                        player.Ammo[(int)AmmoType.Nato9] = 150;
                    }
                }
                else
                {
                    player.AddItem(ItemType.GunProject90);
                    player.Ammo[(int)AmmoType.Nato9] = 150;
                }
                player.AddItem(ItemType.KeycardSeniorGuard);
                player.AddItem(ItemType.Disarmer);
                player.AddItem(ItemType.Radio);
                ArmorHandler.LiteArmor.Give(player, 15);
                Taser.TaserHandler.TaserItem.Give(player);
                Xname.ImpactGrenade.ImpHandler.ImpItem.Give(player);
                player.AddItem(new Inventory.SyncItemInfo
                {
                    id = ItemType.WeaponManagerTablet,
                    durability = 1.301f
                });
                CustomInfoHandler.Set(player, "Guard_Commander", "<color=blue><b>Dowódca Ochrony</b></color>", false);
                PseudoGUIHandler.Set(player, "Guard_Commander", PseudoGUIHandler.Position.MIDDLE, $"<size=150%>Jesteś <color=blue>{this.ClassName}</color></size><br>{this.ClassDescription}", 20);
                RoundLoggerSystem.RoundLogger.Log("CUSTOM CLASSES", "GUARD COMMANDER", $"Spawned {player.PlayerToString()} as Guard Commander");
            }

            public override void OnDie(Player player)
            {
                base.OnDie(player);
                CustomInfoHandler.Set(player, "Guard_Commander", null, false);
            }
        }
        private bool HasCommanderEscorted = false;
        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (!ev.IsEscaped)
                return;
            if (ev.NewRole.GetTeam() != Team.MTF)
                return;
            HasCommanderEscorted = true;
        }


        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (ev.IsAllowed)
                return;
            if (!GuardCommander.Instance.PlayingAsClass.Contains(ev.Player))
                return;
            if (ev.Player.CurrentItem.id != ItemType.KeycardSeniorGuard)
                return;
            var type = ev.Door.Type();
            if(type == DoorType.NukeSurface && false)
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
            else if ((type == DoorType.Scp106Primary || type == DoorType.Scp106Secondary || type == DoorType.Scp106Bottom) && false)
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
                if(HasCommanderEscorted)
                {
                    ev.IsAllowed = true;
                    return;
                }
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

        private void Server_RoundStarted()
        {
            HasCommanderEscorted = false;
            MEC.Timing.CallDelayed(1.2f, () =>
            {
                try
                {
                    var guards = RealPlayers.Get(RoleType.FacilityGuard).ToArray();
                    if (guards.Length < 3)
                        return;
                    var devs = RealPlayers.List.Where(p => p.Role == RoleType.FacilityGuard && p.IsActiveDev()).ToArray();
                    if(devs.Length > 0)
                        GuardCommander.Instance.Spawn(devs[UnityEngine.Random.Range(0, devs.Length)]);
                    else
                        GuardCommander.Instance.Spawn(guards[UnityEngine.Random.Range(0, guards.Length)]);
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
