using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class ScorePrizeDates : ScriptableObject
    {
        public List<ScorePrizeData> Data = new();
    }

    [Serializable]
    public class ScorePrizeData 
    {   
        public int Id;
        public int PriseSetId;
        public int Score;
        public string Title;
        public string Help;
    }
}