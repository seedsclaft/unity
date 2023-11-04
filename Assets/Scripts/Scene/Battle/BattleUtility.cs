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
            foreach (var skillInfo in battlerInfo.DecksData)
            {
                var skillData = skillInfo.Master;
                if (!list.Contains(skillData.AnimationName) && skillData.AnimationName != "")
                {
                    list.Add(skillData.AnimationName);
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
                if (!list.Contains(skillData.AnimationName) && skillData.AnimationName != "")
                {
                    list.Add(skillData.AnimationName);
                }
            }
        }
        return list;
    }
}
