using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RankingInfo 
{
    public int Score;
    public string Name;
    public int Rank;
    public string RankingTypeText = "";

    public List<int> SelectIdx = new ();
    public List<int> SelectRank = new ();
    public List<ActorInfo> ActorInfos = new ();
    public System.Action<int> DetailEvent;
    public RankingInfo()
    {
 
    }
}
