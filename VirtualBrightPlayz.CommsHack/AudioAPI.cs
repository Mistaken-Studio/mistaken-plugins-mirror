using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Assets._Scripts.Dissonance;
using Dissonance;
using Dissonance.Audio.Capture;
using Dissonance.Audio.Codecs;
using Dissonance.Audio.Playback;
using Dissonance.Config;
using Dissonance.Integrations.MirrorIgnorance;
using Dissonance.Networking;
using Dissonance.Networking.Client;
using Exiled.API.Features;
using HarmonyLib;
using MEC;
using Mirror;
using RemoteAdmin;
using UnityEngine;

namespace CommsHack
{
	public class AudioAPI
	{
		public const string FFMPEG_PATH = "";
		public static readonly string BASE_PATH = Paths.Plugins + "/Audio/";
		private static AudioAPI _api;
		public static AudioAPI API
		{
			get
			{
				if (_api == null)
					_api = new AudioAPI();
				return _api;
			}
		}

		public AudioAPI()
		{
			Timing.RunCoroutine(this.UpdateClient());
			new Harmony("virtualbrightplayz.commhack.scpsl").PatchAll();
			_api = this;

			Exiled.Events.Handlers.Server.RestartingRound += () => SpawnPatch.WhitelistedSpawns.Clear();
		}

		public IEnumerator<float> UpdateClient()
		{
			while (true)
			{
				yield return float.NegativeInfinity;
				if (client != null && !client._disconnected)
				{
					for (int i = 0; i < DebugSettings.Instance._levels.Count; i++)
						DebugSettings.Instance._levels[i] = LogLevel.Trace;
					if (client.Update() == ClientStatus.Error)
						Exiled.API.Features.Log.Error("Client error!!!");
				}
			}
		}

		public Stream GetNamedStream(string name) => this._streams[name];
		public void RegisterNamedStream(string name, Stream stream) => this._streams.Add(name, stream);
		public bool ContainsNamedStream(string name) => this._streams.ContainsKey(name);
		public void UnregisterNamedStream(string name) => this._streams.Remove(name);

		public void PlayFile(string path, float volume)
		{
			if (AudioAPI.handle.IsValid)
				Timing.KillCoroutines(AudioAPI.handle);
			Timing.RunCoroutine(this.WaitForConvert(path, volume));
		}

		internal IEnumerator<float> WaitForConvert(string path, float volume)
		{
			string rawPath = path + ".raw";
			if (File.Exists(rawPath))
			{
				this.PlayFileRaw(rawPath, volume);
				yield break;
			}
			Exiled.API.Features.Log.Info("Converting " + path);
			if (AudioAPI.prc != null && !AudioAPI.prc.HasExited)
				AudioAPI.prc.Kill();
			AudioAPI.prc = Process.Start(FFMPEG_PATH, $"-i \"{path}\" -f f32le -ar 48000 -ac 1 {rawPath}");
			yield return Timing.WaitUntilTrue(() => AudioAPI.prc.HasExited);
			Exiled.API.Features.Log.Info("Done!");
			this.PlayFileRaw(rawPath, volume);
			yield break;
		}

		public void PlayFileRaw(string path, float volume) => this.PlayStream(File.OpenRead(path), volume);
		public void PlayStream(Stream stream, float volume) => this.PlayWithParams(stream, 9999, volume, false, Vector3.zero);


