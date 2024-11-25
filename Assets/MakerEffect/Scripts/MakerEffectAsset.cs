using System.Collections.Generic;
using UnityEngine;

namespace Effekseer
{
    public class MakerEffectAsset : EffekseerEffectAsset
    {
        [SerializeField]
        public List<MakerEffectData.SoundTimings> soundTimings;
    }
}
