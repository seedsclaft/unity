using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class AlcanaListModel : BaseModel
    {
        public List<ListData> AlcanaList()
        {
            var alcanaList = StageAlcana.OwnAlcanaList;
            foreach (var alcana in alcanaList)
            {
                alcana.SetSelectedAlcana(StageAlcana.EnableOwnAlcanaList.Contains(alcana));
            }
            return MakeListData(alcanaList);
        }
    }
}