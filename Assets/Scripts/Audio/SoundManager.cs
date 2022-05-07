using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using UnityEditor;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    protected static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Type t = typeof(T);
                instance = (T)FindObjectOfType(t);

                if(instance != null) return instance;
                
                var obj = new GameObject(typeof(T).Name);
                instance = obj.AddComponent<T>();
                DontDestroyOnLoad(obj);
            }

            return instance;
        }
    }
}

namespace Sound
{
    public class SoundManager : SingletonMonoBehaviour<SoundManager>
    {
        public enum SystemSeType
        {
            //	 1	カーソル
            Cursor			= 1,
            //	 2	決定
            Decide			,
            //	 3	大決定
            DecideSP		,
            //	 4	ゲーム開始
            GameStart		,
            //	 5	キャンセル
            Cancel			,
            //	 6	選択不可・エラー
            Error			,
            //	 7	購入・支払い
            BuySell,
            //	 8	デッキ組み換え
            CardSet,
            //	 9	セリフ送り
            NextLine		,
            //	10	キャンセル
            Tab			,
            //	11	ウィンドウオープ
            WindowOpen			,
            //	12	ウィンドウクローズ
            WindowClose,
        }


        const int SeChannel = 16;

        [SerializeField] SoundData _soundData;

        AudioData[] _bgmData;
        AudioData[] _seData;

        class CrossFadeData
        {
            public AudioData _srcAudioData;
            public AudioData _dstAudioData;
            public float _fadeCompleteTime;
            public float _fadeTime;
        }
        CrossFadeData _crossFadeData;

        class MasterVolume
        {
            public float _seVolume;
            public float _bgmVolume;

            public MasterVolume()
            {
                //_seVolume = 1.0f;
                _seVolume = 0.1f;
                _bgmVolume = 1.0f;
            }
        }
        MasterVolume _masterVolume;

        public class AudioData
        {
            public string Guid { get; private set; }
            public AudioSource _audioSource;
            public float _volume;
            public float _audioLength;

            int _pauseRefCount;

            public void Pause()
            {
                _pauseRefCount++;
                _audioSource.Pause();
            }

            public void UnPause()
            {
                if (_pauseRefCount > 0)
                {
                    _pauseRefCount--;
                    if (_pauseRefCount <= 0)
                    {
                        _audioSource.UnPause();
                    }
                }
            }

            public bool Paused()
            {
                return _pauseRefCount > 0;
            }

            public void SetGuid()
            {
                Guid = System.Guid.NewGuid().ToString();
            }

            public AudioDataHandler ToAudioSourceHandler()
            {
                return new AudioDataHandler(this);
            }
        }

        public readonly struct AudioDataHandler
        {
            public readonly string _guid;

            public AudioDataHandler(AudioData audioSourceData)
            {
                _guid = audioSourceData.Guid;
            }
        }

        void Awake()
        {
            _masterVolume = new MasterVolume();

            _bgmData = new AudioData[2];
            for (int i = 0; i < _bgmData.Length; i++)
            {
                _bgmData[i] = new AudioData();
                _bgmData[i]._audioSource = gameObject.AddComponent<AudioSource>();
                _bgmData[i]._audioSource.playOnAwake = false;
            }

            _seData = new AudioData[SeChannel];
            for (int i = 0; i < _seData.Length; i++)
            {
                _seData[i] = new AudioData();
                _seData[i]._audioSource = gameObject.AddComponent<AudioSource>();
                _seData[i]._audioSource.playOnAwake = false;
            }

            // 全体シーンで使うサウンドを初期ロードしておくことができます
            LoadDefaultSound();
        }

           void LoadDefaultSound()
           {
                 _soundData = Resources.Load<SoundData>("DebugTestData");
        // #if UNITY_EDITOR
            //    _soundData = AssetDatabase.LoadAssetAtPath<SoundData>("Assets/Scripts/Sound/DebugTestData.asset");
        // #else
            //    StartCoroutine(LoadAssetBundle());
        // #endif
           }

