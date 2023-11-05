public class BattleViewEvent 
{
    public Battle.CommandType commandType;
    public object templete;

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
        BattleCommand,
        AttributeType,
        DecideActor,
        ActorList,
        UpdateAp,
        SelectedSkill,
        EnemyLayer,
        EndAnimation,
        EndRegeneAnimation,
        EndSlipDamageAnimation,
        SelectEnemy,
        SelectParty,
        EnemyDetail,
        ChangeBattleAuto,
        EndBattle
    }
}
