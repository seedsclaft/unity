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

        private void UpdateLocalPosition(BattlerInfo battlerInfo)
        {
            if (_isInitialized) 
            {
                return;
            }

            var positionX = 0;
            if (battlerInfo.LineIndex == LineType.Front)
            {
                if (!battlerInfo.IsActor)
                {
                    positionX *= -1;
                }
            } else
            {
                positionX *= -1;
                if (!battlerInfo.IsActor)
                {
                    positionX *= -1;
                }
            }
            //thumbRect.localPosition = new Vector3(positionX + thumbRect.localPosition.x,thumbRect.localPosition.y,0);
            _isInitialized = true;
        }
    }
}