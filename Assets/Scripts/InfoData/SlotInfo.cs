using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class SlotInfo
{
    public SlotInfo(List<ActorInfo> actorInfos,int score)
    {
        _actorInfos = actorInfos;
        _score = score;
    }
    private List<ActorInfo> _actorInfos = new ();
    public List<ActorInfo> ActorInfos => _actorInfos; 
    private int _score = 0;
    public int Score => _score; 
    private string _timeRecord;
    public string TimeRecord => _timeRecord; 

    public bool _isLocked = false;
    public bool IsLocked => _isLocked; 
    public void ChangeLock()
    {
        if (_score == 0) return;
        _isLocked = !_isLocked;
    }

    public void SetTimeRecord()
    {
        DateTime dt1 = DateTime.Now;
        _timeRecord = dt1.ToString("yyyy/MM/dd");
    }
}
