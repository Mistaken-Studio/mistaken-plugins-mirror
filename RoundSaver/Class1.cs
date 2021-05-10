using Exiled.API.Features;
using Gamer.Diagnostics;
using MapGeneration;
using MEC;
using Mirror;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.RoundSaver
{
    public class PluginHandler : Plugin<API.Config>
    {
        public override string Name => "RoundSaver";
        public override string Author => "Gamer";
        public override void OnDisabled()
        {
            base.OnDisabled();
        }
        public override void OnEnabled()
        {
            var harmony = new HarmonyLib.Harmony("gamer.roundsaver");
            harmony.PatchAll();
            Exiled.Events.Handlers.Server.RoundEnded += Server_RoundEnded;
            Exiled.Events.Handlers.Server.RestartingRound += Server_RestartingRound;
            Exiled.Events.Handlers.Server.RoundStarted += Server_RoundStarted;
            Exiled.Events.Handlers.Player.Verified += Player_Verified;
            Exiled.Events.Handlers.Server.SendingConsoleCommand += Server_SendingConsoleCommand;
            base.OnEnabled();
        }

        private void Server_RestartingRound()
        {
            stream?.Close();
            stream = null;
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            stream?.Close();
            stream = null;
        }

        private void Server_SendingConsoleCommand(Exiled.Events.EventArgs.SendingConsoleCommandEventArgs ev)
        {
            if(ev.Name == "rsm")
            {
                //ev.Player.IsOverwatchEnabled = true;
                Gamer.Utilities.BetterCourotines.RunCoroutine(Replay(ev.Arguments[0]), "RoundSaver.Replay");
                ev.IsAllowed = true;
                ev.ReturnMessage = "Playing";
            }
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (PluginHandler.stream == null) return;
            var rawms = Round.ElapsedTime.TotalMilliseconds;
            int ms = (int)(rawms - (rawms % 1));
            var toWrite = Encoding.ASCII.GetBytes($"{ms}|CPly|{ev.Player.Id}|{ev.Player.UserId}\n");
            PluginHandler.stream.WriteAsync(toWrite, 0, toWrite.Length);
        }
        public static bool replay = false;
        public static int seed;
        public static FileStream stream;
        private void Server_RoundStarted()
        {
            if (replay)
                return;
            stream = File.OpenWrite(Path.Combine(Paths.Plugins, "RoundSaver", Server.Port.ToString(), $"Round_{DateTime.Now:yyyy_MM_dd_HH:mm:ss}.log"));
            var toWrite = Encoding.ASCII.GetBytes($"{SeedSynchronizer.Seed}\n");
            PluginHandler.stream.WriteAsync(toWrite, 0, toWrite.Length);
            foreach (var item in Player.IdsCache)
            {
                toWrite = Encoding.ASCII.GetBytes($"0|CPly|{item.Key}|{item.Value.UserId}\n");
                PluginHandler.stream.WriteAsync(toWrite, 0, toWrite.Length);
            }
        }

        private IEnumerator<float> Replay(string file)
        {
            replay = true;
            Log.Debug($"A2");
            string[] data;
            try
            {
                data = File.ReadAllLines(Path.Combine(Paths.Plugins, "RoundSaver", Server.Port.ToString(), file));
            }
            catch(System.Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                Log.Debug($"E1");
                yield break;
            }
            Log.Debug($"A3");
            Queue<(Operation operation, object value)> operations = new Queue<(Operation operation, object value)>();
            int lastTime = 0;
            seed = int.Parse(data[0]);
            Round.Restart(true, true);
            Round.IsLobbyLocked = true;
            yield return Timing.WaitForSeconds(25);
            Round.IsLocked = true;
            Round.Start();
            yield return Timing.WaitForSeconds(5);
            foreach (var item in data.Skip(1))
            {
                var tmp = item.Split('|');
                if (tmp.Length == 0)
                {
                    Log.Warn($"Empty line: {item}");
                    continue;
                }
                var time = int.Parse(tmp[0]);
                if(time > lastTime)
                {
                    operations.Enqueue((Operation.WAIT, time - lastTime));
                    lastTime = time;
                }
                switch (tmp[1])
                {
                    case "UPos":
                        operations.Enqueue((Operation.UPDATE_PLAYER_POSITION, (int.Parse(tmp[2]), float.Parse(tmp[3]), float.Parse(tmp[4]), float.Parse(tmp[5]), float.Parse(tmp[6]))));
                        break;
                    case "CPly":
                        operations.Enqueue((Operation.CREATE_PLAYER, (int.Parse(tmp[2]), tmp[3])));
                        break;
                    default:
                        Log.Warn($"Unknown operation: {tmp[1]}");
                        continue;
                }
            }
            Log.Debug($"A4");
            Log.Warn($"Operations: {operations.Count}");
            (Operation operation, object value) operation;
            Dictionary<int, NPCS.Npc> players = new Dictionary<int, NPCS.Npc>();
            while(operations.TryDequeue(out operation))
            {
                Log.Debug(operation.operation);
                switch(operation.operation)
                {
                    case Operation.WAIT:
                        yield return Timing.WaitForSeconds(((int)operation.value) / 1000);
                        break;
                    case Operation.UPDATE_PLAYER_POSITION:
                        {
                            Log.Debug("UP1");
                            var value = ((int pId, float x, float y, float z, float rot))operation.value;
                            Log.Debug("UP2");
                            if (players.TryGetValue(value.pId, out NPCS.Npc player))
                            {
                                Log.Debug("UP3");
                                player.NPCPlayer.Position = new Vector3(value.x, value.y, value.z);
                                Log.Debug("UP4");
                                player.NPCPlayer.Rotations = new Vector2(value.rot, player.NPCPlayer.Rotations.y);
                                Log.Debug("UP5");
                            }
                            Log.Debug("UP6");
                            break;
                        }
                    case Operation.CREATE_PLAYER:
                        {
                            var value = ((int pId, string uId))operation.value;
                            players[value.pId] = NPCS.Methods.CreateNPC(Vector3.zero, Vector2.zero, Vector3.one, RoleType.ClassD, ItemType.None, value.uId);
                            players[value.pId].IsActionLocked = true;
                            Timing.KillCoroutines(players[value.pId].MovementCoroutines.ToArray());
                            break;
                        }
                } 
            }
            yield return Timing.WaitForSeconds(10);
            foreach (var item in Player.IdsCache.ToArray())
            {
                if (item.Key > 1000) 
                    NetworkServer.Destroy(item.Value.GameObject);
            }
            replay = false;
            Log.Debug($"E0");
        }

        public enum Operation
        {
            WAIT,
            UPDATE_PLAYER_POSITION,
            CREATE_PLAYER
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(SeedSynchronizer), nameof(SeedSynchronizer.Start))]
    public class SeedPatch
    {
        static bool Prefix(SeedSynchronizer __instance)
        {
            if (!PluginHandler.replay)
                return true;
            __instance.Network_syncSeed = PluginHandler.seed;
            return false;
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(PlayerPositionManager), nameof(PlayerPositionManager.TransmitData))]
    public class TransmitionPatch
    {
        static void Postfix(PlayerPositionManager __instance)
        {
            if (PluginHandler.stream == null)
                return;
            var rawms = Round.ElapsedTime.TotalMilliseconds;
            int ms = (int)(rawms - (rawms % 1));
            foreach (var item in __instance._transmitBuffer)
            {
                if (item.playerID == 0) 
                    continue;
                var toWrite = Encoding.ASCII.GetBytes($"{ms}|UPos|{item.playerID}|{item.position.x}|{item.position.y}|{item.position.z}|{item.rotation}\n");
                PluginHandler.stream.WriteAsync(toWrite, 0, toWrite.Length);
            }
        }
    }
}
