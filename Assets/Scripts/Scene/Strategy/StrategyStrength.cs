// Lvアップステータスで使用
using UnityEngine;

namespace Ryneus
{
    public class StrategyStrength : ListItem ,IListViewItem 
    {
        [SerializeField] private StrengthComponent strengthComponent;

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<ActorInfo>();
            var paramIndex = Index;
            if (Index > 0)
            {
                paramIndex++;
            }
            strengthComponent.UpdateInfo(data,(StatusParamType)paramIndex);
        }
    }
}