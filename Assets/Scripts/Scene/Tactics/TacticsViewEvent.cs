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
        TacticsCommand,
        SelectTacticsActor, // アクターを決定
        TacticsCommandClose,
        SelectActorAlchemy,
        SelectAlchemyClose,
        SkillAlchemy,
        SelectFrontBattleIndex,
        SelectBackBattleIndex,
        SelectSymbol,
        PopupSkillInfo,
        SymbolClose,
        SelectEnemyClose,
        //SelectActorResource,
        CallEnemyInfo,
        Back,
        Rule,
        Dropout,
        Option,
        SelectSideMenu,
        AlcanaCheck,
        ChangeSelectTacticsActor,
    }
}
