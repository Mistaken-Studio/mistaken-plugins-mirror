using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Utilities;
using HarmonyLib;

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
            if(!Base.PluginHandler.Config.IsPTBServer)
            Round.IsLocked = true;
            ev.Player.SetGUI("experimental", PseudoGUIHandler.Position.BOTTOM, $"<size=50%>Serwer jest w trybie <color=yellow>eksperymentalnym</color>, mogą wystąpić <b>lagi</b> lub błędy<br>Wersja pluginów: {Version.CurrentVersion}</size>");
        }
    }
    /// <inheritdoc/>
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class CommandHandler : IBetterCommand
    {
        /// <inheritdoc/>
        public override string Command => "version";
        /// <inheritdoc/>
        public override string[] Aliases => new string[] { "v" };
        /// <inheritdoc/>
        public override string[] Execute(CommandSystem.ICommandSender sender, string[] args, out bool success)
        {
            success = true;
            return new string[] { "Version: " + Version.CurrentVersion, "Debug: " + Version.Debug,"Full commit SHA: "+ Version.commitSha, "Build timestamp: "+Version.buildTimeStamp, "Build Tools Version: " + Version.ciToolsVersion };
        }
    }

    [HarmonyPatch(typeof(ServerConsole), "ReloadServerName")]
    internal static class ServerNamePatch
    {
        private static void Postfix()
        {
            ServerConsole._serverName += "<color=#00000000><size=1>|MistakenPlugins:" + Version.CurrentVersion + "</size></color>";
        }
    }
}
