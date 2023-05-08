using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class UnitInfo 
{
    private int _deathMemberCount = 0;
    private float _minHpRate = 0f;
    public UnitInfo(int deathMemberCount,float minHpRate)
    {
        _deathMemberCount = deathMemberCount;
        _minHpRate = minHpRate;
    }

    public int DeathMemberCount()
    {
        return _deathMemberCount;
    }

    public bool FindHpRateUnder(float rate)
    {
        return _minHpRate <= rate;
    }
}
