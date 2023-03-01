using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class StageInfo
{
    private int _id;
    public int Id {get {return _id;}}
    private int _turns;
    public int Turns {get {return _turns;}}
    private int _currentTurn;
    public int CurrentTurn {get {return _currentTurn;}}
    private List<StagesData.StageEventData> _stageEvents;
    public List<StagesData.StageEventData> StageEvents {get {return _stageEvents;}}
    private int _clearCount;
    public int ClearCount {get {return _clearCount;}}


    public StageInfo(StagesData.StageData stageInfo)
    {
        _id = stageInfo.Id;
        _turns = stageInfo.Turns;
        _stageEvents = stageInfo.StageEvents;
        _currentTurn = 1;
    }

    public void SeekStage()
    {
        _currentTurn++;
    }
};