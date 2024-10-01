public class BattleViewEvent 
{
    public Battle.CommandType commandType;
    public object template;

    public BattleViewEvent(Battle.CommandType type)
    {
        commandType = type;
    }
}

namespace Battle
{
    public enum CommandType
    {
        None = 0,
        Back,
        Escape,
        SelectSideMenu,
        AttributeType,
        StartSelect,
        DecideActor,
        ActorList,
        SelectActorList,
        SelectEnemyList,
        SkillLog,
        UpdateAp,
        SelectedSkill,
        EnemyLayer,
        SelectEnemy,
        SelectParty,
        EnemyDetail,
        ChangeBattleAuto,
        ChangeBattleSpeed,
        SkipBattle,
        CancelSelectActor,
        CancelSelectEnemy,
        EndBattle
    }
}
