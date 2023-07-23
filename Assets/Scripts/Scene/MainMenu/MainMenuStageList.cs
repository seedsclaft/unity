using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuStageList: ListWindow , IInputHandlerEvent
{   
    private List<StageInfo> _data = new List<StageInfo>();
    [SerializeField] private StageInfoComponent component;

    public void Initialize(List<StageInfo> stages,System.Action<StageInfo> callEvent,System.Action sideMenuEvent)
    {
        InitializeListView(stages.Count);
        _data = stages;
        for (int i = 0; i < stages.Count;i++)
        {
            MainMenuStage mainMenuStage = ObjectList[i].GetComponent<MainMenuStage>();
            mainMenuStage.SetData(stages[i],i);
            mainMenuStage.SetCallHandler(callEvent);
            mainMenuStage.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,sideMenuEvent));
        UpdateAllItems();
        UpdateSelectIndex(0);
        component.UpdateInfo(_data[Index]);
    }
    
    public override void UpdateHelpWindow(){
        if (component != null){
            component.UpdateInfo(_data[Index]);
        }
    }

    private void CallInputHandler(InputKeyType keyType, System.Action<StageInfo> callEvent,System.Action sideMenuEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            callEvent(_data[Index]);
        }
        if (keyType == InputKeyType.Option1)
        {
            sideMenuEvent();
        }
    }
}
