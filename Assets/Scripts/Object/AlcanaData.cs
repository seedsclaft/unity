using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlcanaData : ScriptableObject {
    [SerializeField] public List<Alcana> _data = new List<Alcana>();

    [Serializable]
    public class Alcana
    {   
        public int Id;
        public string Name;
        public string Help;
        public string FilePath;
        public int SkillId;
    }
}

public enum AlcanaType
{
    Fool = 0,
    Magician,
    High_Priestess,
    Empress,
    Emperor,
    Hierophant,
    Lovers,
    Chariot,
    Strength,
    Hermit,
    Fortune,
    Justice,
    Hangedman,
    Death,
    Temperance,
    Devil,
    Tower,
    Star,
    Moon,
    Sun,
    Judgement,
    World
}