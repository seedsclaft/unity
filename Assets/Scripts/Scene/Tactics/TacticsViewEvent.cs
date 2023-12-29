using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsViewEvent
{
    public Tactics.CommandType commandType;
    public object template;

    public TacticsViewEvent(Tactics.CommandType type)
    {
        commandType = type;
    }
}

namespace Tactics
{
    public enum CommandType
    {
        None = 0,
        AddAlcana,
        TacticsCommand,
        SelectTacticsActor, // アクターを決定
        TacticsCommandClose,
        SelectActorAlchemy,
        SelectAlchemyClose,
        SkillAlchemy,
        SelectRecoveryPlus,
        SelectRecoveryMinus,
        SelectBattleEnemy,
        PopupSkillInfo,
        EnemyClose,
        SelectActorResource,
        OpenAlcana,
        CallEnemyInfo,
        Back,
        Rule,
        Dropout,
        Option,
        SelectSideMenu,
        AlcanaCheck,
    }
}
