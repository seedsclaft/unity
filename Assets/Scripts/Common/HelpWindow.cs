using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

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
        if (_lastKey == key) return;
        if (DataSystem.System.InputDataList == null) return;
        List<SystemData.InputData> inputInfos = DataSystem.System.InputDataList.FindAll(a => a.Key == key);
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
            //infoComp.gameObject.SetActive(true);
        }
        _lastKey = key;
        inputCanvasGroup.alpha = 0;
        await UniTask.WaitUntil( () => _inputPrefabs.Count >=  inputInfos.Count );
        foreach(var prefab in _inputPrefabs){
            //prefab.SetActive(false);
        }
        for (int i = 0;i < inputInfos.Count;i++)
        {
            if (_inputPrefabs.Count >= i)
            {            
                _inputPrefabs[i].SetActive(true);
            }
        }
        inputCanvasGroup.alpha = 1;
    }
}
