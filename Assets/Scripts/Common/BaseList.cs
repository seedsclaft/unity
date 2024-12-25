using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BaseList : ListWindow , IInputHandlerEvent
    {
        [SerializeField] private bool beforeSelect = true; 
        private bool _isInit = false;
        public bool IsInit => _isInit;
        private int _beforeSelectIndex = -1;
        public ListData ListData 
        { 
            get 
            {
                if (Index > -1 && ListDates.Count > Index)
                {
                    return ListDates[Index];
                }
                return null;
            }
        }

        public T ListItemData<T>()
        {
            return (T)ListData?.Data;
        }

        public void Initialize()
        {
            if (_isInit)
            {
                return;
            }
            InitializeListView();
            SetInputCallHandler((a) => CallSelectHandler(a));
            _beforeSelectIndex = -1;
            _isInit = true;
        }

        public void SetData(List<ListData> listData,bool resetScrollRect = true,Action initializeAfterEvent = null,bool unselect = false)
        {
            if (resetScrollRect && listData != ListDates)
            {
                ResetScrollRect();
            }
            SetListData(listData);
            CreateList();
            if (ListDates.Count > ObjectList.Count)
            {
                AddCreateList(ListDates.Count-ObjectList.Count);
            }
            UpdateObjectList();
            //await UniTask.DelayFrame(1);
            SetListCallHandler();
            var selectIndex = -1;
            if (resetScrollRect == false)
            {
                selectIndex = Index;
            } else
            {
                selectIndex = listData.FindIndex(a => a.Selected);
                if (selectIndex == -1 && unselect == false)
                {
                    selectIndex = 0;
                }
            }
            Refresh(selectIndex);
            initializeAfterEvent?.Invoke();
        }

        /// <summary>
        /// リストの中身を更新する
        /// </summary>
        /// <param name="listData"></param>
        public void RefreshListData(List<ListData> listData)
        {
            SetListData(listData);
            Refresh(Index);
        }

        private void InitializeRefresh(int selectIndex)
        {
            UpdateItemPrefab(selectIndex);
            UpdateAllItems();
            UpdateSelectIndex(selectIndex);
            _beforeSelectIndex = selectIndex;
        }

        private void SetListCallHandler()
        {
            for (int i = 0; i < ItemPrefabList.Count;i++)
            {
                if (ItemPrefabList.Count > i && ItemPrefabList[i] != null)
                {
                    var listItem = ItemPrefabList[i].GetComponent<ListItem>();
                    listItem.SetCallHandler(() => CallListInputHandlerDecide());
                    listItem.SetSelectHandler((index) => 
                    {
                        UpdateSelectIndex(index);
                    });
                    listItem.SetAddListenHandler(true);
                }
            }
        }    
        
        public new void Refresh(int selectIndex = 0)
        {
            base.Refresh(selectIndex);
            _beforeSelectIndex = selectIndex;
        }

        private void CallListInputHandlerDecide()
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            if (beforeSelect)
            {
                if (Index != _beforeSelectIndex)
                {
                    _beforeSelectIndex = Index;
                    return;
                }
            }
    #endif
            CallListInputHandler(InputKeyType.Decide);
        }
        
        public void RefreshListData(ListData listData)
        {
            var findIndex = ListDates.FindIndex(a => a.Index == listData.Index);
            if (findIndex > -1)
            {
                ListDates[findIndex] = listData;
            }
        }

        public void SetDisableIds(List<int> disableIds)
        {
            for (int i = 0; i < ListDates.Count;i++)
            {
                if (disableIds.Contains(i))
                {
                    ListDates[i].SetEnable(false);
                }
            }
        }
    }
}