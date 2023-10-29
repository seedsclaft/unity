using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MakerEffectData
{
    public int id;
    public int displayType;
    public string effectName;
    public List<FlashTimings> flashTimings;
    public string name;
    public int offsetX;
    public int offsetY;
    public Rotation rotation;
    public int scale;
    public List<SoundTimings> soundTimings;
    public int speed;
    public int[] timings;

    [Serializable]
    public class FlashTimings
    {
        public int frame;
        public int duration;
        public int[] color;
    }
    [Serializable]
    public class Rotation
    {
        public int x;
        public int y;
        public int z;
    }
    [Serializable]
    public class SoundTimings
    {
        public int frame;
        public Se se;
    }
    [Serializable]
    public class Se
    {
        public string name;
        public int pan;
        public int pitch;
        public int volume;
    }
}

[Serializable]
public class MakerEffectDatas
{
    [SerializeField] public MakerEffectData[] data;
}