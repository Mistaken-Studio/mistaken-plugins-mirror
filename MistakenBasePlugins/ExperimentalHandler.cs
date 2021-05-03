﻿using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Mistaken.Base.Staff;
using Gamer.Utilities;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gamer.Mistaken.Base
{
    /// <inheritdoc/>
    public class ExperimentalHandler : Diagnostics.Module
    {
        /// <inheritdoc/>
        public override bool Enabled => Version.Debug;
        /// <inheritdoc/>
        public override bool IsBasic => true;
        /// <inheritdoc/>
        public ExperimentalHandler(PluginHandler p) : base(p)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Experimental";
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            ev.Player.SetGUI("experimental", PseudoGUIHandler.Position.BOTTOM, $"<size=50%>Serwer jest w trybie <color=yellow>eksperymentalnym</color>, mogą wystąpić <b>lagi</b> lub błędy<br>Wersja pluginów: {Version.CurrentVersion}</size>");
        }
    }
}
