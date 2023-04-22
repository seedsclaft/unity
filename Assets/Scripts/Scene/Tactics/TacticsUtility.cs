using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsUtility 
{
    private static int TacticsCostRate(ActorInfo actorInfo)
    {
        return actorInfo.TacticsCostRate;
    }

    public static int TrainCost(ActorInfo actorInfo)
    {
        int bonus = GameSystem.CurrentData.Party.GetTrainNuminosValue();
        return actorInfo.Level * TacticsCostRate(actorInfo) + bonus;
    }

    public static int AlchemyCost(ActorInfo actorInfo,AttributeType attributeType,List<ActorInfo> stageMembers)
    {
        int cost = 2;
        int param = actorInfo.AttirbuteParams(stageMembers)[(int)attributeType-1];
        if (param > 100){
            //cost *= 1;
        } else
        if (param > 80){
            cost *= 2;
        } else
        if (param > 60){
            cost *= 4;
        } else
        if (param > 40){
            cost *= 8;
        } else
        if (param > 20){
            cost *= 16;
        } else
        if (param > 10){
            cost *= 32;
        } else
        {
            cost *= 64;
        }
        
        int bonus = GameSystem.CurrentData.Party.GetAlchemyNuminosValue();
        if (bonus > 0)
        {
            cost = (int)Mathf.Floor(cost * (1f - bonus * 0.1f));
        }
        return Mathf.FloorToInt( cost * TacticsCostRate(actorInfo));
    }
    public static int RecoveryCost(ActorInfo actorInfo)
    {
        int hpCost = (int)Mathf.Ceil((actorInfo.MaxHp - actorInfo.CurrentHp) * 0.1f) * TacticsCostRate(actorInfo);
        int mpCost = (int)Mathf.Ceil((actorInfo.MaxMp - actorInfo.CurrentMp) * 0.1f) * TacticsCostRate(actorInfo);
        return hpCost > mpCost ? hpCost : mpCost;
    }
    public static int ResourceCost(ActorInfo actorInfo)
    {
        int bonus = GameSystem.CurrentData.Party.GetResourceBonusValue();
        return actorInfo.Level + bonus;
    }
}