           IEnumerator LoadAssetBundle()
           {
               var path = Path.Combine(Application.streamingAssetsPath, "DebugTestData");
               using (UnityWebRequest req = UnityWebRequestAssetBundle.GetAssetBundle(path))
               {
                   yield return req.SendWebRequest();

                   if (req.isNetworkError || req.isHttpError)
                   {
                       Debug.LogError(req.error);
                   }
                   else
                   {
                       var bundle = DownloadHandlerAssetBundle.GetContent(req);
                       var abReq = bundle.LoadAssetAsync<SoundData>("DebugTestData");
                       yield return abReq;
                       _soundData = abReq.asset as SoundData;
                       bundle.Unload(false);
                   }
               }
           }

        void LateUpdate()
        {
            UpdateVolume(_bgmData, _masterVolume._bgmVolume);
            UpdateVolume(_seData, _masterVolume._seVolume);

            if (_crossFadeData != null)
            {
                if (_crossFadeData._fadeTime > _crossFadeData._fadeCompleteTime)
                {
                    _crossFadeData._srcAudioData._audioSource.Stop();
                    _crossFadeData._dstAudioData._audioSource.volume = _masterVolume._bgmVolume * _crossFadeData._dstAudioData._volume;
                    _crossFadeData = null;
                }
                else
                {
                    _crossFadeData._srcAudioData._audioSource.volume = _masterVolume._bgmVolume * _crossFadeData._srcAudioData._volume * (1.0f - (_crossFadeData._fadeTime / _crossFadeData._fadeCompleteTime));
                    _crossFadeData._dstAudioData._audioSource.volume = _masterVolume._bgmVolume * _crossFadeData._dstAudioData._volume * (_crossFadeData._fadeTime / _crossFadeData._fadeCompleteTime);
                    _crossFadeData._fadeTime += Time.deltaTime;
                }
            }
        }

        public bool Initialized()
        {
            return _soundData != null;
        }

        void UpdateVolume(AudioData[] audioData, float volume)
        {
            foreach (var data in audioData)
            {
                if (data._audioSource.isPlaying)
                {
                    data._audioSource.volume = volume * data._volume;
                }
            }
        }

        bool TryGetAudioClip(string name, out AudioClip outData)
        {
            foreach (var clip in _soundData._audioClips)
            {
                if(clip == null) break;
                if (clip.name == name)
                {
                    outData = clip;
                    return true;
                }
            }
            outData = null;
            return false;
        }

        bool TryGetAudioSourceData(AudioDataHandler handler, out AudioData outData)
        {
            foreach (var data in _bgmData)
            {
                if (handler._guid == data.Guid)
                {
                    outData = data;
                    return true;
                }
            }

            foreach (var data in _seData)
            {
                if (handler._guid == data.Guid)
                {
                    outData = data;
                    return true;
                }
            }

            outData = null;
            return false;
        }

        public AudioDataHandler Play(SystemSeType type, float volume = 1.0f)
        {
            int index = (int)type; 
            if(_soundData._audioClips.Count() > index && index >= 0)
            {
                if(_soundData._audioClips[index] != null)
                {
                    Play(_soundData._audioClips[index],volume);
                }
            }
            
            return default;
        }


        public AudioDataHandler Play(string name, float volume = 1.0f)
        {
            if (TryGetAudioClip(name, out var clip))
            {
                return Play(clip, volume);
            }
            return default;
        }

        public AudioDataHandler Play(AudioClip clip, float volume = 1.0f)
        {
            foreach (var data in _seData)
            {
                if (!data._audioSource.isPlaying && !data.Paused())
                {
                    data._audioSource.clip = clip;
                    data._audioSource.volume = _masterVolume._seVolume * volume;
                    data._audioSource.Play();

                    data._audioLength = clip.length;
                    data._volume = volume;
                    data.SetGuid();
                    return data.ToAudioSourceHandler();
                }
            }

            return default;
        }

        public AudioDataHandler PlayBgm(string name, float volume = 1.0f, bool loop = true)
        {
            if (TryGetAudioClip(name, out var clip))
            {
                return PlayBgm(clip, volume, loop);
            }
            return default;
        }

