using System;

[Serializable]
public class FeatureInfo 
{
    public FeatureType _feature = 0;
    public int _target = 0;
    public int _value = 0;

    public FeatureInfo(int feature,int target,int value)
    {
        _feature = (FeatureType)feature;
        _target = target;
        _value = value;
    }
}


public enum FeatureType
{
    None = 0,
    HpDamage = 1,
    HpHeal = 2,
    HpDrain = 3,
    AddState = 21,
    RemoveState = 22,
    PlusSkill = 101,
}