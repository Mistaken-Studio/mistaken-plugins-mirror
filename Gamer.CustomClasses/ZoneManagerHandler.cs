using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Gamer.API.CustomClass;
using Gamer.API.CustomItem;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using System.Linq;
using UnityEngine;

namespace Gamer.CustomClasses
{
    internal class ZoneManagerHandler : Module
    {
        /// <inheritdoc/>
        public ZoneManagerHandler(IPlugin<IConfig> plugin) : base(plugin)
        {
            new ZoneManager();
            new ZoneManagerKeycard();
        }
        /// <inheritdoc/>
        public override string Name => "Zone Manager";
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {

            Log.Debug(ev.Player.Nickname);
            Log.Debug(ev.IsEscaped);
            Log.Debug(ev.NewRole);
            Log.Debug(ZoneManager.Instance.PlayingAsClass.Contains(ev.Player));
            if (ev.IsEscaped)
            {
                if (ZoneManager.Instance.PlayingAsClass.Contains(ev.Player))
                {
                    if (ev.NewRole == RoleType.NtfScientist)
                    {
                        ev.NewRole = RoleType.NtfCadet;
                        if (ev.Items.Contains(ItemType.KeycardSeniorGuard))
                        {
                            ev.Items.Remove(ItemType.KeycardSeniorGuard);
                            ev.Items.Add(ItemType.KeycardNTFLieutenant);
                        }
                    }
                    else
                    {
                        ev.Items.Add(ItemType.GunProject90);
                        ev.Items.Add(ItemType.SCP207);
                        this.CallDelayed(1, () =>
                        {
                            ev.Player.Ammo[(int)AmmoType.Nato9] += 100;
                        }, "GiveAmmo");
                    }
                }
            }
        }

        private void Server_RoundStarted()
        {
            MEC.Timing.CallDelayed(1.2f, () =>
            {
                //var scientists = RealPlayers.Get(RoleType.Scientist).ToList();
                var scientists = RealPlayers.List.ToList();
                if (scientists.Count < 2)
                    return;
                scientists = scientists.Where(x => !x.GetSessionVar(Main.SessionVarType.CC_DEPUTY_FACILITY_MANAGER, false)).ToList();
                ZoneManager.Instance.Spawn(scientists[UnityEngine.Random.Range(0, scientists.Count)]);
            });
        }
        /// <inheritdoc/>
        public class ZoneManager : CustomClass
        {
            public static ZoneManager Instance;
            /// <summary>
            /// Constructor
            /// </summary>
            public ZoneManager()
            {
                Instance = this;
            }
            /// <inheritdoc/>
            public override Main.SessionVarType ClassSessionVarType => Main.SessionVarType.CC_ZONE_MANAGER;
            /// <inheritdoc/>
            public override string ClassName => "Zarządca Strefy Podwyższonego Ryzyka";
            /// <inheritdoc/>
            public override string ClassDescription => "TBF";
            /// <inheritdoc/>
            public override RoleType Role => RoleType.Scientist;
            /// <inheritdoc/>
            public override void Spawn(Player player)
            {
                player.SetRole(RoleType.Scientist, true, false);
                PlayingAsClass.Add(player);
                player.SetSessionVar(ClassSessionVarType, true);
                player.Position = Map.Rooms.Where(x => x.Type == RoomType.HczChkpA || x.Type == RoomType.HczChkpB).First().Position + Vector3.up;
                bool hasRadio = false;
                foreach (var item in player.Inventory.items.ToArray())
                {
                    if (item.id == ItemType.KeycardScientist || item.id == ItemType.KeycardScientistMajor)
                    {
                        player.RemoveItem(item);
                    }
                    else if (item.id == ItemType.Radio)
                        hasRadio = true;
                }
                player.AddItem(new Inventory.SyncItemInfo
                {
                    durability = 1000,
                    id = ItemType.KeycardZoneManager
                });
                if (!hasRadio)
                    player.AddItem(ItemType.Radio);
                Mistaken.Base.CustomInfoHandler.Set(player, "ZM", "<color=#217a7b><b>Zarządca Strefy Podwyższonego Ryzyka</b></color>", false);
                player.SetGUI("ZM", Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, $"<size=150%>Jesteś <color=#217a7b>Zarządcą Strefy Podwyższonego Ryzyka</color></size><br>{ClassDescription}", 20);
                player.SetGUI("ZM_Info", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, "<color=yellow>Grasz</color> jako <color=#217a7b>Zarządca Strefy Podwyższonego Ryzyka</color>");
                RoundLoggerSystem.RoundLogger.Log("CUSTOM CLASSES", "ZONE MANAGER", $"Spawned {player.PlayerToString()} as Zone Manager");
            }
        }
        /// <inheritdoc/>
        public class ZoneManagerKeycard : CustomItem
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public ZoneManagerKeycard() => base.Register();
            /// <inheritdoc/>
            public override string ItemName => "Karta Zarządcy Strefy";
            /// <inheritdoc/>
            public override Main.SessionVarType SessionVarType => Main.SessionVarType.CC_ZONE_MANAGER_KEYCARD;
            /// <inheritdoc/>
            public override ItemType Item => ItemType.KeycardZoneManager;
            /// <inheritdoc/>
            public override int Durability => 001;
            /// <inheritdoc/>
            public override void OnStartHolding(Player player, Inventory.SyncItemInfo item)
            {
                player.SetGUI("ZM_Keycard", PseudoGUIHandler.Position.BOTTOM, "<color=yellow>Trzymasz</color> kartę <color=#217a7b>Zarządcy Strefy</color>");
            }
            /// <inheritdoc/>
            public override void OnStopHolding(Player player, Inventory.SyncItemInfo item)
            {
                player.SetGUI("ZM_Keycard", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
            }
        }
    }
}
