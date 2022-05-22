using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesData : ScriptableObject {
    [SerializeField] public List<EnemyData> _data = new List<EnemyData>();
    [SerializeField] public List<TextData> _textdata = new List<TextData>();


    [Serializable]
    public class EnemyData
    {   
        public int Id;
        public int NameId;
        public string ImageName;
        public StatusInfo BaseStatus;
        
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

}
