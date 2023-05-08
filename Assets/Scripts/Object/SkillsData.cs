using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillsData : ScriptableObject {
    [SerializeField] public List<SkillData> _data = new List<SkillData>();
    [Serializable]
    public class SkillData
    {   
        public int Id;
        public string Name;
        public MagicIconType IconIndex;
        public string AnimationName;
        public int AnimationPosition;
        public AnimationType AnimationType;
        public int DamageTiming;
        public int MpCost;
        public int Rank;
        public AttributeType Attribute;
        public ScopeType Scope;
        public SkillType SkillType;
        public TargetType TargetType;
        public string Help;
        public RangeType Range;
        public bool AliveOnly;

        public List<FeatureData> FeatureDatas;
        public List<TriggerData> TriggerDatas;
    }

    [Serializable]
    public class FeatureData
    {   
        public int SkillId;
        public FeatureType FeatureType;
        public int Param1;
        public int Param2;
        public int Param3;
    }
    
    [Serializable]
    public class TriggerData
    {   
        public int SkillId;
        public TriggerType TriggerType;
        public TriggerTiming TriggerTiming;
        public int Param1;
        public int Param2;
        public int Param3;

        public bool CanUseTrigger(BattlerInfo battlerInfo,UnitInfo party,UnitInfo troops)
        {
            bool CanUse = true;
            
            switch (TriggerType)
            {
                case TriggerType.IsExistDeathMember:
                if (troops.DeathMemberCount() < Param1)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.IsExistAliveMember:
                if (troops.DeathMemberCount() < Param1)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.TurnNumUnder:
                if (battlerInfo.TurnCount > Param1)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.TurnNumPer:
                if ((battlerInfo.TurnCount % Param1) - Param2 != 0)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.HpRateUnder:
                if (((float)battlerInfo.Hp / (float)battlerInfo.MaxHp) > Param1 * 0.01f)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.HpRateUpper:
                if (((float)battlerInfo.Hp / (float)battlerInfo.MaxHp) < Param1 * 0.01f)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.PartyHpRateUnder:
                var filter = troops.FindHpRateUnder(Param1);
                if (filter == false)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.SelfLineFront:
                if (battlerInfo.LineIndex == LineType.Back)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.SelfLineBack:
                if (battlerInfo.LineIndex == LineType.Front)
                {
                    CanUse = false;
                }
                break;
            }
            return CanUse;
        }
    }
}

public enum AnimationType
{
    None = 0,
    One = 1,
    Line = 2,
    All = 3
}

public enum ScopeType{
    None = 0,
    One = 1,
    Line = 2,
    All = 3,
    Self = 4,
}

public enum SkillType{
    None = 0,
    Magic = 1, // 通常スキル
    Passive = 2, // パッシブ
    Demigod = 3, // 神化
    Awaken = 4, // 覚醒
    UsedPassive = 5, // 一度切りパッシブ
    UseAlcana = 11 // アルカナ使用
}

public enum AttributeType{
    None = 0,
    Fire = 1,
    Thunder = 2,
    Ice = 3,
    Shine = 4,
    Dark = 5
}

public enum TargetType{
    None = 0,
    Opponent = 1,
    Friend = 2,
    All = 3,
    Self = 4,
    Party = 101
}

public enum RangeType{
    None = 0,
    S = 1,
    L = 2
}

public enum DamageType
{
    None = 0,
    HpDamage = 1,
    HpCritical = 2,
    HpHeal = 3,    
    MpHeal = 4,
    State = 5,

}

public enum TriggerType
{
    None = 0,
    HpRateUnder = 1, // Hpが〇%以下
    HpRateUpper = 2, // Hpが〇%以上
    HpValue = 3, // Hpが〇
    PartyHpRateUnder = 6, // 味方にHpが〇%以下がいる
    TurnNumUnder = 11, // ターン数が〇以内
    TurnNumPer = 13, // ターン数がparam1 x ターン数 + param2
    IsExistDeathMember = 21, // 戦闘不能が〇以上存在する
    IsExistAliveMember = 22, // 生存者が〇以上存在する
    SelfLineFront = 31, // 自分が前列にいる
    SelfLineBack = 32, // 自分が後列にいる
    IsState = 41, // StateId状態になっている
    LessTroopMembers = 51, // 味方より敵が多い
    MoreTroopMembers = 52, // 味方より敵が少ない
    PayBattleMp = 101, // Mpを〇消費する
    ChainCount = 102, // 拘束成功回数
    ActionResultDeath = 103, // 攻撃を受けると戦闘不能になる
    DeadWithoutSelf = 104, // 自身以外が戦闘不能
    SelfDead = 105
}

public enum TriggerTiming
{
    None = 0,
    Use = 1,
    After = 2,
    Interrupt = 3,
    StartBattle = 4
}

public enum FeatureType
{
    None = 0,
    HpDamage = 1,
    HpHeal = 2,
    HpDrain = 3,
    HpDefineDamage = 4,
    MpHeal = 7,
    NoEffectHpDamage = 11,
    AddState = 21,
    RemoveState = 22,
    ApHeal = 32,
    PlusSkill = 101,
    KindHeal = 201,
    Numinous = 301,
    TacticsCost = 302,
    EnemyLv = 303,
    AddSp = 304,
    Subordinate = 305,
    Alcana = 306,
    StatusUpCostDown = 307,
    LineZeroErase = 308,
    VictoryGainSp = 309,
    MagicAlchemy = 310,
    DisableTactics = 311
}

public enum LearningState{
    None = 0,
    Learned = 1,
    Notlearned = 2,
    SelectLearn = 3
}

public enum MagicIconType
{
    None = 0,
    Elementarism = 1, // 元素術
    LightMagic = 2, // 光魔術
    Entropy = 3, // 理術
    Mentalism = 4, // 精神術
    Psionics = 5, // 超次元
    Enchanting = 6, // 自己強化
    Craft = 7, // 工作
    Demigod = 10, //半神
    Awaken = 11, // 覚醒
    
}