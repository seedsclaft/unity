using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class StageInfo
    {
        public StageData Master => DataSystem.FindStage(_id);

        private List<SymbolInfo> _symbolInfos = new();
        public List<SymbolInfo> SymbolInfos => _symbolInfos;
        public void SetSymbolInfos(List<SymbolInfo> symbolInfos) => _symbolInfos = symbolInfos;
        private int _id;
        public int Id => _id;
        private int _currentSeek;
        public int CurrentSeek => _currentSeek;
        public void SetCurrentTurn(int currentSeek)
        {
            _currentSeek = currentSeek;
        }

        private int _currentSeekIndex = -1;
        public int CurrentSeekIndex => _currentSeekIndex;
        public void SetSeekIndex(int seekIndex)
        {
            _currentSeekIndex = seekIndex;
        }



        private EndingType _endingType = EndingType.C;
        public EndingType EndingType => _endingType;
        public void SetEndingType(EndingType endingType) => _endingType = endingType;

        private int _loseCount = 0;
        public int LoseCount => _loseCount;
        public void GainLoseCount(){ _loseCount++;}

        public StageInfo(int id)
        {
            _id = id;
        }
        
        public TroopInfo TestTroops(int troopId,int troopLv)
        {
            var troopDate = DataSystem.Troops.Find(a => a.TroopId == troopId);
            
            var troopInfo = new TroopInfo(troopDate.TroopId);
            for (int i = 0;i < troopDate.TroopEnemies.Count;i++)
            {
                var enemyData = DataSystem.Enemies.Find(a => a.Id == troopDate.TroopEnemies[i].EnemyId);
                bool isBoss = troopDate.TroopEnemies[i].BossFlag;
                var enemy = new BattlerInfo(enemyData,troopDate.TroopEnemies[i].Lv + troopLv - 1,i,troopDate.TroopEnemies[i].Line,isBoss);
                troopInfo.AddEnemy(enemy);
            }
            _currentSeekIndex = 0;
            return troopInfo;
            
            //_stageSymbolInfos.Add(symbolInfo);
        }


        public int SelectActorIdsClassId(int selectIndex)
        {
            return 0;
        }
    }
}