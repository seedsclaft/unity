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

    private float _sliderValue = 0;
    private bool _isMute = false;
    private System.Action<float> _callEvent;

    public void Initialize(System.Action<float> callEvent,System.Action<bool> callMute)
    {
        _callEvent = callEvent;
        volumeSlider.onValueChanged.AddListener(ValueChanged);
        muteButton.onClick.AddListener(() => {
            _isMute = !_isMute;
            UpdateMute();
            callMute(_isMute);
        });
    }
    
    private void ValueChanged(float sliderValue)
    {
        _sliderValue = sliderValue;
        UpdateValue();
        if (_callEvent != null) _callEvent(sliderValue);
    }

    private void UpdateValue()
    {
        volumeValue.text = ((int)(_sliderValue * 100)).ToString("D");
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

    public void ChangeMute()
    {
        _isMute = !_isMute;
        UpdateMute();
    }

    public void UpdateValue(float volume,bool isMute)
    {
        _sliderValue = volume;
        _isMute = isMute;
        UpdateValue();
        UpdateMute();
        volumeSlider.value = volume;
    }
}
