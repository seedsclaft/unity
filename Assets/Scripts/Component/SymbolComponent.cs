using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SymbolComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI commandTitle;
        [SerializeField] private Image symbolImage;
        [SerializeField] private Image enemyImage;
        [SerializeField] private List<Sprite> symbolSprites;
        [SerializeField] private GameObject evaluateRoot;
        [SerializeField] private TextMeshProUGUI evaluate;
        [SerializeField] private GameObject selected;
        [SerializeField] private GameObject lastSelected;
        [SerializeField] private BaseList getItemList = null;
        public BaseList GetItemList => getItemList;
        private SymbolInfo _symbolInfo = null;
        public SymbolInfo SymbolInfo => _symbolInfo;
        public int Seek => _symbolInfo != null ? _symbolInfo.Master.Seek : -1;

        private bool _animationInit = false;

        public void Initialize()
        {
            getItemList?.Initialize();
        }

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
            selected?.SetActive(symbolInfo.Selected);
            if (lastSelected != null)
            {
                //var lastSelect = _symbolInfo.Selected;
                lastSelected.SetActive(symbolInfo.LastSelected);
                //symbolImage.gameObject.SetActive(!_symbolInfo.Past && !lastSelect);
                /*
                if (_animationInit == false && lastSelect)
                {
                    var uiView = lastSelected.GetComponent<RectTransform>();
                    AnimationUtility.LocalMoveToLoopTransform(uiView.gameObject,
                        new Vector3(uiView.localPosition.x,uiView.localPosition.y + 4,0),
                        new Vector3(uiView.localPosition.x,uiView.localPosition.y,0),
                        1.2f);
                    _animationInit = true;
                }
                */
            }
            if (getItemList != null)
            {
                getItemList.SetData(MakeGetItemListData(),false);
            }
        }

        private void UpdateCommandTitle()
        {
            if (commandTitle != null)
            {
                //if (_symbolInfo.SymbolType > SymbolType.None && !_symbolInfo.StageSymbolData.IsRandomSymbol())
                if (_symbolInfo.SymbolType > SymbolType.None && _symbolInfo.SymbolType < SymbolType.Group)
                {
                    commandTitle.transform.parent.gameObject.SetActive(true);
                    var textId = 19610 + (int)_symbolInfo.SymbolType;
                    commandTitle.text = DataSystem.GetTextData(textId).Text;
                    //commandTitle.transform.parent.gameObject.SetActive(!_symbolInfo.Past);
                } else
                {
                    commandTitle.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateSymbolImage()
        {
            if (_symbolInfo.SymbolType == SymbolType.None) 
            {
                symbolImage.gameObject.SetActive(false);
                enemyImage.gameObject.SetActive(false);
                return;
            }
            if (_symbolInfo.SymbolType == SymbolType.Group) return;
            if (_symbolInfo.SymbolType == SymbolType.Random) return;
            if (_symbolInfo.SymbolType == SymbolType.Battle || _symbolInfo.SymbolType == SymbolType.Boss)
            {
                symbolImage.gameObject.SetActive(false);
                enemyImage.gameObject.SetActive(true);
                enemyImage.sprite = ResourceSystem.LoadEnemySprite(_symbolInfo.TroopInfo.BossEnemy.EnemyData.ImagePath);
            } else
            {
                if (symbolImage != null && symbolSprites != null)
                {
                    symbolImage.gameObject.SetActive(true);
                    enemyImage.gameObject.SetActive(false);
                    if (_symbolInfo.SymbolType == SymbolType.SelectActor)
                    {
                        symbolImage.sprite = symbolSprites[(int)SymbolType.Actor];
                    } else
                    {
                        symbolImage.sprite = symbolSprites[(int)_symbolInfo.SymbolType];
                    }
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

        private List<ListData> MakeGetItemListData()
        {
            var list = new List<ListData>();
            foreach (var getItemInfo in _symbolInfo.GetItemInfos)
            {
                if (getItemInfo.GetItemType == GetItemType.None)
                {
                    continue;
                }
                var data = new ListData(getItemInfo);
                //data.SetEnable(symbolInfo.Cleared != true || getItemInfo.GetItemType != GetItemType.Numinous);
                if (getItemInfo.GetItemType == GetItemType.Skill)
                {
                    /*
                    // 入手済みなら
                    if (partyInfo.CurrentAlchemyIdList(currentStageInfo.Id,currentStageInfo.Seek,currentStageInfo.WorldType).Contains(getItemInfo.Param1))
                    {
                        data.SetEnable(false);
                    }
                    if (partyInfo.CurrentAlcanaIdList(currentStageInfo.Id,currentStageInfo.Seek,currentStageInfo.WorldType).Contains(getItemInfo.Param1))
                    {
                        data.SetEnable(false);
                    }
                    */
                    
                } else
                if (getItemInfo.GetItemType == GetItemType.AddActor)
                {
                    /*
                    // 入手済みなら
                    if (partyInfo.CurrentActorIdList(currentStageInfo.Id,currentStageInfo.Seek,currentStageInfo.WorldType).Contains(getItemInfo.ResultParam))
                    {
                        data.SetEnable(false);
                    }
                    */
                }
                list.Add(data);
            }
            return list;
        }
    }
}