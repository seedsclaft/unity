using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace Ryneus
{
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
            optionVolume.gameObject.SetActive(data.ButtonType == OptionButtonType.Slider);
            optionToggles.ForEach(a => a.gameObject.SetActive(data.ButtonType == OptionButtonType.Toggle));
            if (data.ToggleText1 > 0)
            {
                optionTexts[0].text = DataSystem.GetTextData(data.ToggleText1).Text;
            } else
            {
                optionToggles[0].gameObject.SetActive(false);
            }
            if (data.ToggleText2 > 0)
            {
                optionTexts[1].text = DataSystem.GetTextData(data.ToggleText2).Text;
            } else
            {
                optionToggles[1].gameObject.SetActive(false);
            }
            if (data.ToggleText3 > 0)
            {
                optionTexts[2].text = DataSystem.GetTextData(data.ToggleText3).Text;
            } else
            {
                optionToggles[2].gameObject.SetActive(false);
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
                    optionVolume.UpdateValue(Ryneus.SoundManager.Instance.BGMVolume,Ryneus.SoundManager.Instance.BGMMute);
                    return;
                case "SE_VOLUME":
                    optionVolume.UpdateValue(Ryneus.SoundManager.Instance.SeVolume,Ryneus.SoundManager.Instance.SeMute);
                    return;
                case "GRAPHIC_QUALITY":
                    for (int i = 0;i < optionToggles.Count;i++)
                    {
                        var notify = (i == 0 && GameSystem.ConfigData.GraphicIndex == 2) || (i == 1 && GameSystem.ConfigData.GraphicIndex == 1);
                        optionToggles[i].SetIsOnWithoutNotify(notify);
                    }
                    return;
                case "EVENT_SKIP":
                    for (int i = 0;i < optionToggles.Count;i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.ConfigData.EventSkipIndex == true ? 1 : 0));
                    }
                    return;
                case "COMMAND_END_CHECK":
                    for (int i = 0;i < optionToggles.Count;i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.ConfigData.CommandEndCheck == true ? 0 : 1));
                    }
                    return;
                case "BATTLE_WAIT":
                    for (int i = 0;i < optionToggles.Count;i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.ConfigData.BattleWait == true ? 0 : 1));
                    }
                    return;
                case "BATTLE_ANIMATION":
                    for (int i = 0;i < optionToggles.Count;i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.ConfigData.BattleAnimationSkip == true ? 1 : 0));
                    }
                    return;
                case "INPUT_TYPE":
                    for (int i = 0;i < optionToggles.Count;i++)
                    {
                        var notify = i == (GameSystem.TempData.TempInputType == true ? 1 : 0);
                        optionToggles[i].SetIsOnWithoutNotify(notify);
                    }
                    return;
                case "BATTLE_AUTO":
                    for (int i = 0;i < optionToggles.Count;i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.ConfigData.BattleAuto == true ? 1 : 0));
                    }
                    return;
                case "BATTLE_SPEED":
                    for (int i = 0;i < optionToggles.Count;i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(ConfigUtility.SpeedList[i+1] == GameSystem.ConfigData.BattleSpeed);
                    }
                    return;
            }
        }
    }
}