        /// <summary>
        /// 再生中のBGMを即時停止して再生させる
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="volume"></param>
        public AudioDataHandler PlayBgm(AudioClip clip, float volume = 1.0f, bool loop = true)
        {
            foreach (var data in _bgmData)
            {
                if (data._audioSource.isPlaying || data.Paused())
                {
                    data._audioSource.Stop();
                }
            }

            var playData = _bgmData[0];
            playData._audioSource.clip = clip;
            playData._audioSource.volume = _masterVolume._bgmVolume * volume;
            playData._audioSource.loop = loop;
            playData._audioSource.Play();

            playData._audioLength = clip.length;
            playData._volume = volume;
            playData.SetGuid();
            return _bgmData[0].ToAudioSourceHandler();
        }

        public AudioDataHandler CrossFadeBgm(string name, float volume = 1.0f, bool loop = true, float fadeTime = 0.5f)
        {
            if (TryGetAudioClip(name, out var clip))
            {
                return CrossFadeBgm(clip, volume, loop, fadeTime);
            }
            return default;
        }

        public AudioDataHandler CrossFadeBgm(AudioClip clip, float volume = 1.0f, bool loop = true, float fadeTime = 0.5f)
        {
            var src = _bgmData.FirstOrDefault(x => x._audioSource.isPlaying);
            var dst = _bgmData.FirstOrDefault(x => !x._audioSource.isPlaying);

            dst._audioSource.clip = clip;
            dst._audioSource.volume = 0;
            dst._audioSource.loop = loop;
            dst._audioSource.Play();

            dst._audioLength = clip.length;
            dst._volume = volume;
            dst.SetGuid();

            _crossFadeData = new CrossFadeData();
            _crossFadeData._srcAudioData = src;
            _crossFadeData._dstAudioData = dst;
            _crossFadeData._fadeCompleteTime = fadeTime;

            return dst.ToAudioSourceHandler();
        }

        /// <summary>
        /// 再生されていなければfalseを返します
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public bool Stop(AudioDataHandler handler)
        {
            if (TryGetAudioSourceData(handler, out var data))
            {
                if (!data._audioSource.isPlaying) return false;

                data._audioSource.Stop();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 再生されていなければ-1を返します
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public float GetPlayTime(AudioDataHandler handler)
        {
            if (TryGetAudioSourceData(handler, out var data))
            {
                if (!data._audioSource.isPlaying && !data.Paused()) return data._audioLength;

                return data._audioSource.time;
            }
            return -1;
        }

        public bool Completed(AudioDataHandler handler)
        {
            if (TryGetAudioSourceData(handler, out var data))
            {
                return data._audioSource.isPlaying && !data.Paused();
            }
            return false;
        }

        public bool Playing(AudioDataHandler handler)
        {
            if (TryGetAudioSourceData(handler, out var data))
            {
                return data._audioSource.isPlaying;
            }
            return false;
        }

        public bool Paused(AudioDataHandler handler)
        {
            if (TryGetAudioSourceData(handler, out var data))
            {
                return data.Paused();
            }
            return false;
        }

        public bool Pause(AudioDataHandler handler)
        {
            if (TryGetAudioSourceData(handler, out var data))
            {
                if (!data._audioSource.isPlaying) return false;

                data.Pause();
                return true;
            }
            return false;
        }

        public bool UnPause(AudioDataHandler handler)
        {
            if (TryGetAudioSourceData(handler, out var data))
            {
                if (!data.Paused()) return false;

                data.UnPause();
                return true;
            }
            return false;
        }

        public void AllPause()
        {
            foreach (var data in _bgmData)
            {
                if (data._audioSource.isPlaying)
                {
                    data.Pause();
                }
            }

            foreach (var data in _seData)
            {
                if (data._audioSource.isPlaying)
                {
                    data.Pause();
                }
            }
        }

        public void AllUnPause()
        {
            foreach (var data in _bgmData)
            {
                if (data._audioSource.isPlaying)
                {
                    data.UnPause();
                }
            }

            foreach (var data in _seData)
            {
                if (data._audioSource.isPlaying)
                {
                    data.UnPause();
                }
            }
        }
    }
}