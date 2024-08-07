using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
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

        // 行動前パッシブ
        public static List<TriggerTiming> BeforeActionInfoTriggerTimings()
        {
            return new List<TriggerTiming>()
            {
                TriggerTiming.BeforeSelfUse,
                TriggerTiming.BeforeFriendUse,
                TriggerTiming.BeforeOpponentUse,
            };
        }
        // 計算メソッドなど
        
        /// <summary>
        /// 作戦結果対象に複数候補がある場合に列に近い方のIndexを取得
        /// </summary>
        /// <param name="battlerInfo"></param>
        /// <param name="targetIndexList"></param>
        /// <returns></returns>
        public static int NearTargetIndex(BattlerInfo battlerInfo,List<int> targetIndexList)
        {
            if (targetIndexList.Count == 0)
            {
                return -1;
            }
            // 複数候補は列が近い方を選ぶ
            var selfIndex = battlerInfo.Index % 100;
            if (battlerInfo.IsActor == false)
            {
                selfIndex += 1;
            } else
            {
                selfIndex -= 1;
            }
            for (int i = 0;i < 5;i++)
            {
                var same = targetIndexList.FindIndex(a => a%100 == selfIndex+i);
                if (same > -1)
                {
                    return targetIndexList[same];
                }
                if (i > 0)
                {
                    var reBound = targetIndexList.FindIndex(a => a%100 == selfIndex + (i*-1));
                    if (reBound > -1)
                    {
                        return targetIndexList[reBound];
                    }
                }
            }
            return targetIndexList[0];
        }

        public static int NearTargetIndex(BattlerInfo battlerInfo,List<BattlerInfo> targetBattlerInfos)
        {
            if (targetBattlerInfos.Count == 1)
            {
                return targetBattlerInfos[0].Index;
            }
            var targetIndexList = new List<int>();
            foreach (var targetBattlerInfo in targetBattlerInfos)
            {
                targetIndexList.Add(targetBattlerInfo.Index);
            }
            return NearTargetIndex(battlerInfo,targetIndexList);
        }

    }
}