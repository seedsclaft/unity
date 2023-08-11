using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebornSkillList : ListWindow , IInputHandlerEvent
{
    private List<RebornSkillInfo> _data = new List<RebornSkillInfo>();
    public void Initialize(List<RebornSkillInfo> actorInfos)
    {
        _data = actorInfos;
        InitializeListView(actorInfos.Count);
        SetInputHandler((a) => CallInputHandler(a));
    }

    public void Refresh()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (i < _data.Count) 
            {
                SkillInfoComponent skillAction = ObjectList[i].GetComponent<SkillInfoComponent>();
                skillAction.SetRebornInfoData(_data[i]);
            }
            ObjectList[i].SetActive(i < _data.Count);
        }
        ResetScrollPosition();
        UpdateSelectIndex(0);
        UpdateAllItems();
    }

    private void CallInputHandler(InputKeyType keyType)
    {
        if (keyType == InputKeyType.Decide)
        {
        }
        if (keyType == InputKeyType.Cancel)
        {
        }
        if (Index >= 0)
        {
            if (keyType == InputKeyType.Down)
            {
                UpdateScrollRect(keyType,4,_data.Count);
            }
            if (keyType == InputKeyType.Up)
            {
                UpdateScrollRect(keyType,4,_data.Count);
            }
        }
    } 

    public override void RefreshListItem(GameObject gameObject, int itemIndex)
    {
        base.RefreshListItem(gameObject,itemIndex);
        var skillInfoComponent = gameObject.GetComponent<SkillInfoComponent>();
        skillInfoComponent.SetRebornInfoData(_data[itemIndex]);
    }
}
