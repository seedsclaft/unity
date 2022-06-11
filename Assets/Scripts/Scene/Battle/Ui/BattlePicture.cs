using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePicture : MonoBehaviour
{
    [SerializeField] private BattlerInfoComp component;
    private BattlerInfo _data; 

    public void UpdatePicture(BattlerInfo battler)
    {
        _data = battler;
        component.UpdateInfo(battler);
    }
}
