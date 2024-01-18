using System;

[Serializable]
public class StatusInfo
{
    public float _hp = 0;
    public int Hp => (int)Math.Floor(_hp);
    public float BaseHp => _hp;
    public float _mp = 0;
    public int Mp => (int)Math.Floor(_mp);
    public float BaseMp => _mp;
    public float _atk = 0;
    public int Atk => (int)Math.Floor(_atk);
    public float BaseAtk => _atk;
    public float _def = 0;
    public int Def => (int)Math.Floor(_def);
    public float BaseDef => _def;
    public float _spd = 0;
    public int Spd => (int)Math.Floor(_spd);
    public float BaseSpd => _spd;
    public void SetParameter(int hp,int mp,int atk,int def,int spd)
    {
        _hp = hp;
        _mp = mp;
        _atk = atk;
        _def = def;
        _spd = spd;
    }

    public int GetParameter(StatusParamType paramType)
    {
        switch (paramType)
        {
            case StatusParamType.Hp: return Hp;
            case StatusParamType.Mp: return Mp;
            case StatusParamType.Atk: return Atk;
            case StatusParamType.Def: return Def;
            case StatusParamType.Spd: return Spd;
        }
        return 0;
    }
    
    public void AddParameter(StatusParamType paramType,float param)
    {
        switch (paramType)
        {
            case StatusParamType.Hp: _hp += param; break;
            case StatusParamType.Mp: _mp += param; break;
            case StatusParamType.Atk: _atk += param; break;
            case StatusParamType.Def: _def += param; break;
            case StatusParamType.Spd: _spd += param; break;
        }
    }

    public void AddParameterAll(int param)
    {
        _hp += param;
        _mp += param;
        _atk += param;
        _def += param;
        _spd += param;
    }

    public void Clear()
    {
        SetParameter(0,0,0,0,0);
    }
}

public enum StatusParamType
{
    Hp = 0,
    Mp,
    Atk,
    Def,
    Spd,
}
