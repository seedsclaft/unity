using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

namespace Ryneus
{
    public class TacticsSelectCharacter : MonoBehaviour
    {
        [SerializeField] private BaseList characterList;
        public BaseList CharacterList => characterList;
        [SerializeField] private BaseList commandList;
        public BaseList CommandList => commandList;
        [SerializeField] private ActorInfoComponent displaySelectCharacter;
        [SerializeField] private TextMeshProUGUI commandTitle;
        [SerializeField] private TextMeshProUGUI partyEvaluate;
        [SerializeField] private TextMeshProUGUI troopEvaluate;
        [SerializeField] private OnOffButton replayButton;
        
        public ListData CommandData 
        {
            get 
            {
                if (commandList.Index > -1)
                {
                    return commandList.ListData;
                }
                return null;
            }
        }    
        
        public ListData CharacterData 
        {
            get 
            {
                if (characterList.Index > -1)
                {
                    return characterList.ListData;
                }
                return null;
            }
        }    

        public void Initialize(System.Action replayEvent) 
        {
            characterList.Initialize();
            characterList.Activate();
            commandList.Initialize();
            commandList.Activate();
            characterList.SetInputCallHandler((a) => CallCharacterInputHandler(a));
            commandList.SetInputCallHandler((a) => CallCommandInputHandler(a));
            replayButton?.SetText("クリア編成");
            replayButton?.SetCallHandler(() => 
                replayEvent?.Invoke()
            );
            //infoObj.ForEach(a => a.SetActive(false));
        }
        
        public void SetTacticsCommand(List<ListData> commandData)
        {
            commandList.SetData(commandData);
        }

        public void SetTacticsCommandData(TacticsCommandData tacticsCommandData)
        {
            commandTitle.text = tacticsCommandData.Title;
        }

        public void SetTacticsCharacter(List<ListData> characterData)
        {
            characterList.SetData(characterData);
            if (displaySelectCharacter == null)
            {
                displaySelectCharacter.gameObject.SetActive(false);
            }
            Refresh();
        }

        public void UpdateSmoothSelect()
        {
            characterList.Refresh(0);
    #if UNITY_ANDROID
            commandList.Refresh(1);
    #else
            commandList.Refresh(-1);
    #endif
        }

        private void CallCharacterInputHandler(InputKeyType keyType)
        {
            return;
            if (!characterList.Active)
            {
                return;
            }
            if (keyType == InputKeyType.Down)
            {
                var characterListIndex = characterList.Index;
                if (characterListIndex == 0)
                {
                    //Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                    characterList.Deactivate();
                    characterList.UpdateSelectIndex(-1);
                    commandList.UpdateSelectIndex(1);
                    commandList.Activate();
                    commandList.ResetInputFrame(36);
                }
            }
        }

        private void CallCommandInputHandler(InputKeyType keyType)
        {
            if (!commandList.Active)
            {
                return;
            }
            if (keyType == InputKeyType.Up)
            {
                //Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                commandList.Deactivate();
                commandList.UpdateSelectIndex(-1);
                characterList.UpdateSelectIndex(characterList.ObjectList.Count-1);
                characterList.Activate();
                characterList.ResetInputFrame(36);
            }
        }

        private void CallReplay()
        {

        }

        private void DisplaySelectCharacter()
        {
            if (displaySelectCharacter == null)
            {
                return;
            }
            var listData = characterList.ListData;
            if (listData != null)
            {
                var tacticsActorInfo = (TacticsActorInfo)listData.Data;
                if (tacticsActorInfo != null)
                {
                    displaySelectCharacter.gameObject.SetActive(true);
                    displaySelectCharacter.UpdateInfo(tacticsActorInfo.ActorInfo,tacticsActorInfo.ActorInfos);
                    
                    //infoObj.ForEach(a => a.gameObject.SetActive(false));
                    //infoObj[(int)tacticsActorInfo.TacticsCommandType-1].SetActive(true);
                    //infoObj[(int)tacticsActorInfo.TacticsCommandType-1].GetComponent<ActorInfoComponent>().UpdateInfo(tacticsActorInfo.ActorInfo,tacticsActorInfo.ActorInfos);
                }
            }
        }

        public void SetInputHandlerCharacter(InputKeyType keyType,System.Action callEvent)
        {
            characterList.SetInputHandler(keyType,callEvent);
            if (keyType == InputKeyType.Decide)
            {
                for (int i = 0; i < characterList.ItemPrefabList.Count;i++)
                {
                    var tacticsTrain = characterList.ItemPrefabList[i].GetComponent<TacticsTrain>();
                    tacticsTrain.SetToggleHandler(() => callEvent());
                }
            }
    #if UNITY_ANDROID
            // Androidはトグルで決定、リストで選択にする
            if (keyType == InputKeyType.Decide)
            {
                for (int i = 0; i < characterList.ItemPrefabList.Count;i++)
                {
                    var tacticsTrain = characterList.ItemPrefabList[i].GetComponent<TacticsTrain>();
                    tacticsTrain.SetToggleHandler(() => callEvent());
                }
            }
            characterList.SetInputHandler(keyType,() => DisplaySelectCharacter());
    #endif
            if (keyType == InputKeyType.Right)
            {
                for (int i = 0; i < characterList.ItemPrefabList.Count;i++)
                {
                    var tacticsTrain = characterList.ItemPrefabList[i].GetComponent<TacticsTrain>();
                    //tacticsTrain.SetPlusHandler(() => callEvent());
                    tacticsTrain.SetBattleFrontToggleHandler(() => callEvent());
                }
            }
            if (keyType == InputKeyType.Left)
            {
                for (int i = 0; i < characterList.ItemPrefabList.Count;i++)
                {
                    var tacticsTrain = characterList.ItemPrefabList[i].GetComponent<TacticsTrain>();
                    //tacticsTrain.SetMinusHandler(() => callEvent());
                    tacticsTrain.SetBattleBackToggleHandler(() => callEvent());
                }
            }
            if (keyType == InputKeyType.Option1)
            {
                for (int i = 0; i < characterList.ItemPrefabList.Count;i++)
                {
                    var tacticsTrain = characterList.ItemPrefabList[i].GetComponent<TacticsTrain>();
                    //tacticsTrain.SetMinusHandler(() => callEvent());
                    tacticsTrain.SetSkillTriggerHandler(() => callEvent());
                }
            }
        }

        public void SetInputHandlerCommand(InputKeyType keyType,System.Action callEvent)
        {
            commandList.SetInputHandler(keyType,callEvent);
        }

        public void Refresh()
        {
            characterList.UpdateAllItems();
            DisplaySelectCharacter();
        }

        public void SetEvaluate(int value,int value2)
        {
            partyEvaluate?.SetText(DataSystem.GetReplaceDecimalText(value).ToString());
            troopEvaluate?.SetText(DataSystem.GetReplaceDecimalText(value2).ToString());
        }

        public void ShowCharacterList()
        {
            characterList.gameObject.SetActive(true);
        }

        public void HideCharacterList()
        {
            characterList.gameObject.SetActive(false);
        }

        public void ShowCommandList()
        {
            commandList.gameObject.SetActive(true);
        }

        public void ShowBattleReplay(bool isActive)
        {
            replayButton?.gameObject.SetActive(isActive);
        }

        public void HideCommandList()
        {
            commandList.gameObject.SetActive(false);
        }
    }
}