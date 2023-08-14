using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebornActor : ListItem ,IListViewItem 
{

    [SerializeField] private ActorInfoComponent actorInfoComponent;
    [SerializeField] private GameObject RebornPrefab;
    [SerializeField] private GameObject RebornRoot;

    private ActorInfo _data;
    private bool _rebornInit = false;

    public void SetData(ActorInfo data,int index){
        _data = data;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        clickButton.onClick.AddListener(() => 
        {   
            if (Disable.activeSelf) return;
            handler(Index);
        });
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        actorInfoComponent.UpdateInfo(_data,null);
        UpdateRebornSkills();
    }

    private void UpdateRebornSkills()
    {
        if (_rebornInit) return;
        _rebornInit = true;
        foreach (var rebornSkill in _data.RebornSkillInfos)
        {
            var prefab = Instantiate(RebornPrefab);
            prefab.transform.SetParent(RebornRoot.transform,false);
            var rebornSkillInfo = prefab.GetComponent<SkillInfoComponent>();
            rebornSkillInfo.SetInfoData(rebornSkill);
        }
    }

    public void SetDisable(int index,bool IsDisable)
    {
        if (index == Index)
        {
            Disable.gameObject.SetActive(IsDisable);
        }
    }
}
