using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionModel : BaseModel
{
    public OptionModel()
    {

    }

    public List<SystemData.OptionCommand> OptionCommand()
    {
        return DataSystem.OptionCommand;
    }

    public List<ListData> OptionCommandData(System.Action<float> sliderEvent,System.Action<bool> muteEvent,System.Action<int> toggleEvent)
    {
        var list = new List<OptionInfo>();
        foreach (var optionCommand in DataSystem.OptionCommand)
        {
#if UNITY_ANDROID
            if (optionCommand.ExistAndroid == false)
            {
                continue;
            }
#elif UNITY_WEBGL
            if (optionCommand.ExistWebGL == false)
            {
                continue;
            }
#endif
            var optionInfo = new OptionInfo();
            optionInfo.OptionCommand = optionCommand;
            optionInfo.SliderEvent = sliderEvent;
            optionInfo.MuteEvent = muteEvent;
            optionInfo.ToggleEvent = toggleEvent;
            list.Add(optionInfo);
        }
        return MakeListData(list);
    }

    public int OptionIndex(OptionCategory optionCategory)
    {
        return (int)optionCategory;
    }

    public void ChangeTempInputType(bool inputType)
    {
        TempData.SetInputType(inputType);
    }

}

public class OptionInfo
{
    public SystemData.OptionCommand OptionCommand;
    public InputKeyType keyType;
    public System.Action<float> SliderEvent;
    public System.Action<bool> MuteEvent;
    public System.Action<int> ToggleEvent;
}

public enum OptionCategory{
    System = 0,
    Tactics,
    Battle,
    Data
}
