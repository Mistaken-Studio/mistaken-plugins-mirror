using Exiled.API.Features;
using Gamer.Utilities;
using System.Collections.Generic;

namespace Gamer.API
{
    namespace CustomClass
    {
        /// <summary>
        /// Custom Class
        /// </summary>
        public abstract class CustomClass
        {
            /// <summary>
            /// All registered custom classes
            /// </summary>
            public static readonly List<CustomClass> CustomClasses = new List<CustomClass>();
            /// <summary>
            /// Default constructor
            /// </summary>
            public CustomClass() => Register();
            /// <summary>
            /// Registeres custom class
            /// </summary>
            protected void Register()
            {
                if (!CustomClasses.Contains(this))
                    CustomClasses.Add(this);
            }
            /// <summary>
            /// List of players that play as class
            /// </summary>
            public readonly HashSet<Player> PlayingAsClass = new HashSet<Player>();
            /// <summary>
            /// SessionVar bound to this custom class
            /// </summary>
            public abstract Main.SessionVarType ClassSessionVarType { get; }
            /// <summary>
            /// Custom class name
            /// </summary>
            public abstract string ClassName { get; }
            /// <summary>
            /// Custom class description
            /// </summary>
            public abstract string ClassDescription { get; }
            /// <summary>
            /// Custom Class Role
            /// </summary>
            public abstract RoleType Role { get; }
            /// <summary>
            /// Custom Class Color
            /// </summary>
            public abstract string Color { get; }
            /// <summary>
            /// Called when resurecting
            /// </summary>
            /// <param name="player">Player that is beeing resurected</param>
            /// <param name="resurecter">Resurecting player</param>
            /// <returns>If resurecting should be allowed</returns>
            public virtual bool OnResurect(Player player, Player resurecter) => true;
            /// <summary>
            /// Used to spawn <paramref name="player"/> as custom class
            /// </summary>
            /// <param name="player">Player to spawn as custom class</param>
            public virtual void Spawn(Player player)
            {
                PlayingAsClass.Add(player);
                player.SetSessionVar(ClassSessionVarType, true);
                player.Role = Role;
            }
            /// <summary>
            /// Called when player playing as custom class dies
            /// </summary>
            /// <param name="player"></param>
            public virtual void OnDie(Player player)
            {
                PlayingAsClass.Remove(player);
                player.SetSessionVar(ClassSessionVarType, false);
            }
            /// <summary>
            /// Called on round restart
            /// </summary>
            public virtual void OnRestart()
            {
                PlayingAsClass.Clear();
            }
        }
    }
}
