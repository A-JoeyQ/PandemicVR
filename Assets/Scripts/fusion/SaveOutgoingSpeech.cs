namespace Photon.Voice.Unity.UtilityScripts
{
    using UnityEngine;
    using System.IO;
    // 引用 EyeTracker 所在的命名空间，如果没有，则不需要这行
    // using YourNamespace; 

    [RequireComponent(typeof(Recorder))]
    [DisallowMultipleComponent]
    public class SaveOutgoingSpeech : VoiceComponent
    {
        // WavWriter 不再由主类管理，而是由 Processor 内部管理
        // private WaveWriter wavWriter; 

        private void PhotonVoiceCreated(PhotonVoiceCreatedParams photonVoiceCreatedParams)
        {
            VoiceInfo voiceInfo = photonVoiceCreatedParams.Voice.Info;
            string filePath = this.GetFilePath(); // 文件路径预先生成

            if (photonVoiceCreatedParams.Voice is LocalVoiceAudioFloat localVoiceAudioFloat)
            {
                // 创建 Processor 时不再传入 WaveWriter，而是传入文件路径和语音信息
                var processor = new OutgoingStreamSaverFloat(filePath, voiceInfo);
                localVoiceAudioFloat.AddPostProcessor(processor);
                this.Logger.Log(LogLevel.Info, "Outgoing 32 bit stream processor attached. Waiting for EyeTracker to start recording to: {0}", filePath);
            }
            else if (photonVoiceCreatedParams.Voice is LocalVoiceAudioShort localVoiceAudioShort)
            {
                var processor = new OutgoingStreamSaverShort(filePath, voiceInfo);
                localVoiceAudioShort.AddPostProcessor(processor);
                this.Logger.Log(LogLevel.Info, "Outgoing 16 bit stream processor attached. Waiting for EyeTracker to start recording to: {0}", filePath);
            }
        }

        private string GetFilePath()
        {
            // 文件名格式可以保持不变
            string filename = string.Format("out_{0}_{1}.wav", System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss-ffff"), Random.Range(0, 1000));

            string path;
            if (Application.isEditor) // Dev mode
                path = Path.Combine(Application.dataPath, "Speech");
            else // Prod (build) mode
                path = Path.Combine(Application.persistentDataPath, "Speech");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Path.Combine(path, filename);
        }

        private void PhotonVoiceRemoved()
        {
            // Processor 会在被移除时自动调用 Dispose 方法，主类无需再管理 wavWriter
            this.Logger.Log(LogLevel.Info, "Voice removed. Recording resources will be released by the processor.");
        }

        // 内部类 OutgoingStreamSaverFloat 的修改
        class OutgoingStreamSaverFloat : IProcessor<float>
        {
            private WaveWriter wavWriter;
            private bool isRecordingStarted = false;
            private readonly string filePath;
            private readonly VoiceInfo voiceInfo;

            public OutgoingStreamSaverFloat(string filePath, VoiceInfo voiceInfo)
            {
                this.filePath = filePath;
                this.voiceInfo = voiceInfo;
                this.wavWriter = null; // 初始为 null
            }

            public float[] Process(float[] buf)
            {
                // 检查眼动追踪是否已开始，并且我们尚未开始录制
                if (!this.isRecordingStarted && EyeTracker.isTracking)
                {
                    this.isRecordingStarted = true;
                    // 在此刻创建 WaveWriter 实例
                    this.wavWriter = new WaveWriter(this.filePath, this.voiceInfo.SamplingRate, 32, this.voiceInfo.Channels);
                    Debug.Log($"[SaveOutgoingSpeech] Eye tracking started. Began recording audio to {this.filePath}");
                }

                // 如果已经开始录制，则写入数据
                if (this.isRecordingStarted && this.wavWriter != null)
                {
                    this.wavWriter.WriteSamples(buf, 0, buf.Length);
                }

                // 无论是否录制，都必须返回原始数据，以保证正常通话
                return buf;
            }

            public void Dispose()
            {
                // 仅当 wavWriter 被创建后才需要 Dispose
                if (this.wavWriter != null)
                {
                    this.wavWriter.Dispose();
                    this.wavWriter = null;
                }
            }
        }

        // 内部类 OutgoingStreamSaverShort 的修改
        class OutgoingStreamSaverShort : IProcessor<short>
        {
            private WaveWriter wavWriter;
            private bool isRecordingStarted = false;
            private readonly string filePath;
            private readonly VoiceInfo voiceInfo;

            public OutgoingStreamSaverShort(string filePath, VoiceInfo voiceInfo)
            {
                this.filePath = filePath;
                this.voiceInfo = voiceInfo;
                this.wavWriter = null; // 初始为 null
            }

            public short[] Process(short[] buf)
            {
                // 检查眼动追踪是否已开始，并且我们尚未开始录制
                if (!this.isRecordingStarted && EyeTracker.isTracking)
                {
                    this.isRecordingStarted = true;
                    // 在此刻创建 WaveWriter 实例
                    this.wavWriter = new WaveWriter(this.filePath, this.voiceInfo.SamplingRate, 16, this.voiceInfo.Channels);
                    Debug.Log($"[SaveOutgoingSpeech] Eye tracking started. Began recording audio to {this.filePath}");
                }

                // 如果已经开始录制，则写入数据
                if (this.isRecordingStarted && this.wavWriter != null)
                {
                    for (int i = 0; i < buf.Length; i++)
                    {
                        this.wavWriter.Write(buf[i]);
                    }
                }

                // 无论是否录制，都必须返回原始数据，以保证正常通话
                return buf;
            }

            public void Dispose()
            {
                // 仅当 wavWriter 被创建后才需要 Dispose
                if (this.wavWriter != null)
                {
                    this.wavWriter.Dispose();
                    this.wavWriter = null;
                }
            }
        }
    }
}

/*namespace Photon.Voice.Unity.UtilityScripts
{
    using UnityEngine;
    using System.IO;

    [RequireComponent(typeof(Recorder))]
    [DisallowMultipleComponent]
    public class SaveOutgoingSpeech : VoiceComponent
    {
        private WaveWriter wavWriter;

        private void PhotonVoiceCreated(PhotonVoiceCreatedParams photonVoiceCreatedParams)
        {
            VoiceInfo voiceInfo = photonVoiceCreatedParams.Voice.Info;
            string filePath = this.GetFilePath();

            if (photonVoiceCreatedParams.Voice is LocalVoiceAudioFloat)
            {
                this.wavWriter = new WaveWriter(filePath, voiceInfo.SamplingRate, 32, voiceInfo.Channels);
                this.Logger.Log(LogLevel.Info, "Outgoing 32 bit stream {0}, output file path: {1}", voiceInfo, filePath);
                LocalVoiceAudioFloat localVoiceAudioFloat = photonVoiceCreatedParams.Voice as LocalVoiceAudioFloat;
                localVoiceAudioFloat.AddPostProcessor(new OutgoingStreamSaverFloat(this.wavWriter));
            }
            else if (photonVoiceCreatedParams.Voice is LocalVoiceAudioShort)
            {
                this.wavWriter = new WaveWriter(filePath, voiceInfo.SamplingRate, 16, voiceInfo.Channels);
                this.Logger.Log(LogLevel.Info, "Outgoing 16 bit stream {0}, output file path: {1}", voiceInfo, filePath);
                LocalVoiceAudioShort localVoiceAudioShort = photonVoiceCreatedParams.Voice as LocalVoiceAudioShort;
                localVoiceAudioShort.AddPostProcessor(new OutgoingStreamSaverShort(this.wavWriter));
            }
        }

        private string GetFilePath()
        {
            string filename = string.Format("out_{0}_{1}.wav", System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss-ffff"), Random.Range(0, 1000));

            string path;
            if (Application.isEditor) // Dev mode
                path = Path.Combine(Application.dataPath, "Speech");
            else // Prod (build) mode
                path = Path.Combine(Application.persistentDataPath, "Speech");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Path.Combine(path, filename);
        }

        private void PhotonVoiceRemoved()
        {
            this.wavWriter.Dispose();
            this.Logger.Log(LogLevel.Info, "Recording stopped: Saving wav file.");
        }

        class OutgoingStreamSaverFloat : IProcessor<float>
        {
            private WaveWriter wavWriter;

            public OutgoingStreamSaverFloat(WaveWriter waveWriter)
            {
                this.wavWriter = waveWriter;
            }

            public float[] Process(float[] buf)
            {
                this.wavWriter.WriteSamples(buf, 0, buf.Length);
                return buf;
            }

            public void Dispose()
            {
                this.wavWriter.Dispose();
            }
        }

        class OutgoingStreamSaverShort : IProcessor<short>
        {
            private WaveWriter wavWriter;

            public OutgoingStreamSaverShort(WaveWriter waveWriter)
            {
                this.wavWriter = waveWriter;
            }

            public short[] Process(short[] buf)
            {
                for (int i = 0; i < buf.Length; i++)
                {
                    this.wavWriter.Write(buf[i]);
                }
                return buf;
            }

            public void Dispose()
            {
                this.wavWriter.Dispose();
            }
        }
    }
}

*/