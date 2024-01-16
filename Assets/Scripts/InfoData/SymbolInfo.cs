using System;
using System.Collections.Generic;

[Serializable]
public class SymbolInfo
{
    private SymbolType _symbolType;
    public SymbolType SymbolType => _symbolType;
    private TroopInfo _troopInfo;
    public TroopInfo TroopInfo => _troopInfo;
    private List<GetItemInfo> _getItemInfos = new ();
    public List<GetItemInfo> GetItemInfos => _getItemInfos;
    public SymbolInfo()
    {
    }

    public void SetSymbolType(SymbolType symbolType)
    {
        _symbolType = symbolType;
    }

    public void SetTroopInfo(TroopInfo troopInfo)
    {
        _troopInfo = troopInfo;
    }

    public List<BattlerInfo> BattlerInfos()
    {
        return _troopInfo.BattlerInfos;
    }

    public void MakeGetItemInfos(List<GetItemInfo> getItemInfos)
    {
        _getItemInfos = getItemInfos;
    }

    public int BattleEvaluate()
    {
        if (_troopInfo != null)
        {
            return 1;
        }
        return 0;
    }
}
