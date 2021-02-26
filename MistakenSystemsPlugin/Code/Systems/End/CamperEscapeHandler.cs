using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using Grenades;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.Systems.End
{
    internal class CamperEscapeHandler : Module
    {
        public CamperEscapeHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => "CamperEscape";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            bool hasFacilityManager = ev.Items.Any(i => i == ItemType.KeycardFacilityManager);
            int weapons = 0;
            foreach (var item in ev.Items)
            {
                if (item.IsWeapon(false))
                {
                    if (weapons > 2) 
                        ev.Items.Remove(item);
                    else 
                        weapons++;
                }
                else if(hasFacilityManager)
                {
                    if(item == ItemType.KeycardChaosInsurgency || item == ItemType.KeycardNTFCommander || item == ItemType.KeycardNTFLieutenant)
                    {
                        ev.Items.Remove(item);
                        ev.Items.Remove(ItemType.KeycardFacilityManager);
                        ev.Items.Add(ItemType.KeycardO5);
                        hasFacilityManager = false;
                    }
                }
            }
        }

        private void Server_RoundStarted()
        {
            foreach (var door in Map.Doors)
            {
                if (door.Type() == DoorType.Scp012Bottom)
                    Center012 = door.transform.position + (door.transform.right * 1.5f) + (door.transform.forward * 8.5f);
            }
            Timing.RunCoroutine(CheckCamper());
            Timing.RunCoroutine(CheckEscape());  
        }

        private Vector3 Center012 = new Vector3();

        private IEnumerator<float> CheckCamper()
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId; 
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                foreach (Player player in RealPlayers.List.Where(p => p.IsAlive && p.Role != RoleType.Scp079))
                {
                    Vector3 pos = player.Position;
                    
                    if (((pos.x >= 172 && pos.x <= 185 && pos.z >= 23 && pos.z <= 42) || (pos.x >= 164 && pos.x <= 185 && pos.z >= 32 && pos.z <= 42)) && pos.y >= 980 && pos.y <= 1000)
                    {
                        switch (player.Team)
                        {
                            case Team.RIP:
                            case Team.CDP:
                            case Team.RSC:
                                {
                                    break;
                                }
                            default:
                                {
                                    if (player.IsCuffed || player.UserId == "") 
                                        break;

                                    player.ShowHint("<color=red><b>Warning!</b></color><br>If you stay here you will receive <b>damage</b>", 1);
                                    if (player.Team == Team.SCP) 
                                        player.Hurt(10, new DamageTypes.DamageType("*Anty Camper"));
                                    else 
                                        player.Hurt(2, new DamageTypes.DamageType("*Anty Camper"));
                                    Log.Debug("Done damage to " + player.Nickname + " because of unauthorizated escape!");
                                    break;
                                }
                        }
                    }
                    else if ((pos.x >= 12 && pos.x <= 15 && pos.z <= -18 && pos.z >= -22) && pos.y >= 1000 && pos.y <= 1005)
                    {
                        player.ShowHint("<color=red><b>Warning!</b></color><br>If you stay here you will receive <b>damage</b>", 1);
                        if (player.Team == Team.SCP) 
                            player.Hurt(10, new DamageTypes.DamageType("*Anty Camper"));
                        else 
                            player.Hurt(5, new DamageTypes.DamageType("*Anty Camper"));
                        Log.Debug("Done damage to " + player.Nickname + " because of unauthorizated camping!");
                    }
                    else
                    {
                        if ((Center012.x - 0.5f <= pos.x && Center012.x + 1 >= pos.x && Center012.z - 0.5f <= pos.z && Center012.z + 0.5f >= pos.z) && pos.y >= -10 && pos.y <= -2)
                        {
                            switch (player.Team)
                            {
                                case Team.RIP:
                                case Team.SCP:
                                    break;
                                default:
                                    {
                                        player.ShowHint("<color=red><b>Warning!</b></color><br>If you stay here you will recive <b>damage</b>", 1);
                                        player.Hurt(5, new DamageTypes.DamageType("*Anty Camper"));
                                        Log.Debug("Done damage to " + player.Nickname + " because of unauthorizated 012!");
                                        break;
                                    }
                            }
                        }
                    }
                }
                yield return Timing.WaitForSeconds(1f);
            }
        }

        private IEnumerator<float> CheckEscape()
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId; while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                foreach (Player player in RealPlayers.List)
                {
                    Vector3 pos = player.Position;
                    if (player.IsCuffed && pos.x >= 174 && pos.x <= 183 && pos.y >= 980 && pos.y <= 990 && pos.z >= 25 && pos.z <= 34)
                    {
                        switch (player.Team)
                        {
                            case Team.CHI:
                                {
                                    //player.CufferId = -1;
                                    Respawning.RespawnTickets.Singleton.GrantTickets(Respawning.SpawnableTeamType.NineTailedFox, 2);
                                    player.ReferenceHub.characterClassManager.SetPlayersClass(RoleType.NtfLieutenant, player.GameObject, false, true);
                                    break;
                                }
                            case Team.MTF:
                                {
                                    //player.CufferId = -1;
                                    Respawning.RespawnTickets.Singleton.GrantTickets(Respawning.SpawnableTeamType.ChaosInsurgency, 2);
                                    player.ReferenceHub.characterClassManager.SetPlayersClass(RoleType.ChaosInsurgency, player.GameObject, false, true);
                                    break;
                                }
                        }
                    }
                }
                yield return Timing.WaitForSeconds(1f);
            }
        }
    }
}
