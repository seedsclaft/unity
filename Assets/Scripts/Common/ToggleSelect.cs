using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class ToggleSelect : MonoBehaviour
    {
        [SerializeField] private GameObject tabPrefab;
        [SerializeField] private GameObject tabRoot;
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private List<GameObject> _viewObjs = new ();
        private List<Toggle> _selectTabs = new ();
        private List<CanvasGroup> _selectTabCanvasGroup = new ();

        private int _selectTabIndex = -1;
        public int SelectTabIndex => _selectTabIndex;
        public void SetSelectTabIndex(int selectIndex)
        {
            if (_selectTabIndex != selectIndex)
            {
                _selectTabIndex = selectIndex;
                UpdateTabs();
            }
        }

        public void Initialize(List<string> tabTitles)
        {
            foreach (var tabTitle in tabTitles)
            {
                var prefab = Instantiate(tabPrefab);
                prefab.transform.SetParent(tabRoot.transform, false);
                var toggle = prefab.GetComponent<Toggle>();
                _selectTabs.Add(toggle);
                toggle.group = toggleGroup;
                var text = prefab.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null)
                {
                    text.SetText(tabTitle);
                }
                var canvasGroup = prefab.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    _selectTabCanvasGroup.Add(canvasGroup);
                }
            }
        }

        public void SetClickHandler(System.Action clickEvent)
        {
            var idx = 0;
            foreach (var selectTab in _selectTabs)
            {
                var tabIndex = idx;
                selectTab.onValueChanged.AddListener((a) => 
                {
                    SetSelectTabIndex(tabIndex);
                    if (a == true)
                    {
                        SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                        if (clickEvent != null)
                        {
                            clickEvent();
                        }
                    }
                });
                idx++;
            }
        }
        
        public void SetActiveTab(int selectCharacterTabType,bool isActive)
        {    
            _selectTabs[selectCharacterTabType].gameObject.SetActive(isActive);
        }

        public void SelectCharacterTabSmooth(int index)
        {
            var nextIndex = _selectTabIndex + index;
            var displayTabs = _selectTabs.FindAll(a => a.gameObject.activeSelf);
            if (nextIndex < 0)
            {
                for (int i = 0;i < _selectTabs.Count;i++)
                {
                    if (_selectTabs[i].gameObject.activeSelf)
                    {            
                        nextIndex = i;
                    }
                }
            } else
            if (nextIndex > displayTabs.Count)
            {
                nextIndex = _selectTabs.FindIndex(a => a.gameObject.activeSelf);
            } else
            {
                if (index > 0)
                {
                    for (int i = 0;i < _selectTabs.Count;i++)
                    {
                        if (!_selectTabs[i].gameObject.activeSelf && (i == nextIndex))
                        {
                            nextIndex++;
                        }
                    }
                } else
                {
                    for (int i = _selectTabs.Count-1;i >= 0;i--)
                    {
                        if (!_selectTabs[i].gameObject.activeSelf && (i == nextIndex))
                        {
                            nextIndex--;
                        }
                    }
                }
            }
            SetSelectTabIndex(nextIndex);
        }

        public void UpdateTabs()
        {
            for (int i = 0;i < _selectTabs.Count;i++)
            {
                _selectTabs[i].SetIsOnWithoutNotify(_selectTabIndex == i);
            }
            for (int i = 0;i < _viewObjs.Count;i++)
            {
                _viewObjs[i].SetActive(_selectTabIndex == i);
            }
            for (int i = 0;i < _selectTabCanvasGroup.Count;i++)
            {
                _selectTabCanvasGroup[i].alpha = _selectTabIndex == i ? 1 : 0.75f;
            }
        }
    }
}
