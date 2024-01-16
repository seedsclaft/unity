using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsSymbolList : BaseList
{
    
    public bool IsSelectSymbol(){
        if (ObjectList.Count > Index)
        {
            var tacticsSymbol = ObjectList[Index].GetComponent<TacticsSymbol>();
            if (tacticsSymbol.Selectable)
            {
                return true;
            }
        }
        return false;
    }

    public GetItemInfo GetItemInfo(){
        var tacticsSymbol = ObjectList[Index].GetComponent<TacticsSymbol>();
        if (tacticsSymbol.GetItemList.Index != -1)
        {
            return tacticsSymbol.GetItemInfo();
        }
        return null;
    }

    public void SetInputCallHandler()
    {
        //SetInputCallHandler((a) => CallInputHandler(a));
        SetInputHandler(InputKeyType.Right,() => CallInputHandler(InputKeyType.Right));
        SetInputHandler(InputKeyType.Left,() => CallInputHandler(InputKeyType.Left));
        SetInputHandler(InputKeyType.Down,() => CallInputHandler(InputKeyType.Down));
        SetInputHandler(InputKeyType.Up,() => CallInputHandler(InputKeyType.Up));
    }

    private void CallInputHandler(InputKeyType keyType)
    {
        if (keyType == InputKeyType.Right || keyType == InputKeyType.Left)
        {
            if (DataCount > 1)
            {
                var tacticsSymbol = ObjectList[Index].GetComponent<TacticsSymbol>();
                tacticsSymbol.UpdateItemIndex(-1);
            }
        }
        if (keyType == InputKeyType.Up)
        {
            var tacticsSymbol = ObjectList[Index].GetComponent<TacticsSymbol>();
            tacticsSymbol.UpdateItemIndex(tacticsSymbol.GetItemIndex-1);
            if (tacticsSymbol.GetItemIndex == -1)
            {
                tacticsSymbol.SetSelectable(true);
            }
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
        }
        if (keyType == InputKeyType.Down)
        {
            var tacticsSymbol = ObjectList[Index].GetComponent<TacticsSymbol>();
            tacticsSymbol.UpdateItemIndex(tacticsSymbol.GetItemIndex+1);
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
        }
    }

    public new void SetData(List<ListData> symbolInfos)
    {
        base.SetData(symbolInfos);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var tacticsSymbol = ObjectList[i].GetComponent<TacticsSymbol>();
            tacticsSymbol.SetCallHandler(() => CallSelectHandler(InputKeyType.Decide));
            tacticsSymbol.SetSelectHandler((System.Action<int>)((a) => {
                UpdateUnSelectAll();
                UpdateSelectIndex(a);
                tacticsSymbol.SetSelectable(true);
            }));
            tacticsSymbol.SetGetItemInfoCallHandler(() => 
            {
                CallListInputHandler(InputKeyType.Decide);
            });
            //tacticsSymbol.SetSymbolInfoCallHandler((a) => CallListInputHandler(InputKeyType.Option1));
            tacticsSymbol.SetGetItemInfoSelectHandler((a) => {
                for (int i = 0; i < ObjectList.Count;i++)
                {
                    var tacticsSymbol = ObjectList[i].GetComponent<TacticsSymbol>();
                    if (a != i)
                    {
                        tacticsSymbol.UpdateItemIndex(-1);
                    }
                    tacticsSymbol.SetSelectable(false);
                }
                UpdateSelectIndex(a);
            });
            tacticsSymbol.UpdateItemIndex(-1);
            tacticsSymbol.SetSelectable(i == 0);
        }
    }

    public void SetInfoHandler(System.Action<int> infoHandler)
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var tacticsSymbol = ObjectList[i].GetComponent<TacticsSymbol>();
            tacticsSymbol.SetSymbolInfoCallHandler((a) => infoHandler(a));
        }
    }

    private void UpdateUnSelectAll()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var tacticsSymbol = ObjectList[i].GetComponent<TacticsSymbol>();
            tacticsSymbol.UpdateItemIndex(-1);
            tacticsSymbol.SetSelectable(false);
        }
    }
}
