using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassesData : ScriptableObject {
    [SerializeField] public List<ClassData> _data = new();


    [Serializable]
    public class ClassData
    {   
        public int Id;
        public string Name;
        

        public string GetName()
        {
            string name = Name;
            return name;
        }

    }


}