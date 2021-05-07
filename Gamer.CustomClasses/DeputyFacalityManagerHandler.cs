using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Gamer.API.CustomClass;
using Gamer.API.CustomItem;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Mistaken.Systems.Misc;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.CustomClasses
{
    class DeputyFacalityManagerHandler : Diagnostics.Module
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
        internal static DoorVariant escapeLock;
        private void Server_RoundStarted()
        {
            escapeLock = UnityEngine.Object.Instantiate(DoorUtils.GetPrefab(DoorUtils.DoorType.HCZ_BREAKABLE), new Vector3(170, 984, 20), Quaternion.identity);
            GameObject.Destroy(escapeLock.GetComponent<DoorEventOpenerExtension>());
            if (escapeLock.TryGetComponent<Scp079Interactable>(out var scp079Interactable))
                GameObject.Destroy(scp079Interactable);
            escapeLock.transform.localScale = new Vector3(1.7f, 1.5f, 1f);
            if (escapeLock is BasicDoor door)
                door._portalCode = 1;
            escapeLock.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            (escapeLock as BreakableDoor)._brokenPrefab = null;
            escapeLock.gameObject.SetActive(false);
            var scientists = RealPlayers.Get(RoleType.Scientist).ToList();
            if (scientists.Count < 4)
                return;
            scientists = scientists.Where(x => !x.GetSessionVar(Main.SessionVarType.CC_ZONE_MANAGER, false)).ToList();
            DeputyFacalityManager.Instance.Spawn(scientists[UnityEngine.Random.Range(0, scientists.Count)]);
            //170 984 20 0 0 0 1.7 1.5 1
            this.CallDelayed(60 * 12, () =>
            {
                if (DeputyFacalityManager.removeFromVisList == null)
                    DeputyFacalityManager.removeFromVisList = typeof(NetworkConnection).GetMethod("RemoveFromVisList", BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var item in DeputyFacalityManager.Instance.PlayingAsClass)
                {
                    ObjectDestroyMessage msg = new ObjectDestroyMessage
                    {
                        netId = DeputyFacalityManagerHandler.escapeLock.netIdentity.netId
                    };
                    NetworkServer.SendToClientOfPlayer<ObjectDestroyMessage>(item.ReferenceHub.networkIdentity, msg);
                    if (DeputyFacalityManagerHandler.escapeLock.netIdentity.observers.ContainsKey(item.Connection.connectionId))
                    {
                        DeputyFacalityManagerHandler.escapeLock.netIdentity.observers.Remove(item.Connection.connectionId);
                        if (DeputyFacalityManager.removeFromVisList == null)
                            DeputyFacalityManager.removeFromVisList = typeof(NetworkConnection).GetMethod("RemoveFromVisList", BindingFlags.NonPublic | BindingFlags.Instance);
                        DeputyFacalityManager.removeFromVisList?.Invoke(item.Connection, new object[] { DeputyFacalityManagerHandler.escapeLock.netIdentity, true });
                    }
                }
                
            }, "RemoveDoors");
        }

        public class DeputyFacalityManager : CustomClass
        {
            public static DeputyFacalityManager Instance;
            public DeputyFacalityManager() : base() => Instance = this;
            public override Main.SessionVarType ClassSessionVarType => Main.SessionVarType.CC_DEPUTY_FACILITY_MANAGER;

            public override string ClassName => "Zastępca Dyrektora Placówki";

            public override string ClassDescription => "Twoim zadaniem jest pomoc ochronie w odeskortowaniu <color=yellow>naukowców</color>";

            public override RoleType Role => RoleType.Scientist;
            public override void Spawn(Player player)
            {
                player.Role = RoleType.Scientist;
                PlayingAsClass.Add(player);
                player.SetSessionVar(ClassSessionVarType, true);
                bool hasRadio = false;
                foreach (var item in player.Inventory.items.ToArray())
                {
                    if (item.id == ItemType.KeycardScientist || item.id == ItemType.KeycardScientistMajor || item.id == ItemType.Coin)
                        player.RemoveItem(item);
                    else if (item.id == ItemType.Radio)
                        hasRadio = true;
                }
                player.AddItem(new Inventory.SyncItemInfo
                {
                    id = ItemType.KeycardFacilityManager,
                    durability = 1000f
                });
                player.AddItem(new Inventory.SyncItemInfo
                {
                    id = ItemType.WeaponManagerTablet,
                    durability = 401000f
                });
                if (!hasRadio)
                    player.AddItem(ItemType.Radio);
                ArmorHandler.LiteArmor.Give(player, 25);
                Mistaken.Base.CustomInfoHandler.Set(player, "DFM", "<color=#bd1a47><b>Zastępca Dyrektora Placówki</b></color>", false);
                player.SetGUI("DFM", Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, $"<size=150%>Jesteś <color=#bd1a47>Zastępcą Dyrektora Placowki</color></size><br>{ClassDescription}", 20);
                player.SetGUI("DFM_Info", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, "<color=yellow>Grasz</color> jako <color=#bd1a47>Zastępca Dyrektora Placówki</color>");
                RoundLoggerSystem.RoundLogger.Log("CUSTOM CLASSES", "DEPUTY FACILITY MANAGER", $"Spawned {player.PlayerToString()} as Deputy Facility Manager");

                MethodInfo sendSpawnMessage = Server.SendSpawnMessage;
                if (sendSpawnMessage != null)
                {
                    if (player.ReferenceHub.networkIdentity.connectionToClient == null)
                        return;
                    sendSpawnMessage.Invoke(null, new object[]
                    {
                        DeputyFacalityManagerHandler.escapeLock.netIdentity,
                        player.Connection
                    });
                }
            }
            internal static MethodInfo removeFromVisList = null;
            public override void OnDie(Player player)
            {
                base.OnDie(player);
                Mistaken.Base.CustomInfoHandler.Set(player, "DFM", null, false);
                player.SetGUI("DFM_Info", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
                RoundLogger.Log("CUSTOM CLASSES", "DEPUTY FACILITY MANAGER", $"{player.PlayerToString()} is no longer Deputy Facility Manager");

                ObjectDestroyMessage msg = new ObjectDestroyMessage
                {
                    netId = DeputyFacalityManagerHandler.escapeLock.netIdentity.netId
                };
                NetworkServer.SendToClientOfPlayer<ObjectDestroyMessage>(player.ReferenceHub.networkIdentity, msg);
                if (DeputyFacalityManagerHandler.escapeLock.netIdentity.observers.ContainsKey(player.Connection.connectionId))
                {
                    DeputyFacalityManagerHandler.escapeLock.netIdentity.observers.Remove(player.Connection.connectionId);
                    if (removeFromVisList == null)
                        removeFromVisList = typeof(NetworkConnection).GetMethod("RemoveFromVisList", BindingFlags.NonPublic | BindingFlags.Instance);
                    removeFromVisList?.Invoke(player.Connection, new object[] { DeputyFacalityManagerHandler.escapeLock.netIdentity, true });
                }
            }
        }
        public class DeputyFacalityManagerKeycard : CustomItem
        {
            public DeputyFacalityManagerKeycard() => base.Register();
            public override string ItemName => "Karta Zastępcy Dyrektora Placówki";

            public override Main.SessionVarType SessionVarType => Main.SessionVarType.CC_DEPUTY_FACILITY_MANAGER_KEYCARD;

            public override ItemType Item => ItemType.KeycardFacilityManager;

            public override int Durability => 001;

            /// <inheritdoc/>
            public override void OnStartHolding(Player player, Inventory.SyncItemInfo item)
            {
                player.SetGUI("DFM_Keycard", PseudoGUIHandler.Position.BOTTOM, "<color=yellow>Trzymasz</color> kartę <color=#bd1a47>Zastępcy Dyrektora Placówki</color>");
            }
            /// <inheritdoc/>
            public override void OnStopHolding(Player player, Inventory.SyncItemInfo item)
            {
                player.SetGUI("DFM_Keycard", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
            }
        }
    }
}
