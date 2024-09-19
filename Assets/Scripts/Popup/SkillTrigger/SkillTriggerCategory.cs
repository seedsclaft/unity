using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class SkillTriggerCategory : ListItem ,IListViewItem
    {
        [SerializeField] private TextMeshProUGUI triggerText;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<string>();
            triggerText.SetText(data);
        }
    }
}
