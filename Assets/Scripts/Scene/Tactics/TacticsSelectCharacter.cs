using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TacticsSelectCharacter : MonoBehaviour
{
    [SerializeField] private BaseList characterList;
    public BaseList CharacterList => characterList;
    [SerializeField] private BaseList commandList;
    public BaseList CommandList => commandList;
    [SerializeField] private ActorInfoComponent displaySelectCharacter;
    [SerializeField] private TextMeshProUGUI commandTitle;
    [SerializeField] private TextMeshProUGUI commandLv;
    [SerializeField] private TextMeshProUGUI commandDescription;
    [SerializeField] private List<GameObject> infoObj;
    
    public ListData CommandData {
        get {
            if (commandList.Index > -1)
            {
                return commandList.ListData;
            }
            return null;
        }
    }    
    
    public ListData CharacterData {
        get {
            if (characterList.Index > -1)
            {
                return characterList.ListData;
            }
            return null;
        }
    }    

    public void Initialize(int createCount) {
        characterList.Initialize(createCount);
        characterList.Activate();
        commandList.Initialize(0);
        commandList.Activate();
        characterList.SetInputCallHandler((a) => CallCharacterInputHandler(a));
        commandList.SetInputCallHandler((a) => CallCommandInputHandler(a));
        infoObj.ForEach(a => a.SetActive(false));
    }    
    
    public void SetTacticsCommand(List<ListData> commandData)
    {
        commandList.SetData(commandData);
    }

    public void SetTacticsCommandData(TacticsCommandData tacticsCommandData)
    {
        commandTitle.text = tacticsCommandData.Title;
        commandLv.text = tacticsCommandData.Rank.ToString();
        commandDescription.text = tacticsCommandData.Description;
    }

    public void SetTacticsCharacter(List<ListData> characterData)
    {
        characterList.SetData(characterData);
        characterList.SetSelectedHandler(() => DisplaySelectCharacter());
        if (displaySelectCharacter == null)
        {
            displaySelectCharacter.gameObject.SetActive(false);
        }
        DisplaySelectCharacter();
    }

    public void UpdateSmoothSelect()
    {
        characterList.UpdateSelectIndex(0);
        commandList.UpdateSelectIndex(-1);
    }

    private void CallCharacterInputHandler(InputKeyType keyType)
    {
        if (!characterList.Active)
        {
            return;
        }
        if (keyType == InputKeyType.Down)
        {
            var characterListIndex = characterList.Index;
            if (characterListIndex == 0)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
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
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            commandList.Deactivate();
            commandList.UpdateSelectIndex(-1);
            characterList.UpdateSelectIndex(characterList.ObjectList.Count-1);
            characterList.Activate();
            characterList.ResetInputFrame(36);
        }
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
                infoObj.ForEach(a => a.gameObject.SetActive(false));
                infoObj[(int)tacticsActorInfo.TacticsCommandType-1].SetActive(true);
                infoObj[(int)tacticsActorInfo.TacticsCommandType-1].GetComponent<ActorInfoComponent>().UpdateInfo(tacticsActorInfo.ActorInfo,tacticsActorInfo.ActorInfos);
            }
        }
    }

    public void SetInputHandlerCharacter(InputKeyType keyType,System.Action callEvent)
    {
        characterList.SetInputHandler(keyType,callEvent);
        if (keyType == InputKeyType.Right)
        {
            for (int i = 0; i < characterList.ObjectList.Count;i++)
            {
                var tacticsTrain = characterList.ObjectList[i].GetComponent<TacticsTrain>();
                tacticsTrain.SetPlusHandler(() => callEvent());
            }
        }
        if (keyType == InputKeyType.Left)
        {
            for (int i = 0; i < characterList.ObjectList.Count;i++)
            {
                var tacticsTrain = characterList.ObjectList[i].GetComponent<TacticsTrain>();
                tacticsTrain.SetMinusHandler(() => callEvent());
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

    public void HideCommandList()
    {
        commandList.gameObject.SetActive(false);
    }
}
