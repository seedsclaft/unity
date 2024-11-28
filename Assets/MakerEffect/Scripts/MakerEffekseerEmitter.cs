using System.Collections.Generic;
using UnityEngine;
using Effekseer;

namespace Ryneus
{
    public class MakerEffekseerEmitter : EffekseerEmitter
    {
        private List<MakerEffectSound> _effectSounds = new();
		public new EffekseerHandle Play(EffekseerEffectAsset effectAsset)
		{
            if (effectAsset is MakerEffectAsset)
            {
                var makerEffectAsset = (MakerEffectAsset)effectAsset;
                foreach (var soundTiming in makerEffectAsset.soundTimings)
                {
                    PlayMakerEffectSound(soundTiming);
                }
            }
            return base.Play(effectAsset);
		}

        private void PlayMakerEffectSound(MakerEffectData.SoundTimings soundTimings)
        {
            var sound = new MakerEffectSound
            {
                frame = soundTimings.frame,
                soundTimings = soundTimings
            };
            _effectSounds.Add(sound);
        }

        private void PlaySound(MakerEffectData.SoundTimings soundTimings)
        {
            var clip = Resources.Load<AudioClip>("Animations/Sound/" + soundTimings.se.name);
            var volume = soundTimings.se.volume * 0.01f;
            var pitch = (soundTimings.se.pitch * 0.01f) - 1f;
            var pan = soundTimings.se.pan * 0.01f;
            var resource = new Effekseer.Internal.EffekseerSoundResource
            {
                clip = clip
            };
            Effekseer.Internal.EffekseerSoundPlayer.Instance.PlaySound(
                resource,volume,pan,pitch,false,0,0,0,0
            );
        }

        private void Update() 
        {
            base.Update();
            for (int i = _effectSounds.Count-1;i >= 0;i--)
            {
                if (_effectSounds[i].frame == 0)
                {
                    PlaySound(_effectSounds[i].soundTimings);
                    _effectSounds.RemoveAt(i);
                } else
                {
                    _effectSounds[i].frame--;
                }
            }
        }

        private class MakerEffectSound
        {
            public MakerEffectData.SoundTimings soundTimings;
            public int frame;
        }
    }
}
