﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSkillList : ListWindow , IInputHandlerEvent
{
    public void Initialize(List<SkillInfo> skills,System.Action<SkillInfo> callEvent)
    {
        InitializeListView(skills.Count);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var actor = ObjectList[i].GetComponent<BattleSkillListItem>();
            actor.SetData(skills[i]);
            actor.SetCallHandler(callEvent);
        }
        UpdateAllItems();
    }

    public void InputHandler(InputKeyType keyType)
    {
        if (!IsInputEnable())
        {
            return;
        }
        if (keyType == InputKeyType.Down){
            ObjectList[Index].GetComponent<ListItem>().SetUnSelect();
            SelectIndex(Index+1);
            ObjectList[Index].GetComponent<ListItem>().SetSelect();
        } else
        if (keyType == InputKeyType.Up){
            ObjectList[Index].GetComponent<ListItem>().SetUnSelect();
            SelectIndex(Index-1);
            ObjectList[Index].GetComponent<ListItem>().SetSelect();
        } 
        ResetInputFrame();
    }
}
