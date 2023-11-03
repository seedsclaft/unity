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


    private bool _rebornInit = false;

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var data = (ActorInfo)ListData.Data;
        actorInfoComponent.UpdateInfo(data,null);
        UpdateRebornSkills(data);
        Disable.gameObject.SetActive(ListData.Enable == false);
    }

    private void UpdateRebornSkills(ActorInfo actorInfo)
    {
        if (_rebornInit) return;
        _rebornInit = true;
        var idx = 0;
        foreach (var rebornSkill in actorInfo.RebornSkillInfos)
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
