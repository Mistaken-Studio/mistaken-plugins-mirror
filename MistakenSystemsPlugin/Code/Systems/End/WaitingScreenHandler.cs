using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Utilities;
using Grenades;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using Exiled.API.Extensions;
using Gamer.Diagnostics;

namespace Gamer.Mistaken.Systems.End
{
    internal class WaitingScreenHandler : Module
    {
        public WaitingScreenHandler(PluginHandler p) : base(p)
        {
        }

        public override bool Enabled => true;
        public override string Name => "WaitingScreen";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.IntercomSpeaking += this.Handle<Exiled.Events.EventArgs.IntercomSpeakingEventArgs>((ev) => Player_IntercomSpeaking(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => Player_Verified(ev));
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.IntercomSpeaking -= this.Handle<Exiled.Events.EventArgs.IntercomSpeakingEventArgs>((ev) => Player_IntercomSpeaking(ev));
        }

        private void Player_IntercomSpeaking(Exiled.Events.EventArgs.IntercomSpeakingEventArgs ev)
        {
            if (Round.ElapsedTime.TotalSeconds < 5)
                ev.IsAllowed = false;
        }

        private void Server_RoundStarted()
        {
            MEC.Timing.CallDelayed(10, () =>
            {
                ReferenceHub.HostHub.GetComponent<Intercom>().CustomContent = null;
            });
        }

        private void Server_WaitingForPlayers()
        {
            var startRound = GameObject.Find("StartRound");
            if(startRound == null)
            {
                Log.Error("StartRound is NULL");
                return;
            }
            startRound.transform.localScale = Vector3.zero;
            Timing.RunCoroutine(WaitingForPlayers());
            var intercomDoor = Map.Doors.First(d => d.Type() == DoorType.Intercom)?.transform;
            startPos = intercomDoor.position + intercomDoor.forward * -8 + Vector3.down * 6 + intercomDoor.right * 3;
        }

        private Vector3 startPos;
        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (!Round.IsStarted)
                ev.Player.Position = startPos;
        }

        IEnumerator<float> WaitingForPlayers()
        {
            while (!Round.IsStarted)
            {
                if (GameCore.RoundStart.singleton.NetworkTimer == 0)
                {
                    Intercom.host.CustomContent = null;
                    CharacterClassManager.ForceRoundStart();
                    break;
                }
                var players = RealPlayers.List.ToArray().Length;
                var timeMessage = $"<color=yellow>{GameCore.RoundStart.singleton.NetworkTimer}</color> sekund pozostało";
                if (GameCore.RoundStart.singleton.NetworkTimer == 1)
                    timeMessage = $"<color=yellow>Zaczynamy</color>";
                else if (GameCore.RoundStart.singleton.NetworkTimer == 1)
                    timeMessage = $"<color=yellow>1</color> sekunda pozostała";
                else if (GameCore.RoundStart.singleton.NetworkTimer < 5 && GameCore.RoundStart.singleton.NetworkTimer > 1)
                    timeMessage = $"<color=yellow>{GameCore.RoundStart.singleton.NetworkTimer}</color> sekundy pozostały";
                else if (GameCore.RoundStart.singleton.NetworkTimer == -1)
                    timeMessage = "Runda <color=yellow>rozpoczęta</color>";
                else if (GameCore.RoundStart.singleton.NetworkTimer == -2)
                    timeMessage = "Lobby <b><color=red>zablokowane</color></b>";
                var playersMessage = $"<color=yellow>{players}</color> gracz{(players == 1 ? "" : "y")} połączony{(players == 1 ? "" : "ch")}";
                Intercom.host.CustomContent = $"<color=white>{timeMessage} <size=200%><b><color=orange>Mistaken</color></b></size> <color=#00000000>|</color>                 {playersMessage}                <color=#00000000>|</color> <size=25%><color=#CCC9>Nieznajomość regulaminu nie zwalnia z przestrzegania go</color></size></color>";
                yield return Timing.WaitForSeconds(0.5f);
            }
            Intercom.host.CustomContent = null;
        }
    }
}
