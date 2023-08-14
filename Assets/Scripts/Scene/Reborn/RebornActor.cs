using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebornActor : ListItem ,IListViewItem 
{

    [SerializeField] private ActorInfoComponent actorInfoComponent;
    [SerializeField] private GameObject RebornPrefab;
    [SerializeField] private GameObject RebornRoot;
    
    [SerializeField] private GameObject LimitRebornPrefab;
    [SerializeField] private bool LimitedSkillMax;

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
        var idx = 0;
        foreach (var rebornSkill in _data.RebornSkillInfos)
        {
            if (LimitedSkillMax && idx == 3)
            {
                var limitprefab = Instantiate(LimitRebornPrefab);
                limitprefab.transform.SetParent(RebornRoot.transform,false);
                return;
            }
            var prefab = Instantiate(RebornPrefab);
            prefab.transform.SetParent(RebornRoot.transform,false);
            var rebornSkillInfo = prefab.GetComponent<SkillInfoComponent>();
            rebornSkillInfo.SetInfoData(rebornSkill);
            idx++;
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
