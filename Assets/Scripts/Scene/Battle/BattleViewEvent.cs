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
        Option,
        OpenSideMenu,
        SelectSideMenu,
        CloseSideMenu,
        AttributeType,
        StartSelect,
        DecideActor,
        ActorList,
        UpdateAp,
        SelectedSkill,
        EnemyLayer,
        EndAnimation,
        EndRegenerateAnimation,
        EndSlipDamageAnimation,
        SelectEnemy,
        SelectParty,
        EnemyDetail,
        ChangeBattleAuto,
        EndBattle
    }
}
