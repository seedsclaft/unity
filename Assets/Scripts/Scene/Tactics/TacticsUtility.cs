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

    public static int AlchemyCost(ActorInfo actorInfo,int skillId,List<ActorInfo> stageMembers,int hintLv)
    {
        SkillsData.SkillData skillData = DataSystem.Skills.Find(a => a.Id == skillId);
        int cost = skillData.Rank;
        if (skillData != null)
        {
            AttributeType attributeType = skillData.Attribute;
            var param = actorInfo.AttirbuteParams(stageMembers)[(int)attributeType-1];
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
            if (param > 0){
                cost *= 64;
            }
        }
        return Mathf.FloorToInt( cost * TacticsCostRate(actorInfo) * (1 - hintLv * 0.1f));
    }
    public static int RecoveryCost(ActorInfo actorInfo)
    {
        int hpCost = (int)Mathf.Ceil((actorInfo.MaxHp - actorInfo.CurrentHp) * 0.1f) * TacticsCostRate(actorInfo);
        int mpCost = (int)Mathf.Ceil((actorInfo.MaxMp - actorInfo.CurrentMp) * 0.1f) * TacticsCostRate(actorInfo);
        return hpCost > mpCost ? hpCost : mpCost;
    }
    public static int ResourceCost(ActorInfo actorInfo)
    {
        return actorInfo.Level;
    }
}
