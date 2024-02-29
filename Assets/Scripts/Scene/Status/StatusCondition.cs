using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class StatusCondition : ListItem ,IListViewItem 
    {
        [SerializeField] private StateInfoComponent stateInfoComponent;

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = (StateInfo)ListData.Data;
            stateInfoComponent.UpdateInfo(data);
        }
    }
}