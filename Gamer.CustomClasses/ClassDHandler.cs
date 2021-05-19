using Exiled.Events.EventArgs;
using Gamer.Diagnostics;
using Gamer.Utilities;

namespace Gamer.CustomClasses
{
    internal class ClassDHandler : Diagnostics.Module
    {
        public ClassDHandler(PluginHandler p) : base(p)
        {
        }
        public override string Name => "ClassD";

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.ChangedRole -= this.Handle<Exiled.Events.EventArgs.ChangedRoleEventArgs>((ev) => Player_ChangedRole(ev));
        }
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangedRole += this.Handle<Exiled.Events.EventArgs.ChangedRoleEventArgs>((ev) => Player_ChangedRole(ev));
        }

        private void Player_ChangedRole(ChangedRoleEventArgs ev)
        {
            if (ev.Player.Role != RoleType.ClassD)
            {
                ev.Player.SetSpeed(ServerConfigSynchronizer.Singleton.HumanWalkSpeedMultiplier, ServerConfigSynchronizer.Singleton.HumanSprintSpeedMultiplier);
            }
            else
            {
                int rand = UnityEngine.Random.Range(-5, 5);
                Log.Debug(rand);
                ev.Player.MaxHealth -= rand * 3;
                ev.Player.Health = ev.Player.MaxHealth;
                ev.Player.SetSpeed(ServerConfigSynchronizer.Singleton.HumanWalkSpeedMultiplier * (1f + rand / 100f), ServerConfigSynchronizer.Singleton.HumanSprintSpeedMultiplier * (1f + rand / 100f));
            }
        }
    }
}
