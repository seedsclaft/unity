using UnityEngine;

namespace Ryneus
{
    public class StatusMemberItem : ListItem ,IListViewItem  
    {
        [SerializeField] private GameObject innerObj;
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private _2dxFX_Shiny_Reflect shinyReflect;

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<ActorInfo>();
            actorInfoComponent.Clear();
            actorInfoComponent.UpdateInfo(data,null);
        }
    }
}
