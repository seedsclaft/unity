using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class AlcanaListModel : BaseModel
    {
        public List<ListData> AlcanaList()
        {
            return MakeListData(AlcanaSkillInfos());
        }
    }
}