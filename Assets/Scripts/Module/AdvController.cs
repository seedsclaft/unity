using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utage;

public class AdvController : BaseView, IInputHandlerEvent
{
    [SerializeField] private AdvUguiManager advUguiManager = null;
    [SerializeField] private Button advInputButton = null;
    [SerializeField] private List<BaseCommand> skipButtonList = null;
    [SerializeField] private List<BaseCommand> autoButtonList = null;

    private bool _advPlaying = false;

    private string _lastKey = "";
    public override void Initialize() 
    {
        base.Initialize();
        advInputButton.onClick.AddListener(() => {advUguiManager.OnInput();});
        autoButtonList.ForEach(a => a.SetCallHandler(() => {
            OnClickAuto();
        }));
        UpdateAutoButton();
        skipButtonList.ForEach(a => a.SetCallHandler(() => {
            OnClickSkip();
        }));
        UpdateSkipButton();
    }

    public void StartAdv()
    {
        _advPlaying = true;
        advInputButton.gameObject.SetActive(true);
        UpdateSkipButton();
    }

    public void EndAdv()
    {
        _advPlaying = false;
        SaveSystem.SaveConfigStart(GameSystem.ConfigData);
        advInputButton.gameObject.SetActive(false);
    }
    
    public void InputHandler(InputKeyType keyType,bool pressed)
    {
        if (_advPlaying == false) return;
        if (keyType == InputKeyType.Decide || keyType == InputKeyType.Cancel)
        {
            advUguiManager.OnInput();
        }
        if (keyType == InputKeyType.Option1)
        {
            advUguiManager.Engine.Config.ToggleSkip();        
            GameSystem.ConfigData.EventSkipIndex = advUguiManager.Engine.Config.IsSkip;
        }
        if (keyType == InputKeyType.SideLeft1)
        {
            if (advUguiManager.Engine.SelectionManager.TotalCount > 0)
            {
                advUguiManager.Engine.SelectionManager.Select(0);
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            }
        }
        if (keyType == InputKeyType.SideRight1)
        {
            if (advUguiManager.Engine.SelectionManager.TotalCount > 1)
            {
                advUguiManager.Engine.SelectionManager.Select(1);
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
            }
        }
    }

    private new void Update() {
        base.Update();
        if (advUguiManager.Engine.SelectionManager.IsWaitInput == true && (HelpWindow.LastKey != "ADV_SELECTING" || HelpWindow.LastKey != "ADV_SELECTING_ONE"))
        {
            _lastKey = HelpWindow.LastKey;
            if (advUguiManager.Engine.SelectionManager.TotalCount == 1)
            {
                HelpWindow.SetInputInfo("ADV_SELECTING_ONE");

            } else
            {
                HelpWindow.SetInputInfo("ADV_SELECTING");

            }
        }
        if (advUguiManager.Engine.SelectionManager.IsWaitInput == false && HelpWindow.LastKey != "ADV_READING")
        {
            _lastKey = HelpWindow.LastKey;
            HelpWindow.SetInputInfo("ADV_READING");
        }
    }

    private void OnClickAuto()
    {
        advUguiManager.Engine.Config.ToggleAuto();
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        UpdateAutoButton();
    }

    private void UpdateAutoButton()
    {
        var auto = advUguiManager.Engine.Config.IsAutoBrPage;
        autoButtonList.ForEach(a => 
            a.Cursor.SetActive(auto)
        );
    }

    private void OnClickSkip()
    {
        advUguiManager.Engine.Config.ToggleSkip();
        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        UpdateSkipButton();
    }

    private void UpdateSkipButton()
    {
        var skip = advUguiManager.Engine.Config.IsSkip;
        skipButtonList.ForEach(a => 
            a.Cursor.SetActive(skip)
        );
        if (GameSystem.ConfigData != null)
        {
            GameSystem.ConfigData.EventSkipIndex = skip;
        }
    }
}
