using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebornSkillList : ListWindow , IInputHandlerEvent
{
    private List<SkillInfo> _data = new List<SkillInfo>();
    public List<SkillInfo> Data => _data;
    public void Initialize(List<SkillInfo> actorInfos)
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

    public override void RefreshListItem(GameObject gameObject, int itemIndex)
    {
        base.RefreshListItem(gameObject,itemIndex);
        var skillAction = gameObject.GetComponent<SkillAction>();
        skillAction.SetData(_data[itemIndex],itemIndex);
        skillAction.UpdateViewItem();
    }
}
