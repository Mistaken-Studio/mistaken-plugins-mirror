using Exiled.API.Enums;
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
using Exiled.API.Extensions;
using Gamer.Diagnostics;
using Interactables.Interobjects.DoorUtils;

namespace Gamer.Mistaken.Systems.Misc
{
    internal class ClassDCellsDecontaminationHandler : Module
    {
        public ClassDCellsDecontaminationHandler(PluginHandler p) : base(p)
        {
        }

        //This is broken, and private cassie is not working | Disabled
        public override bool Enabled => false;
        public override string Name => "ClassDCellsDecontamination";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }

        private void Server_RestartingRound()
        {
            RoundId++;
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(DoCycle(RoundId));
        }

        public static bool Decontaminated = false;
        public static int DecontaminatedIn = 0;
        private int RoundId = 0;
        private IEnumerator<float> DoCycle(int rId)
        {
            Decontaminated = false;
            var cdRoom = Map.Rooms.FirstOrDefault(d => d.Type == RoomType.LczClassDSpawn);
            if (cdRoom == null)
                yield break;
            int time = UnityEngine.Random.Range(90, 300);
            DecontaminatedIn = time + 40 + 18;
            while(time > 0)
            {
                time--;
                DecontaminatedIn--;
                yield return Timing.WaitForSeconds(1);
            }
            Log.Info("Decontamination Checkpoint Passed");
            while (Cassie.IsSpeaking)
                yield return Timing.WaitForOneFrame;
            if (rId != RoundId)
                yield break;
            if (!PluginHandler.Config.IsHardRP()) InformOnlyInLCZ("Warning . ClassD Chamber Decontamination in T minus 1 minute");
            time = 40;
            while (time > 0)
            {
                time--;
                DecontaminatedIn--;
                yield return Timing.WaitForSeconds(1);
            }
            while (Cassie.IsSpeaking)
                yield return Timing.WaitForOneFrame;
            if (rId != RoundId)
                yield break;
            if (!PluginHandler.Config.IsHardRP()) InformOnlyInLCZ("Warning . ClassD Chamber Decontamination in T minus 10 yield_1 9 yield_1 8 yield_1 7 yield_1 6 yield_1 5 yield_1 4 yield_1 3 yield_1 2 yield_1 1 yield_3");
            time = 18;
            while (time > 0)
            {
                time--;
                DecontaminatedIn--;
                yield return Timing.WaitForSeconds(1);
            }
            while (Cassie.IsSpeaking)
                yield return Timing.WaitForOneFrame;
            if (rId != RoundId)
                yield break;
            Decontaminated = true;
            Cassie.Message("ClassD Chamber DECONTAMINATION Successful", false, Mistaken.PluginHandler.Config.IsHardRP());
            foreach (var item in Map.Doors)
            {
                if(item.name.Contains("PrisonDoor"))
                {
                    item.NetworkTargetState = false;
                    item.NetworkActiveLocks = (byte)DoorLockReason.AdminCommand;
                }
            }

            int rid = RoundPlus.RoundId; while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                if (rId != RoundId)
                    break;
                foreach (var item in GameObject.FindGameObjectsWithTag("SP_CDP"))
                {
                    Vector3 roomPos = new Vector3(item.transform.position.x, 0, item.transform.position.z);
                    foreach (var player in cdRoom.Players?.ToArray() ?? new Player[0])
                    {
                        Vector3 playerPos = new Vector3(player.Position.x, 0, player.Position.z);
                        if (Vector3.Distance(playerPos, roomPos) < (RightSpawns.Contains(item.name) ? 3.8f : 2.9f) || ((playerPos - roomPos) * (RightSpawns.Contains(item.name) ? -1 : 1)).z > 1.5f)
                            player.ReferenceHub.playerEffectsController.EnableEffect<CustomPlayerEffects.Decontaminating>();
                    }
                }

                yield return Timing.WaitForSeconds(10);
            }
        }

        private static readonly string[] RightSpawns = new string[]
        {
            "SP_DCP",
            "SP_DCP (1)",
            "SP_DCP (2)",
            "SP_DCP (3)",
            "SP_DCP (4)",
            "SP_DCP (5)",
            "SP_DCP (6)",
        };
    
        private static void InformOnlyInLCZ(string message)
        {
            foreach (var player in RealPlayers.List.Where(p => p.CurrentRoom?.Zone == ZoneType.LightContainment).ToArray())
            {
                try
                {
                    player.PrivateCassie(message);
                }
                catch(System.Exception ex) 
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                    Log.Error("Inner:");
                    Log.Error(ex.InnerException?.Message);
                    Log.Error(ex.InnerException?.StackTrace);
                }
            }
        }
    }
}
