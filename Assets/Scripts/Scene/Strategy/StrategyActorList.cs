using System.Collections;
using System.Collections.Generic;

public class StrategyActorList : BaseList
{   

    public void StartResultAnimation(int actorCount,List<bool> isBonusList,System.Action callEvent)
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            bool isBonus = (isBonusList != null && isBonusList.Count > i) ? isBonusList[i] : false;
            ObjectList[i].SetActive(false);
            if (i < actorCount)
            {
                var StrategyActor = ObjectList[i].GetComponent<StrategyActor>();
                StrategyActor.StartResultAnimation(i);
                StrategyActor.gameObject.SetActive(true);
                if (i == actorCount-1)
                {
                    StrategyActor.SetEndCallEvent(callEvent);
                }
            }
        }
    }

    public void SetShinyRefrect(bool isEnable)
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var StrategyActor = ObjectList[i].GetComponent<StrategyActor>();
            StrategyActor.SetShinyRefrect(isEnable);
        }
    }
}
