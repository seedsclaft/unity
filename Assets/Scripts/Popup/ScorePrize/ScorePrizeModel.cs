using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class ScorePrizeModel : BaseModel
    {
        public List<ListData> ScorePrize()
        {
            return MakeListData(PartyInfo.ScorePrizeInfos);
        }
    }
}