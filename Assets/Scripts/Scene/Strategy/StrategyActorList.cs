using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategyActorList : BaseList
{   
    private List<StrategyActor> _comps = new List<StrategyActor>();

    public void StartResultAnimation(List<ActorInfo> actors,List<bool> isBonusList,System.Action callEvent)
    {
        for (int i = 0; i < _comps.Count;i++)
        {
            bool isBonus = (isBonusList != null && isBonusList.Count > i) ? isBonusList[i] : false;
            _comps[i].gameObject.SetActive(false);
            if (i < actors.Count)
            {
                _comps[i].StartResultAnimation(i);
                _comps[i].gameObject.SetActive(true);
                if (i == actors.Count-1)
                {
                    _comps[i].SetEndCallEvent(callEvent);
                }
            }
        }
    }

    public void SetShinyRefrect(bool isEnable)
    {
        for (int i = 0; i < _comps.Count;i++)
        {
            _comps[i].SetShinyRefrect(isEnable);
        }
    }
}
