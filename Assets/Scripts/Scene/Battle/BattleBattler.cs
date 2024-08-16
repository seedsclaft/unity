using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BattleBattler : ListItem ,IListViewItem 
    {
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private RectTransform thumbRect;
        private bool _isInitialized = false;
        public BattlerInfoComponent BattlerInfoComponent => battlerInfoComponent;

        public void SetDamageRoot(GameObject damageRoot)
        {
            battlerInfoComponent.SetDamageRoot(damageRoot);
        }

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var battlerInfo = (BattlerInfo)ListData.Data;
            //battlerInfoComponent.SetSelectable(ListData.Enable);
            battlerInfoComponent.UpdateInfo(battlerInfo);
            battlerInfoComponent.RefreshStatus();
            //UpdateLocalPosition(battlerInfo);
            if (Disable != null)
            {
                Disable.SetActive(!ListData.Enable);
            }
        }

        
        public void SetDisable()
        {
            if (Disable != null)
            {
                Disable.SetActive(true);
            }
        }
    }
}