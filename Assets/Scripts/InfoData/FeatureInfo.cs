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
    GainHp = 1,
}