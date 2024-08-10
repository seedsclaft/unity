using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    abstract public partial class ListWindow : MonoBehaviour
    {
        private void CreateListPrefab(int count)
        {
            var listCount = count;
            for (var i = 0; i < listCount;i++)
            {
                var prefab = Instantiate(itemPrefab);
                prefab.name = i.ToString();
                _itemPrefabList.Add(prefab);
                var view = prefab.GetComponent<IListViewItem>();
                if (view != null)
                {
                    _itemList.AddLast(view);
                    var listItem = prefab.GetComponent<ListItem>();
                    listItem.SetListData(ListDates[i],i);
                }
                prefab.transform.SetParent(scrollRect.content, false);
                _objectList.Add(prefab);
            }
        }

        public void AddCreateListPlus(int count)
        {
            if (itemPrefabMode == true) 
            {
                return;
            }
            var listCount = count;
            for (var i = 0; i < listCount;i++)
            {
                var prefab = Instantiate(itemPrefab);
                prefab.name = i.ToString();
                _itemPrefabList.Add(prefab);
                var view = prefab.GetComponent<IListViewItem>();
                if (view != null)
                {
                    _itemList.AddLast(view);
                }
                prefab.transform.SetParent(scrollRect.content, false);
                _objectList.Add(prefab);
            }
            UpdateListItemData();
        }

        private void UpdateListItemData()
        {
            for (int i = 0;i < _itemPrefabList.Count;i++)
            {
                if (ListDates.Count > i)
                {
                    var listItem = _itemPrefabList[i].GetComponent<ListItem>();
                    listItem.SetListData(ListDates[i],i);
                }
            }
        }
    }
}
