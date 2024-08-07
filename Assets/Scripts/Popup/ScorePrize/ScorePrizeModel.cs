using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class ScorePrizeModel : BaseModel
    {
        public List<ScorePrizeInfo> ScorePrize()
        {
            return PartyInfo.ScorePrizeInfos;
        }
    }
}