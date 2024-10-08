using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class TutorialDates : ScriptableObject
    {
        [SerializeField] public List<TutorialData> Data = new();
    }

    [Serializable]
    public class TutorialData 
    {   
        public int Id;
        public Scene SceneType;
        public int Type;
        public int Param1;
        public int Param2;
        public int Param3;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public string Name;
        public string Help;
    }

}