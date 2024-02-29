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
            var data = (ActorInfo)ListData.Data;
            strengthComponent.UpdateInfo(data,(StatusParamType)Index);
        }
    }
}