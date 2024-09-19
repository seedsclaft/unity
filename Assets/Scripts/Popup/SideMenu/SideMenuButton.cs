using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SideMenuButton : ListItem ,IListViewItem 
    {
        [SerializeField] private TextMeshProUGUI commandName;

        private SystemData.CommandData _data; 
        public void SetData(SystemData.CommandData data,int index)
        {
            _data = data;
            SetIndex(index);
            commandName.text = data.Name;
        }

        public void SetText(string text)
        {
            commandName.text = text;
        }

        public void SetCallHandler(System.Action<SystemData.CommandData> handler)
        {
            clickButton.onClick.AddListener(() => 
            {
                handler(_data);
            });
        }

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<SystemData.CommandData>();
            commandName.text = data.Name;
        }
    }
}