using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Gamer.API.CustomClass;
using Gamer.API.CustomItem;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Mistaken.Systems.Misc;
using Gamer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.CustomClasses
{
    class DeputyFacalityManagerHandler : Module
    {
        public DeputyFacalityManagerHandler(PluginHandler p) : base(p)
        {
            new DeputyFacalityManagerKeycard();
            new DeputyFacalityManager();
        }
        public override string Name => "DeputyFacalityManager";

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.Escaping -= this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
        }
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.Escaping += this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
        }

        private void Player_Escaping(EscapingEventArgs ev)
        {
            if (DeputyFacalityManager.Instance.PlayingAsClass.Contains(ev.Player)) 
            {
                if (!MapPlus.IsLCZDecontaminated())
                {
                    ev.Player.SetGUI("cc_deputy_escape", PseudoGUIHandler.Position.MIDDLE, "<size=200%>Nie możesz uciec przed dekontaminacją LCZ</size>",1/60f);
                    ev.IsAllowed = false;
                }
            }
        }


        private void Player_InteractingDoor(InteractingDoorEventArgs ev)
        {
            if (!(Mistaken.Base.CustomItems.CustomItemsHandler.GetCustomItem(ev.Player.CurrentItem) is DeputyFacalityManagerKeycard deputyFacilityManagerKeycard) || MapPlus.IsLCZDecontaminated())
                return;
            var type = ev.Door.Type();
            if (type == DoorType.GateA || type == DoorType.GateB)
            {
                foreach (var player in RealPlayers.List.Where(p => p.Id != ev.Player.Id && p.Role == RoleType.Scientist))
                {
                    if (Vector3.Distance(player.Position, ev.Player.Position) < 10)
                    {
                        ev.IsAllowed = true;
                        return;
                    }
                }
                ev.IsAllowed = false;
                return;
            }
            else if(type == DoorType.NukeSurface || type == DoorType.Scp106Primary || type == DoorType.Scp106Secondary)
            {
                ev.IsAllowed = false;
                return;
            }
        }

        private void Server_RoundStarted()
        {
            //var scientists = RealPlayers.Get(RoleType.Scientist).ToList();
            var scientists = RealPlayers.List.ToList();
           // if (scientists.Count < 4)
            //    return;
            scientists = scientists.Where(x => !x.GetSessionVar(Main.SessionVarType.CC_ZONE_MANAGER, false)).ToList();
            DeputyFacalityManager.Instance.Spawn(scientists[UnityEngine.Random.Range(0, scientists.Count)]);
        }

        public class DeputyFacalityManager : CustomClass
        {
            public static DeputyFacalityManager Instance;
            public DeputyFacalityManager() => Instance = this;
            public override Main.SessionVarType ClassSessionVarType => Main.SessionVarType.CC_DEPUTY_FACILITY_MANAGER;

            public override string ClassName => "Zastępca Dyrektora Placówki";

            public override string ClassDescription => "";

            public override RoleType Role => RoleType.Scientist;
            public override void Spawn(Player player)
            {
                player.SetRole(RoleType.Scientist, true, false);
                PlayingAsClass.Add(player);
                player.SetSessionVar(ClassSessionVarType, true);
                foreach(var item in player.Inventory.items)
                {
                    if (item.id == ItemType.KeycardScientist || item.id == ItemType.KeycardScientistMajor || item.id == ItemType.Coin)
                        player.RemoveItem(item);
                }
                player.AddItem(new Inventory.SyncItemInfo
                {
                    id = ItemType.KeycardFacilityManager,
                    durability = 1000f
                });
                ArmorHandler.LiteArmor.Give(player, 25);
            }

        }
        public class DeputyFacalityManagerKeycard : CustomItem
        {
            public override string ItemName => "Karta Zastępcy Dyrektora Placówki";

            public override Main.SessionVarType SessionVarType => Main.SessionVarType.CC_DEPUTY_FACILITY_MANAGER_KEYCARD;

            public override ItemType Item => ItemType.KeycardFacilityManager;

            public override int Durability => 1;

        }
    }
}
