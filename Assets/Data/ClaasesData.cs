using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassesData : ScriptableObject {
    [SerializeField] public List<ClassData> _data = new List<ClassData>();


    [Serializable]
    public class ClassData
    {   
        public int Id;
        public string Name;
        public StatusInfo BaseStatus;
        public StatusInfo MaxStatus;
        public WeaponRankInfo BaseWeaponRank;
        public WeaponRankInfo MaxWeaponRank;
        

        public string GetName()
        {
            string name = Name;
            return name;
        }

    }


}