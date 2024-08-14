using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class TacticsUtility 
    {
        private static int TacticsCostRate(ActorInfo actorInfo)
        {
            return actorInfo.TacticsCostRate;
        }

        public static int TrainCost(ActorInfo actorInfo)
        {
            return actorInfo.TrainCost() * TacticsCostRate(actorInfo);
        }

        public static int TrainCost(int level,ActorInfo actorInfo)
        {
            return level * TacticsCostRate(actorInfo);
        }

        public static int LearningMagicCost(ActorInfo actorInfo,AttributeType attributeType,List<ActorInfo> stageMembers,RankType rank = RankType.None)
        {
            var cost = 1;
            var rankCost = ConvertRankCost(rank);
            var param = actorInfo.AttributeRanks(stageMembers)[(int)attributeType-1];
            switch (param)
            {
                case AttributeRank.S:
                    cost = 2;
                    break;
                case AttributeRank.A:
                    cost = 4;
                    break;
                case AttributeRank.B:
                    cost = 8;
                    break;
                case AttributeRank.C:
                    cost = 12;
                    break;
                case AttributeRank.D:
                    cost = 24;
                    break;
                case AttributeRank.E:
                    cost = 36;
                    break;
                case AttributeRank.F:
                    cost = 48;
                    break;
                case AttributeRank.G:
                    cost = 64;
                    break;
            }
            
            return Mathf.FloorToInt(cost * TacticsCostRate(actorInfo) * rankCost);
        }

        private static int ConvertRankCost(RankType rankType)
        {
            switch (rankType)
            {
                case RankType.ActiveRank1:
                case RankType.PassiveRank1:
                case RankType.EnhanceRank1:
                    return 1;
                case RankType.ActiveRank2:
                case RankType.PassiveRank2:
                case RankType.EnhanceRank2:
                    return 2;
            }
            return 1;
        }

        public static int RecoveryCost(ActorInfo actorInfo,bool checkAlcana = false)
        {
            return (int)Mathf.Ceil(actorInfo.LinkedLevel() * 0.1f) * TacticsCostRate(actorInfo);
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
            return actorInfo.LinkedLevel();
        }
    }
}