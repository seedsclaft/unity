using UnityEngine;
using TMPro;

public class BaseCommand : ListItem ,IListViewItem 
{
    [SerializeField] private TextMeshProUGUI commandName;

    private SystemData.CommandData _data; 
    public void SetData(SystemData.CommandData data,int index){
        _data = data;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler());
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        commandName.text = _data.Name;
    }

    public void SetDisable(SystemData.CommandData menuCommandData,bool IsDisable)
    {
        if (_data.Id == menuCommandData.Id)
        {
            Disable.gameObject.SetActive(IsDisable);
        }
    }
}
