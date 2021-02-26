using Assets._Scripts.Dissonance;
using Dissonance;
using Dissonance.Audio;
using Exiled.API.Features;
using Gamer.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Log = Exiled.API.Features.Log;

namespace Gamer.AudioLog
{
    public class PluginHandler : Plugin<Config>
    {
        public override string Author => "Gamer";
        public override string Name => "AudioLog";
        public override void OnEnabled()
        {
            if (Server.Port != 7791)
            {
                base.OnDisabled();
                return;
            }
            Exiled.Events.Handlers.Server.WaitingForPlayers += Server_WaitingForPlayers;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= Server_WaitingForPlayers;
            base.OnDisabled();
        }


        private void Server_WaitingForPlayers()
        {
            var host = Server.Host.GameObject;
            var comms = GameObject.FindObjectOfType<Dissonance.DissonanceComms>();
            comms._net.VoicePacketReceived += (_) => Log.Debug("VoicePacketRecived");
            comms._net.TextPacketReceived += (_) => Log.Debug("TextPacketReceived|"+_.Message);
            comms._net.ModeChanged += (_) => Log.Debug("ModeChanged|"+_);
            comms._net.PlayerEnteredRoom += (_) => Log.Debug("PlayerEnteredRoom|"+_.PlayerName+"|"+_.Room);
            comms._net.PlayerExitedRoom += (_) => Log.Debug("PlayerExitedRoom|" + _.PlayerName + "|" + _.Room);
            comms._net.PlayerJoined += (_, c) => Log.Debug("PlayerJoined|"+_);
            comms._net.PlayerLeft += (_) => Log.Debug("PlayerLeft|"+_);
            comms._net.PlayerStartedSpeaking += (_) => Log.Debug("PlayerStartedSpeaking" + _);
            comms._net.PlayerStoppedSpeaking += (_) => Log.Debug("PlayerStoppedSpeaking|" + _);
            comms.IsDeafened = false;
            DissonanceAudioRecorder tmp = host.AddComponent<DissonanceAudioRecorder>();
            comms.SubcribeToRecordedAudio(tmp);
            DissonanceUserSetup dus = host.GetComponent<DissonanceUserSetup>();
            dus.ResetToDefault();
            dus.EnableListening(TriggerType.Role | TriggerType.Intercom | TriggerType.Proximity, Assets._Scripts.Dissonance.RoleType.Ghost);
            //dus.CallTargetUpdateForTeam(Team.RIP);
            if(dus.TryGetVoiceTrigger(TriggerType.Intercom, false, out BaseCommsTrigger trigger))
            {
                if (!(trigger as VoiceReceiptTrigger)._membership.HasValue)
                    (trigger as VoiceReceiptTrigger).JoinRoom();
                trigger.Comms.SubcribeToRecordedAudio(tmp);
            }
            if (dus.TryGetVoiceTrigger(TriggerType.Role, false, out trigger))
            {
                if (!(trigger as VoiceReceiptTrigger)._membership.HasValue)
                    (trigger as VoiceReceiptTrigger).JoinRoom();
            }
            if (dus.TryGetVoiceTrigger(TriggerType.Proximity, false, out trigger))
            {
                if(!(trigger as VoiceReceiptTrigger)._membership.HasValue)
                    (trigger as VoiceReceiptTrigger).JoinRoom();
            }
            foreach (var item in comms._capture._audioListeners)
            {
                Log.Debug(item.GetType().FullName);
            }
        }
        /*
            var host = Server.Host.GameObject;
            MirrorIgnoranceCommsNetwork mirrorIgnoranceCommsNetwork = UnityEngine.Object.FindObjectOfType<MirrorIgnoranceCommsNetwork>();
            var dissonanceComms = GameObject.FindObjectOfType<Dissonance.DissonanceComms>();
            if (mirrorIgnoranceCommsNetwork.Client == null)
                mirrorIgnoranceCommsNetwork.StartClient(Unit.None);
            client = mirrorIgnoranceCommsNetwork.Client;
            mirrorIgnoranceCommsNetwork.Mode = NetworkMode.Host;
            DissonanceAudioRecorder recorder = dissonanceComms.gameObject.AddComponent<DissonanceAudioRecorder>();
            dissonanceComms.SubcribeToRecordedAudio(recorder);
            clientInfo = mirrorIgnoranceCommsNetwork.Server._clients.GetOrCreateClientInfo(9990, "Dummy", new CodecSettings(Codec.Opus, 48000u, 480), new MirrorConn(NetworkServer.localConnection));
            dissonanceComms.IsMuted = false;
            dissonanceComms.IsDeafened = false;
            KeyValuePair<ushort, RoomChannel>[] array = dissonanceComms.RoomChannels._openChannelsBySubId.ToArray();
            foreach (KeyValuePair<ushort, RoomChannel> keyValuePair in array)
                dissonanceComms.RoomChannels.Close(keyValuePair.Value);
            mirrorIgnoranceCommsNetwork.Server._clients.LeaveRoom("Null", clientInfo);
            mirrorIgnoranceCommsNetwork.Server._clients.LeaveRoom("Intercom", clientInfo);
            mirrorIgnoranceCommsNetwork.Server._clients.JoinRoom("Null", clientInfo);
            mirrorIgnoranceCommsNetwork.Server._clients.JoinRoom("Intercom", clientInfo);
            dissonanceComms.RoomChannels.Open("Null", false, ChannelPriority.None, 1);
            dissonanceComms.RoomChannels.Open("Intercom", false, ChannelPriority.None, 1);
        */
    }
}
