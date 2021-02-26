using Exiled.API.Features;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamer.Utilities
{
    public static class SyncVar
    {
		public static void TargetBadge(this Player player, Player target, string name, string color)
        {
			player.SendCustomSync(target.ReferenceHub.networkIdentity, typeof(ServerRoles), null, (writer) =>
			{
				writer.WritePackedUInt64(3UL);            // DirtyBit
				writer.WriteString(color);                // Color
				writer.WriteString(name);                 // Name
			});
		}

		public static void TargetNickname(this Player player, Player target, string nickname)
		{
			player.SendCustomSync(target.ReferenceHub.networkIdentity, typeof(NicknameSync), null, (writer) =>
			{
				writer.WritePackedUInt64(2UL);            // DirtyBit
				writer.WriteString(nickname);             // Nickname
			});
		}

		public static void SendCustomSync(this Player player, NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncObject, Action<NetworkWriter> customSyncVar)
		{
			/* 

			Example(SyncList) [EffectOnlySCP207]:
			player.SendCustomSync(player.ReferenceHub.networkIdentity, typeof(PlayerEffectsController), (writer) => {
				writer.WritePackedUInt64(1ul);					// DirtyObjectsBit
				writer.WritePackedUInt32((uint)1);				// DirtyIndexCount
				writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);	// Operations
				writer.WritePackedUInt32((uint)0);				// EditIndex
				writer.WriteByte((byte)1);					// Item
			}, null);

			*/
			NetworkWriter writer = NetworkWriterPool.GetWriter();
			NetworkWriter writer2 = NetworkWriterPool.GetWriter();
			MakeCustomSyncWriter(behaviorOwner, targetType, customSyncObject, customSyncVar, writer, writer2);
			NetworkServer.SendToClientOfPlayer(player.ReferenceHub.networkIdentity, new UpdateVarsMessage() { netId = behaviorOwner.netId, payload = writer.ToArraySegment() });
			NetworkWriterPool.Recycle(writer);
			NetworkWriterPool.Recycle(writer2);
		}

		public static void MakeCustomSyncWriter(NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncObject, Action<NetworkWriter> customSyncVar, NetworkWriter owner, NetworkWriter observer)
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

			if (customSyncObject != null)
				customSyncObject.Invoke(owner);
			else
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
	}
}
