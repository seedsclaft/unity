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

    public int OptionIndex(OptionCategory optionCategory)
    {
        return (int)optionCategory;
    }
}


public enum OptionCategory{
    System = 0,
    Tactics,
    Battle,
    Data
}