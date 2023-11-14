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
        var list = new List<ListData>();
        foreach (var optionCommand in DataSystem.OptionCommand)
        {
            var optionInfo = new OptionInfo();
            optionInfo.OptionCommand = optionCommand;
            optionInfo.SliderEvent = sliderEvent;
            optionInfo.MuteEvent = muteEvent;
            optionInfo.ToggleEvent = toggleEvent;
            var listData = new ListData(optionInfo);
            list.Add(listData);
        }
        return list;
    }

    public int OptionIndex(OptionCategory optionCategory)
    {
        return (int)optionCategory;
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
