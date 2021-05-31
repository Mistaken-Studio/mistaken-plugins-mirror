using Exiled.API.Interfaces;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.Staff;
using Gamer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xname.Radio
{
    internal class RadioHandler : Module
    {
        /// <inheritdoc/>
        public RadioHandler(IPlugin<IConfig> plugin) : base(plugin)
        {
        }
        /// <inheritdoc/>
        public override string Name => "RadioHandler";
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangingRadioPreset += this.Handle<Exiled.Events.EventArgs.ChangingRadioPresetEventArgs>((ev) => Player_ChangingRadioPreset(ev));
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.ChangingRadioPreset -= this.Handle<Exiled.Events.EventArgs.ChangingRadioPresetEventArgs>((ev) => Player_ChangingRadioPreset(ev));
        }
        private void Player_ChangingRadioPreset(Exiled.Events.EventArgs.ChangingRadioPresetEventArgs ev)
        {
            foreach (var d in RealPlayers.List.Where(x => x.IsActiveDev()))
            {
                d.SendConsoleMessage($"{ev.NewValue}", "green");
            }
        }
    }
}
