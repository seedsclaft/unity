using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotInfoComponent : MonoBehaviour
{
    [SerializeField] private GameObject actorRoot = null;
    [SerializeField] private GameObject actorPrefab = null;
    [SerializeField] private TextMeshProUGUI timeRecord;

    [SerializeField] private GameObject noData = null;
    [SerializeField] private Button infoButton = null;
    private List<ActorInfoComponent> _actorInfoComponents = new ();
    private SlotInfo _slotInfo;

    public void UpdateInfo(SlotInfo slotInfo)
    {
        _slotInfo = slotInfo;
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
        timeRecord.text = _slotInfo.TimeRecord;
        noData.SetActive(_slotInfo.ActorInfos.Count == 0);
    }

    private void OnDestroy() {
        _actorInfoComponents.Clear();
        _slotInfo = null;
    }
}