		public GameObject PlayWithParams(Stream stream, ushort playerid, float volume, bool _3d, Vector3 position = default, ReferenceHub target = null)
		{
			MirrorIgnoranceCommsNetwork mirrorIgnoranceCommsNetwork = UnityEngine.Object.FindObjectOfType<MirrorIgnoranceCommsNetwork>();
			DissonanceComms dissonanceComms = UnityEngine.Object.FindObjectOfType<DissonanceComms>();
			if (mirrorIgnoranceCommsNetwork.Client == null)
				mirrorIgnoranceCommsNetwork.StartClient(Unit.None);
			client = mirrorIgnoranceCommsNetwork.Client;
			if (dissonanceComms.TryGetComponent<IMicrophoneCapture>(out IMicrophoneCapture microphoneCapture))
			{
				if (microphoneCapture.IsRecording)
					microphoneCapture.StopCapture();
				UnityEngine.Object.Destroy((Component)microphoneCapture);
			}
			mirrorIgnoranceCommsNetwork.Mode = NetworkMode.Host;
			FloatArrayCapture floatArrayCapture = dissonanceComms.gameObject.AddComponent<FloatArrayCapture>();
			floatArrayCapture._file = stream;
			dissonanceComms._capture.Start(mirrorIgnoranceCommsNetwork, floatArrayCapture);
			dissonanceComms._capture._micName = "StreamedMic";
			dissonanceComms._capture.RestartTransmissionPipeline("Dummy");
			clientInfo = mirrorIgnoranceCommsNetwork.Server._clients.GetOrCreateClientInfo(playerid, "Dummy", new CodecSettings(Codec.Opus, 48000u, 480), new MirrorConn(NetworkServer.localConnection));
			dissonanceComms.IsMuted = false;
			if (_3d)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(NetworkManager.singleton.playerPrefab);
				gameObject.transform.position = position == default ? Vector3.zero : position;
				gameObject.transform.localScale = Vector3.one * 0.0001f;
				gameObject.GetComponent<CharacterClassManager>().CurClass = global::RoleType.Tutorial;
				gameObject.GetComponent<CharacterClassManager>().GodMode = true;
				gameObject.GetComponent<NicknameSync>().Network_myNickSync = clientInfo.PlayerName;
				gameObject.GetComponent<QueryProcessor>().PlayerId = (int)playerid;
				gameObject.GetComponent<QueryProcessor>().NetworkPlayerId = (int)playerid;
				gameObject.GetComponent<CustomBroadcastTrigger>().enabled = true;
				gameObject.GetComponent<CustomBroadcastTrigger>().PlayerId = playerid.ToString();
				gameObject.GetComponent<CustomBroadcastTrigger>().ChannelType = CommTriggerTarget.Room;
				gameObject.GetComponent<Radio>().isVoiceChatting = true;
				gameObject.GetComponent<Radio>().NetworkisVoiceChatting = true;
				gameObject.GetComponent<MirrorIgnorancePlayer>()._playerId = clientInfo.PlayerName;
				gameObject.GetComponent<MirrorIgnorancePlayer>().Network_playerId = clientInfo.PlayerName;
				gameObject.GetComponent<DisableUselessComponents>()._added = true;
				gameObject.GetComponent<CharacterClassManager>().IsVerified = true;
				gameObject.GetComponent<CharacterClassManager>().NetworkIsVerified = true;
				if (target == null)
				{
					gameObject.GetComponent<CustomBroadcastTrigger>().BroadcastPosition = true;
					gameObject.GetComponent<CustomBroadcastTrigger>().RoomName = "Proximity";
					NetworkServer.Spawn(gameObject);
					mirrorIgnoranceCommsNetwork.Server._clients.JoinRoom("Proximity", clientInfo);
					dissonanceComms.RoomChannels.Open("Proximity", true, ChannelPriority.None, volume);
					Timing.RunCoroutine(this.SpawnLate(gameObject, clientInfo, playerid, false));
				}
				else
				{
					gameObject.GetComponent<CustomBroadcastTrigger>().BroadcastPosition = false;
					gameObject.GetComponent<CustomBroadcastTrigger>().RoomName = "Intercom";
					SpawnPatch.WhitelistedSpawns.Add(gameObject.GetComponent<NetworkIdentity>(), new HashSet<NetworkConnection>
					{
						target.characterClassManager.connectionToClient
					});
					NetworkServer.Spawn(gameObject);
					mirrorIgnoranceCommsNetwork.Server._clients.JoinRoom("Null", clientInfo);
					mirrorIgnoranceCommsNetwork.Server._clients.JoinRoom("Intercom", clientInfo);
					dissonanceComms.RoomChannels.Open("Null", false, ChannelPriority.None, volume);
					dissonanceComms.RoomChannels.Open("Intercom", false, ChannelPriority.None, volume);
					Timing.RunCoroutine(this.SpawnLate(gameObject, clientInfo, playerid, true));
				}
				return gameObject;
			}
			else
			{
				KeyValuePair<ushort, RoomChannel>[] array = dissonanceComms.RoomChannels._openChannelsBySubId.ToArray();
				foreach (KeyValuePair<ushort, RoomChannel> keyValuePair in array)
					dissonanceComms.RoomChannels.Close(keyValuePair.Value);
				mirrorIgnoranceCommsNetwork.Server._clients.LeaveRoom("Null", clientInfo);
				mirrorIgnoranceCommsNetwork.Server._clients.LeaveRoom("Intercom", clientInfo);
				mirrorIgnoranceCommsNetwork.Server._clients.JoinRoom("Null", clientInfo);
				mirrorIgnoranceCommsNetwork.Server._clients.JoinRoom("Intercom", clientInfo);
				dissonanceComms.RoomChannels.Open("Null", false, ChannelPriority.None, volume);
				dissonanceComms.RoomChannels.Open("Intercom", false, ChannelPriority.None, volume);

				return null;
			}
		}

