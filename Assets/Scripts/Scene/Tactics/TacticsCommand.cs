using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TacticsCommand : ListItem ,IListViewItem 
{
    [SerializeField] private TextMeshProUGUI commandName;

    private SystemData.MenuCommandData _data; 
    public void SetData(SystemData.MenuCommandData data,int index){
        _data = data;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<TacticsComandType> handler)
    {
        if (_data == null) return;        
        clickButton.onClick.AddListener(() =>
            {
                if (Disable.gameObject.activeSelf) return;
                handler((TacticsComandType)_data.Id);
            }
        );
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        commandName.text = _data.Name;
    }

    public void SetDisable(SystemData.MenuCommandData menuCommandData,bool IsDisable)
    {
        if (_data.Id == menuCommandData.Id)
        {
            Disable.gameObject.SetActive(IsDisable);
        }
    }
}
