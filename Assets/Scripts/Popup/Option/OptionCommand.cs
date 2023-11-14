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

    private bool _isInitEvent = false;

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var optionInfo = (OptionInfo)ListData.Data;
        var data = optionInfo.OptionCommand;
        optionName.text = data.Name;
        optionHelp.text = data.Help;
        optionVolume.gameObject.SetActive(data.Toggles == false);
        optionToggles.ForEach(a => a.gameObject.SetActive(data.Toggles == true));
        if (data.ToggleText1 > 0)
        {
            optionTexts[0].text = DataSystem.System.GetTextData(data.ToggleText1).Text;
        }
        if (data.ToggleText2 > 0)
        {
            optionTexts[1].text = DataSystem.System.GetTextData(data.ToggleText2).Text;
        }
        UpdateOptionValues(data);

        if (_isInitEvent == false)
        {
            if (optionInfo.SliderEvent != null && optionInfo.MuteEvent != null)
            {
                optionVolume.Initialize(optionInfo.SliderEvent,optionInfo.MuteEvent);
            }
            if (optionInfo.ToggleEvent != null)
            {
                var toggleIndex = 0;
                for (int i = 0;i < optionToggles.Count;i++)
                {
                    var idx = toggleIndex;
                    optionToggles[i].onValueChanged.AddListener((a) => {
                        if (a == true)
                        {
                            optionInfo.ToggleEvent(idx);
                        }
                    });
                    toggleIndex++;
                }
            }
            _isInitEvent = true;
        }
    }

    private void UpdateOptionValues(SystemData.OptionCommand optionCommand)
    {
        switch (optionCommand.Key)
        {
            case "BGM_VOLUME":
                optionVolume.UpdateValue(Ryneus.SoundManager.Instance._bgmVolume,Ryneus.SoundManager.Instance._bgmMute);
                return;
            case "SE_VOLUME":
                optionVolume.UpdateValue(Ryneus.SoundManager.Instance._seVolume,Ryneus.SoundManager.Instance._seMute);
                return;
            case "GRAPHIC_QUALITY":
                for (int i = 0;i < optionToggles.Count;i++)
                {
                    optionToggles[i].SetIsOnWithoutNotify(i == GameSystem.ConfigData._graphicIndex);
                }
                return;
            case "EVENT_SKIP":
                for (int i = 0;i < optionToggles.Count;i++)
                {
                    optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.ConfigData._eventSkipIndex == true ? 1 : 0));
                }
                return;
            case "COMMAND_END_CHECK":
                for (int i = 0;i < optionToggles.Count;i++)
                {
                    optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.ConfigData._commandEndCheck == true ? 1 : 0));
                }
                return;
            case "BATTLE_WAIT":
                for (int i = 0;i < optionToggles.Count;i++)
                {
                    optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.ConfigData._battleWait == true ? 1 : 0));
                }
                return;
            case "BATTLE_ANIMATION":
                for (int i = 0;i < optionToggles.Count;i++)
                {
                    optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.ConfigData._battleAnimationSkip == true ? 1 : 0));
                }
                return;
            case "INPUT_TYPE":
                for (int i = 0;i < optionToggles.Count;i++)
                {
                    optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.ConfigData._inputType == true ? 1 : 0));
                }
                return;
            case "BATTLE_AUTO":
                for (int i = 0;i < optionToggles.Count;i++)
                {
                    optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.ConfigData._battleAuto == true ? 1 : 0));
                }
                return;
        }
    }
}
