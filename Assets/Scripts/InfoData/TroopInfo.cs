using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class TroopInfo 
    {
        public TroopData TroopMaster => DataSystem.Troops.Find(a => a.TroopId == _troopId);
        private int _troopId = 0;
        private List<BattlerInfo> _battlerInfos = new(); 
        public List<BattlerInfo> BattlerInfos => _battlerInfos;
        public BattlerInfo BossEnemy 
        {
            get 
            {
                var boss = _battlerInfos.Find(a => a.BossFlag == true);
                if (boss != null) return boss;
                if (_battlerInfos.Count > 0)
                {
                    return _battlerInfos[_battlerInfos.Count-1];
                }
                return null;
            }
        }
        private List<GetItemInfo> _getItemInfos = new (); 
        public List<GetItemInfo> GetItemInfos => _getItemInfos;

        // リプレイを保存するか
        private bool _needReplayData = false;
        public bool NeedReplayData => _needReplayData;
        public TroopInfo(int troopId,bool needReplayData)
        {
            _troopId = troopId;
            _battlerInfos.Clear();
            _getItemInfos.Clear();
            _needReplayData = needReplayData;
        }

        public void MakeEnemyTroopDates(int plusLevel)
        {
            foreach (var troopEnemies in TroopMaster.TroopEnemies)
            {
                if (troopEnemies.StageLv <= plusLevel)
                {
                    var enemyData = DataSystem.Enemies.Find(a => a.Id == troopEnemies.EnemyId);
                    var battlerInfo = new BattlerInfo(enemyData,troopEnemies.Lv + plusLevel,_battlerInfos.Count,troopEnemies.Line,troopEnemies.BossFlag);
                    AddEnemy(battlerInfo);
                }
            }
        }

        public void MakeEnemyRandomTroopDates(int level)
        {
            var randMax = MathF.Min(3,level / 15);
            var targetLengthRand = 1 + randMax;
            while (_battlerInfos.Count <= targetLengthRand)
            {
                var targetIdRand = UnityEngine.Random.Range(1,15);
                var enemyData = DataSystem.Enemies.Find(a => a.Id == targetIdRand);
                var lineRand = UnityEngine.Random.Range(0,1);
                // 遠隔持っていない場合は前列
                if (!enemyData.Kinds.Contains(KindType.Air) && lineRand == 1)
                {
                    lineRand = 0;
                }
                var battlerInfo = new BattlerInfo(enemyData,level,_battlerInfos.Count,(LineType)lineRand,_battlerInfos.Count == 0);
                AddEnemy(battlerInfo);
            }
            var getItemData2 = new GetItemData
            {
                Param1 = 1,
                Type = GetItemType.BattleScoreBonus
            };
            _getItemInfos.Add(new GetItemInfo(getItemData2));
        }

        public void AddEnemy(BattlerInfo battlerInfo)
        {
            _battlerInfos.Add(battlerInfo);
        }
        
        public void AddGetItemInfo(GetItemInfo getItemInfo)
        {
            _getItemInfos.Add(getItemInfo);
        }
    }
}