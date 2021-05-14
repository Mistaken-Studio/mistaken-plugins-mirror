using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.API.CustomClass;
using Gamer.API.CustomItem;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Mistaken.Base.Staff;
using Gamer.Mistaken.Ranks;
using Gamer.Mistaken.Systems.Misc;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using MEC;
using Respawning;
using Respawning.NamingRules;
using Scp914;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.CustomClasses
{
    /// <inheritdoc/>
    public class TAU5Handler : Module
    {
        /// <inheritdoc/>
        public TAU5Handler(PluginHandler plugin) : base(plugin)
        {
            new Tau5Soldier();
        }
        /// <inheritdoc/>
        public override string Name => "Tau-5";
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.UnlockingGenerator += this.Handle<Exiled.Events.EventArgs.UnlockingGeneratorEventArgs>((ev) => Player_UnlockingGenerator(ev));
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
            Exiled.Events.Handlers.Player.UnlockingGenerator -= this.Handle<Exiled.Events.EventArgs.UnlockingGeneratorEventArgs>((ev) => Player_UnlockingGenerator(ev));
        }
        /// <inheritdoc/>
        public class Tau5Soldier : CustomClass
        {
            /// <summary>
            /// Instance
            /// </summary>
            public static Tau5Soldier Instance;
            /// <inheritdoc/>
            public Tau5Soldier() : base() => Instance = this;  
            /// <inheritdoc/>
            public override Main.SessionVarType ClassSessionVarType => Main.SessionVarType.CC_TAU5;
            /// <inheritdoc/>
            public override string ClassName => "Tau-5 Samsara Soldier";
            /// <inheritdoc/>
            public override string ClassDescription => "Twoje zadanie: <color=red>Zneutralizować wszystko poza personelem fundacji</color><br><b>Karta O5 jest wbudowana w twoją rękę</b>, więc <color=yellow>możesz otwierać <b>wszystkie</b> drzwi nie używając karty</color>";
            /// <inheritdoc/>
            public override RoleType Role => RoleType.NtfCommander;
            /// <inheritdoc/>
            public override string Color => "#C00";
            /// <inheritdoc/>
            public override void Spawn(Player player)
            {
                player.InfoArea &= ~PlayerInfoArea.Role;
                PlayingAsClass.Add(player);
                player.SetSessionVar(ClassSessionVarType, true);
                var old = Respawning.RespawnManager.CurrentSequence();
                Respawning.RespawnManager.Singleton._curSequence = RespawnManager.RespawnSequencePhase.SpawningSelectedTeam;
                player.Role = this.Role;
                player.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType = 2;
                player.UnitName = Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames.Last().UnitName;
                Respawning.RespawnManager.Singleton._curSequence = old;
                player.ClearInventory();
                //player.AddItem(ItemType.KeycardO5);
                player.AddItem(ItemType.GunE11SR);
                player.AddItem(ItemType.GunProject90);
                player.AddItem(ItemType.SCP500);
                player.AddItem(ItemType.Radio);
                player.AddItem(ItemType.Medkit);
                player.AddItem(ItemType.Medkit);
                player.AddItem(ItemType.GrenadeFrag);
                player.AddItem(new Inventory.SyncItemInfo
                {
                    id = ItemType.WeaponManagerTablet,
                    durability = 401000f
                });
                player.Ammo[(int)AmmoType.Nato556] = 500;
                player.Ammo[(int)AmmoType.Nato9] = 500;
                player.Ammo[(int)AmmoType.Nato762] = 500;
                player.Health = 500;
                player.ArtificialHealth = 50;
                Mistaken.Systems.Shield.ShieldedManager.Add(new Mistaken.Systems.Shield.Shielded(player, 50, 0.25f, 30, 0, -1));
                player.UnitName = Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames.Last().UnitName;
                Mistaken.Base.CustomInfoHandler.Set(player, "TAU5", "<color=#C00><b>Zołnierz Tau-5 Samsara</b></color>");
                player.SetGUI("TAU5", Mistaken.Base.GUI.PseudoGUIHandler.Position.MIDDLE, $"<size=150%>Jesteś <color=#C00>Zołnierzem Tau-5 Samsara</color></size><br>{ClassDescription}", 20);
                player.SetGUI("TAU5_Info", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, "<color=yellow>Grasz</color> jako <color=#C00>Zołnierz Tau-5 Samsara</color>");
                RoundLoggerSystem.RoundLogger.Log("CUSTOM CLASSES", "TAU-5", $"Spawned {player.PlayerToString()} as Tau-5 Samsara Soldier");
            }

            /// <inheritdoc/>
            public override void OnDie(Player player)
            {
                base.OnDie(player);
                Mistaken.Base.CustomInfoHandler.Set(player, "TAU5", null);
                player.SetGUI("TAU5_Info", Mistaken.Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
                RoundLogger.Log("CUSTOM CLASSES", "TAU-5", $"{player.PlayerToString()} is no longer Tau-5 Samsara Soldier");
            }
        }

        private void Player_UnlockingGenerator(Exiled.Events.EventArgs.UnlockingGeneratorEventArgs ev)
        {
            if (Tau5Soldier.Instance.PlayingAsClass.Contains(ev.Player))
                ev.IsAllowed = true;
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (Tau5Soldier.Instance.PlayingAsClass.Contains(ev.Player))
                ev.IsAllowed = true;
        }
    }
}
