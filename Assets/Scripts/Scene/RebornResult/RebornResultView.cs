using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RebornResult;
using Effekseer;
using TMPro;

namespace Ryneus
{
    public class RebornResultView : BaseView
    {   

    }
}

namespace RebornResult
{
    public enum CommandType
    {
        None = 0,

        EndAnimation = 2,
        RebornResultClose = 5,
        

    }
}
public class RebornResultViewEvent
{
    public RebornResult.CommandType commandType;
    public object template;

    public RebornResultViewEvent(RebornResult.CommandType type)
    {
        commandType = type;
    }
}