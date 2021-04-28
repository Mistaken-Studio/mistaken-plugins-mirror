using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.Events.EventArgs;

namespace Exiled.Events
{
	namespace Handlers
	{
		public static class CustomEvents
		{
			public static event Exiled.Events.Events.CustomEventHandler<PickItemRequestEventArgs> OnRequestPickItem;

			public static void InvokeOnRequestPickItem(ref PickItemRequestEventArgs ev)
			{
				OnRequestPickItem?.Invoke(ev);
			}
			public static event Exiled.Events.Events.CustomEventHandler<TransmitPositionEventArgs> OnTransmitPositionData;

			public static void InvokeOnTransmitPositionData(ref TransmitPositionEventArgs ev)
			{
				OnTransmitPositionData?.Invoke(ev);
			}

			public static event Exiled.Events.Events.CustomEventHandler<BroadcastEventArgs> OnBroadcast;

			public static void InvokeOnBroadcast(ref BroadcastEventArgs ev)
			{
				OnBroadcast?.Invoke(ev);
			}

			public static event Exiled.Events.Events.CustomEventHandler<RAExecutedEventArgs> OnRAExecuted;

			public static void InvokeOnRAExecuted(ref RAExecutedEventArgs ev)
			{
				OnRAExecuted?.Invoke(ev);
			}

			public static event Exiled.Events.Events.CustomEventHandler<FirstTimeJoinedEventArgs> OnFirstTimeJoined;

			public static void InvokeOnFirstTimeJoined(FirstTimeJoinedEventArgs ev)
			{
				OnFirstTimeJoined?.Invoke(ev);
			}

			public static class SCP079
			{
				public static int TimeToRecontainment;

				public static bool IsBeingRecontained
				{
					get
					{
						return TimeToRecontainment > -1;
					}
				}

				public static bool IsRecontainmentPaused;
			}
		}
	}

	namespace EventArgs
	{
		public class PickItemRequestEventArgs : System.EventArgs
		{
			public API.Features.Player Player { get; }
			public Pickup Pickup { get; }
			public bool IsAllowed { get; set; } = true;

			public PickItemRequestEventArgs(API.Features.Player player, Pickup pickup)
			{
				Player = player;
				Pickup = pickup;
				IsAllowed = true;
			}
		}
		public class TransmitPositionEventArgs : System.EventArgs
		{
			public API.Features.Player Player { get; }
			public PlayerPositionData[] PositionMessages { get; }

			public TransmitPositionEventArgs(API.Features.Player player, PlayerPositionData[] positionMessages)
			{
				Player = player;
				PositionMessages = positionMessages;
			}
		}
		public class BroadcastEventArgs : System.EventArgs
		{
			public Broadcast.BroadcastFlags Type { get; }
			public string Content { get; }
			public string AdminName { get; }
			public string[] Targets { get; }

			public BroadcastEventArgs(Broadcast.BroadcastFlags type, string content, string adminName, string[] targets)
			{
				Type = type;
				Content = content;
				AdminName = adminName;
				Targets = targets;
			}
		}
		public class RAExecutedEventArgs : System.EventArgs
		{
			public string Query { get; }
			public string UserId { get; }
			public string Result { get; }

			public RAExecutedEventArgs(string query, string userId, string result)
			{
				Query = query;
				UserId = userId;
				Result = result;
			}
		}
		public class FirstTimeJoinedEventArgs : System.EventArgs
		{
			public Player Player { get; }

			public FirstTimeJoinedEventArgs(Player player)
			{
				this.Player = player;
			}
		}
	}
}