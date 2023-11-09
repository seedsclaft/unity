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
    }    
    
    public void SetTacticsCommand(List<ListData> commandData)
    {
        if (commandList.IsInit == false)
        {
            commandList.Initialize(commandData.Count);
        }
        commandList.SetData(commandData);
    }

    public void SetTacticsCommandData(TacticsCommandData tacticsCommandData)
    {
        commandTitle.text = tacticsCommandData.Title;
        commandLv.text = tacticsCommandData.Rank.ToString();
        commandDescription.text = tacticsCommandData.Description;
    }

    public void SetTacticsChracter(List<ListData> characterData)
    {
        if (characterList.IsInit == false)
        {
            characterList.Initialize(characterData.Count);
            characterList.SetSelectedHandler(() => DisplaySelectCharacter());
        }
        characterList.SetData(characterData);
        if (displaySelectCharacter == null)
        {
            displaySelectCharacter.gameObject.SetActive(false);
        }
        DisplaySelectCharacter();
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
                displaySelectCharacter.UpdateInfo(tacticsActorInfo.ActorInfo,null);
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
