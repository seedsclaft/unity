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
        private int _savedCount = 0;
        public int SavedCount => _savedCount;
        public void GainSaveCount()
        {
            _savedCount++;
        }
        private int _continueCount = 0;
        public int ContinueCount => _continueCount;
        public void GainContinueCount()
        {
            _continueCount++;
        }
        private int _currentTurn;
        public int CurrentTurn => _currentTurn;
        private int _clearCount;
        public int ClearCount => _clearCount;
        public void SetClearCount(int count)
        {
            _clearCount = count;
        }
        
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

        private List<TroopData> _troopDates = new();
        
        private int _currentSeekIndex = -1;
        public int CurrentSeekIndex => _currentSeekIndex;


        private List<int> _clearTroopIds = new ();
        public List<int> ClearTroopIds => _clearTroopIds;

        private List<string> _readEventKeys = new ();
        public List<string> ReadEventKeys => _readEventKeys;

        private int _routeSelect = 0;
        public int RouteSelect => _routeSelect;


        private EndingType _endingType = EndingType.C;
        public EndingType EndingType => _endingType;
        public void SetEndingType(EndingType endingType) {_endingType = endingType;}

        private bool _stageClear = false;
        public bool StageClear => _stageClear;
        public void SetStageClear(bool stageClear) {_stageClear = stageClear;}

        private bool _recordStage = false;
        public bool RecordStage => _recordStage;
        public void SetRecordStage(bool recordStage) {_recordStage = recordStage;}

        private bool _parallelStage = false;
        public bool ParallelStage => _parallelStage;
        public void SetParallelMode(bool parallelStage) {_parallelStage = parallelStage;}

        private int _rebornActorIndex = -1;
        public int RebornActorIndex => _rebornActorIndex;
        public void SetRebornActorIndex(int rebornActorIndex) {_rebornActorIndex = rebornActorIndex;}
        
        private bool _survivalMode = false;
        public bool SurvivalMode => _survivalMode;

        public StageInfo(StageData stageData)
        {
            _id = stageData.Id;
            _currentTurn = 1;
            _troopClearCount = 0;
            _savedCount = 0;
            _clearTroopIds.Clear();
        }

        public void SetSymbolInfos(List<SymbolInfo> symbolInfos)
        {
            //_currentSymbolInfos = symbolInfos;
        }


        public void SetSeekIndex(int battleIndex)
        {
            _currentSeekIndex = battleIndex;
        }

        
        public void TestTroops(int troopId,int troopLv)
        {
            //_currentSymbolInfos.Clear();
            var troopDate = DataSystem.Troops.Find(a => a.TroopId == troopId);
            
            var troopInfo = new TroopInfo(troopDate.TroopId,false);
            for (int i = 0;i < troopDate.TroopEnemies.Count;i++)
            {
                var enemyData = DataSystem.Enemies.Find(a => a.Id == troopDate.TroopEnemies[i].EnemyId);
                bool isBoss = troopDate.TroopEnemies[i].BossFlag;
                var enemy = new BattlerInfo(enemyData,troopDate.TroopEnemies[i].Lv + troopLv - 1,i,troopDate.TroopEnemies[i].Line,isBoss);
                troopInfo.AddEnemy(enemy);
            }
            _currentSeekIndex = 0;
            var stageSymbol = new StageSymbolData();
            stageSymbol.StageId = 1;
            stageSymbol.Seek = 1;
            stageSymbol.SeekIndex = 0;
            var symbolInfo = new SymbolInfo(stageSymbol);
            symbolInfo.SetTroopInfo(troopInfo);
            
            //_stageSymbolInfos.Add(symbolInfo);
        }

        public void SeekStage()
        {
            _currentTurn++;
        }

        public void DeSeekStage()
        {
            _currentTurn--;
        }

        public void AddEventReadFlag(string key)
        {
            _readEventKeys.Add(key);
        }

        public void SetRouteSelect(int routeSelect)
        {
            _routeSelect = 0;
            if (routeSelect > 0)
            {
                _routeSelect = routeSelect;
            }
        }

        public void SetMoveStageData(StageInfo stageInfo)
        {
            _clearCount = stageInfo.ClearCount;
            _troopClearCount = stageInfo._troopClearCount;
            _routeSelect = stageInfo.RouteSelect;
            _troopDates = stageInfo._troopDates;
            _savedCount = stageInfo._savedCount;
            _continueCount = stageInfo._continueCount;
            _clearTroopIds = stageInfo._clearTroopIds;
            //_readEventKeys = stageInfo._readEventKeys;
            _endingType = stageInfo._endingType;
            _stageClear = stageInfo._stageClear;
            _rebornActorIndex = stageInfo._rebornActorIndex;
            _survivalMode = stageInfo._survivalMode;
        }

        public int SelectActorIdsClassId(int selectIndex)
        {
            return 0;
        }

        public void SetSurvivalMode()
        {
            _survivalMode = true;
        }
    }
}