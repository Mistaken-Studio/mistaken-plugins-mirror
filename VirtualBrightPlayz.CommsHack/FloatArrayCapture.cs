using Dissonance.Audio.Capture;
using Exiled.API.Features;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommsHack
{
	public class FloatArrayCapture : MonoBehaviour, IMicrophoneCapture
	{
		public bool IsRecording { get; private set; }
		public TimeSpan Latency { get; private set; }

		public WaveFormat StartCapture(string name)
		{
			bool flag = this._file == null || !this._file.CanRead;
			WaveFormat format;
			if (flag)
			{
				Log.Error("_file==null: " + (this._file == null).ToString());
				if (this._file != null)
					Log.Error("_file.CanRead==" + this._file.CanRead.ToString());
				this.IsRecording = false;
				this.Latency = TimeSpan.FromMilliseconds(0.0);
				format = this._format;
			}
			else
			{
				this.IsRecording = true;
				this.Latency = TimeSpan.FromMilliseconds(0.0);
				Log.Info("[FloatArrayCapture] Enabled: " + name);
				format = this._format;
			}
			return format;
		}

		public void StopCapture()
		{
			this.IsRecording = false;
			Log.Info("[FloatArrayCapture] Disabled");
			bool flag = this._file != null;
			if (flag)
			{
				this._file.Dispose();
				this._file.Close();
			}
			this._file = null;
		}

		public void Subscribe(IMicrophoneSubscriber listener)
		{
			this._subscribers.Add(listener);
		}

		public bool Unsubscribe(IMicrophoneSubscriber listener)
		{
			return this._subscribers.Remove(listener);
		}

		public bool UpdateSubscribers()
		{
			bool flag = this._file == null;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				this._elapsedTime += Time.unscaledDeltaTime;
				while (this._elapsedTime > 0.02f)
				{
					this._elapsedTime -= 0.02f;
					int num = this._file.Read(this._frameBytes, 0, this._frameBytes.Length);
					this._readOffset += num;
					Array.Clear(this._frame, 0, this._frame.Length);
					Buffer.BlockCopy(this._frameBytes, 0, this._frame, 0, num);
					foreach (IMicrophoneSubscriber microphoneSubscriber in this._subscribers)
					{
						microphoneSubscriber.ReceiveMicrophoneData(new ArraySegment<float>(this._frame), this._format);
					}
				}
				result = false;
			}
			return result;
		}

		private readonly List<IMicrophoneSubscriber> _subscribers = new List<IMicrophoneSubscriber>();

		private readonly WaveFormat _format = new WaveFormat(48000, 1);

		private readonly float[] _frame = new float[480];

		private readonly byte[] _frameBytes = new byte[1920];

		private float _elapsedTime;

		public Stream _file;

		private int _readOffset;
	}
}
