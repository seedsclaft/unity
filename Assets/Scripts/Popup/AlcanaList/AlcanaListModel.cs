using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlcanaListModel : BaseModel
{
    public List<ListData> AlcanaList()
    {
        var alcanaList = CurrentAlcana.OwnAlcanaList;
        foreach (var alcana in alcanaList)
        {
            alcana.SetSelectedAlcana(CurrentAlcana.CurrentTurnAlcanaList.Contains(alcana));
        }
        return MakeListData(alcanaList);
    }
}
