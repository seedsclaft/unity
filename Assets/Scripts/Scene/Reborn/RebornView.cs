using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Reborn;

namespace Ryneus
{
    public class RebornView : BaseView
    {
    }
}

namespace Reborn
    {
    public enum CommandType
    {
        None = 0,
        DecideActor = 1,
        CancelActor = 2,
        UpdateActor = 3,
        Back = 4,
        LeftActor = 5,
        RightActor = 6,
    }
}
public class RebornViewEvent
{
    public Reborn.CommandType commandType;
    public object template;

    public RebornViewEvent(Reborn.CommandType type)
    {
        commandType = type;
    }
}
