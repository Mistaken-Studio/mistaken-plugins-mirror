using Gamer.Diagnostics;

namespace Gamer.CustomClasses
{
    /// <inheritdoc/>
    public class CustomClassesHandler : Module
    {
        /// <inheritdoc/>
        public CustomClassesHandler(PluginHandler plugin) : base(plugin)
        {
        }
        /// <inheritdoc/>
        public override string Name => "CustomClasses";
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            foreach (var item in API.CustomClass.CustomClass.CustomClasses)
            {
                if (item.PlayingAsClass.Contains(ev.Target))
                {
                    item.OnDie(ev.Target);
                    break;
                }
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            foreach (var item in API.CustomClass.CustomClass.CustomClasses)
            {
                if (ev.NewRole == item.Role)
                    continue;
                if (item.PlayingAsClass.Contains(ev.Player))
                {
                    item.OnDie(ev.Player);
                    break;
                }
            }
        }
    }
}
