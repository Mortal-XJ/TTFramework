using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame
{
    public enum AudioType
    {
        //9999 以下为循环播放
        BackgroundMusic = 0,

        //10001 以上为不循环播放
        SpecialEffectsSounds = 10001
    }

    public class AudioHandler : SingletonMono<AudioHandler>
    {
        private ConcurrentDictionary<AudioType, Audio> _audios;
        private ConcurrentDictionary<string, AudioClip> _sharedAudio;

        public override void Init()
        {
            _audios = new ConcurrentDictionary<AudioType, Audio>();
            _sharedAudio = new ConcurrentDictionary<string, AudioClip>();
            gameObject.AddComponent<AudioListener>();
            Array arrays = Enum.GetValues(typeof(AudioType));
            for (int i = 0; i < arrays.Length; i++)
            {
                AudioType audio = (AudioType)arrays.GetValue(i);
                CreateAudioSource(audio);
            }
        }

        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="type">音频类型</param>
        /// <param name="path">音频路径</param>
        /// <param name="sharedAudioClip">该音频是否是共享音频文件</param>
        public void Play(AudioType type, string path, bool sharedAudioClip = false)
        {
            if (!_audios.TryGetValue(type, out Audio value)) return;
            if (!sharedAudioClip)
                value.Play(path).Forget();
            else
                PlayClip(value, path);
        }

        async UniTaskVoid PlayClip(Audio audio, string path)
        {
            AudioClip value;
            if (!_sharedAudio.TryGetValue(path, out value))
                value = await ExplorerHandler.Instance.LoadResourceAsync<AudioClip>(path: path);
            _sharedAudio.TryAdd(path, value);
            audio.Play(value);
        }

        public void SetVolume(AudioType type, float target)
        {
            if (_audios.TryGetValue(type, out Audio value))
                value.SetVolume(target).Forget();
        }

        void CreateAudioSource(AudioType audioType)
        {
            GameObject audio = new GameObject(audioType.ToString());
            audio.transform.SetParent(gameObject.transform);
            AudioSource source = audio.AddComponent<AudioSource>();
            _audios.TryAdd(audioType, new Audio(source));
        }

        private void OnDestroy()
        {
            _sharedAudio.Clear();
            this._audios.Clear();
        }

        public void Unload(AudioClip audioClip)
        {
            string key = "";
            foreach (var audio in _sharedAudio)
            {
                if (audio.Value == audioClip)
                {
                    key = audio.Key;
                    break;
                }
            }

            _sharedAudio.TryRemove(key, out AudioClip tempAudioClip);
            ExplorerHandler.Instance.UnloadAsset<AudioClip>(tempAudioClip);

            foreach (var audio in _audios)
            {
                if (audio.Value.IsEqual(audioClip))
                {
                    audio.Value.RemoveAudioClip(audioClip);
                    break;
                }
            }
        }

        public void Unload(string path)
        {
            _sharedAudio.TryRemove(path, out AudioClip tempAudioClip);
            ExplorerHandler.Instance.UnloadAsset<AudioClip>(tempAudioClip);

            foreach (var audio in _audios)
            {
                if (audio.Value.IsEqual(path))
                {
                    audio.Value.RemoveAudioClip(path);
                    break;
                }
            }
        }
        
        public class LoadANewSceneToPlay : TEvent<GameFrame.TheSceneFinishLoading_Event>
        {
            public override void Handler(GameFrame.TheSceneFinishLoading_Event tEvent)
            {
                AudioListener[] audioListeners = GameObject.FindObjectsOfType<AudioListener>();
                for (int i = 1; i < audioListeners.Length; i++)
                    Destroy(audioListeners[i]);
            }
        }

    }

    internal class Audio
    {
        private AudioSource _audioSource;
        private ConcurrentDictionary<string, AudioClip> _clips;
        private AudioType _audioType;

        public Audio(AudioSource audioSource)
        {
            this._audioType = (AudioType)Enum.Parse(typeof(AudioType), audioSource.name);
            this._audioSource = audioSource;
            this._audioSource.loop = (int)_audioType < 1000;
            this._audioSource.playOnAwake = false;
            this._audioSource.volume = 0.5f;
            this._clips = new ConcurrentDictionary<string, AudioClip>();
        }

        public async UniTaskVoid Play(string path)
        {
            AudioClip audioClip;
            if (!_clips.TryGetValue(path, out audioClip))
                audioClip = await ExplorerHandler.Instance.LoadResourceAsync<AudioClip>(path);
            _clips.TryAdd(path, audioClip);
            this.Play(audioClip);
        }

        public void Play(AudioClip clip)
        {
            this._audioSource.clip = clip;
            this._audioSource.Play();
        }

        public async UniTaskVoid SetVolume(float target)
        {
            target = target > 1f ? 1f : target;
            target = target < 0f ? 0f : target;
            float floating = 0.1f;
            while (this._audioSource.volume != target)
            {
                if (this._audioSource.volume > target)
                    this._audioSource.volume -= floating;
                else
                    this._audioSource.volume += floating;
                await Task.Yield();
            }
        }

        public void RemoveAudioClip(string path)
        {
            _clips.TryRemove(path, out AudioClip value);
            ExplorerHandler.Instance.UnloadAsset<AudioClip>(value);
        }

        public void RemoveAudioClip(AudioClip audioClip)
        {
            string key = "";
            foreach (var audio in _clips)
            {
                if (audio.Value == audioClip)
                {
                    key = audio.Key;
                    break;
                }
            }

            ExplorerHandler.Instance.UnloadAsset<AudioClip>(audioClip);
            _clips.TryRemove(key, out audioClip);
        }

        public bool IsEqual(AudioClip audioClip)
        {
            foreach (var audio in _clips)
            {
                if (audio.Value == audioClip)
                    return true;
            }

            return false;
        }

        public bool IsEqual(string path)
        {
            return _clips.ContainsKey(path);
        }

        public void Clear()
        {
            foreach (var clip in _clips)
            {
                ExplorerHandler.Instance.UnloadAsset(clip.Value);
            }
            this._clips.Clear();
        }
    }
}