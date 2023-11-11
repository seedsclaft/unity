using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle;
using Effekseer;

public class FastBattleView : BaseView
{
    [SerializeField] private BattleActorList battleActorList = null;
    [SerializeField] private BattleEnemyLayer battleEnemyLayer = null;
    [SerializeField] private BattleGridLayer battleGridLayer = null;

    private new System.Action<BattleViewEvent> _commandData = null;

    [SerializeField] private GameObject animRoot = null;
    [SerializeField] private GameObject animPrefab = null;

    private BattleStartAnim _battleStartAnim = null;

    private bool _battleBusy = false;
    public void SetBattleBusy(bool isBusy)
    {
        _battleBusy = isBusy;
    }
    private bool _animationBusy = false;

    private Dictionary<int,BattlerInfoComponent> _battlerComps = new Dictionary<int, BattlerInfoComponent>();
    private int _animationEndTiming = 0;
    public void SetAnimationEndTiming(int value)
    {
        _animationEndTiming = value;
    }

    public override void Initialize() 
    {
        base.Initialize();

        new FastBattlePresenter(this);
    }

    public void CreateObject(int battleActorsCount)
    {
        battleActorList.Initialize(battleActorsCount,a => CallActorList(a));
        battleActorList.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
        SetInputHandler(battleActorList.GetComponent<IInputHandlerEvent>());
        battleActorList.Deactivate();
        
        GameObject prefab = Instantiate(animPrefab);
        prefab.transform.SetParent(animRoot.transform, false);
        _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
    }

    public void StartBattleStartAnim(string text)
    {
        _battleStartAnim.SetText(text);
        _battleStartAnim.StartAnim();
        _animationBusy = true;
    }

    public void SetUIButton()
    {
        SetBackCommand(() => OnClickBack());
    }

    private void OnClickBack()
    {
        var eventData = new BattleViewEvent(CommandType.Back);
        _commandData(eventData);
    }

    public void SetEvent(System.Action<BattleViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetActors(List<BattlerInfo> battlerInfos)
    {
        battleActorList.gameObject.SetActive(false);

        battleActorList.Refresh(battlerInfos);
        foreach (var item in battlerInfos)
        {
            _battlerComps[item.Index] = battleActorList.GetBattlerInfoComp(item.Index);
        }
        //battleGridLayer.SetActorInfo(battlerInfos);
    }
    
    public void SetEnemies(List<BattlerInfo> battlerInfos)
    {
        battleEnemyLayer.gameObject.SetActive(false);

        battleEnemyLayer.Initialize(battlerInfos,(a) => CallEnemyInfo(a));
        battleEnemyLayer.SetInputHandler(InputKeyType.Cancel,() => OnClickBack());
        SetInputHandler(battleEnemyLayer.GetComponent<IInputHandlerEvent>());
        foreach (var item in battlerInfos)
        {
            _battlerComps[item.Index] = battleEnemyLayer.GetBattlerInfoComp(item.Index);
        }
        //battleGridLayer.SetEnemyInfo(battlerInfos);
    }

    private void CallEnemyInfo(List<int> indexList)
    {
        return;
    }

    private void CallActorList(List<int> indexList)
    {
        return;
    }

    public void StartAnimation(int targetIndex,EffekseerEffectAsset effekseerEffectAsset,int animationPosition)
    {
        _animationBusy = true;
        _animationEndTiming = 0;
    }

    public void StartAnimationAll(EffekseerEffectAsset effekseerEffectAsset)
    {
        _animationBusy = true;
        _animationEndTiming = 0;
    }

    public void RefreshStatus()
    {
        return;
        //battleGridLayer.RefreshStatus();
        foreach (var item in _battlerComps)
        {
            item.Value.RefreshStatus();
        }
    }



    private new void Update() 
    {
        if (_animationBusy == true)
        {
            CheckAnimationBusy();
            return;
        }
        base.Update();
        if (_battleBusy == true) return;
        var eventData = new BattleViewEvent(CommandType.UpdateAp);
        _commandData(eventData);
    }

    public void UpdateAp() 
    {
        //battleGridLayer.UpdatePosition();
    }

    private void CheckAnimationBusy()
    {
        if (_animationBusy == true)
        {
            bool IsBusy = false;
            if (_animationEndTiming > 0)
            {
                _animationEndTiming--;
                return;
            }
            /*
            foreach (var item in _battlerComps)
            {
                if (item.Value.IsBusy == true)
                {
                    IsBusy = true;
                    break;
                }
            }
            */
            if (IsBusy == false && _animationEndTiming <= 0 && _battleStartAnim.IsBusy == false)
            {
                _animationBusy = false;
                /*
                foreach (var item in _battlerComps)
                {
                    if (item.Value.EffekseerEmitter.enabled)
                    {
                        item.Value.DisableEmitter();
                    }
                }
                */
                var eventData = new BattleViewEvent(CommandType.EndAnimation);
                _commandData(eventData);
            }
        }
    }

}
