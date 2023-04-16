using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuStageList: ListWindow , IInputHandlerEvent
{   
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<StageInfo> _data = new List<StageInfo>();
    [SerializeField] private StageInfoComponent component;

    public int selectIndex{
        get {return Index;}
    }

    public void Initialize(List<StageInfo> stages,System.Action<StageInfo> callEvent)
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
        SetInputHandler((a) => CallInputHandler(a,callEvent));
        UpdateAllItems();
        UpdateSelectIndex(0);
        component.UpdateInfo(_data[Index]);
    }
    
    public override void UpdateHelpWindow(){
        if (component != null){
            component.UpdateInfo(_data[Index]);
        }
    }

    private void CallInputHandler(InputKeyType keyType, System.Action<StageInfo> callEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            callEvent(_data[Index]);
        }
    }
}
