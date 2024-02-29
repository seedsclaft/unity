using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
            get {
                if (Index > -1 && ListDates.Count > Index)
                {
                    return ListDates[Index];
                }
                return null;
            }
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

        public void SetData(List<ListData> listData)
        {
            if (listData != ListDates)
            {
                ResetScrollRect();
            }
            SetListData(listData);
            CreateObjectList();
            if (ListDates.Count > ObjectList.Count)
            {
                var objectListCount = ObjectList.Count;
                AddCreateList(ListDates.Count-ObjectList.Count);
            }
            SetListCallHandler();
            Refresh(ListDates.FindIndex(a => a.Selected || a.Enable));
        }

        
        private void SetListCallHandler()
        {
            for (int i = 0; i < ItemPrefabList.Count;i++)
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
            Refresh(ListDates.FindIndex(a => a.Selected));
        }
    }


    [System.Serializable]
    public class ListData
    {    
        private int _index;
        public int Index => _index;
        private object _data;
        public object Data => _data;
        private bool _enable = true;
        public bool Enable => _enable;
        public void SetEnable(bool enable)
        {
            _enable = enable;
        }
        private bool _selected = true;
        public bool Selected => _selected;
        public void SetSelected(bool selected)
        {
            _selected = selected;
        }

        public ListData(object data,int index = 0,bool enable = true)
        {
            _data = data;
            _index = index;
            _enable = enable;
        }
        
        public static List<ListData> MakeListData<T>(T data,bool isEnable = true)
        {
            var list = new List<ListData>();
            var listData = new ListData(data,0);
            listData.SetEnable(isEnable);
            list.Add(listData);
            return list;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList,bool isEnable = true)
        {
            var list = new List<ListData>();
            var idx = 0;
            foreach (var data in dataList)
            {
                var listData = new ListData(data,idx);
                listData.SetEnable(isEnable);
                list.Add(listData);
                idx++;
            }
            return list;
        }
    }
}