using Exiled.API.Features;
using Exiled.API.Interfaces;
using Gamer.API.CustomClass;
using Gamer.Diagnostics;
using Gamer.Utilities;
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
            new ZoneManger();
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
            if (ev.IsEscaped) 
            {
                if (ZoneManger.Instance.PlayingAsClass.Contains(ev.Player))
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
                    ZoneManger.Instance.Spawn(RealPlayers.List.First());
                else
                {
                    var scientist = RealPlayers.Get(RoleType.Scientist).ToArray();
                    if (scientist.Length < 2)
                        return;
                    ZoneManger.Instance.Spawn(scientist[UnityEngine.Random.Range(0, scientist.Length)]);
                }
            });
        }
        public class ZoneManger : CustomClass
        {
            public static ZoneManger Instance;
            public ZoneManger()
            {
                Instance = this;
            }
            public override Main.SessionVarType ClassSessionVarType => Main.SessionVarType.CC_ZONE_MANAGER;

            public override string ClassName => "Zarządca Strefy";

            public override string ClassDescription => "TBF";

            public override RoleType Role => RoleType.Scientist;
            public override void Spawn(Player player)
            {
                PlayingAsClass.Add(player);
                player.SetSessionVar(ClassSessionVarType, true);
                player.SetRole(RoleType.Scientist,true, false);
                player.Position = Exiled.API.Features.Map.Rooms.Where(x => x.Type == Exiled.API.Enums.RoomType.HczChkpA || x.Type == Exiled.API.Enums.RoomType.HczChkpB).First().Position + Vector3.up;
                foreach(var item in player.Inventory.items)
                {
                    if(item.id== ItemType.KeycardScientist || item.id == ItemType.KeycardScientistMajor)
                    {
                        player.RemoveItem(item);
                        player.AddItem(ItemType.KeycardZoneManager);
                        player.AddItem(ItemType.Radio);
                    }
                }
            }
        }
    }
}
