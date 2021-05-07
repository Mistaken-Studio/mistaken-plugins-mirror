using Gamer.Diagnostics;
using Gamer.Utilities;

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
            Exiled.Events.Handlers.Player.Escaping += this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
            Exiled.Events.Handlers.Player.Escaping -= this.Handle<Exiled.Events.EventArgs.EscapingEventArgs>((ev) => Player_Escaping(ev));
        }

        private void Player_Escaping(Exiled.Events.EventArgs.EscapingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if (ev.Player.GetSessionVar<bool>(Main.SessionVarType.CC_IGNORE_CHANGE_ROLE))
                return;
            foreach (var item in API.CustomClass.CustomClass.CustomClasses)
            {
                if (item.PlayingAsClass.Contains(ev.Player))
                {
                    item.OnDie(ev.Player);
                    break;
                }
            }
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
            if (ev.IsEscaped)
                return;
            if (ev.Player.GetSessionVar<bool>(Main.SessionVarType.CC_IGNORE_CHANGE_ROLE))
                return;
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
