using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Ryneus
{
    public class DebugConsole : MonoBehaviour
    {
        [SerializeField] private TMP_InputField consoleInputField = null;
        [SerializeField] private GameSystem gameSystem = null;
        void Start()
        {
    #if UNITY_EDITOR
            consoleInputField.onEndEdit.AddListener((a) => CallConsoleCommand(a));
            gameObject.SetActive(true);
    #else
            gameObject.SetActive(false);
    #endif
        }

        void CallConsoleCommand(string inputText)
        {
            if (consoleInputField.text.Contains("TS"))
            {
                var replace = consoleInputField.text.Replace("TS","");
                var command = replace.Split(",");
                if (command.Length != 2) return;
                GameSystem.CurrentStageData.MakeStageData(int.Parse( replace ));
            }
            if (consoleInputField.text.Contains("TT"))
            {
                var replace = consoleInputField.text.Replace("TT","");
                GameSystem.CurrentStageData.CurrentStage.SetCurrentTurn(int.Parse( replace ));
            }
            if (consoleInputField.text.Contains("DEBUG"))
            {
                GameSystem.CurrentStageData.Party.ChangeCurrency(10000);
                for (int i = 1; i <= 11;i++)
                {
                    var stageSymbol = new StageSymbolData
                    {
                        StageId = 1,
                        Seek = 0,
                        SeekIndex = 0
                    };
                    stageSymbol.SymbolType = SymbolType.Actor;
                    stageSymbol.Param1 = i;
                    var symbolInfo = new SymbolInfo(stageSymbol.SymbolType);
                    var getItemData = new GetItemData();
                    getItemData.Type = GetItemType.AddActor;
                    getItemData.Param1 = i;
                    symbolInfo.SetGetItemInfos(new List<GetItemInfo>(){new GetItemInfo(getItemData)});
                    var record = new SymbolResultInfo(symbolInfo,stageSymbol);
                    
                    record.SetSelected(true);
                    GameSystem.CurrentStageData.Party.SetSymbolResultInfo(record,false);
                    //GameSystem.CurrentStageData.AddTestActor(DataSystem.FindActor(i),0);
                }
                foreach (var skill in DataSystem.Skills)
                {
                    if (skill.Value.Rank > RankType.PassiveRank2 || skill.Value.Rank == 0)
                    {
                        continue;
                    }
                    var stageSymbol = new StageSymbolData
                    {
                        StageId = 1,
                        Seek = 0,
                        SeekIndex = 0
                    };
                    var symbolInfo = new SymbolInfo(stageSymbol.SymbolType);
                    var getItemData = new GetItemData();
                    getItemData.Type = GetItemType.Skill;
                    getItemData.Param1 = skill.Value.Id;
                    symbolInfo.SetGetItemInfos(new List<GetItemInfo>(){new GetItemInfo(getItemData)});
                    var record = new SymbolResultInfo(symbolInfo,stageSymbol);
                    
                    record.SetSelected(true);
                    GameSystem.CurrentStageData.Party.SetSymbolResultInfo(record,false);
                }
            }
            if (consoleInputField.text == "R")
            {
                SceneManager.LoadScene(0);
            }
            if (consoleInputField.text == "S0")
            {
                SaveSystem.SaveStageInfo(GameSystem.CurrentStageData,0);
            }
            if (consoleInputField.text == "S1")
            {
                SaveSystem.SaveStageInfo(GameSystem.CurrentStageData,1);
            }
            if (consoleInputField.text == "S2")
            {
                SaveSystem.SaveStageInfo(GameSystem.CurrentStageData,2);
            }
            if (consoleInputField.text == "S3")
            {
                SaveSystem.SaveStageInfo(GameSystem.CurrentStageData,3);
            }
            if (consoleInputField.text == "S4")
            {
                SaveSystem.SaveStageInfo(GameSystem.CurrentStageData,4);
            }
            if (consoleInputField.text == "L1")
            {
                SaveSystem.LoadStageInfo(1);
                var sceneInfo = new SceneInfo(){ToScene = Scene.Tactics};
                gameSystem.CommandSceneChange(sceneInfo);
            }
            if (consoleInputField.text == "L2")
            {
                SaveSystem.LoadStageInfo(2);
                var sceneInfo = new SceneInfo(){ToScene = Scene.Tactics};
                gameSystem.CommandSceneChange(sceneInfo);
            }
            if (consoleInputField.text == "L3")
            {
                SaveSystem.LoadStageInfo(3);
                var sceneInfo = new SceneInfo(){ToScene = Scene.Tactics};
                gameSystem.CommandSceneChange(sceneInfo);
            }
            if (consoleInputField.text == "L4")
            {
                SaveSystem.LoadStageInfo(4);
                var sceneInfo = new SceneInfo(){ToScene = Scene.Tactics};
                gameSystem.CommandSceneChange(sceneInfo);
            }
        }
    }
}