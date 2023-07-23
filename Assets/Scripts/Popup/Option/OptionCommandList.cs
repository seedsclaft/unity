using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class OptionCommandList : ListWindow , IInputHandlerEvent
{

    private List<SystemData.OptionCommand> _optionCommands;
    public void Initialize(List<SystemData.OptionCommand> optionCommands,System.Action<InputKeyType,SystemData.OptionCommand> optionEvent,System.Action optionEventLisner)
    {
        InitializeListView(optionCommands.Count);
        _optionCommands = optionCommands;
        for (int i = 0; i < _optionCommands.Count;i++)
        {
            var optionCommand = ObjectList[i].GetComponent<OptionCommand>();
            optionCommand.SetData(_optionCommands[i],i);
            optionCommand.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputHandler((a) => CallInputHandler(a,optionEvent));
        UpdateSelectIndex(0);
        
        Refresh();
        optionEventLisner();
    }


    public void Refresh()
    {
        UpdateAllItems();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }

    private void CallInputHandler(InputKeyType keyType,System.Action<InputKeyType,SystemData.OptionCommand> optionEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            optionEvent(InputKeyType.Decide,_optionCommands[0]);
        }
        if (keyType == InputKeyType.Cancel)
        {
            optionEvent(InputKeyType.Cancel,_optionCommands[0]);
        }
        if (Index == -1)
        {
            return;
        }
        if (keyType == InputKeyType.Down)
        {
            if (Index > _optionCommands.Count - 3)
            {
                SetNormalizedPosition(1.0f - (float)(Index+1) / (float)_optionCommands.Count);
            } else
            if (Index == 0)
            {
                SetNormalizedPosition(1.0f - (float)Index / (float)_optionCommands.Count);
            }
        }
        if (keyType == InputKeyType.Up)
        {
            if (Index < 2)
            {
                SetNormalizedPosition(1.0f - (float)Index / (float)_optionCommands.Count);
            } else
            if (Index == 6)
            {
                SetNormalizedPosition(1.0f - (float)(Index+1) / (float)_optionCommands.Count);
            }
        }
        if (keyType == InputKeyType.Right)
        {
            optionEvent(InputKeyType.Right,_optionCommands[Index]);
        }
        if (keyType == InputKeyType.Left)
        {
            optionEvent(InputKeyType.Left,_optionCommands[Index]);
        }
        if (keyType == InputKeyType.Option1)
        {
            optionEvent(InputKeyType.Option1,_optionCommands[Index]);
        }
    }
}
