using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDates : ScriptableObject
{
    [SerializeField] public List<EnemyData> Data = new();
}

[Serializable]
public class EnemyData
{   
    public int Id;
    public string Name;
    public string ImagePath;
    public StatusInfo BaseStatus;
    public List<KindType> Kinds;
    public List<LearningData> LearningSkills = new();
    
    public int CurrentParam(StatusParamType growType,int level)
    {
        return 0;
    }

    public StatusInfo LevelUpStatus(int level)
    {
        StatusInfo upStatus = new StatusInfo();
        foreach (StatusParamType growType in Enum.GetValues(typeof(StatusParamType)))
        {
            int currentParam = CurrentParam(growType,level);
            int nextParam = CurrentParam(growType,level + 1);
            if (currentParam < nextParam){
                int upParam = nextParam - currentParam;
                upStatus.AddParameter(growType,upParam);
            }
        }
        return upStatus;
    }
}


public enum KindType
{
    None = 0,
    Undead = 1
}

public enum LineType
{
    Front = 0,
    Back = 1
}