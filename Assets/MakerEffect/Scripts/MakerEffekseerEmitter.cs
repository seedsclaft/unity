using UnityEngine;
using Effekseer;

namespace Ryneus
{
    public class MakerEffekseerEmitter : EffekseerEmitter
    {
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
            var clip = Resources.Load<AudioClip>("Animations/Sound/" + soundTimings.se.name);
            var volume = soundTimings.se.volume * 0.01f;
            var pitch = soundTimings.se.pitch * 0.01f;
            var frame = soundTimings.frame;
            SoundManager.Instance.PlaySe(clip,volume,pitch,frame);
        }
    }
}
