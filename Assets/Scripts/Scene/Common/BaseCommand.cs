using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class BaseCommand : ListItem ,IListViewItem 
    {
        [SerializeField] private TextMeshProUGUI commandName;

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = (SystemData.CommandData)ListData.Data;
            commandName.text = data.Name;
            Disable.gameObject.SetActive(ListData.Enable == false);
        }
    }
}