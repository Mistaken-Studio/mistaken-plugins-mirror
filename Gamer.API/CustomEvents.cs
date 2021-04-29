using Exiled.API.Features;
using Exiled.Events.EventArgs;

namespace Exiled.Events
{
    namespace Handlers
    {
        /// <summary>
        /// Custom Events
        /// </summary>
        public static class CustomEvents
        {
            /// <summary>
            /// Event called when requesting Item Pickup
            /// </summary>
            public static event Exiled.Events.Events.CustomEventHandler<PickItemRequestEventArgs> OnRequestPickItem;
            /// <summary>
            /// Invokes <see cref="OnRequestPickItem"/> with <paramref name="ev"/> as parameter
            /// </summary>
            public static void InvokeOnRequestPickItem(ref PickItemRequestEventArgs ev)
            {
                OnRequestPickItem?.Invoke(ev);
            }
            /// <summary>
            /// Event called when broadcast was sent
            /// </summary>
            public static event Exiled.Events.Events.CustomEventHandler<BroadcastEventArgs> OnBroadcast;
            /// <summary>
            /// Invokes <see cref="OnBroadcast"/> with <paramref name="ev"/> as parameter
            /// </summary>
            public static void InvokeOnBroadcast(ref BroadcastEventArgs ev)
            {
                OnBroadcast?.Invoke(ev);
            }
            /// <summary>
            /// Event called when player joins first time in row
            /// </summary>
            public static event Exiled.Events.Events.CustomEventHandler<FirstTimeJoinedEventArgs> OnFirstTimeJoined;
            /// <summary>
            /// Invokes <see cref="OnRequestPickItem"/> with <paramref name="ev"/> as parameter
            /// </summary>
            public static void InvokeOnFirstTimeJoined(FirstTimeJoinedEventArgs ev)
            {
                OnFirstTimeJoined?.Invoke(ev);
            }
            /// <summary>
            /// Data about SCP079
            /// </summary>
            public static class SCP079
            {
                /// <summary>
                /// Time left to end of recontainment or -1 if not in proggres
                /// </summary>
                public static int TimeToRecontainment;

                /// <summary>
                /// Is recontainment in proggres
                /// </summary>
                public static bool IsBeingRecontained
                {
                    get
                    {
                        return TimeToRecontainment > -1;
                    }
                }
                /// <summary>
                /// Is recontainment paused
                /// </summary>
                public static bool IsRecontainmentPaused;
            }
        }
    }

    namespace EventArgs
    {
        /// <inheritdoc/>
        public class PickItemRequestEventArgs : System.EventArgs
        {
            /// <summary>
            /// Player that pickups
            /// </summary>
            public API.Features.Player Player { get; }
            /// <summary>
            /// Pickup
            /// </summary>
            public Pickup Pickup { get; }
            /// <summary>
            /// If picking is allowed
            /// </summary>
            public bool IsAllowed { get; set; } = true;
            /// <summary>
            /// Constructor
            /// </summary>
            public PickItemRequestEventArgs(API.Features.Player player, Pickup pickup)
            {
                Player = player;
                Pickup = pickup;
                IsAllowed = true;
            }
        }
        /// <inheritdoc/>
        public class BroadcastEventArgs : System.EventArgs
        {
            /// <summary>
            /// Broadcast type
            /// </summary>
            public Broadcast.BroadcastFlags Type { get; }
            /// <summary>
            /// Broadcast content
            /// </summary>
            public string Content { get; }
            /// <summary>
            /// Admin Name
            /// </summary>
            public string AdminName { get; }
            /// <summary>
            /// Broadcast Targets
            /// </summary>
            public string[] Targets { get; }
            /// <summary>
            /// Constructor
            /// </summary>
            public BroadcastEventArgs(Broadcast.BroadcastFlags type, string content, string adminName, string[] targets)
            {
                Type = type;
                Content = content;
                AdminName = adminName;
                Targets = targets;
            }
        }
        /// <inheritdoc/>
        public class FirstTimeJoinedEventArgs : System.EventArgs
        {
            /// <summary>
            /// Player that joins
            /// </summary>
            public Player Player { get; }
            /// <summary>
            /// Constructor
            /// </summary>
            public FirstTimeJoinedEventArgs(Player player)
            {
                Player = player;
            }
        }
    }
}