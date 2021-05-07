using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Gamer.API.CustomClass;
using Gamer.API.CustomItem;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Utilities;
using Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.CustomClasses
{
    class ZoneManagerHandler : Module
    {
        public ZoneManagerHandler(IPlugin<IConfig> plugin) : base(plugin)
        {
            new ZoneManager();
        }

        public override string Name => "Zone Manager";

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(),"RoundStart");
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
                    }
                }
            }
        }

        private void Server_RoundStarted()
        {
            MEC.Timing.CallDelayed(1.2f, () =>
            {
                if (Mistaken.Base.Version.Debug)
                    ZoneManager.Instance.Spawn(RealPlayers.List.First());
                else
                {
                    var scientist = RealPlayers.Get(RoleType.Scientist).ToArray();
                    if (scientist.Length < 2)
                        return;
                    ZoneManager.Instance.Spawn(scientist[UnityEngine.Random.Range(0, scientist.Length)]);
                }
            });
        }
        public class ZoneManager : CustomClass
        {
            public static ZoneManager Instance;
            public ZoneManager()
            {
                Instance = this;
            }
            public override Main.SessionVarType ClassSessionVarType => Main.SessionVarType.CC_ZONE_MANAGER;

            public override string ClassName => "Zarządca Strefy";

            public override string ClassDescription => "TBF";

            public override RoleType Role => RoleType.Scientist;
            public override void Spawn(Player player)
            {
                player.SetRole(RoleType.Scientist,true, false);
                PlayingAsClass.Add(player);
                player.SetSessionVar(ClassSessionVarType, true);
                player.Position = Map.Rooms.Where(x => x.Type == RoomType.HczChkpA || x.Type == RoomType.HczChkpB).First().Position + Vector3.up;
                bool hasRadio = false;
                foreach(var item in player.Inventory.items)
                {
                    if(item.id== ItemType.KeycardScientist || item.id == ItemType.KeycardScientistMajor)
                    {
                        player.RemoveItem(item);
                        player.AddItem(new Inventory.SyncItemInfo
                        {
                            durability = 1000,
                            id = ItemType.KeycardZoneManager
                        });
                    }
                    if (item.id == ItemType.Radio)
                        hasRadio = true;
                }
                if(!hasRadio)
                    player.AddItem(ItemType.Radio);
            }
        }
        /// <inheritdoc/>
        public class ZoneManagerKeycard : CustomItem
        {
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
