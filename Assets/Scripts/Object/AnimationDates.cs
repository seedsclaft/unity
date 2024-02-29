using System;
using System.Collections.Generic;
using UnityEngine;
using Effekseer;
using UnityEditor;

namespace Ryneus
{
    [Serializable]
    public class AnimationDates : ScriptableObject
    {
        [SerializeField] public List<AnimationData> Data = new();
    }

    [Serializable]
    public class AnimationData 
    {   
        public int Id;
        public string AnimationPath;
        public bool MakerEffect;
        public int Position;
        public float Scale;
        public float Speed;
        public int DamageTiming;

        private static string _resourcesPath = "Assets/Resources/Animations/";
        private static string _backupPath = "Assets/Resources_bak/Animations/";
    }
}