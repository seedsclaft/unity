using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlcanaListModel : BaseModel
{
    public List<ListData> AlcanaList()
    {
        var alcanaList = StageAlcana.OwnAlcanaList;
        foreach (var alcana in alcanaList)
        {
            alcana.SetSelectedAlcana(StageAlcana.CurrentTurnAlcanaList.Contains(alcana));
        }
        return MakeListData(alcanaList);
    }
}
