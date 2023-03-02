using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategyActorList: ListWindow , IInputHandlerEvent
{   
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<ActorInfo> _data = new List<ActorInfo>();

    public void Initialize(List<ActorInfo> actors,System.Action<ActorInfo> callEvent)
    {
        InitializeListView(cols);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            ObjectList[i].SetActive(false);
            if (i < actors.Count)
            {
                var actor = ObjectList[i].GetComponent<StrategyActor>();
                actor.SetData(actors[i]);
                ObjectList[i].SetActive(true);
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

    public void StartResultAnimation(List<ActorInfo> actors,System.Action callEvent)
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            ObjectList[i].SetActive(false);
            if (i < actors.Count)
            {
                ObjectList[i].SetActive(true);
                var actor = ObjectList[i].GetComponent<StrategyActor>();
                actor.SetData(actors[i]);
                actor.StartResultAnimation(i);
                if (i == ObjectList.Count-1)
                {
                    actor.SetEndCallEvent(callEvent);
                }
            }
        }
    }
}
