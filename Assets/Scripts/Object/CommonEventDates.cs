using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class CommonEventDates : ScriptableObject
    {
        [SerializeField] public CommonEventDate[] data;
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
    public class EventCommandDate
    {
        public string rgss3_klass;
        public int indent;
        public int code;
        public string[] parameters;
    }
    
    [Serializable]
    public class CommonEventMasterDates
    {
        [SerializeField] public CommonEventDate[] data;
    }
}
