using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [System.Serializable]
    public class RankingInfo 
    {
        public int Score;
        public string Name;
        public int Rank;
        public string ActorData;

        private List<ActorInfo> _actorInfos = new ();
        public List<ActorInfo> ActorInfos => _actorInfos;
        public void SetActorInfos(List<ActorInfo> actorInfos) => _actorInfos = actorInfos;
        public System.Action<int> DetailEvent;
        public RankingInfo()
        {
    
        }

        public void CopyInfo(RankingInfo baseRankingInfo)
        {
            Score = baseRankingInfo.Score;
            Name = baseRankingInfo.Name;
            Rank = baseRankingInfo.Rank;
            _actorInfos = baseRankingInfo.ActorInfos;
            DetailEvent = baseRankingInfo.DetailEvent;
        }
    }
}