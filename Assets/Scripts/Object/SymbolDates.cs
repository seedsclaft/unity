using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class SymbolDates : ScriptableObject
    {
        [SerializeField] public List<SymbolData> Data = new();
    }

    [Serializable]
    public class SymbolData 
    {   
        public int Id;
        public string Name;
        public string ImagePath;
        public int Param1;
        public int Param2;
        public int Param3;
    }
}