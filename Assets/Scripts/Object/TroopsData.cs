using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopsData : ScriptableObject {
    [SerializeField] public List<TroopData> _data = new List<TroopData>();
    [SerializeField] public List<GetItemData> _getItemData = new List<GetItemData>();


    [Serializable]
    public class TroopData
    {   
        public int Id;

        public int TroopId;
        public int EnemyId;
        public int Lv;
        public int Line;
        public List<GetItemData> GetItemDatas;
    }

}
