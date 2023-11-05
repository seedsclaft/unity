using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankingInfo 
{
    public int Score;
    public string Name;

    public List<int> SelectIdx = new List<int>();
    public List<int> SelectRank = new List<int>();

    public int UserId;
    public RankingInfo()
    {
 
    }
}
