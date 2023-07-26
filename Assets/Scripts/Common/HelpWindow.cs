using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class HelpWindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI helpText;
    [SerializeField] private GameObject inputPrefab;
    [SerializeField] private GameObject inputRoot;
    [SerializeField] private CanvasGroup inputCanvasGroup;

    private List<GameObject> _inputPrefabs = new ();

    private string _lastKey = "";

    public void SetHelpText(string text){
        helpText.text = text;
    }

    public async void SetInputInfo(string key)
    {
        if (GameSystem.ConfigData._inputType == false) return;
        if (_lastKey == key) return;
        if (DataSystem.System.InputDataList == null) return;
        List<SystemData.InputData> inputInfos;
        if (InputSystem.IsGamePad)
        {
            inputInfos = DataSystem.System.InputDataList.FindAll(a => a.Key == key && a.KeyId != 0 &&a.KeyId != 2);
        } else
        {
            inputInfos = DataSystem.System.InputDataList.FindAll(a => a.Key == key);
        }
        foreach(var prefab in _inputPrefabs){
            prefab.SetActive(false);
        }
        for (int i = 0;i < inputInfos.Count;i++)
        {
            if (_inputPrefabs.Count <= i)
            {
                var prefab = Instantiate(inputPrefab);
                prefab.transform.SetParent(inputRoot.transform,false);
                _inputPrefabs.Add(prefab);
            }
            var infoComp = _inputPrefabs[i].GetComponent<InputInfoComponent>();
            infoComp.SetData(inputInfos[i]);
            infoComp.gameObject.SetActive(true);
        }
        inputCanvasGroup.alpha = 0;
        await UniTask.Yield();
        _lastKey = key;
        foreach(var prefab in _inputPrefabs){
            if (prefab != null)
            {
                prefab.SetActive(false);
            }
        }
        for (int i = 0;i < inputInfos.Count;i++)
        {
            if (_inputPrefabs.Count >= i && _inputPrefabs[i] != null)
            {            
                _inputPrefabs[i].SetActive(true);
            }
        }
        
        Sequence main = DOTween.Sequence()
            .Append(inputCanvasGroup.DOFade(1f,0.4f));
        //inputCanvasGroup.alpha = 1;
    }
}
