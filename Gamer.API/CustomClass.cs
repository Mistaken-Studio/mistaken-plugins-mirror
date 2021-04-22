using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using Scp914;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.API
{
    namespace CustomClass
    {
        public abstract class CustomClass
        {
            public static readonly List<CustomClass> CustomClasses = new List<CustomClass>();
            public CustomClass() => Register();
            protected void Register()
            {
                if (!CustomClasses.Contains(this))
                    CustomClasses.Add(this);
            }
            public readonly HashSet<Player> PlayingAsClass = new HashSet<Player>();
            public abstract Main.SessionVarType ClassSessionVarType { get; }
            public abstract string ClassName { get; }
            public abstract string ClassDescription { get; }
            public abstract RoleType Role { get; }
            public virtual bool OnResurect(Player player, Player resurecter) => true;
            public virtual void Spawn(Player player)
            {
                PlayingAsClass.Add(player);
                player.SetSessionVar(ClassSessionVarType, true);
                player.Role = Role;
            }
            public virtual void OnDie(Player player)
            {
                PlayingAsClass.Remove(player);
                player.SetSessionVar(ClassSessionVarType, false);
            }
        }
    }
}
