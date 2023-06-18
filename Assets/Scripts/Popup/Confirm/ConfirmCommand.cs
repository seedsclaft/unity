using UnityEngine;
using TMPro;

public class ConfirmCommand : ListItem ,IListViewItem 
{
    [SerializeField] private TextMeshProUGUI commandName;

    private SystemData.MenuCommandData _data; 
    public void SetData(SystemData.MenuCommandData data,int index){
        _data = data;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<ConfirmComandType> handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler((ConfirmComandType)_data.Id));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        commandName.text = _data.Name;
    }
}
