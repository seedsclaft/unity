using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TacticsTrain : ListItem ,IListViewItem 
{
    [SerializeField] private TacticsComponent tacticsComponent;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;

    public void SetPlusHandler(System.Action handler)
    {
        Debug.Log("SetPlusHandler");
        plusButton.onClick.AddListener(() => handler());
    }

    public void SetMinusHandler(System.Action handler)
    {
        Debug.Log("SetMinusHandler");
        minusButton.onClick.AddListener(() => handler());
    }

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var data = (TacticsActorInfo)ListData.Data;
        tacticsComponent.UpdateInfo(data.ActorInfo,data.TacticsComandType);
        Disable.SetActive(!data.ActorInfo.EnableTactics(data.TacticsComandType));
    }
}
