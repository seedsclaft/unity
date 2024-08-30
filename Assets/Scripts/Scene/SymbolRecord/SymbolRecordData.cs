using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SymbolRecordData : ListItem ,IListViewItem
    {
        [SerializeField] private List<SymbolComponent> symbolComponents;
        [SerializeField] private GameObject pastObj;
        [SerializeField] private GameObject nextObj;
        [SerializeField] private GameObject currentObj;
        [SerializeField] private GameObject futureObj;
        [SerializeField] private TextMeshProUGUI seekerText;
        [SerializeField] private TextMeshProUGUI stageDataText;

        private bool _isButtonInit = false;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var dates = (List<SymbolResultInfo>)ListData.Data;
            foreach (var symbolComponent in symbolComponents)
            {
                symbolComponent.gameObject.SetActive(false);
            }
            foreach (var data in dates)
            {
                if (symbolComponents.Count > 0)
                {
                    var symbolComponent = symbolComponents[data.StageSymbolData.SeekIndex];
                    symbolComponent.gameObject.SetActive(true);
                    symbolComponent.UpdateInfo(data.SymbolInfo,data.Selected,data.Seek);
                }
            }
            if (dates.Count > 0)
            {
                var symbolStageId = dates[0].StageSymbolData.StageId;
                var symbolSeek = dates[0].StageSymbolData.Seek;
                var currentStageId = GameSystem.CurrentStageData.CurrentStage.Id;
                var currentTurn = GameSystem.CurrentStageData.CurrentStage.Seek;
                pastObj?.SetActive(symbolStageId < currentStageId || symbolSeek < currentTurn);
                nextObj?.SetActive(dates[0].SymbolType != SymbolType.None && symbolStageId == currentStageId && symbolSeek == currentTurn);
                futureObj?.SetActive(symbolStageId >= currentStageId && symbolSeek > currentTurn);
                currentObj?.SetActive(dates[0].SymbolType == SymbolType.None);
                var textId = 19631;
                if (nextObj.activeSelf)
                {
                    textId = 19632;
                } else
                if (futureObj.activeSelf)
                {
                    textId = 19633;
                } else
                if (currentObj.activeSelf)
                {
                    textId = 19634;
                }
                seekerText?.SetText(DataSystem.GetText(textId));
                if (dates[0].SymbolType == SymbolType.None)
                {
                    stageDataText?.SetText("");
                } else
                {
                    stageDataText?.SetText(dates[0].StageSymbolData.StageId.ToString() + "-" + dates[0].StageSymbolData.Seek.ToString());
                }
            }
        }

        public void SetSymbolItemCallHandler(System.Action<int> handler)
        {
            if (_isButtonInit) return;
            _isButtonInit = true;
            foreach (var symbolComponent in symbolComponents)
            {
                var button = symbolComponent.GetComponentInChildren<Button>();
                button.onClick.AddListener(() => 
                {
                    if (symbolComponent?.SymbolInfo?.SymbolType == SymbolType.None)
                    {
                        return;
                    }
                    handler(symbolComponent.Seek);
                });
            }
        }

        void Update() 
        {
            if (seekerText != null && Cursor != null)
            {
                seekerText.gameObject.SetActive(Cursor.activeSelf);
            }
        }

        void LateUpdate() 
        {
            if (seekerText != null && Cursor != null)
            {
                seekerText.gameObject.SetActive(Cursor.activeSelf);
            }
        }
        
    }
}