		internal IEnumerator<float> SpawnLate(GameObject go, ClientInfo<MirrorConn> clientInfo, ushort playerid, bool global = false)
		{
			yield return float.NegativeInfinity;
			go.GetComponent<QueryProcessor>().PlayerId = (int)playerid;
			go.GetComponent<QueryProcessor>().NetworkPlayerId = (int)playerid;
			if (!global)
			{
				go.GetComponent<DissonanceUserSetup>().SetSpeakingFlags((SpeakingFlags)0);
				go.GetComponent<CustomBroadcastTrigger>().RoomName = "Proximity";
			}
			else
			{
				go.GetComponent<DissonanceUserSetup>().SetSpeakingFlags(SpeakingFlags.IntercomAsHuman);
				go.GetComponent<CustomBroadcastTrigger>().RoomName = "Intercom";
			}
			go.GetComponent<MirrorIgnorancePlayer>().RpcSetPlayerName(clientInfo.PlayerName);
			yield return float.NegativeInfinity;
			Exiled.API.Features.Log.Info(go.GetComponent<Radio>().state == null);
			try
			{
				((IVoicePlaybackInternal)go.GetComponent<Radio>().unityPlayback).StartPlayback();
			}
			catch (Exception ex)
			{
				Exception e = ex;
				Exiled.API.Features.Log.Error(e);
			}
			yield return float.NegativeInfinity;
			Exiled.API.Features.Log.Info(go.GetComponent<CustomBroadcastTrigger>().IsTransmitting);
			Exiled.API.Features.Log.Info(go.GetComponent<CustomBroadcastTrigger>().ChannelType);
			Exiled.API.Features.Log.Info(go.GetComponent<CustomBroadcastTrigger>().RoomName);
			Exiled.API.Features.Log.Info(go.GetComponent<CustomBroadcastTrigger>().ShouldActivate(go.GetComponent<CustomBroadcastTrigger>().IsUserActivated()));
			yield return float.NegativeInfinity;
			Exiled.API.Features.Log.Info(go.GetComponent<Radio>().unityPlayback.IsSpeaking);
			PlayerManager.RemovePlayer(go);
			yield break;
		}

		private readonly Dictionary<string, Stream> _streams = new Dictionary<string, Stream>();

		private static Process prc;

		private static readonly CoroutineHandle handle;

		private static ClientInfo<MirrorConn> clientInfo;
		private static MirrorIgnoranceClient client;
	}
}
