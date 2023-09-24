using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotInfoComponent : ListItem,IListViewItem
{
    [SerializeField] private GameObject actorRoot = null;
    [SerializeField] private GameObject actorPrefab = null;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private TextMeshProUGUI timeRecord;
    [SerializeField] private GameObject locked = null;

    [SerializeField] private GameObject noData = null;
    [SerializeField] private Button infoButton = null;
    private List<ActorInfoComponent> _actorInfoComponents = new ();
    private SlotInfo _slotInfo;

    public void SetData(SlotInfo slotInfo,int index)
    {
        _slotInfo = slotInfo;
        SetIndex(index);
        UpdateViewItem();
    }
    
    public void SetCallHandler(System.Action handler)
    {
        clickButton.onClick.AddListener(() =>
            {
                if (Disable != null && Disable.gameObject.activeSelf) return;
                handler();
            }
        );
    }
    public void SetCallInfoHandler(System.Action handler)
    {
        infoButton.onClick.AddListener(() =>
            {
                if (Disable != null && Disable.gameObject.activeSelf) return;
                handler();
            }
        );
    }

    public void UpdateViewItem()
    {
        if (_slotInfo == null) return;
        foreach (var comp in _actorInfoComponents)
        {
            comp.gameObject.SetActive(false);
        }
        for (int i = 0;i < _slotInfo.ActorInfos.Count;i++)
        {
            if (_actorInfoComponents.Count <= i)
            {
                var prefab = Instantiate(actorPrefab);
                prefab.transform.SetParent(actorRoot.transform,false);
                var comp = prefab.GetComponent<ActorInfoComponent>();
                _actorInfoComponents.Add(comp);
            }
            _actorInfoComponents[i].UpdateInfo(_slotInfo.ActorInfos[i],_slotInfo.ActorInfos);
            _actorInfoComponents[i].gameObject.SetActive(true);
        }
        score.text = _slotInfo.Score.ToString();
        timeRecord.text = _slotInfo.TimeRecord;
        locked.SetActive(_slotInfo.IsLocked);
        noData.SetActive(_slotInfo.ActorInfos.Count == 0);
    }

    private void OnDestroy() {
        _actorInfoComponents.Clear();
        _slotInfo = null;
    }
}