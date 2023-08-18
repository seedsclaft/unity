using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebornSkillList : ListWindow , IInputHandlerEvent
{
    private List<SkillInfo> _data = new List<SkillInfo>();
    public List<SkillInfo> Data => _data;
    public void Initialize(List<SkillInfo> actorInfos,System.Action pageUpEvent,System.Action pageDownEvent)
    {
        _data = actorInfos;
        InitializeListView(actorInfos.Count);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            SkillAction skillAction = ObjectList[i].GetComponent<SkillAction>();
            skillAction.SetCallHandler((d) => {
            });
            skillAction.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputCallHandler((a) => CallInputHandler(a,pageUpEvent,pageDownEvent));
    }

    public void Refresh()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (i < _data.Count) 
            {
                SkillAction skillAction = ObjectList[i].GetComponent<SkillAction>();
                //SkillInfoComponent skillAction = ObjectList[i].GetComponent<SkillInfoComponent>();
                skillAction.SetData(_data[i],i);
            }
            ObjectList[i].SetActive(i < _data.Count);
        }
        ResetScrollPosition();
        UpdateSelectIndex(0);
        UpdateAllItems();
    }

    private void CallInputHandler(InputKeyType keyType,System.Action pageUpEvent,System.Action pageDownEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
        }
        if (keyType == InputKeyType.Cancel)
        {
        }
        if (Index >= 0)
        {
            if (keyType == InputKeyType.SideLeft1)
            {
                if (pageUpEvent != null)
                {
                    pageUpEvent();
                }
            }
            if (keyType == InputKeyType.SideRight1)
            {
                if (pageDownEvent != null)
                {
                    pageDownEvent();
                }
            }
        }
    } 

    public override void RefreshListItem(GameObject gameObject, int itemIndex)
    {
        base.RefreshListItem(gameObject,itemIndex);
        var skillAction = gameObject.GetComponent<SkillAction>();
        skillAction.SetData(_data[itemIndex],itemIndex);
        skillAction.UpdateViewItem();
    }
}
