using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class CommonEventDates : ScriptableObject
    {
        [SerializeField] public List<CommonEventDate> data;
    }

    [Serializable]
    public class CommonEventDate
    {
        public string rgss3_klass;
        public int trigger;
        public string name;
        public int switch_id;
        public EventCommandDate[] list;
        public int id;
    }

    [Serializable]
    public class CommonEventSoundDate
    {
        public string rgss3_klass;
        public int trigger;
        public string name;
        public int switch_id;
        
        public EventCommandDate[] list;
        public int id;
    }

    [Serializable]
    public class EventCommandDate
    {
        public string rgss3_klass;
        public int indent;
        public int code;
        public string[] parameters;
        public EventCommandSound soundDate;
    }

    [Serializable]
    public class CommonEventMasterDates
    {
        [SerializeField] public CommonEventDate[] data;
    }

    // 補助データ
    [Serializable]
    public class CommonEventMasterSoundDates
    {
        [SerializeField] public CommonEventSoundListDate[] data;
    }

    [Serializable]
    public class CommonEventSoundListDate
    {
        public string rgss3_klass;
        public int trigger;
        public string name;
        public int switch_id;
        
        public EventCommandSoundDate[] list;
        public int id;
    }
    
    [Serializable]
    public class EventCommandSoundDate
    {
        public string rgss3_klass;
        public int indent;
        public int code;
        public EventCommandSound[] parameters;
    }

    [Serializable]
    public class EventCommandSound
    {
        public string name;
        public int volume;
        public int pitch;
    }
}
