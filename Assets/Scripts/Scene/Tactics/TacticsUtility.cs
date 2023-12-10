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
        return actorInfo.Level * TacticsCostRate(actorInfo);
    }

    public static int AlchemyCost(ActorInfo actorInfo,AttributeType attributeType,List<ActorInfo> stageMembers)
    {
        int cost = 4;
        var param = actorInfo.AttributeParams(stageMembers)[(int)attributeType-1];
        switch (param)
        {
            case AttributeRank.S:
                cost /= 2;
                break;
            case AttributeRank.A:
                break;
            case AttributeRank.B:
                cost *= 2;
                break;
            case AttributeRank.C:
                cost *= 4;
                break;
            case AttributeRank.D:
                cost *= 6;
                break;
            case AttributeRank.E:
                cost *= 8;
                break;
            case AttributeRank.F:
                cost *= 12;
                break;
            case AttributeRank.G:
                cost *= 16;
                break;
        }
        
        return Mathf.FloorToInt(cost * TacticsCostRate(actorInfo));
    }
    public static int RecoveryCost(ActorInfo actorInfo)
    {
        int hpCost = (int)Mathf.Ceil((actorInfo.MaxHp - actorInfo.CurrentHp) * 0.1f) * TacticsCostRate(actorInfo);
        int mpCost = (int)Mathf.Ceil((actorInfo.MaxMp - actorInfo.CurrentMp) * 0.1f) * TacticsCostRate(actorInfo);
        return hpCost > mpCost ? hpCost : mpCost;
    }
    public static int ResourceCost(ActorInfo actorInfo)
    {
        return 0;
    }
    public static int ResourceGain(ActorInfo actorInfo)
    {
        return actorInfo.Level;
    }
}
