using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Ryneus
{
    public class StrategyActor : ListItem ,IListViewItem  
    {   
        [SerializeField] private ActorInfoComponent component;
        [SerializeField] private Image bonusImage;

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<ActorInfo>();
            component.Clear();
            component.UpdateInfo(data,null);
        }

        public void StartResultAnimation(int animId,bool isBonus)
        {
        }

        public void SetShinyReflect(bool isEnable)
        {
        }

        public void SetEndCallEvent(System.Action callEvent)
        {
        }
    }
}