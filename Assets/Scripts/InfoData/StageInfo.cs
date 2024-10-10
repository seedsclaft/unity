using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class StageInfo
    {
        public StageData Master => DataSystem.FindStage(_id);
        private int _id;
        public int Id => _id;
        public void SetStageId(int stageId)
        {
            _id = stageId;
        }
        private int _savedCount = 0;
        public int SavedCount => _savedCount;
        public void GainSaveCount()
        {
            _savedCount++;
        }
        private int _currentSeek;
        public int Seek => _currentSeek;
        public void SetCurrentTurn(int currentSeek)
        {
            _currentSeek = currentSeek;
        }
        /*
        private int _clearCount;
        public int ClearCount => _clearCount;
        public void SetClearCount(int count)
        {
            _clearCount = count;
        }
        */
        
        private int _score;
        public int Score => _score;
        public void SetScore(int score)
        {
            _score = score;
        }

        private int _scoreMax;
        public int ScoreMax => _scoreMax;
        public void SetScoreMax(int scoreMax)
        {
            _scoreMax = scoreMax;
        }
        private int _troopClearCount;
        public int TroopClearCount => _troopClearCount;

        //private List<TroopData> _troopDates = new();
        
        private int _currentSeekIndex = -1;
        public int CurrentSeekIndex => _currentSeekIndex;
        public void SetSeekIndex(int battleIndex)
        {
            _currentSeekIndex = battleIndex;
        }

        private WorldType _worldType;
        public WorldType WorldType => _worldType;
        public void SetWorldType(WorldType worldType)
        {
            _worldType = worldType;
        }

        private List<int> _clearTroopIds = new ();
        public List<int> ClearTroopIds => _clearTroopIds;

        private List<string> _readEventKeys = new ();
        public List<string> ReadEventKeys => _readEventKeys;

        //private int _routeSelect = 0;
        //public int RouteSelect => _routeSelect;


        private EndingType _endingType = EndingType.C;
        public EndingType EndingType => _endingType;
        public void SetEndingType(EndingType endingType) {_endingType = endingType;}

        private int _loseCount = 0;
        public int LoseCount => _loseCount;
        public void GainLoseCount(){ _loseCount++;}

        public StageInfo(StageData stageData)
        {
            _id = stageData.Id;
            _currentSeek = 1;
            _troopClearCount = 0;
            _savedCount = 0;
            _clearTroopIds.Clear();
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

        public void SeekStage()
        {
            _currentSeek++;
        }

        public void DeSeekStage()
        {
            _currentSeek--;
        }

        public void AddEventReadFlag(string key)
        {
            _readEventKeys.Add(key);
        }

        public void SetRouteSelect(int routeSelect)
        {
            /*
            _routeSelect = 0;
            if (routeSelect > 0)
            {
                _routeSelect = routeSelect;
            }
            */
        }

        public void SetMoveStageData(StageInfo stageInfo)
        {
            //_clearCount = stageInfo.ClearCount;
            _troopClearCount = stageInfo._troopClearCount;
            //_routeSelect = stageInfo.RouteSelect;
            //_troopDates = stageInfo._troopDates;
            _savedCount = stageInfo._savedCount;
            _clearTroopIds = stageInfo._clearTroopIds;
            //_readEventKeys = stageInfo._readEventKeys;
            _endingType = stageInfo._endingType;
        }

        public int SelectActorIdsClassId(int selectIndex)
        {
            return 0;
        }
    }
}