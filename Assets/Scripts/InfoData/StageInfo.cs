using System;

[Serializable]
public class StageInfo
{
    private int _id;
    public int Id {get {return _id;}}
    private int _clearCount;
    public int ClearCount {get {return _clearCount;}}


    public StageInfo(StagesData.StageData stageInfo)
    {
        _id = stageInfo.Id;
    }
};