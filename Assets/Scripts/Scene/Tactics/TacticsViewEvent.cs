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
        SkillAlchemy,
        SelectFrontBattleIndex,
        SelectBackBattleIndex,
        SelectSymbol,
        PopupSkillInfo,
        SymbolClose,
        //SelectActorResource,
        CallEnemyInfo,
        Back,
        Dropout,
        Option,
        SelectSideMenu,
        AlcanaCheck,
        ChangeSelectTacticsActor,
    }
}
