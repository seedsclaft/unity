using System.Collections;
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
    private List<SystemData.MenuCommandData> _sideMenus;
    private System.Action _openEvent = null;
    private System.Action _closeEvent = null;
    public void Initialize(List<SystemData.MenuCommandData> sideMenus,System.Action<SystemData.MenuCommandData> callEvent,System.Action optionEvent,System.Action cancelEvent)
    {
        var prefab = Instantiate(optionPrefab);
        prefab.transform.SetParent(optionRoot.transform,false);
        prefab.GetComponent<Button>().onClick.AddListener(() => optionEvent());

        openButton.onClick.AddListener(() => OpenSideMenu());
        closeButton.onClick.AddListener(() => CloseSideMenu());
        backButton.onClick.AddListener(() => CloseSideMenu());

        InitializeListView(sideMenus.Count);
        _sideMenus = sideMenus;
        for (int i = 0; i < sideMenus.Count;i++)
        {
            var sideMenu = ObjectList[i].GetComponent<SideMenu>();
            sideMenu.SetData(_sideMenus[i],i);
            sideMenu.SetSelectHandler((data) => UpdateSelectIndex(data));
            sideMenu.SetCallHandler((a) => callEvent(a));
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,optionEvent,cancelEvent));
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
        if (_helpWindow != null)
        {
            _helpWindow.SetInputInfo("SIDEMENU");
            _helpWindow.SetHelpText(DataSystem.System.GetTextData(701).Help);
        }
        if (_openEvent != null)
        {
            _openEvent();
        }
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
    }

    public void Refresh()
    {
        UpdateAllItems();
    }

    public new void UpdateSelectIndex(int index){
        SelectIndex(index);
        UpdateHelpWindow();
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (ObjectList[i] == null) continue;
            ListItem listItem = ObjectList[i].GetComponent<ListItem>();
            if (listItem == null) continue;
            if (index == listItem.Index){
                listItem.SetSelect();
            } else{
                listItem.SetUnSelect();
            }
        }
        optionCursor.SetActive(index == -1);
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }

    private void CallInputHandler(InputKeyType keyType,System.Action<SystemData.MenuCommandData> callEvent,System.Action optionEvent, System.Action cancelEvent)
    {
        if (keyType == InputKeyType.Right)
        {
            if (Index < 0)
            {
                UpdateSelectIndex(-1);
            } else
            {
                UpdateSelectIndex(Index);
            }
        }
        if (keyType == InputKeyType.Left)
        {
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
            callEvent(_sideMenus[Index]);
        }
        if (keyType == InputKeyType.Cancel)
        {
            cancelEvent();
        }
    }
}
