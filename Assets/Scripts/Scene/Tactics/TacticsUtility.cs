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
                return 0;
        if (GameSystem.CurrentStageData != null)
        {
            if (GameSystem.CurrentStageData.StageAlcana.CheckCommandCostZero(TacticsCommandType.Train))
            {
                return 0;
            }
        }
        return actorInfo.Level * TacticsCostRate(actorInfo);
    }

    public static int AlchemyTurns(ActorInfo actorInfo,AttributeType attributeType,List<ActorInfo> stageMembers)
    {
        int turns = 1;
        var param = actorInfo.AttributeRanks(stageMembers)[(int)attributeType-1];
        switch (param)
        {
            case AttributeRank.S:
                turns = 1;
                break;
            case AttributeRank.A:
                turns = 2;
                break;
            case AttributeRank.B:
                turns = 3;
                break;
            case AttributeRank.C:
                turns = 4;
                break;
            case AttributeRank.D:
                turns = 6;
                break;
            case AttributeRank.E:
                turns = 8;
                break;
            case AttributeRank.F:
                turns = 12;
                break;
            case AttributeRank.G:
                turns = 16;
                break;
        }
        
        return Mathf.FloorToInt(turns * TacticsCostRate(actorInfo));
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
        return actorInfo.Level * TacticsCostRate(actorInfo);
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
