using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class HelpDates : ScriptableObject
    {
        public List<HelpData> Data = new();
    }

    [Serializable]
    public class HelpData
    {   
        public int Id;
        public string Key;
        public string Help;
    }
}