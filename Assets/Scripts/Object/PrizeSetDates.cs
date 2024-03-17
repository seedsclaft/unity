using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ryneus
{
    [Serializable]
    public class PrizeSetDates : ScriptableObject
    {
        public List<PrizeSetData> Data = new();
    }

    [Serializable]
    public class PrizeSetData 
    {   
        public int Id;
        public GetItemData GetItem;
    }
}