#pragma warning disable IDE0079
#pragma warning disable IDE0042
#pragma warning disable IDE0060

using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Utilities;
using Gamer.Mistaken.Utilities.APILib;
using MistakenSocket.Client.SL;
using MistakenSocket.Shared.ClientToCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Gamer.Diagnostics;
using MistakenSocket.Shared.API;
using MistakenSocket.Shared;
using Gamer.Mistaken.Systems.Staff;
using MEC;
using Gamer.Mistaken.Systems.GUI;

namespace Gamer.Mistaken.Subtitles
{
    public class SubtitlesHandler : Module
    {
        public static readonly HashSet<string> WithoutSubtitles = new HashSet<string>();
        internal SubtitlesHandler(PluginHandler plugin) : base(plugin)
        {
        }

        public override string Name => "Subtitles";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
        }

        public static void UpdateAll()
        {
            foreach (var item in RealPlayers.List)
                Update(item);
        }

        private static void Update(Player player)
        {
            if (WithoutSubtitles.Contains(player.UserId))
                return;
            if (CassiePatch.Messages.Count == 0)
                PseudoGUIHandler.Set(player, "subtitles", PseudoGUIHandler.Position.BOTTOM, null);
            else
            {
                string tmp = CassiePatch.Messages.Peek();
                string[] tmp2 = tmp.Split(' ');
                if (CassiePatch2.Index != -1 && CassiePatch2.Index < tmp2.Length)
                {
                    Log.Debug(tmp2[CassiePatch2.Index]);
                    tmp2[CassiePatch2.Index] = $"<color=yellow>{tmp2[CassiePatch2.Index]}</color>";
                    tmp = string.Join(" ", tmp2);
                }
                PseudoGUIHandler.Set(player, "subtitles", PseudoGUIHandler.Position.BOTTOM, $"<size=66%><color=yellow>Transkrypt</color>: {tmp}</size><br><br>");
            }
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            Update(ev.Player);
        }
    }
}
