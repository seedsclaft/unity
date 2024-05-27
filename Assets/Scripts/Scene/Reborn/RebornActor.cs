using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
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
            Disable.gameObject.SetActive(ListData.Enable == false);
        }

        public void SetDisable(int index,bool IsDisable)
        {
            if (index == Index)
            {
                Disable.gameObject.SetActive(IsDisable);
            }
        }
    }
}