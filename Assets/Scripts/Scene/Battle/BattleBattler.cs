using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBattler : ListItem ,IListViewItem 
{    [SerializeField] private BattlerInfoComponent battlerInfoComponent;
    public BattlerInfoComponent BattlerInfoComponent{get { return battlerInfoComponent;}}
    private BattlerInfo _data; 

    public void SetData(BattlerInfo data,int index,bool isFront){
        _data = data;
        SetIndex(index);
    }

    public void SetDamageRoot(GameObject damageRoot)
    {
        battlerInfoComponent.SetDamageRoot(damageRoot);
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        clickButton.onClick.AddListener(() => handler((int)_data.Index));
    }

    public void SetPressHandler(System.Action<int> handler)
    {
		var pressListener = clickButton.gameObject.AddComponent<ContentPressListener>();
        pressListener.SetPressEvent(() => handler(_data.Index));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        battlerInfoComponent.UpdateInfo(_data);
        battlerInfoComponent.RefreshStatus();
    }
}
