using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.Events;
using HarmonyLib;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.CITester
{
    [HarmonyPatch(typeof(PlayerPositionManager), "TransmitData")]
    internal static class GhostMode
    {
        private static bool Prefix(PlayerPositionManager __instance)
        {
            return false;
            bool result;
            try
            {
                int num = __instance._frame + 1;
                __instance._frame = num;
                if (num != __instance._syncFrequency)
                {
                    result = false;
                }
                else
                {
                    __instance._frame = 0;
                    List<GameObject> players = PlayerManager.players;
                    __instance._usedData = players.Count;
                    if (__instance._receivedData == null || __instance._receivedData.Length < __instance._usedData)
                    {
                        __instance._receivedData = new PlayerPositionData[__instance._usedData * 2];
                    }
                    for (int i = 0; i < __instance._usedData; i++)
                    {
                        __instance._receivedData[i] = new PlayerPositionData(ReferenceHub.GetHub(players[i]));
                    }
                    if (__instance._transmitBuffer == null || __instance._transmitBuffer.Length < __instance._usedData)
                    {
                        __instance._transmitBuffer = new PlayerPositionData[__instance._usedData * 2];
                    }
                    foreach (GameObject gameObject in players)
                    {
                        Player playerOrServer = GhostMode.GetPlayerOrServer(gameObject);
                        if (playerOrServer != null)
                        {
                            Array.Copy(__instance._receivedData, __instance._transmitBuffer, __instance._usedData);
                            if (playerOrServer.Role.Is939())
                            {
                                for (int j = 0; j < __instance._usedData; j++)
                                {
                                    if (__instance._transmitBuffer[j].position.y < 800f)
                                    {
                                        ReferenceHub hub = ReferenceHub.GetHub(__instance._transmitBuffer[j].playerID);
                                        if (hub.characterClassManager.CurRole.team != Team.SCP && hub.characterClassManager.CurRole.team != Team.RIP && !hub.GetComponent<Scp939_VisionController>().CanSee(playerOrServer.ReferenceHub.characterClassManager.Scp939))
                                        {
                                            GhostMode.MakeGhost(j, __instance._transmitBuffer);
                                        }
                                    }
                                }
                            }
                            else if (playerOrServer.Role != RoleType.Spectator && playerOrServer.Role != RoleType.Scp079)
                            {
                                for (int k = 0; k < __instance._usedData; k++)
                                {
                                    PlayerPositionData playerPositionData = __instance._transmitBuffer[k];
                                    ReferenceHub referenceHub;
                                    if (ReferenceHub.TryGetHub(playerPositionData.playerID, out referenceHub))
                                    {
                                        Player playerOrServer2 = GhostMode.GetPlayerOrServer(referenceHub.gameObject);
                                        if (playerOrServer2 != null)
                                        {
                                            PlayableScps.Scp096 scp = playerOrServer.ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096;
                                            Vector3 vector = playerPositionData.position - playerOrServer.ReferenceHub.playerMovementSync.RealModelPosition;
                                            if (Math.Abs(vector.y) > 35f)
                                            {
                                                GhostMode.MakeGhost(k, __instance._transmitBuffer);
                                            }
                                            else
                                            {
                                                float sqrMagnitude = vector.sqrMagnitude;
                                                if (playerOrServer.ReferenceHub.playerMovementSync.RealModelPosition.y < 800f)
                                                {
                                                    if (sqrMagnitude >= 1764f)
                                                    {
                                                        if (sqrMagnitude >= 4225f)
                                                        {
                                                            GhostMode.MakeGhost(k, __instance._transmitBuffer);
                                                            goto IL_38B;
                                                        }
                                                        PlayableScps.Scp096 scp2 = playerOrServer2.ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096;
                                                        if (scp2 == null || !scp2.EnragedOrEnraging)
                                                        {
                                                            GhostMode.MakeGhost(k, __instance._transmitBuffer);
                                                            goto IL_38B;
                                                        }
                                                    }
                                                }
                                                else if (sqrMagnitude >= 7225f)
                                                {
                                                    GhostMode.MakeGhost(k, __instance._transmitBuffer);
                                                    goto IL_38B;
                                                }
                                                if (scp != null && scp.EnragedOrEnraging && !scp.HasTarget(playerOrServer2.ReferenceHub) && playerOrServer2.Team != Team.SCP)
                                                {
                                                    GhostMode.MakeGhost(k, __instance._transmitBuffer);
                                                }
                                                else if (playerOrServer2.ReferenceHub.playerEffectsController.GetEffect<Scp268>().Enabled)
                                                {
                                                    bool flag = false;
                                                    if (scp != null)
                                                    {
                                                        flag = scp.HasTarget(playerOrServer2.ReferenceHub);
                                                    }
                                                    if (playerOrServer.Role != RoleType.Scp079 && playerOrServer.Role != RoleType.Spectator && !flag)
                                                    {
                                                        GhostMode.MakeGhost(k, __instance._transmitBuffer);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    IL_38B:;
                                }
                            }
                            for (int l = 0; l < __instance._usedData; l++)
                            {
                                PlayerPositionData playerPositionData2 = __instance._transmitBuffer[l];
                                ReferenceHub referenceHub2;
                                if (playerOrServer.Id != playerPositionData2.playerID && !(playerPositionData2.position == GhostMode.GhostPos) && ReferenceHub.TryGetHub(playerPositionData2.playerID, out referenceHub2))
                                {
                                    Player playerOrServer3 = GhostMode.GetPlayerOrServer(referenceHub2.gameObject);
                                    if (playerOrServer3 != null && !(((playerOrServer3 != null) ? playerOrServer3.ReferenceHub : null) == null))
                                    {
                                        if (playerOrServer3.IsInvisible || GhostMode.PlayerCannotSee(playerOrServer, playerOrServer3.Id))
                                        {
                                            GhostMode.MakeGhost(l, __instance._transmitBuffer);
                                        }
                                        else if (playerOrServer.Role == RoleType.Scp173 && ((!Events.Instance.Config.CanTutorialBlockScp173 && playerOrServer3.Role == RoleType.Tutorial) || Scp173.TurnedPlayers.Contains(playerOrServer3)))
                                        {
                                            GhostMode.RotatePlayer(l, __instance._transmitBuffer, GhostMode.FindLookRotation(playerOrServer.Position, playerOrServer3.Position));
                                        }
                                    }
                                }
                            }
                            NetworkConnection networkConnection = playerOrServer.ReferenceHub.characterClassManager.netIdentity.isLocalPlayer ? NetworkServer.localConnection : playerOrServer.ReferenceHub.characterClassManager.netIdentity.connectionToClient;
                            if (__instance._usedData <= 20)
                            {
                                networkConnection.Send<PlayerPositionManager.PositionMessage>(new PlayerPositionManager.PositionMessage(__instance._transmitBuffer, (byte)__instance._usedData, 0), 1);
                            }
                            else
                            {
                                byte b = 0;
                                while ((int)b < __instance._usedData / 20)
                                {
                                    networkConnection.Send<PlayerPositionManager.PositionMessage>(new PlayerPositionManager.PositionMessage(__instance._transmitBuffer, 20, b), 1);
                                    b += 1;
                                }
                                byte b2 = (byte)(__instance._usedData % (int)(b * 20));
                                if (b2 > 0)
                                {
                                    networkConnection.Send<PlayerPositionManager.PositionMessage>(new PlayerPositionManager.PositionMessage(__instance._transmitBuffer, b2, b), 1);
                                }
                            }
                        }
                    }
                    result = false;
                }
            }
            catch (Exception arg)
            {
                Log.Error(string.Format("GhostMode error: {0}", arg));
                result = true;
            }
            return result;
        }

        // Token: 0x0600003B RID: 59 RVA: 0x0000697C File Offset: 0x00004B7C
        private static Vector3 FindLookRotation(Vector3 player, Vector3 target)
        {
            return (target - player).normalized;
        }

        // Token: 0x0600003C RID: 60 RVA: 0x0000223D File Offset: 0x0000043D
        private static bool PlayerCannotSee(Player source, int playerId)
        {
            return source.TargetGhostsHashSet.Contains(playerId) || source.TargetGhosts.Contains(playerId);
        }

        // Token: 0x0600003D RID: 61 RVA: 0x0000225B File Offset: 0x0000045B
        private static void MakeGhost(int index, PlayerPositionData[] buff)
        {
            buff[index] = new PlayerPositionData(GhostMode.GhostPos, buff[index].rotation, buff[index].playerID);
        }

        // Token: 0x0600003E RID: 62 RVA: 0x00006998 File Offset: 0x00004B98
        private static void RotatePlayer(int index, PlayerPositionData[] buff, Vector3 rotation)
        {
            buff[index] = new PlayerPositionData(buff[index].position, Quaternion.LookRotation(rotation).eulerAngles.y, buff[index].playerID);
        }

        // Token: 0x0600003F RID: 63 RVA: 0x00002286 File Offset: 0x00000486
        private static Player GetPlayerOrServer(GameObject gameObject)
        {
            if (!ReferenceHub.GetHub(gameObject).isLocalPlayer)
            {
                return Player.Get(gameObject);
            }
            return Server.Host;
        }

        // Token: 0x04000014 RID: 20
        private static readonly Vector3 GhostPos = Vector3.up * 6000f;
    }
}
