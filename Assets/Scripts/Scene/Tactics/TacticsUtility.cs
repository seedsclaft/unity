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
            return actorInfo.Level * TacticsCostRate(actorInfo);
        }

        public static int TrainCost(int level,ActorInfo actorInfo)
        {
            return level * TacticsCostRate(actorInfo);
        }

        public static int LearningMagicCost(ActorInfo actorInfo,AttributeType attributeType,List<ActorInfo> stageMembers)
        {
            int cost = 4;
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
                    cost = 9;
                    break;
                case AttributeRank.C:
                    cost = 16;
                    break;
                case AttributeRank.D:
                    cost = 25;
                    break;
                case AttributeRank.E:
                    cost = 36;
                    break;
                case AttributeRank.F:
                    cost = 42;
                    break;
                case AttributeRank.G:
                    cost = 64;
                    break;
            }
            
            return Mathf.FloorToInt(cost * TacticsCostRate(actorInfo));
        }

        public static int RecoveryCost(ActorInfo actorInfo,bool checkAlcana = false)
        {
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
}