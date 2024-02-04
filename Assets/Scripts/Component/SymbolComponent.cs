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
    private SymbolInfo _symbolInfo = null;

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
