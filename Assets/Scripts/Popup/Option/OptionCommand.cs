using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class OptionCommand : ListItem ,IListViewItem 
{
    [SerializeField] private TextMeshProUGUI optionName;
    [SerializeField] private TextMeshProUGUI optionHelp;
    [SerializeField] private OptionVolume optionVolume;
    [SerializeField] private List<Toggle> optionToggles;
    [SerializeField] private List<TextMeshProUGUI> optionTexts;
    

    private SystemData.OptionCommand _data; 
    public void SetData(SystemData.OptionCommand data,int index){
        _data = data;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<ConfirmCommandType> handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler((ConfirmCommandType)_data.Id));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        optionName.text = _data.Name;
        optionHelp.text = _data.Help;
        if (_data.Toggles == false)
        {
        }
        optionVolume.gameObject.SetActive(_data.Toggles == false);
        optionToggles.ForEach(a => a.gameObject.SetActive(_data.Toggles == true));
        if (_data.ToggleText1 > 0)
        {
            optionTexts[0].text = DataSystem.System.GetTextData(_data.ToggleText1).Text;
        }
        if (_data.ToggleText2 > 0)
        {
            optionTexts[1].text = DataSystem.System.GetTextData(_data.ToggleText2).Text;
        }
    }
}
