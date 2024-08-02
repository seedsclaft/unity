using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class TacticsSymbolList : BaseList
    {
        [SerializeField] private Button symbolBackButton = null;
        public bool IsSelectSymbol()
        {
            if (ItemPrefabList.Count > Index)
            {
                var tacticsSymbol = ItemPrefabList[Index].GetComponent<TacticsSymbol>();
                if (tacticsSymbol.Selectable && tacticsSymbol.GetItemIndex == -1)
                {
                    return true;
                }
            }
            return false;
        }

        public List<GetItemInfo> GetItemInfos()
        {
            var tacticsSymbol = ItemPrefabList[Index].GetComponent<TacticsSymbol>();
            if (tacticsSymbol.GetItemList.Index != -1)
            {
                return tacticsSymbol.GetItemInfos();
            }
            return null;
        }

        public GetItemInfo GetItemInfo()
        {
            var tacticsSymbol = ItemPrefabList[Index].GetComponent<TacticsSymbol>();
            if (tacticsSymbol.GetItemList.Index != -1)
            {
                return tacticsSymbol.GetItemInfo();
            }
            return null;
        }

        public void Initialize(System.Action backEvent)
        {
            symbolBackButton?.onClick.AddListener(() => 
            {
                backEvent?.Invoke();
            });
        }

        public void ChangeSymbolBackCommandActive(bool IsActive)
        {
            symbolBackButton?.gameObject.SetActive(IsActive);
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
            if (keyType == InputKeyType.Right)
            {
                var selectIndex = Index;
                if (selectIndex > DataCount)
                {
                    selectIndex = 0;
                }
                Refresh(selectIndex);
            } else
            if (keyType == InputKeyType.Left)
            {
                var selectIndex = Index;
                if (selectIndex < 0)
                {
                    selectIndex = DataCount;
                }
                Refresh(selectIndex);
            } else
            if (keyType == InputKeyType.Up)
            {
                var tacticsSymbol = ItemPrefabList[Index].GetComponent<TacticsSymbol>();
                tacticsSymbol.UpdateItemIndex(tacticsSymbol.GetItemIndex-1);
                if (tacticsSymbol.GetItemIndex == -1)
                {
                    tacticsSymbol.SetSelectable(true);
                }
                //Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            } else
            if (keyType == InputKeyType.Down)
            {
                var tacticsSymbol = ItemPrefabList[Index].GetComponent<TacticsSymbol>();
                tacticsSymbol.UpdateItemIndex(tacticsSymbol.GetItemIndex+1);
                //Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            }
        }

        public void SetData(List<ListData> symbolInfos)
        {
            base.SetData(symbolInfos);
            for (int i = 0; i < ObjectList.Count;i++)
            {
                var tacticsSymbol = ObjectList[i].GetComponentInChildren<TacticsSymbol>();
                if (tacticsSymbol == null) continue;
                tacticsSymbol.SetCallHandler(() => CallListInputHandler(InputKeyType.Decide));
                tacticsSymbol.SetAddListenHandler(false);
                tacticsSymbol.SetSelectHandler((a) =>
                {
                    Refresh(a);
                });
                tacticsSymbol.SetGetItemInfoCallHandler(() => 
                {
                    CallListInputHandler(InputKeyType.Decide);
                });
                tacticsSymbol.SetSymbolInfoCallHandler((a) => CallListInputHandler(InputKeyType.Option1));
                tacticsSymbol.SetGetItemInfoSelectHandler((a) => 
                {
                    UpdateSelectIndex(a);
                    for (int i = 0; i < ItemPrefabList.Count;i++)
                    {
                        var tacticsSymbol = ItemPrefabList[i].GetComponent<TacticsSymbol>();
                        if (a != i)
                        {
                            tacticsSymbol.UpdateItemIndex(-1);
                        }
                        tacticsSymbol.SetSelectable(false);
                    }
                });
                tacticsSymbol.UpdateItemIndex(-1);
                tacticsSymbol.SetSelectable(i == 0);
                tacticsSymbol.SetAddListenHandler(true);
            }
            Refresh(symbolInfos.FindIndex(a => a.Selected));
        }

        public new void Refresh(int selectIndex = 0)
        {
            UpdateUnSelectAll();
            UpdateSelectIndex(selectIndex);
            if (ObjectList.Count > selectIndex && selectIndex > -1)
            {
                var tacticsSymbol = ObjectList[selectIndex].GetComponentInChildren<TacticsSymbol>();
                tacticsSymbol.SetSelectable(true);
                tacticsSymbol.UpdateItemIndex(-1);
            }
        }

        public void SetInfoHandler(System.Action<int> infoHandler)
        {
            for (int i = 0; i < ItemPrefabList.Count;i++)
            {
                var tacticsSymbol = ItemPrefabList[i].GetComponent<TacticsSymbol>();
                tacticsSymbol.SetSymbolInfoCallHandler((a) => infoHandler(a));
            }
        }

        private void UpdateUnSelectAll()
        {
            for (int i = 0; i < ItemPrefabList.Count;i++)
            {
                var tacticsSymbol = ItemPrefabList[i].GetComponent<TacticsSymbol>();
                tacticsSymbol.UpdateItemIndex(-1);
                tacticsSymbol.SetSelectable(false);
            }
        }
    }
}