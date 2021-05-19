using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Mistaken.Base.CustomItems;
using Gamer.Mistaken.Base.GUI;
using Gamer.Mistaken.Base.Staff;
using Gamer.Mistaken.Systems.Misc;
using Gamer.Utilities;
using MEC;
using Respawning;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class TalkCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "talk";

        public string PluginName => PluginHandler.PluginName;

        public override string Command => "talk";

        public string GetUsage()
        {
            return "Talk [players]";
        }

        public static readonly Queue<string> Warps = new Queue<string>(new string[]
        {
            "jail1",
            //"jail2",
            //"jail3",
            "jail4",
            "jail5",
        });

        internal static readonly List<Vector3> AfterDecontRooms = new List<Vector3>();

        internal static readonly List<Vector3> AfterWarHeadRooms = new List<Vector3>();

        public static readonly Dictionary<string, int[]> Active = new Dictionary<string, int[]>();
        public static readonly Dictionary<int, (Vector3 Pos, RoleType Role, float HP, float AP, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType, (CustomPlayerEffects.PlayerEffect effect, float dur, byte intensity)[] effects)> SavedInfo = new Dictionary<int, (Vector3 Pos, RoleType Role, float HP, float AP, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType, (CustomPlayerEffects.PlayerEffect effect, float dur, byte intensity)[] effects)>();
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            var player = sender.GetPlayer();
            if (Active.TryGetValue(player.UserId, out int[] players))
            {
                foreach (var playerId in players)
                {
                    if (SavedInfo.TryGetValue(playerId, out (Vector3 Pos, RoleType Role, float HP, float AP, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762, int UnitIndex, byte UnitType, (CustomPlayerEffects.PlayerEffect effect, float dur, byte intensity)[] effects) data))
                    {
                        SavedInfo.Remove(playerId);
                        Player p = RealPlayers.Get(playerId);
                        if (p == null)
                            continue;
                        p.DisableAllEffects();
                        p.SetSessionVar(Main.SessionVarType.NO_SPAWN_PROTECT, true);
                        p.SetSessionVar(Main.SessionVarType.CC_IGNORE_CHANGE_ROLE, true);
                        var old = Respawning.RespawnManager.CurrentSequence();
                        Respawning.RespawnManager.Singleton._curSequence = RespawnManager.RespawnSequencePhase.SpawningSelectedTeam;
                        p.Role = data.Role;
                        p.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType = data.UnitType;
                        if (Respawning.RespawnManager.Singleton.NamingManager.TryGetAllNamesFromGroup(data.UnitType, out var array))
                            p.UnitName = array[data.UnitIndex];
                        Respawning.RespawnManager.Singleton._curSequence = old;
                        p.SetSessionVar(Main.SessionVarType.CC_IGNORE_CHANGE_ROLE, false);
                        p.SetSessionVar(Main.SessionVarType.NO_SPAWN_PROTECT, false);
                        Gamer.Utilities.BetterCourotines.CallDelayed(0.5f, () =>
                        {
                            if (!p.IsConnected)
                                return;
                            if (!Warhead.IsDetonated)
                            {
                                if (Map.IsLCZDecontaminated)
                                {
                                    if (!(data.Pos.y > -100 && data.Pos.y < 100))
                                        p.Position = data.Pos;
                                    else
                                        p.Position = AfterDecontRooms[UnityEngine.Random.Range(0, AfterDecontRooms.Count)];
                                }
                                else
                                    p.Position = data.Pos;
                            }
                            else
                            {
                                if (data.Pos.y > 900)
                                    p.Position = data.Pos; 
                                else
                                    p.Position = AfterWarHeadRooms[UnityEngine.Random.Range(0, AfterWarHeadRooms.Count)];
                            }
                            
                            p.Health = data.HP;
                            p.ArtificialHealth = data.AP;
                            p.Inventory.Clear();
                            foreach (var item in data.Inventory)
                            {
                                var citem = CustomItemsHandler.GetCustomItem(item);
                                if (citem != null)
                                {
                                    if (citem is ArmorHandler.Armor armor)
                                    {
                                        ArmorHandler.Armor.Give(p, (int)armor.GetInternalDurability(item.durability));
                                        continue;
                                    }
                                    else if (citem is ArmorHandler.LiteArmor liteArmor)
                                    {
                                        ArmorHandler.LiteArmor.Give(p, (int)liteArmor.GetInternalDurability(item.durability));
                                        continue;
                                    }
                                    else if (citem is ArmorHandler.HeavyArmor heavyArmor)
                                    {
                                        ArmorHandler.HeavyArmor.Give(p, (int)heavyArmor.GetInternalDurability(item.durability));
                                        continue;
                                    }
                                }
                                p.Inventory.items.Add(item);
                            }
                            p.Ammo[(int)AmmoType.Nato9] = data.Ammo9;
                            p.Ammo[(int)AmmoType.Nato556] = data.Ammo556;
                            p.Ammo[(int)AmmoType.Nato762] = data.Ammo762;

                            foreach (var item in data.effects)
                            {
                                item.effect.ServerChangeIntensity(item.intensity);
                                item.effect.ServerChangeDuration(item.dur);
                            }

                            p.SetSessionVar(Main.SessionVarType.TALK, false);
                        }, "TalkRestore");
                    }
                }
                Active.Remove(player.UserId);
            }
            else
            {
                int[] targets = (args[0] + $".{player.Id}").Split('.').Select(i => int.Parse(i)).ToHashSet().ToArray();
                string pos = Warps.Dequeue();
                int counter = 0;
                Warps.Enqueue(pos);
                List<Player> talkPlayers = new List<Player>();
                for (int i = 0; i < targets.Length; i++)
                    talkPlayers.Add(RealPlayers.Get(targets[i]));
                if (talkPlayers.Any(x => x.Side == Side.Scp) && Round.ElapsedTime.TotalSeconds < 35)
                {
                    success = true;
                    return new string[] { "You cannot use this command for the first 35 seconds of the round if one player is an SCP" };
                }
                foreach (var p in talkPlayers)
                {
                    if (p == null || !p.IsConnected)
                        continue;
                    p.SetSessionVar(Main.SessionVarType.TALK, true);
                    p.SetSessionVar(Main.SessionVarType.CC_IGNORE_CHANGE_ROLE, true);
                    SavedInfo.Add(p.Id, (
                        p.Position,
                        p.Role,
                        p.Health,
                        p.ArtificialHealth,
                        p.Inventory.items.ToArray(),
                        p.Ammo[(int)AmmoType.Nato9],
                        p.Ammo[(int)AmmoType.Nato556],
                        p.Ammo[(int)AmmoType.Nato762],
                        RespawnManager.Singleton.NamingManager.AllUnitNames.FindIndex(x => x.UnitName == p.ReferenceHub.characterClassManager.NetworkCurUnitName),
                        p.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType,
                        p.ReferenceHub.playerEffectsController.AllEffects.Values.Select(i => (i, i.Duration, i.Intensity)).ToArray()
                        )
                    );
                    var old = Respawning.RespawnManager.CurrentSequence();
                    Respawning.RespawnManager.Singleton._curSequence = RespawnManager.RespawnSequencePhase.SpawningSelectedTeam;
                    p.Role = RoleType.Tutorial;
                    Respawning.RespawnManager.Singleton._curSequence = old;
                    p.SetSessionVar(Main.SessionVarType.CC_IGNORE_CHANGE_ROLE, false);
                    p.DisableAllEffects();
                    Gamer.Utilities.BetterCourotines.CallDelayed(0.5f, () =>
                    {
                        WarpCommand.ExecuteWarp(p, pos);
                        if (!p.IsStaff())
                        {
                            p.EnableEffect<CustomPlayerEffects.Ensnared>();
                            Gamer.Utilities.BetterCourotines.CallDelayed(0.5f, () =>
                            {
                                p.Position += GetPosByCounter(counter++);
                            }, "TalkTeleport");
                        }
                    }, "TalkEnable");

                }
                Active.Add(player.UserId, targets);
                Gamer.Utilities.BetterCourotines.RunCoroutine(ShowHint(player), "Talk.ShowHint");
            }

            success = true;
            return new string[] { "Done" };
        }

        private Vector3 GetPosByCounter(int counter)
        {
            switch (counter)
            {
                case 0:
                    return new Vector3(0.5f, -0.2f, 0);
                case 1:
                    return new Vector3(0, -0.2f, 0.5f);
                case 2:
                    return new Vector3(-0.5f, -0.2f, 0);
                case 3:
                    return new Vector3(0, -0.2f, -.5f);
                case 4:
                    return new Vector3(0.5f, -0.2f, 0.5f);
                case 5:
                    return new Vector3(0.5f, -0.2f, -0.5f);
                case 6:
                    return new Vector3(-0.5f, -0.2f, 0.5f);
                case 7:
                    return new Vector3(-0.5f, -0.2f, -0.5f);
                default:
                    return new Vector3(0, -0.2f, 0);
            }
        }

        private IEnumerator<float> ShowHint(Player p)
        {
            if (!Active.TryGetValue(p.UserId, out int[] playerIds))
                yield break;
            Player[] players = playerIds.Select(pId => RealPlayers.Get(pId)).ToArray();
            foreach (var player in players)
                player.SetGUI("talk", Gamer.Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, $"<size=150%><color=#F00><b>Trwa przesłuchanie</b></color></size>");
            while (Active.ContainsKey(p.UserId))
                yield return Timing.WaitForSeconds(1f);
            foreach (var player in players)
                player.SetGUI("talk", Gamer.Mistaken.Base.GUI.PseudoGUIHandler.Position.TOP, "<size=150%><color=#F00><b>Przesłuchanie zakończone</b></color></size>", 5);
        }
    }
}
