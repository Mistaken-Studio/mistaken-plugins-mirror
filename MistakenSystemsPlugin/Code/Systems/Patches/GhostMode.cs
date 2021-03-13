using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAdmin;
using HarmonyLib;
using UnityEngine;
using Exiled.API.Features;
using Gamer.Mistaken.Utilities.APILib;
using PlayableScps;
using CustomPlayerEffects;
using Mirror;
using Exiled.Events;
using Gamer.Utilities;


namespace Gamer.Mistaken.Systems.Patches
{
    //[HarmonyPatch(typeof(PlayerPositionManager), "TransmitData")] //Check ?
	[System.Obsolete("Disabled", true)]
	public static class GhostMode
	{
		public static bool Prefix(PlayerPositionManager __instance)
		{
			bool result;
			try
			{
				DateTime start = DateTime.Now;
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
						try
						{
							Player playerOrServer = null;
							try
							{
								if (gameObject == null)
									continue;
								if (NPCS.Npc.Dictionary == null || NPCS.Npc.Dictionary.ContainsKey(gameObject))
									continue;
								playerOrServer = GhostMode.GetPlayerOrServer(gameObject);
								if (playerOrServer?.GameObject == null || !playerOrServer.IsConnected)
								{
									Log.Warn("GhostMode patch Skip Code: 1.1");
									continue;
								}
							}
							catch (System.Exception ex)
							{
								Log.Error("[CRITICAL] First Checks GhostMode patch [CRITICAL]");
								Log.Error(ex.Message);
								Log.Error(ex.StackTrace);
								if (ex.InnerException != null)
								{
									Log.Error("Inner");
									Log.Error(ex.InnerException.Message);
									Log.Error(ex.InnerException.StackTrace);
								}
								continue;
							}
							Array.Copy(__instance._receivedData, __instance._transmitBuffer, __instance._usedData);
							if (playerOrServer.Role.Is939())
							{
								try
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
								catch (System.Exception ex)
								{
									Log.Error("[CRITICAL] SCP939 GhostMode patch [CRITICAL]");
									Log.Error(ex.Message);
									Log.Error(ex.StackTrace);
									if (ex.InnerException != null)
									{
										Log.Error("Inner");
										Log.Error(ex.InnerException.Message);
										Log.Error(ex.InnerException.StackTrace);
									}
									continue;
								}
							}
							else if (playerOrServer.Role != RoleType.Spectator && playerOrServer.Role != RoleType.Scp079)
							{
								try
								{
									for (int k = 0; k < __instance._usedData; k++)
									{
										PlayerPositionData playerPositionData = __instance._transmitBuffer[k];
										ReferenceHub referenceHub;
										if (ReferenceHub.TryGetHub(playerPositionData.playerID, out referenceHub))
										{
											Player playerOrServer2 = GhostMode.GetPlayerOrServer(referenceHub.gameObject);
											if (playerOrServer2?.GameObject == null || !playerOrServer2.IsConnected)
											{
												Log.Warn("GhostMode patch Skip Code: 1.2");
												continue;
											}
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
															goto IL_37D;
														}
														PlayableScps.Scp096 scp2 = playerOrServer2.ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096;
														if (scp2 == null || !scp2.EnragedOrEnraging)
														{
															GhostMode.MakeGhost(k, __instance._transmitBuffer);
															goto IL_37D;
														}
													}
												}
												else if (sqrMagnitude >= 7225f)
												{
													GhostMode.MakeGhost(k, __instance._transmitBuffer);
													goto IL_37D;
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
										IL_37D:;
									}
								}
								catch (System.Exception ex)
								{
									Log.Error("[CRITICAL] Alive GhostMode patch [CRITICAL]");
									Log.Error($"{playerOrServer?.ToString(true, false)}");
									Log.Error($"{playerOrServer?.IsConnected} | {playerOrServer?.GameObject}");
									Log.Error(ex.Message);
									Log.Error(ex.StackTrace);
									if (ex.InnerException != null)
									{
										Log.Error("Inner");
										Log.Error(ex.InnerException.Message);
										Log.Error(ex.InnerException.StackTrace);
									}
									continue;
								}
							}
							for (int l = 0; l < __instance._usedData; l++)
							{
								try
								{
									PlayerPositionData playerPositionData2 = __instance._transmitBuffer[l];
									ReferenceHub referenceHub2;
									if (playerOrServer.Id != playerPositionData2.playerID && !(playerPositionData2.position == GhostMode.GhostPos) && ReferenceHub.TryGetHub(playerPositionData2.playerID, out referenceHub2))
									{
										Player playerOrServer3 = GhostMode.GetPlayerOrServer(referenceHub2.gameObject);
										if (playerOrServer3?.GameObject == null || !playerOrServer3.IsConnected)
										{
											Log.Warn("GhostMode patch Skip Code: 1.3");
											continue;
										}
										if (!(((playerOrServer3 != null) ? playerOrServer3.ReferenceHub : null) == null))
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
								catch (System.Exception ex)
								{
									Log.Error("[CRITICAL] Else GhostMode patch [CRITICAL]");
									Log.Error(ex.Message);
									Log.Error(ex.StackTrace);
									if (ex.InnerException != null)
									{
										Log.Error("Inner");
										Log.Error(ex.InnerException.Message);
										Log.Error(ex.InnerException.StackTrace);
									}
									continue;
								}
							}

							/*try
							{
								var ev = new Exiled.Events.EventArgs.TransmitPositionEventArgs(playerOrServer, __instance._transmitBuffer);
								Exiled.Events.Handlers.CustomEvents.InvokeOnTransmitPositionData(ref ev);
								__instance._transmitBuffer = ev.PositionMessages;
							}
							catch (System.Exception ex)
							{
								Log.Error("[CRITICAL] Event GhostMode patch [CRITICAL]");
								Log.Error(ex.Message);
								Log.Error(ex.StackTrace);
								if (ex.InnerException != null)
								{
									Log.Error("Inner");
									Log.Error(ex.InnerException.Message);
									Log.Error(ex.InnerException.StackTrace);
								}
							}*/

							try
							{
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
							catch (System.Exception ex)
							{
								Log.Error("[CRITICAL] Send GhostMode patch [CRITICAL]");
								Log.Error(ex.Message);
								Log.Error(ex.StackTrace);
								if (ex.InnerException != null)
								{
									Log.Error("Inner");
									Log.Error(ex.InnerException.Message);
									Log.Error(ex.InnerException.StackTrace);
								}
								continue;
							}
						}
						catch (System.Exception ex)
						{
							Log.Error("[CRITICAL] General GhostMode patch [CRITICAL]");
							Log.Error(ex.Message);
							Log.Error(ex.StackTrace);
							if (ex.InnerException != null)
							{
								Log.Error("Inner");
								Log.Error(ex.InnerException.Message);
								Log.Error(ex.InnerException.StackTrace);
							}
							continue;
						}
					}
					result = false;
				}
				Diagnostics.MasterHandler.LogTime($"{PluginHandler.PluginName}.Patch", "GhostMode", start, DateTime.Now);
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("GhostMode error: {0}", arg));
				result = true;
			}
			return result;
		}

		internal static Vector3 FindLookRotation(Vector3 player, Vector3 target) => 
			(target - player).normalized;
		internal static bool PlayerCannotSee(Player source, int playerId) => 
			source.TargetGhostsHashSet.Contains(playerId) || source.TargetGhosts.Contains(playerId); //source can't see playerId
		internal static void MakeGhost(int index, PlayerPositionData[] buff) => 
			buff[index] = new PlayerPositionData(GhostMode.GhostPos, buff[index].rotation, buff[index].playerID);
		internal static void RotatePlayer(int index, PlayerPositionData[] buff, Vector3 rotation) =>
			buff[index] = new PlayerPositionData(buff[index].position, Quaternion.LookRotation(rotation).eulerAngles.y, buff[index].playerID);
		internal static readonly Vector3 GhostPos = Vector3.up * 6000f;
		private static Player GetPlayerOrServer(GameObject gameObject)
		{
			if (gameObject == null)
			{
				return null;
			}
			if (!ReferenceHub.GetHub(gameObject).isLocalPlayer)
			{
				return Player.Get(gameObject);
			}
			return Server.Host;
		}
	}
}
