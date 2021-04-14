using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Mistaken.Systems.Staff;
using Gamer.Utilities;
using MEC;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace Gamer.Mistaken.CommandsExtender.Commands
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))] 
    class TalkCommand : IBetterCommand, IPermissionLocked
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
        public static readonly Dictionary<string, int[]> Active = new Dictionary<string, int[]>();
        public static readonly Dictionary<int, (Vector3 Pos, RoleType Role, float HP, float AP, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762)> SavedInfo = new Dictionary<int, (Vector3 Pos, RoleType Role, float HP, float AP, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762)>();
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            var player = sender.GetPlayer();
            if(Active.TryGetValue(player.UserId, out int[] players))
            {
                foreach (var playerId in players)
                {
                    if(SavedInfo.TryGetValue(playerId, out (Vector3 Pos, RoleType Role, float HP, float AP, Inventory.SyncItemInfo[] Inventory, uint Ammo9, uint Ammo556, uint Ammo762) data))
                    {
                        SavedInfo.Remove(playerId);
                        Player p = RealPlayers.Get(playerId);
                        if (p == null)
                            continue;
                        p.DisableAllEffects();
                        p.SetSessionVar(Main.SessionVarType.NO_SPAWN_PROTECT, true);
                        p.Role = data.Role;
                        p.SetSessionVar(Main.SessionVarType.NO_SPAWN_PROTECT, false);
                        Timing.CallDelayed(0.5f, () =>
                        {
                            if (!p.IsConnected)
                                return;
                            if(!Warhead.IsDetonated)
                            {
                                if(!(data.Pos.y > -100 && data.Pos.y < 100 && Map.IsLCZDecontaminated))
                                    p.Position = data.Pos;
                            }
                            
                            p.Health = data.HP;
                            p.ArtificialHealth = data.AP;
                            p.Inventory.Clear();
                            foreach (var item in data.Inventory)
                                p.Inventory.items.Add(item);
                            p.Ammo[(int)AmmoType.Nato9] = data.Ammo9;
                            p.Ammo[(int)AmmoType.Nato556] = data.Ammo556;
                            p.Ammo[(int)AmmoType.Nato762] = data.Ammo762;
                            p.SetSessionVar(Main.SessionVarType.TALK, false);
                        });
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
                foreach (var playerId in targets)
                {
                    var p = RealPlayers.Get(playerId);
                    if (p == null || !p.IsConnected)
                        continue;
                    p.SetSessionVar(Main.SessionVarType.TALK, true);
                    SavedInfo.Add(playerId, (p.Position, p.Role, p.Health, p.ArtificialHealth, p.Inventory.items.ToArray(), p.Ammo[(int)AmmoType.Nato9], p.Ammo[(int)AmmoType.Nato556], p.Ammo[(int)AmmoType.Nato762]));
                    p.Role = RoleType.Tutorial;
                    p.DisableAllEffects();
                    Timing.CallDelayed(0.5f, () =>
                    {
                        WarpCommand.ExecuteWarp(p, pos);
                        if (!p.IsStaff())
                        {
                            p.EnableEffect<CustomPlayerEffects.Ensnared>();
                            Timing.CallDelayed(0.5f, () =>
                            {
                                p.Position += GetPosByCounter(counter++);
                            });
                        }
                    });
                    
                }
                Active.Add(player.UserId, targets);
                Timing.RunCoroutine(ShowHint(player));
            }

            success = true;
            return new string[] { "Done" };
        }

        private Vector3 GetPosByCounter(int counter)
        {
            switch(counter)
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
                Gamer.Mistaken.Systems.GUI.PseudoGUIHandler.Set(player, "talk", Gamer.Mistaken.Systems.GUI.PseudoGUIHandler.Position.TOP, $"<size=150%><color=#F00><b>Trwa przesłuchanie</b></color></size>");
            while (Active.ContainsKey(p.UserId))
                yield return Timing.WaitForSeconds(1f);
            foreach (var player in players)
                Gamer.Mistaken.Systems.GUI.PseudoGUIHandler.Set(player, "talk", Gamer.Mistaken.Systems.GUI.PseudoGUIHandler.Position.TOP, "<size=150%><color=#F00><b>Przesłuchanie zakończone</b></color></size>", 5);
        }
    }
}
