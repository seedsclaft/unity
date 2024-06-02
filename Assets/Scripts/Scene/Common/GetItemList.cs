using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class GetItemList : BaseList
    {
        [SerializeField] private BaseList tacticsCommandList;
        public BaseList TacticsCommandList => tacticsCommandList;
        
        public void InitializeConfirm(List<ListData> confirmCommands ,System.Action<ConfirmCommandType> callEvent)
        {
            tacticsCommandList.Initialize();
            tacticsCommandList.SetData(confirmCommands);
            tacticsCommandList.SetInputHandler(InputKeyType.Decide,() => 
            {
                var data = (SystemData.CommandData)tacticsCommandList.ListData.Data;
                if (data.Key == "Yes")
                {
                    callEvent(ConfirmCommandType.Yes);
                } else
                if (data.Key == "No")
                {
                    callEvent(ConfirmCommandType.No);
                }
            });
            tacticsCommandList.UpdateSelectIndex(0);
        }
    }
}