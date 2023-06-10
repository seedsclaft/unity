using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionVolume : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider = null;
    [SerializeField] private TextMeshProUGUI volumeValue = null;
    [SerializeField] private Button muteButton = null;
    [SerializeField] private List<Sprite> muteSprites = null;

    private float _silderValue = 0;
    private bool _isMute = false;
    private System.Action<float> _callEvent;

    public void Initialize(float initValue,bool initMute, System.Action<float> callEvent,System.Action<bool> callMute)
    {
        _callEvent = callEvent;
        volumeSlider.onValueChanged.AddListener(ValueChanged);
        _isMute = initMute;
        muteButton.onClick.AddListener(() => {
            _isMute = !_isMute;
            UpdateMute();
            callMute(_isMute);
        });
        _silderValue = initValue;
        UpdateValue();
        UpdateMute();
        volumeSlider.value = initValue;
    }
    
    private void ValueChanged(float sliderValue)
    {
        _silderValue = sliderValue;
        UpdateValue();
        if (_callEvent != null) _callEvent(sliderValue);
    }

    private void UpdateValue()
    {
        volumeValue.text = ((int)(_silderValue * 100)).ToString("D");
    }

    private void UpdateMute()
    {
        if (_isMute)
        {
            muteButton.image.sprite = muteSprites[0];
        } else
        {
            muteButton.image.sprite = muteSprites[1];
        }
    }
}
