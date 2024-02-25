using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUtility
{
    public static List<string> AnimationResourcePaths(List<BattlerInfo> battlerInfos)
    {
        var list = new List<string>();
        foreach (var battlerInfo in battlerInfos)
        {
            foreach (var skillInfo in battlerInfo.Skills)
            {
                var skillData = skillInfo.Master;
                var animationData = AnimationData(skillData.AnimationId);
                if (animationData != null && !list.Contains(animationData.AnimationPath) && animationData.AnimationPath != "")
                {
                    list.Add(animationData.AnimationPath);
                }
            }
        }
        return list;
    }

    public static List<string> AnimationResourcePaths(List<ActorInfo> actorInfos)
    {
        var list = new List<string>();
        foreach (var actorInfo in actorInfos)
        {
            foreach (var skillInfo in actorInfo.Skills)
            {
                var skillData = skillInfo.Master;
                var animationData = AnimationData(skillData.AnimationId);
                if (!list.Contains(animationData.AnimationPath) && animationData.AnimationPath != "")
                {
                    list.Add(animationData.AnimationPath);
                }
            }
        }
        return list;
    }

    public static AnimationData AnimationData(int animationId)
    {
        return DataSystem.Animations.Find(a => a.Id == animationId);
    }

    public static List<TriggerTiming> StartTriggerTimings()
    {
        return new List<TriggerTiming>(){
            TriggerTiming.StartBattle,
            TriggerTiming.AfterAndStartBattle,
        };
    }

    public static List<TriggerTiming> BeforeTriggerTimings()
    {
        return new List<TriggerTiming>(){
            TriggerTiming.Before,
            TriggerTiming.BeforeAndStartBattle,
        };
    }

    public static List<TriggerTiming> AfterTriggerTimings()
    {
        return new List<TriggerTiming>(){
            TriggerTiming.After,
            TriggerTiming.AfterAndStartBattle,
        };
    }    
    
    public static List<TriggerTiming> HpDamagedTriggerTimings()
    {
        return new List<TriggerTiming>(){
            TriggerTiming.HpDamaged,
        };
    }

}
