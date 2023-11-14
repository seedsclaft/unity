using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillDates : ScriptableObject
{
    [SerializeField] public List<SkillData> Data = new();
}

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
    public int RepeatTime;
    public bool AliveOnly;

    public List<FeatureData> FeatureDates;
    public List<TriggerData> TriggerDates;
    public bool IsHpDamageFeature()
    {
        return FeatureDates.Find(a => a.FeatureType == FeatureType.HpDamage) != null;
    }
    public bool IsHpHealFeature()
    {
        return FeatureDates.Find(a => a.FeatureType == FeatureType.HpHeal) != null;
    }
    public bool IsStateFeature(StateType stateType)
    {
        return FeatureDates.Find(a => a.FeatureType == FeatureType.AddState && a.Param1 == (int)stateType) != null;
    }
    [Serializable]
    public class SkillAttributeInfo
    {   
        public AttributeType AttributeType;
        public string ValueText;
        public int LearningCost;
        public string LearningCount;
    }

    public string TargteTypeText()
    {
        switch (TargetType)
        {
            case TargetType.Friend:
            return DataSystem.System.GetTextData(600).Text;
            case TargetType.Opponent:
            return DataSystem.System.GetTextData(601).Text;
            case TargetType.Self:
            return DataSystem.System.GetTextData(602).Text;
            case TargetType.All:
            return DataSystem.System.GetTextData(603).Text;
        }
        return "";
    }
    public string ScopeTypeText()
    {
        switch (Scope)
        {
            case ScopeType.All:
            if (TargetType == TargetType.Opponent)
            {
                return DataSystem.System.GetTextData(620).Text;
            }
            return DataSystem.System.GetTextData(621).Text;
            case ScopeType.Line:
            return DataSystem.System.GetTextData(622).Text;
            case ScopeType.One:
            if (TargetType == TargetType.Opponent){
                return DataSystem.System.GetTextData(623).Text;
            }
            return DataSystem.System.GetTextData(624).Text;
            case ScopeType.Self:
            return "";
        }
        return "";
    }
    public string ConvertHelpText(string help)
    {
        var targetText = TargteTypeText();
        var scopeText = ScopeTypeText();
        return help.Replace("\\s",targetText + scopeText);
    }

    [Serializable]
    public class FeatureData
    {   
        public int SkillId;
        public FeatureType FeatureType;
        public int Param1;
        public int Param2;
        public int Param3;
        public FeatureData Copy()
        {
            var feature = new FeatureData();
            feature.SkillId = SkillId;
            feature.FeatureType = FeatureType;
            feature.Param1 = Param1;
            feature.Param2 = Param2;
            feature.Param3 = Param3;
            return feature;
        }
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

        public bool IsTriggerdSkillInfo(BattlerInfo battlerInfo,List<BattlerInfo> party,List<BattlerInfo> troops)
        {
            bool CanUse = false;
            
            switch (TriggerType)
            {
                case TriggerType.None:
                    CanUse = true;
                break;
                case TriggerType.HpRateUnder:
                if (((float)battlerInfo.Hp / (float)battlerInfo.MaxHp) < Param1 * 0.01f)
                {
                    CanUse = true;
                }
                break;
                case TriggerType.HpRateUpper:
                if (((float)battlerInfo.Hp / (float)battlerInfo.MaxHp) > Param1 * 0.01f)
                {
                    CanUse = true;
                }
                break;
                case TriggerType.HpValue:
                if (battlerInfo.Hp == Param1)
                {
                    CanUse = true;
                }
                break;
                case TriggerType.MpUnder:
                if (battlerInfo.Mp <= Param1)
                {
                    CanUse = true;
                }
                break;
                case TriggerType.MpUpper:
                if (battlerInfo.Mp >= Param1)
                {
                    CanUse = true;
                }
                break;
                case TriggerType.IsExistDeathMember:
                if (troops.FindAll(a => !a.IsAlive()).Count >= Param1)
                {
                    CanUse = true;
                }
                break;
                case TriggerType.IsExistAliveMember:
                if (troops.FindAll(a => a.IsAlive()).Count >= Param1)
                {
                    CanUse = true;
                }
                break;
                case TriggerType.TurnNumUnder:
                if (battlerInfo.TurnCount < Param1)
                {
                    CanUse = true;
                }
                break;
                case TriggerType.TurnNumPer:
                if ((battlerInfo.TurnCount % Param1) - Param2 == 0)
                {
                    CanUse = true;
                }
                break;
                case TriggerType.PartyHpRateUnder:
                var filter = troops.Find(a => ((float)a.Hp / (float)a.MaxHp) < Param1 * 0.01f);
                if (filter == null)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.SelfLineFront:
                if (battlerInfo.LineIndex == LineType.Front)
                {
                    CanUse = true;
                }
                break;
                case TriggerType.SelfLineBack:
                if (battlerInfo.LineIndex == LineType.Back)
                {
                    CanUse = true;
                }
                break;
                case TriggerType.IsState:
                if (battlerInfo.IsState((StateType)Param1))
                {
                    CanUse = true;
                }
                break;
                case TriggerType.IsNotState:
                if (!battlerInfo.IsState((StateType)Param1))
                {
                    CanUse = true;
                }
                break;
                case TriggerType.LessTroopMembers:
                if ( troops.Count >= party.Count )
                {
                    CanUse = true;
                }
                break;
                case TriggerType.MoreTroopMembers:
                if ( troops.Count <= party.Count )
                {
                    CanUse = true;
                }
                break;
                case TriggerType.ChainCount:
                if (battlerInfo.ChainSuccessCount >= Param1)
                {
                    CanUse = true;
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
    WithoutSelfOne = 11,
    WithoutSelfAll = 13,
}

public enum SkillType{
    None = 0,
    Magic = 1, // 通常スキル
    Passive = 2, // パッシブ
    Demigod = 3, // 神化
    Awaken = 4, // 覚醒
    UseAlcana = 11, // アルカナ使用
    Reborn  = 12 // 転生
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
    AttackTarget = 7,
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
    MpDamage = 6,

}

public enum TriggerType
{
    None = 0,
    HpRateUnder = 1, // Hpが〇%以下
    HpRateUpper = 2, // Hpが〇%以上
    HpValue = 3, // Hpが〇
    PartyHpRateUnder = 6, // 味方にHpが〇%以下がいる
    MpUnder = 11, // Mpが〇以下
    MpUpper = 12, // Mpが〇以上
    IsExistDeathMember = 21, // 戦闘不能が〇以上存在する
    IsExistAliveMember = 22, // 生存者が〇以上存在する
    SelfLineFront = 31, // 自分が前列にいる
    SelfLineBack = 32, // 自分が後列にいる
    IsState = 41, // StateId状態になっている
    IsNotState = 42, // StateId状態になっていない
    LessTroopMembers = 51, // 味方より敵が多い
    MoreTroopMembers = 52, // 味方より敵が少ない
    TurnNumUnder = 61, // ターン数が〇以内
    TurnNumPer = 63, // ターン数がparam1 x ターン数 + param2
    AttackState = 71, // 攻撃成功時〇%で
    PayBattleMp = 101, // Mpを〇消費する
    ChainCount = 102, // 拘束成功回数
    ActionResultDeath = 103, // 攻撃を受けると戦闘不能になる
    DeadWithoutSelf = 104, // 自身以外が戦闘不能
    SelfDead = 105, // 自身が戦闘不能
    AttackedCount = 106, // 攻撃を〇回受ける
    AllEnemyCurseState = 107, // 敵全員が呪い
    ActionResultAddState = 113, // 相手が状態異常を発動する前
    DefeatEnemyByAttack = 114, // 攻撃で敵を撃破する
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
    HpStateDamage = 5,
    MpDamage = 6,
    MpHeal = 7,
    NoEffectHpDamage = 11,
    AddState = 21,
    RemoveState = 22,
    SetAfterAp = 31,
    ApHeal = 32,
    NoResetAp = 33,
    RemainHpOne = 41,
    PlusSkill = 101,
    KindHeal = 201,
    BreakUndead = 202,
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
    DisableTactics = 311,
    RebornCommandLvUp = 401,
    RebornStatusUp = 402,
    RebornAddSkill = 403,
    RebornQuest = 404,
    HpCursedDamage = 2004, // 制御用
    RemoveStatePassive = 2022, // 制御用
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