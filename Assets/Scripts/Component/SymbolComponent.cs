using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SymbolComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI commandTitle;
    [SerializeField] private Image symbolImage;
    [SerializeField] private List<Sprite> symbolSprites;
    [SerializeField] private GameObject evaluateRoot;
    [SerializeField] private TextMeshProUGUI evaluate;
    [SerializeField] private GameObject selected;
    [SerializeField] private GameObject lastSelected;
    private SymbolInfo _symbolInfo = null;
    public SymbolInfo SymbolInfo => _symbolInfo;

    private bool _animationInit = false;
    public void UpdateInfo(SymbolInfo symbolInfo)
    {
        _symbolInfo = symbolInfo;
        if (_symbolInfo == null)
        {
            return;
        }
        UpdateCommandTitle();
        UpdateSymbolImage();
        UpdateEvaluate();
        if (selected != null)
        {
            selected.SetActive(_symbolInfo.Selected);
        }
        if (lastSelected != null)
        {
            lastSelected.SetActive(_symbolInfo.LastSelected);
            selected.SetActive(_symbolInfo.Selected && !_symbolInfo.LastSelected);
            symbolImage.gameObject.SetActive(!_symbolInfo.LastSelected);
            if (_animationInit == false && _symbolInfo.LastSelected)
            {
                var uiView = lastSelected.GetComponent<RectTransform>();
                AnimationUtility.LocalMoveToLoopTransform(uiView.gameObject,
                    new Vector3(uiView.localPosition.x,uiView.localPosition.y + 4,0),
                    new Vector3(uiView.localPosition.x,uiView.localPosition.y,0),
                    1.2f);
                _animationInit = true;
            }
        }
    }

    private void UpdateCommandTitle()
    {
        if (commandTitle != null)
        {
            var textId = 40 + (int)_symbolInfo.SymbolType;
            commandTitle.text = DataSystem.System.GetTextData(textId).Text;
        }
    }

    private void UpdateSymbolImage()
    {
        if (_symbolInfo.SymbolType == SymbolType.Battle || _symbolInfo.SymbolType == SymbolType.Boss){
            symbolImage.sprite = ResourceSystem.LoadEnemySprite(_symbolInfo.TroopInfo.BossEnemy.EnemyData.ImagePath);
        } else
        {
            if (symbolImage != null && symbolSprites != null)
            {
                symbolImage.sprite = symbolSprites[(int)_symbolInfo.SymbolType];
            }
        }
    }

    private void UpdateEvaluate()
    {
        if (evaluateRoot != null)
        {
            evaluateRoot.SetActive(_symbolInfo.SymbolType == SymbolType.Battle || _symbolInfo.SymbolType == SymbolType.Boss);
        }
        if (evaluate != null)
        {
            var value = _symbolInfo.BattleEvaluate();
            evaluate.text = DataSystem.System.GetTextData(51).Text + ":" + value.ToString();
        }
    }
}
