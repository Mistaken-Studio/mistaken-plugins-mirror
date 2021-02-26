using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Gamer.Utilities;
using Gamer.Mistaken.BetterSCP;
using HarmonyLib;
using MEC;
using UnityEngine;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.BetterSCP.SCP0492
{
    class SCP0492Handler : Diagnostics.Module
    {
        public SCP0492Handler(PluginHandler p) : base(p)
        {
        }

        public override string Name => nameof(SCP0492Handler);
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            
        }

        private void Server_RoundStarted()
        {
            
        }

        public static void Spawn(Room room) => Timing.RunCoroutine(SpawnNPCZombie(room));

        private static IEnumerator<float> SpawnNPCZombie(Room room)
        {
            var zombie = NPCS.Methods.CreateNPC(room.Position + Vector3.up, Vector2.zero, Vector3.one, RoleType.Scp0492, ItemType.None, "Zombie");

            zombie.NPCPlayer.ReferenceHub.playerMovementSync._hub = zombie.NPCPlayer.ReferenceHub;

            //zombie.AddNavTarget(NPCS.Navigation.NavigationNode.FromRoom());
            var AIFindTarget = NPCS.AI.AITarget.GetFromToken("AIFindPlayerTarget");
            AIFindTarget.Arguments["range"] = "20";
            AIFindTarget.Arguments["allow_self_select"] = "false";
            AIFindTarget.Arguments["role_whitelist"] = "1";
            AIFindTarget.Arguments["role_blacklist"] = "0";// "0,3,5,7,9,10,16,17";

            AIFindTarget.Arguments["target_npc"] = "false";
            AIFindTarget.Arguments["target_godmode"] = "true";
            AIFindTarget.Arguments["filter"] = "common";
            AIFindTarget.Construct();

            var AIAttackTarget = NPCS.AI.AITarget.GetFromToken("AIAttackTarget");
            AIAttackTarget.Arguments["accuracy"] = "40";
            AIAttackTarget.Arguments["damage"] = "25";
            AIAttackTarget.Arguments["firerate"] = "1";
            AIAttackTarget.Arguments["hitboxes"] = "BODY: 100";
            AIAttackTarget.Arguments["use_ammo"] = "false";
            AIAttackTarget.Construct();

            var AINavigateToRoom = NPCS.AI.AITarget.GetFromToken("AINavigateToRoom");
            AINavigateToRoom.Arguments["safe"] = "true";
            AINavigateToRoom.Arguments["room"] = "random";
            AINavigateToRoom.Construct();

            zombie.OnTargetLostBehaviour = NPCS.Npc.TargetLostBehaviour.SEARCH;
            zombie.AIQueue.AddLast(AIFindTarget);
            zombie.AIQueue.AddLast(AIAttackTarget);
            //zombie.AIQueue.AddLast(AINavigateToRoom);
            zombie.AIEnabled = true;

            while (zombie.NPCPlayer.IsAlive)
            {
                yield return Timing.WaitForSeconds(1);
            }
            yield break;
        }
    }
}
