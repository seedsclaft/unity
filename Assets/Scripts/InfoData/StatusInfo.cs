using System;


[Serializable]
public class StatusInfo
{
    public int _hp = 0;
    public int _mp = 0;
    public int _atk = 0;
    public int _def = 0;
    public int _spd = 0;
    public int Hp {get { return _hp;}}
    public int Mp {get { return _mp;}}
    public int Atk {get { return _atk;}}
    public int Def {get { return _def;}}
    public int Spd {get { return _spd;}}
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
            case StatusParamType.Hp: return _hp;
            case StatusParamType.Mp: return _mp;
            case StatusParamType.Atk: return _atk;
            case StatusParamType.Def: return _def;
            case StatusParamType.Spd: return _spd;
        }
        return 0;
    }
    
    public void AddParameter(StatusParamType paramType,int param)
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
