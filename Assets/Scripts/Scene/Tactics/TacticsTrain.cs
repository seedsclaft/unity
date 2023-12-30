using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TacticsTrain : ListItem ,IListViewItem 
{
    [SerializeField] private TacticsComponent tacticsComponent;
    [SerializeField] private Toggle checkToggle;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;

    public void SetToggleHandler(System.Action handler)
    {
        checkToggle.onValueChanged.AddListener((a) => handler());
    }

    public void SetPlusHandler(System.Action handler)
    {
        plusButton.onClick.AddListener(() => handler());
    }

    public void SetMinusHandler(System.Action handler)
    {
        minusButton.onClick.AddListener(() => handler());
    }

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var data = (TacticsActorInfo)ListData.Data;
        tacticsComponent.UpdateInfo(data.ActorInfo,data.TacticsCommandType);
        Disable.SetActive(!data.ActorInfo.EnableTactics(data.TacticsCommandType));
    }
}
