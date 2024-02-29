using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class SlotParty : ListItem,IListViewItem
    {
        [SerializeField] private SlotInfoComponent slotInfoComponent = null;
        [SerializeField] private Button infoButton = null;
        
        private bool _IsInit = false;
        public void SetCallInfoHandler(System.Action<int> handler)
        {
            if (_IsInit == true) return;
            infoButton.onClick.AddListener(() =>
                {
                    if (Disable != null && Disable.gameObject.activeSelf) return;
                    handler(Index);
                }
            );
            _IsInit = true;
        }

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = (SlotInfo)ListData.Data;
            slotInfoComponent.UpdateInfo(data);
            if (Disable != null) Disable.SetActive(data.ActorInfos.Count == 0);
        }
    }
}