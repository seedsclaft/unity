using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleEnemy : MonoBehaviour
{
    [SerializeField] private BattlerInfoComponent battlerInfoComponent;
    
    [SerializeField] private Button clickButton;
    private BattlerInfo _data;
    public void SetData(BattlerInfo battlerInfo)
    {
        battlerInfoComponent.UpdateInfo(battlerInfo);
        _data = battlerInfo;
    }
    
    public void SetCallHandler(System.Action<BattlerInfo> handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler((BattlerInfo)_data));
    }
}
