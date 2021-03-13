using Exiled.API.Features;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Extensions;

namespace Gamer.Utilities
{
    public static class SyncVar
    {
		/// <summary>
		/// Zmienia rangę <paramref name="player"/> dla <paramref name="target"/>
		/// </summary>
		/// <param name="player">Osob</param>
		/// <param name="target">Osoba która ma widzieć inną rangę</param>
		/// <param name="name">Nazwa</param>
		/// <param name="color">Kolor</param>
		public static void TargetSetBadge(this Player player, Player target, string name, string color)
        {
			MirrorExtensions.SendFakeSyncVar(target, player.ReferenceHub.networkIdentity, typeof(ServerRoles), nameof(ServerRoles.NetworkMyText), name);
			MirrorExtensions.SendFakeSyncVar(target, player.ReferenceHub.networkIdentity, typeof(ServerRoles), nameof(ServerRoles.NetworkMyColor), color);
		}

		public static void TargetSetNickname(this Player player, Player target, string nickname)
		{
			MirrorExtensions.SendFakeSyncVar(target, player.ReferenceHub.networkIdentity, typeof(NicknameSync), nameof(NicknameSync.Network_displayName), nickname);
		}

		public static void SetSpeed(this Player player, float walkSpeed, float sprintSpeed)
		{
			//MirrorExtensions.SendFakeSyncVar(player, Server.Host.ReferenceHub.networkIdentity, typeof(ServerConfigSynchronizer), nameof(ServerConfigSynchronizer.NetworkHumanWalkSpeedMultiplier), walkSpeed);
			//MirrorExtensions.SendFakeSyncVar(player, Server.Host.ReferenceHub.networkIdentity, typeof(ServerConfigSynchronizer), nameof(ServerConfigSynchronizer.NetworkHumanSprintSpeedMultiplier), sprintSpeed);
			SendCustomSync(player, ServerConfigSynchronizer.Singleton.netIdentity, typeof(ServerConfigSynchronizer), (writer) =>
			{
				writer.WritePackedUInt64(6UL);
				writer.WriteSingle(walkSpeed);
				writer.WriteSingle(sprintSpeed);
			});
		}

		//Only for internal for Speed
		private static void SendCustomSync(this Player player, NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncVar)
		{
			NetworkWriter owner = NetworkWriterPool.GetWriter();
			NetworkWriter observer = NetworkWriterPool.GetWriter();
			MakeCustomSyncWriter(behaviorOwner, targetType, customSyncVar, owner, observer);
			NetworkServer.SendToClientOfPlayer(player.ReferenceHub.networkIdentity, new UpdateVarsMessage() { netId = behaviorOwner.netId, payload = owner.ToArraySegment() });
			NetworkWriterPool.Recycle(owner);
			NetworkWriterPool.Recycle(observer);
		}

		//Only for internal for Speed
		private static void MakeCustomSyncWriter(NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncVar, NetworkWriter owner, NetworkWriter observer)
		{
			ulong dirty = 0ul;
			ulong dirty_o = 0ul;
			NetworkBehaviour behaviour = null;
			for (int i = 0; i < behaviorOwner.NetworkBehaviours.Length; i++)
			{
				behaviour = behaviorOwner.NetworkBehaviours[i];
				if (behaviour.GetType() == targetType)
				{
					dirty |= 1UL << i;
					if (behaviour.syncMode == SyncMode.Observers) dirty_o |= 1UL << i;
				}
			}
			owner.WritePackedUInt64(dirty);
			observer.WritePackedUInt64(dirty & dirty_o);

			int position = owner.Position;
			owner.WriteInt32(0);
			int position2 = owner.Position;

			behaviour.SerializeObjectsDelta(owner);

			customSyncVar?.Invoke(owner);

			int position3 = owner.Position;
			owner.Position = position;
			owner.WriteInt32(position3 - position2);
			owner.Position = position3;

			if (dirty_o != 0ul)
			{
				ArraySegment<byte> arraySegment = owner.ToArraySegment();
				observer.WriteBytes(arraySegment.Array, position, owner.Position - position);
			}
		}

		public static void ChangeAppearance(this Player player, Player target, RoleType type)
		{
			MirrorExtensions.SendFakeSyncVar(target, player.ReferenceHub.networkIdentity, typeof(CharacterClassManager), nameof(CharacterClassManager.NetworkCurClass), (sbyte)type);
		}
	}
}
