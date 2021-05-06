using HarmonyLib;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Patches
{
    [HarmonyPatch(typeof(RagdollManager), "SpawnRagdoll")]
    public static class RagdollManagerPatch
    {
        public static bool Prefix(RagdollManager __instance, Vector3 pos, Quaternion rot, Vector3 velocity, int classId, PlayerStats.HitInfo ragdollInfo, bool allowRecall, string ownerID, string ownerNick, int playerId)
        {
            global::Role role = __instance.hub.characterClassManager.Classes.SafeGet(classId);
            if (role.model_ragdoll == null) return false;
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(role.model_ragdoll, pos + role.ragdoll_offset.position, Quaternion.Euler(rot.eulerAngles + role.ragdoll_offset.rotation));
            //gameObject.transform.localScale = Player.Get(__instance.hub).Scale;
            NetworkServer.Spawn(gameObject);
            global::Ragdoll ragdoll = gameObject.GetComponent<global::Ragdoll>();
            ragdoll.Networkowner = new global::Ragdoll.Info(ownerID, ownerNick, ragdollInfo, role, playerId);
            ragdoll.NetworkallowRecall = allowRecall;
            ragdoll.NetworkPlayerVelo = velocity;
            var item = new RagdollInfo
            {
                ragdoll = ragdoll,
                OwnerNickname = ownerNick,
                Role = role.roleId,
                Team = role.team,
                DeathTime = DateTime.Now
            };
            Ragdolls.Add(item);
            Gamer.Utilities.BetterCourotines.CallDelayed(30, () =>
            {
                Ragdolls.Remove(item);
            }, "RagdollsPatch Remove");
            return false;
        }

        public class RagdollInfo
        {
            public Ragdoll ragdoll;
            public string OwnerNickname;
            public RoleType Role;
            public Team Team;
            public DateTime DeathTime;
        }
        public static readonly List<RagdollInfo> Ragdolls = new List<RagdollInfo>();
    }
}
