using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utage;

public class AdvController : BaseView, IInputHandlerEvent
{
    [SerializeField] private AdvUguiManager advUguiManager = null;
    [SerializeField] private Button advInputButton = null;

    private bool _advPlaying = false;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        advInputButton.onClick.AddListener(() => {advUguiManager.OnInput();});
        SetInputHandler(gameObject.GetComponent<IInputHandlerEvent>());
    }

    public void StartAdv()
    {
        _advPlaying = true;
        advInputButton.gameObject.SetActive(true);
    }

    public void EndAdv()
    {
        _advPlaying = false;
        advInputButton.gameObject.SetActive(false);
    }
    
    public void InputHandler(InputKeyType keyType)
    {
        if (_advPlaying == false) return;
        if (keyType == InputKeyType.Decide || keyType == InputKeyType.Cancel)
        {
            advUguiManager.OnInput();
        }
    }
}
