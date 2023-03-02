using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategyResultList: ListWindow , IInputHandlerEvent
{   
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<ActorInfo> _data = new List<ActorInfo>();
    [SerializeField] private TacticsCommandList tacticsCommandList;

    public void Initialize(List<GetItemInfo> getItemInfos)
    {
        InitializeListView(getItemInfos.Count);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (i < getItemInfos.Count)
            {
                var actor = ObjectList[i].GetComponent<StrategyResult>();
                actor.SetData(getItemInfos[i]);
            }
        }
        UpdateAllItems();
    }
    public new void InputHandler(InputKeyType keyType)
    {
        if (!IsInputEnable())
        {
            return;
        }
        ResetInputFrame();
    }
    
    public void InitializeConfirm(List<SystemData.MenuCommandData> confirmCommands ,System.Action<TacticsComandType> callEvent)
    {
        tacticsCommandList.Initialize(confirmCommands,callEvent);
    }
}
