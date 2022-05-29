using System;

[Serializable]
public class TempInfo
{
    public object ParamData;
    public TempInfo(object paramData)
    {
        ParamData = paramData;
    }
};