#pragma warning disable IDE0079
#pragma warning disable IDE0042
#pragma warning disable IDE0060

using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Base.GUI;
using Gamer.Utilities;
using MEC;
using System.Collections.Generic;

namespace Gamer.Mistaken.Subtitles
{
    public class SubtitlesHandler : Module
    {
        public static readonly HashSet<string> IgnoredSubtitles = new HashSet<string>();
        internal SubtitlesHandler(PluginHandler plugin) : base(plugin)
        {
            Timing.RunCoroutine(Loop());
        }

        private IEnumerator<float> Loop()
        {
            while(true)
            {
                if (!Cassie.IsSpeaking)
                    CassiePatch.Messages.Clear();
                UpdateAll();
                yield return Timing.WaitForSeconds(10);
            }
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
            if ((Systems.Handler.PlayerPreferencesDict[player.UserId] & API.PlayerPreferences.DISABLE_TRANSCRYPT) != API.PlayerPreferences.NONE)
                return;
            if (CassiePatch.Messages.Count == 0)
                player.SetGUI("subtitles", Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
            else
            {
                string tmp = CassiePatch.Messages.Peek();
                if (IgnoredSubtitles.Contains(tmp))
                    return;
                string[] tmp2 = tmp.Split(' ');
                if (CassiePatch2.Index != -1 && CassiePatch2.Index < tmp2.Length)
                {
                    tmp2[CassiePatch2.Index] = $"<color=yellow>{tmp2[CassiePatch2.Index]}</color>";
                    tmp = string.Join(" ", tmp2);
                }
                if (tmp.Trim().Length == 0)
                    player.SetGUI("subtitles", Base.GUI.PseudoGUIHandler.Position.BOTTOM, null);
                else
                    player.SetGUI("subtitles", Base.GUI.PseudoGUIHandler.Position.BOTTOM, $"<size=66%><color=yellow>Transkrypt</color>: {tmp}</size><br><br>");
            }
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            Update(ev.Player);
        }
    }
}
