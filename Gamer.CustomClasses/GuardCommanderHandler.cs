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
using Gamer.API.CustomClass;
using Gamer.Mistaken.Systems.GUI;
using Gamer.Mistaken.Ranks;
using Gamer.Mistaken.Systems.Staff;
using Gamer.API.CustomItem;
using Scp914;

namespace Gamer.CustomClasses
{
    public class GuardCommanderHandler : Module
    {
        public GuardCommanderHandler(PluginHandler plugin) : base(plugin)
        {
            new GuardCommanderKeycard();
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
            public override string ClassDescription => "Twoim zadaniem jest <color=yellow>dowodzenie</color> <color=#7795a9>ochroną placówki</color>.<br>Twoja karta <color=yellow>pozwala</color> ci otworzyć Gate A i Gate B, ale tylko gdy:<br>- Obok jest <color=#f1e96e>Naukowiec</color><br>- Obok jest skuta <color=#ff8400>Klasa D</color><br>- Obok jest skuty <color=#1d6f00>Rebeliant Chaosu</color>";
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
                player.AddItem(new Inventory.SyncItemInfo
                {
                    id = ItemType.KeycardSeniorGuard,
                    durability = 1.001f
                });
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
                PseudoGUIHandler.Set(player, "Guard_Commander", PseudoGUIHandler.Position.MIDDLE, $"<size=150%>Jesteś <color=blue>Dowódcą Ochrony</color></size><br>{this.ClassDescription}", 20);
                PseudoGUIHandler.Set(player, "Guard_Commander_Info", PseudoGUIHandler.Position.BOTTOM, "<color=yellow>Grasz</color> jako <color=blue>Dowódca Ochrony</color>");
                RoundLoggerSystem.RoundLogger.Log("CUSTOM CLASSES", "GUARD COMMANDER", $"Spawned {player.PlayerToString()} as Guard Commander");
            }

            public override void OnDie(Player player)
            {
                base.OnDie(player);
                CustomInfoHandler.Set(player, "Guard_Commander", null, false);
                PseudoGUIHandler.Set(player, "Guard_Commander_Info", PseudoGUIHandler.Position.BOTTOM, null);
            }
        }
        private bool HasCommanderEscorted = false;
        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (!ev.IsEscaped)
                return;
            if (ev.NewRole.GetTeam() != Team.MTF)
                return;
            if (!HasCommanderEscorted)
            {
                foreach (var item in GuardCommander.Instance.PlayingAsClass)
                    PseudoGUIHandler.Set(item, "GuardCommander_Escort", PseudoGUIHandler.Position.TOP, "Dostałeś <color=yellow>informację</color> przez pager: W związu z <color=yellow>eskortą personelu</color>, od teraz jesteś <color=yellow>autoryzowany</color> do otwierania Gatów bez kogoś obok.", 10);
            }
            HasCommanderEscorted = true;
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (ev.Player.CurrentItem.id != ItemType.KeycardSeniorGuard)
                return;
            if (!(Mistaken.Systems.CustomItems.CustomItemsHandler.GetCustomItem(ev.Player.CurrentItem) is GuardCommanderKeycard))
                return;
            if (!GuardCommander.Instance.PlayingAsClass.Contains(ev.Player))
            {
                ev.IsAllowed = false;
                return;
            }
            if (ev.IsAllowed)
                return;
            var type = ev.Door.Type();
            if(type == DoorType.Intercom)
            {
                ev.IsAllowed = true;
                return;
            }
            else if(type == DoorType.NukeSurface && false)
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
            MEC.Timing.CallDelayed(60 * 6, () =>
            {
                if (!HasCommanderEscorted)
                {
                    foreach (var item in GuardCommander.Instance.PlayingAsClass)
                        PseudoGUIHandler.Set(item, "GuardCommander_Access", PseudoGUIHandler.Position.TOP, "Dostałeś <color=yellow>informację</color> przez pager: Aktywowano protokuł <color=yellow>GB-12</color>, od teraz jesteś <color=yellow>autoryzowany</color> do otwierania Gatów bez kogoś obok.", 10);
                    HasCommanderEscorted = true;
                }
            });
            MEC.Timing.CallDelayed(1.2f, () =>
            {
                try
                {
                    var guards = RealPlayers.Get(RoleType.FacilityGuard).ToArray();
                    if (guards.Length < 3 && false)
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
        public class GuardCommanderKeycard : CustomItem
        {
            public GuardCommanderKeycard()
            {
                base.Register();
            }
            public override string ItemName => "Karta Dowódcy Ochrony";

            public override ItemType Item => ItemType.KeycardSeniorGuard;

            public override int Durability => 001;
            public override void OnStartHolding(Player player, Inventory.SyncItemInfo item)
            {
                if(GuardCommander.Instance.PlayingAsClass.Contains(player))
                    PseudoGUIHandler.Set(player, "GC_Keycard", PseudoGUIHandler.Position.BOTTOM, "<color=yellow>Trzymasz</color> kartę <color=blue>Dowódcy Ochrony</color>");
                else
                    PseudoGUIHandler.Set(player, "GC_Keycard", PseudoGUIHandler.Position.BOTTOM, "<color=yellow>Trzymasz</color> kartę <color=blue>Dowódcy Ochrony</color>, ale chyba <color=yellow>nie</color> możesz jej używać");
            }
            public override void OnStopHolding(Player player, Inventory.SyncItemInfo item)
            {
                PseudoGUIHandler.Set(player, "GC_Keycard", PseudoGUIHandler.Position.BOTTOM, null);
            }
            public override Pickup OnUpgrade(Pickup pickup, Scp914Knob setting)
            {
                if (setting == Scp914Knob.Coarse || setting == Scp914Knob.Rough)
                    return null;
                return base.OnUpgrade(pickup, setting);
            }
            public override Vector3 Size => new Vector3(1, 5, 1);
        }
    }
}
