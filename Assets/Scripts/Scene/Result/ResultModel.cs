using System;
using System.Collections.Generic;

namespace Ryneus
{
    public class ResultModel : BaseModel
    {
        public ResultModel()
        {
        }

        private List<StrategyResultViewInfo> _resultInfos = new();
        public List<StrategyResultViewInfo> ResultViewInfos => _resultInfos;


        public List<ActorInfo> TacticsActors()
        {
            return StageMembers();
        }

        public string BattleTotalScore()
        {
            return TotalScore.ToString("F2") + "%";
        }

        public List<SystemData.CommandData> ResultCommand()
        {
            return BaseConfirmCommand(3040,19040);
        }
    }
}