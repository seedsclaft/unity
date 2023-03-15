using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class ConfirmModel : BaseModel
{
    private int _currentIndex = 0; 
    public int CurrentIndex
    {
        get {return _currentIndex;}
    }
}

