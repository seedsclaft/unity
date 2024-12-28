namespace Ryneus
{
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
            OnSelectActor,
            SelectActorList,
            SelectEnemyList,
            SkillLog,
            UpdateAp,
            OnSelectSkill,  // 魔法を選択
            OnSelectEnemy,  // 敵を選択
            OnCancelEnemy,  // 敵を選択から戻る
            TargetSelectCursor,  // 対象にカーソル選択
            EnemyLayer,
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
}