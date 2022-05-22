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


}
