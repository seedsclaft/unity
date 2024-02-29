using System.Collections.Generic;

namespace Ryneus
{
    public class StrategyActorList : BaseList
    {   
        public void StartResultAnimation(int actorCount,List<bool> isBonusList,System.Action callEvent)
        {
            for (int i = 0; i < ItemPrefabList.Count;i++)
            {
                var isBonus = (isBonusList != null && isBonusList.Count > i) ? isBonusList[i] : false;
                ItemPrefabList[i].SetActive(false);
                if (i < actorCount)
                {
                    var StrategyActor = ItemPrefabList[i].GetComponent<StrategyActor>();
                    StrategyActor.gameObject.SetActive(true);
                    StrategyActor.StartResultAnimation(i,isBonus);
                    if (i == actorCount-1)
                    {
                        StrategyActor.SetEndCallEvent(callEvent);
                    }
                }
            }
        }

        public void SetShinyReflect(bool isEnable)
        {
            for (int i = 0; i < ItemPrefabList.Count;i++)
            {
                var StrategyActor = ItemPrefabList[i].GetComponent<StrategyActor>();
                StrategyActor.SetShinyReflect(isEnable);
            }
        }
    }
}