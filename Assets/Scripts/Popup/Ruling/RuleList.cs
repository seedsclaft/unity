using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuleList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int cols = 0;

    public void Initialize()
    {
        InitializeListView();
        UpdateAllItems();
        UpdateSelectIndex(0);
    }

    public void Refresh(List<string> helpList)
    {
        for (int i = 0; i < ItemPrefabList.Count;i++)
        {
            var ruleHelp = ItemPrefabList[i].GetComponent<RuleHelp>();
            if (helpList.Count > i)
            {
                ruleHelp.SetData(helpList[i],i);
            } else
            {
                ruleHelp.SetData("",i);
            }
            ruleHelp.gameObject.SetActive(helpList.Count > i);
        }
        UpdateSelectIndex(0);
    }
}
