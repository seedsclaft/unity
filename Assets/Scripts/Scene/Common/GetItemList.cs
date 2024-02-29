using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Ryneus
{
    public class GetItemList : BaseList
    {
        [SerializeField] private BaseList tacticsCommandList;
        public BaseList TacticsCommandList {get {return tacticsCommandList;}}
        
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