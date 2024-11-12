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
        OnSelectSkill,  // 魔法を選択
        OnSelectEnemy,  // 敵を選択
        OnCancelEnemy,  // 敵を選択から戻る
        TargetSelectCursor,  // 対象にカーソル選択
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
