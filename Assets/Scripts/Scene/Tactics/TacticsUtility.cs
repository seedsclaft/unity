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
        if (GameSystem.CurrentStageData != null)
        {
            if (GameSystem.CurrentStageData.StageAlcana.CheckCommandCostZero(TacticsCommandType.Train))
            {
                return 0;
            }
        }
        return actorInfo.Level * TacticsCostRate(actorInfo);
    }

    public static int TrainCost(int level,ActorInfo actorInfo)
    {
        if (GameSystem.CurrentStageData != null)
        {
            if (GameSystem.CurrentStageData.StageAlcana.CheckCommandCostZero(TacticsCommandType.Train))
            {
                return 0;
            }
        }
        return level * TacticsCostRate(actorInfo);
    }

    public static int LearningMagicCost(ActorInfo actorInfo,AttributeType attributeType,List<ActorInfo> stageMembers)
    {
        int cost = 2;
        var param = actorInfo.AttributeRanks(stageMembers)[(int)attributeType-1];
        switch (param)
        {
            case AttributeRank.S:
                cost = 1;
                break;
            case AttributeRank.A:
                cost = 2;
                break;
            case AttributeRank.B:
                cost = 4;
                break;
            case AttributeRank.C:
                cost = 6;
                break;
            case AttributeRank.D:
                cost = 8;
                break;
            case AttributeRank.E:
                cost = 12;
                break;
            case AttributeRank.F:
                cost = 16;
                break;
            case AttributeRank.G:
                cost = 24;
                break;
        }
        
        return Mathf.FloorToInt(cost * TacticsCostRate(actorInfo));
    }

    public static int RecoveryCost(ActorInfo actorInfo,bool checkAlcana = false)
    {
        if (checkAlcana && GameSystem.CurrentStageData != null)
        {
            if (GameSystem.CurrentStageData.StageAlcana.CheckCommandCostZero(TacticsCommandType.Recovery))
            {
                return 0;
            }
        }
        return (int)Mathf.Ceil((float)actorInfo.Level * 0.1f) * TacticsCostRate(actorInfo);
    }

    public static int RemainRecoveryCost(ActorInfo actorInfo,bool checkAlcana = false)
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
