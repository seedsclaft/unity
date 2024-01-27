using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SideMenuList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private Button openButton = null;
    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button backButton = null;
    [SerializeField] private GameObject optionPrefab = null;
    [SerializeField] private GameObject optionRoot = null;
    [SerializeField] private GameObject optionCursor = null;
    private List<ListData> _sideMenus;
    private System.Action _openEvent = null;
    private System.Action _closeEvent = null;
    private int _lastSelectIndex = -1;
    public void Initialize(List<ListData> sideMenus,Action<SystemData.CommandData> callEvent,Action optionEvent,Action cancelEvent)
    {
        var prefab = Instantiate(optionPrefab);
        prefab.transform.SetParent(optionRoot.transform,false);
        prefab.GetComponent<Button>().onClick.AddListener(() => optionEvent());

        openButton.onClick.AddListener(() => OpenSideMenu());
        closeButton.onClick.AddListener(() => CloseSideMenu());
        backButton.onClick.AddListener(() => CloseSideMenu());

        InitializeListView();
        SetListData(sideMenus);
        CreateObjectList();
        _sideMenus = sideMenus;
        for (int i = 0; i < sideMenus.Count;i++)
        {
            var sideMenu = ItemPrefabList[i].GetComponent<SideMenu>();
            sideMenu.SetData((SystemData.CommandData)_sideMenus[i].Data,i);
            sideMenu.SetSelectHandler((data) => UpdateSelectIndex(data));
            sideMenu.SetCallHandler((a) => callEvent(a));
        }
        SetInputCallHandler((a) => CallInputHandler(a,callEvent,optionEvent,cancelEvent));
        UpdateSelectIndex(-1);
        
        Refresh();
        CloseSideMenu(false);
    }

    public void SetOpenEvent(System.Action openEvent)
    {
        _openEvent = openEvent;
    }

    public void SetCloseEvent(System.Action closeEvent)
    {
        _closeEvent = closeEvent;
    }

    public void OpenSideMenu()
    {
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        openButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);
        optionRoot.SetActive(true);
        ScrollRect.gameObject.SetActive(true);
        if (_openEvent != null)
        {
            _openEvent();
        }
        Activate();
    }

    public void CloseSideMenu(bool isSoundNeed = true)
    {
        if (isSoundNeed)
        {
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }
        openButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        optionRoot.SetActive(false);
        ScrollRect.gameObject.SetActive(false);
        if (_closeEvent != null)
        {
            _closeEvent();
        }
        Deactivate();
    }

    public void Refresh()
    {
        UpdateAllItems();
    }

    public new void UpdateSelectIndex(int index){
        _lastSelectIndex = index;
        SelectIndex(index);
        UpdateHelpWindow();
        for (int i = 0; i < ItemPrefabList.Count;i++)
        {
            if (ItemPrefabList[i] == null) continue;
            if (i > ItemPrefabList.Count) continue;
            var listItem = ItemPrefabList[i].GetComponent<ListItem>();
            if (listItem == null) continue;
            if (index == listItem.Index){
                listItem.SetSelect();
            } else{
                listItem.SetUnSelect();
            }
        }
        optionCursor.SetActive(index == -1);
    }

    private void CallInputHandler(InputKeyType keyType,System.Action<SystemData.CommandData> callEvent,System.Action optionEvent, System.Action cancelEvent)
    {
        if (keyType == InputKeyType.Right)
        {
            if (_lastSelectIndex == 0)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                UpdateSelectIndex(-1);
            } else
            if (_lastSelectIndex == -1)
            {
                UpdateSelectIndex(_sideMenus.Count-1);
            } else
            {
                UpdateSelectIndex(Index);
            }
        }
        if (keyType == InputKeyType.Left)
        {
            if (_lastSelectIndex == (_sideMenus.Count-1))
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                UpdateSelectIndex(-1);
            } else
            if (Index >= (_sideMenus.Count))
            {
                UpdateSelectIndex(_sideMenus.Count-1);
            } else
            {
                UpdateSelectIndex(Index);
            }
        }
        if (keyType == InputKeyType.Decide)
        {
            
            if (Index == -1)
            {
                optionEvent();
                return;
            }
            callEvent((SystemData.CommandData)_sideMenus[Index].Data);
        }
        if (keyType == InputKeyType.Cancel)
        {
            cancelEvent();
        }
    }
}
