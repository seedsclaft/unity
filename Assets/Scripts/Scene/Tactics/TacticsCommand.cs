using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TacticsCommand : ListItem ,IListViewItem 
{
    [SerializeField] private TextMeshProUGUI commandName;

    private SystemData.CommandData _data; 
    public void SetData(SystemData.CommandData data,int index){
        _data = data;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<TacticsComandType> handler)
    {    
        clickButton.onClick.AddListener(() =>
            {
                if (Disable.gameObject.activeSelf) return;
                handler((TacticsComandType)_data.Id);
            }
        );
    }

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var data = (SystemData.CommandData)ListData.Data;
        commandName.text = data.Name;
        Disable.gameObject.SetActive(ListData.Enable == false);
    }

    public void SetDisable(SystemData.CommandData menuCommandData,bool IsDisable)
    {
        if (_data.Id == menuCommandData.Id)
        {
            Disable.gameObject.SetActive(IsDisable);
        }
    }
}
