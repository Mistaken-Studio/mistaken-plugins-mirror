using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using NPCS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Pets
{
    internal class PetsHandler : Module
    {
        private new static __Log Log;
        public PetsHandler(PluginHandler p) : base(p)
        {
            Log = base.Log;
        }

        //public override bool Enabled => false;

        public override string Name => "Pets";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Left += this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.TriggeringTesla += this.Handle<Exiled.Events.EventArgs.TriggeringTeslaEventArgs>((ev) => Player_TriggeringTesla(ev));
            if (Server.Port % 2 == 1)
                Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Left -= this.Handle<Exiled.Events.EventArgs.LeftEventArgs>((ev) => Player_Left(ev));
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Exiled.Events.Handlers.Player.TriggeringTesla -= this.Handle<Exiled.Events.EventArgs.TriggeringTeslaEventArgs>((ev) => Player_TriggeringTesla(ev));
            if (Server.Port % 2 == 1)
                Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (ev.Target.UserId == null)
                return;
            if (AlivePets.TryGetValue(ev.Target.UserId, out NPCS.Npc pet))
            {
                pet.VisibleForRoles.Clear();
            }
        }

        private void Player_TriggeringTesla(Exiled.Events.EventArgs.TriggeringTeslaEventArgs ev)
        {
            if (ev.Player?.IsNPC() ?? false)
                ev.IsTriggerable = false;
        }

        private void Server_RestartingRound()
        {
            PetsIds.Clear();
        }

        private static Func<Player, Action<Player>> OnEnter = (player1) => (player2) =>
        {
            if (!player1.IsAlive)
                return;
            if (!player2.IsNPC())
                return;
            if (AlivePets.TryGetValue(player1.UserId, out Npc npc) && npc.NPCPlayer == player2)
                return;
            player1.TargetGhostsHashSet.Add(player2.Id);
            //player2.DisplayNickname = "Hidden";
        };
        private static Func<Player, Action<Player>> OnExit = (player1) => (player2) =>
        {
            if (!player2.IsNPC())
                return;
            player1.TargetGhostsHashSet.Remove(player2.Id);
            //player2.DisplayNickname = "Normal";
        };
        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (ev.Player.IsReadyPlayer())
                Components.InRageBall.Spawn(ev.Player.GameObject.transform, Vector3.zero, 2, 4, OnEnter(ev.Player), OnExit(ev.Player)).AllowNPCs = true;
        }

        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            if (AlivePets.TryGetValue(ev.Player.UserId, out NPCS.Npc pet))
            {
                PetsIds.Remove(pet.NPCPlayer.Id);
                pet.Kill(false);
                AlivePets.Remove(ev.Player.UserId);
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            MEC.Timing.CallDelayed(0.5f, () =>
            {
                bool hasLivePet = AlivePets.TryGetValue(ev.Player.UserId, out NPCS.Npc pet);
                if (ev.NewRole == RoleType.Spectator && hasLivePet)
                {
                    PetsIds.Remove(pet.NPCPlayer.Id);
                    pet.Kill(false);
                    AlivePets.Remove(ev.Player.UserId);
                }
                else if (ev.NewRole != RoleType.Spectator)
                {
                    if (Pets.TryGetValue(ev.Player.UserId, out (RoleType role, string name) petInfo))
                        CreateFolowingNPC(ev.Player, petInfo.role, petInfo.name);
                    else if (hasLivePet)
                    {
                        PetsIds.Remove(pet.NPCPlayer.Id);
                        pet.Kill(false);
                        AlivePets.Remove(ev.Player.UserId);
                    }
                }
                //else if (ev.NewRole == RoleType.Scp173)
                //    Timing.RunCoroutine(SyncSCP173Speed(ev.Player, pet));
            });
        }
        public static readonly HashSet<int> PetsIds = new HashSet<int>();
        public static void RefreshPets(Player player)
        {
            MEC.Timing.CallDelayed(0.5f, () =>
            {
                if (!player.IsAlive)
                    return;
                if (Pets.TryGetValue(player.UserId, out (RoleType role, string name) petInfo))
                    CreateFolowingNPC(player, petInfo.role, petInfo.name);
                else if (AlivePets.TryGetValue(player.UserId, out NPCS.Npc value))
                {
                    PetsIds.Remove(value.NPCPlayer.Id);
                    value.Kill(false);
                    AlivePets.Remove(player.UserId);
                }
            });
        }

        private static IEnumerator<float> SyncSCP173Speed(Player player, NPCS.Npc pet)
        {
            yield return Timing.WaitForSeconds(1);
            while (player.Role == RoleType.Scp173 && AlivePets.Any(i => i.Key == player.UserId && i.Value.NPCPlayer.Id == pet.NPCPlayer.Id))
            {
                pet.MovementSpeed = player.ReferenceHub.characterClassManager.Scp173.boost_speed.Evaluate(player.ReferenceHub.playerStats.GetHealthPercent()) * 1.1f;
                yield return Timing.WaitForSeconds(1);
            }
        }

        public static float Speed = 1.5f;
        public static bool ShoudRun = false;

        public static readonly Dictionary<string, (RoleType role, string name)> Pets = new Dictionary<string, (RoleType role, string name)>();
        public static readonly Dictionary<string, NPCS.Npc> AlivePets = new Dictionary<string, NPCS.Npc>();
        public static Vector3 PetSize = new Vector3(0.3f, 0.3f, 0.3f);
        public static NPCS.Npc CreateFolowingNPC(Player player, RoleType role, string name)
        {
            //if(!player.IsDev())
            role = player.Role;
            var npc = NPCS.Methods.CreateNPC(player.Position, Vector2.zero, PetSize, role, ItemType.None, name ?? "(NULL)");
            //npc.VisibleForPlayers = new HashSet<Player>();
            npc.VisibleForRoles = new HashSet<RoleType>();
            //npc.VisibleForPlayers.Add(player);
            npc.VisibleForRoles.Add(RoleType.Tutorial);
            npc.VisibleForRoles.Add(RoleType.Spectator);
            switch (role)
            {
                case RoleType.ChaosInsurgency:
                    npc.VisibleForRoles.Add(RoleType.ChaosInsurgency);
                    npc.VisibleForRoles.Add(RoleType.ClassD);
                    break;
                case RoleType.ClassD:
                    npc.VisibleForRoles.Add(RoleType.ChaosInsurgency);
                    npc.VisibleForRoles.Add(RoleType.ClassD);
                    npc.VisibleForRoles.Add(RoleType.Scientist);
                    break;
                case RoleType.Scientist:
                    npc.VisibleForRoles.Add(RoleType.FacilityGuard);
                    npc.VisibleForRoles.Add(RoleType.NtfCadet);
                    npc.VisibleForRoles.Add(RoleType.NtfLieutenant);
                    npc.VisibleForRoles.Add(RoleType.NtfScientist);
                    npc.VisibleForRoles.Add(RoleType.NtfCommander);
                    npc.VisibleForRoles.Add(RoleType.Scientist);

                    npc.VisibleForRoles.Add(RoleType.ClassD);
                    break;
                case var r when r.GetTeam() == Team.MTF:
                    npc.VisibleForRoles.Add(RoleType.FacilityGuard);
                    npc.VisibleForRoles.Add(RoleType.NtfCadet);
                    npc.VisibleForRoles.Add(RoleType.NtfLieutenant);
                    npc.VisibleForRoles.Add(RoleType.NtfScientist);
                    npc.VisibleForRoles.Add(RoleType.NtfCommander);
                    npc.VisibleForRoles.Add(RoleType.Scientist);
                    break;
                case var r when r.GetTeam() == Team.SCP:
                    npc.VisibleForRoles.Add(RoleType.Scp049);
                    npc.VisibleForRoles.Add(RoleType.Scp0492);
                    npc.VisibleForRoles.Add(RoleType.Scp079);
                    npc.VisibleForRoles.Add(RoleType.Scp096);
                    npc.VisibleForRoles.Add(RoleType.Scp106);
                    npc.VisibleForRoles.Add(RoleType.Scp173);
                    npc.VisibleForRoles.Add(RoleType.Scp93953);
                    npc.VisibleForRoles.Add(RoleType.Scp93989);
                    break;
                default:
                    npc.VisibleForRoles.Clear();
                    break;
            }
            npc.AIEnabled = true;
            npc.Follow(player);
            npc.DisableRun = true;
            MEC.Timing.CallDelayed(0.5f, () =>
            {
                try
                {
                    //npc.NPCPlayer.ReferenceHub.animationController.Network_curMoveState = 1;
                    //Timing.RunCoroutine(ForceAnim(npc));
                    npc.NPCPlayer.ReferenceHub.nicknameSync.CustomPlayerInfo = $"{player.GetDisplayName()}'s pet";
                    npc.NPCPlayer.RankName = "PET";
                    PetsIds.Add(npc.NPCPlayer.Id);
                    npc.DisableDialogSystem = true;
                    npc.NPCPlayer.IsGodModeEnabled = true;
                    npc.IsRunning = ShoudRun;
                    npc.MovementSpeed *= Speed;
                    npc.DisableRun = false;
                    npc.AffectRoundSummary = false;
                    npc.DontCleanup = true;
                    npc.ShouldTrigger096 = false;
                    if (role == RoleType.Scp106)
                        npc.ProcessSCPLogic = true;
                    else
                        npc.ProcessSCPLogic = false;
                    if (AlivePets.TryGetValue(player.UserId, out NPCS.Npc value))
                    {
                        PetsIds.Remove(value.NPCPlayer.Id);
                        value.Kill(false);
                        AlivePets.Remove(player.UserId);
                    }
                    AlivePets.Add(player.UserId, npc);
                    if (player.Role == RoleType.Scp173)
                        Timing.RunCoroutine(SyncSCP173Speed(player, npc));
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
            });
            return npc;
        }

        private static IEnumerator<float> ForceAnim(NPCS.Npc npc)
        {
            while (npc.NPCPlayer?.IsAlive ?? false)
            {
                yield return Timing.WaitForSeconds(0.5f);
                npc.NPCPlayer.ReferenceHub.animationController.Network_curMoveState = 1;
            }
        }
    }
}
