using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class TrainViewEvent
    {
        public Train.CommandType commandType;
        public object template;

        public TrainViewEvent(Train.CommandType type)
        {
            commandType = type;
        }
    }
}

namespace Train
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
        SkillTrigger,
        PopupSkillInfo,
        SymbolClose,
        SelectRecord,
        CancelSymbolRecord,
        CallEnemyInfo,
        Back,
        SelectSideMenu,
        StageHelp,
        CommandHelp,
        AlcanaCheck,
        ChangeSelectTacticsActor,
    }
}
