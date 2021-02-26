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
using Gamer.Mistaken.Utilities.APILib;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.Systems.Pets
{
    internal class PetsHandler : Module
    {
        public PetsHandler(PluginHandler p) : base(p)
        {
        }

        public override bool Enabled => false;

        public override string Name => "Pets";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Left += this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Left -= this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
        }

        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            if (AlivePets.TryGetValue(ev.Player.UserId, out NPCS.Npc pet))
            {
                pet.Kill(false);
                AlivePets.Remove(ev.Player.UserId);
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            bool hasLivePet = AlivePets.TryGetValue(ev.Player.UserId, out NPCS.Npc pet);
            if (ev.NewRole == RoleType.Spectator && hasLivePet)
            {
                pet.Kill(false);
                AlivePets.Remove(ev.Player.UserId);
            }
            else if(ev.NewRole != RoleType.Spectator && !hasLivePet)
            {
                if(Pets.TryGetValue(ev.Player.UserId, out (RoleType role, string name) petInfo))
                    CreateFolowingNPC(ev.Player, petInfo.role, petInfo.name);
            }
            else if (ev.NewRole == RoleType.Scp173)
                Timing.RunCoroutine(SyncSCP173Speed(ev.Player, pet));
        }

        public static void RefreshPets(Player player)
        {
            if (!player.IsAlive)
                return;
            if (Pets.TryGetValue(player.UserId, out (RoleType role, string name) petInfo))
                CreateFolowingNPC(player, petInfo.role, petInfo.name);
            else if (AlivePets.TryGetValue(player.UserId, out NPCS.Npc value))
            {
                value.Kill(false);
                AlivePets.Remove(player.UserId);
            }
        }

        private static IEnumerator<float> SyncSCP173Speed(Player player, NPCS.Npc pet)
        {
            yield return Timing.WaitForSeconds(1);
            while(player.Role == RoleType.Scp173 && AlivePets.Any(i => i.Key == player.UserId && i.Value.NPCPlayer.Id == pet.NPCPlayer.Id))
            {
                pet.MovementSpeed = player.ReferenceHub.characterClassManager.Scp173.boost_speed.Evaluate(player.ReferenceHub.playerStats.GetHealthPercent());
                yield return Timing.WaitForSeconds(1);
            }
        }

        public static bool SCP173TeleportSound = false;
        public static float Speed = 1.25f;
        public static bool ShoudRun = false;

        public static readonly Dictionary<string, (RoleType role, string name)> Pets = new Dictionary<string, (RoleType role, string name)>();
        public static readonly Dictionary<string, NPCS.Npc> AlivePets = new Dictionary<string, NPCS.Npc>();
        public static Vector3 PetSize = new Vector3(0.3f, 0.3f, 0.3f);
        public static NPCS.Npc CreateFolowingNPC(Player player, RoleType role, string name)
        {
            var npc = NPCS.Methods.CreateNPC(player.Position, Vector2.zero, PetSize, role, ItemType.None, name);
            npc.AIEnabled = true;
            npc.Follow(player);
            npc.NPCPlayer.ReferenceHub.nicknameSync.CustomPlayerInfo = $"{player.Nickname}'s pet";
            npc.NPCPlayer.RankName = "PET";
            npc.DisableDialogSystem = true;
            npc.NPCPlayer.IsGodModeEnabled = true;
            npc.IsRunning = ShoudRun;
            npc.MovementSpeed *= Speed;
            npc.DisableRun = false;
            npc.AffectRoundSummary = false;
            npc.DontCleanup = true;
            npc.ShouldTrigger096 = false;
            if(role == RoleType.Scp106) 
                npc.ProcessSCPLogic = true;
            else 
                npc.ProcessSCPLogic = false;
            if(AlivePets.TryGetValue(player.UserId, out NPCS.Npc value))
            {
                value.Kill(false);
                AlivePets.Remove(player.UserId);
            }
            AlivePets.Add(player.UserId, npc);
            if (player.Role == RoleType.Scp173)
                Timing.RunCoroutine(SyncSCP173Speed(player, npc));
            return npc;
        }
    }
}
