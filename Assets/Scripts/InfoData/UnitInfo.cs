using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInfo 
{
    private List<BattlerInfo> _battlerInfos;
    public List<BattlerInfo> BattlerInfos => _battlerInfos;
    public List<BattlerInfo> AliveBattlerInfos => _battlerInfos.FindAll(a => a.IsAlive());
    public List<BattlerInfo> FrontBattlers()
    {
        // 最前列は
        bool IsFrontAlive = BattlerInfos.Find(a => a.LineIndex == LineType.Front) != null;
        if (IsFrontAlive)
        {
            return BattlerInfos.FindAll(a => a.LineIndex == LineType.Front);
        }
        return BattlerInfos;
    }
    
    public void SetBattlers(List<BattlerInfo> battlerInfos)
    {
        _battlerInfos = battlerInfos;
    }

}
