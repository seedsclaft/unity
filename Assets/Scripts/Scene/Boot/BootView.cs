using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootView : BaseView
{
    public override void Initialize() 
    {
        base.Initialize();
        new BootPresenter(this);
    }
}
