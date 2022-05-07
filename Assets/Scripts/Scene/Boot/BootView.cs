using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootView : BaseView
{
    protected void Start(){
        Initialize();
    }

    void Initialize(){
        new BootPresenter(this);
    }

    public void CommandSceneChange()
    {
        var eventData = new BaseViewEvent(Base.CommandType.SceneChange);
        CallSceneChangeCommand(eventData);
    }

    public void CommandInitSaveInfo()
    {
        var eventData = new BaseViewEvent(Base.CommandType.InitSaveInfo);
        CallSceneChangeCommand(eventData);
    }
}
