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
    private bool _selected;
    public bool Selected => _selected;
    public SymbolInfo(StageSymbolData symbol = null)
    {
        if (symbol == null)
        {
            return;
        }
        if (symbol.BattleSymbol == 1){
            SetSymbolType(SymbolType.Battle);
        } else
        if (symbol.BossSymbol == 1){
            SetSymbolType(SymbolType.Boss);
        } else
        if (symbol.RecoverSymbol == 1){
            SetSymbolType(SymbolType.Recover);
        } else
        if (symbol.AlcanaSymbol == 1){
            SetSymbolType(SymbolType.Alcana);
        } else
        if (symbol.ActorSymbol == 1){
            SetSymbolType(SymbolType.Actor);
        } else
        if (symbol.ResourceSymbol == 1){
            SetSymbolType(SymbolType.Resource);
        } else
        if (symbol.RebirthSymbol == 1){
            SetSymbolType(SymbolType.Rebirth);
        }
    }

    private void SetSymbolType(SymbolType symbolType)
    {
        _symbolType = symbolType;
    }

    public void SetTroopInfo(TroopInfo troopInfo)
    {
        _troopInfo = troopInfo;
    }

    public void SetSelected(bool selected)
    {
        _selected = selected;
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
            var evaluate = 0;
            foreach (var battlerInfo in _troopInfo.BattlerInfos)
            {
                evaluate += battlerInfo.Evaluate();
            }
            return evaluate;
        }
        return 0;
    }
}
