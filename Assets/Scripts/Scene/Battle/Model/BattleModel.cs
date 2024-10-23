using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        public BattleModel()
        {
            _sceneParam = (BattleSceneInfo)GameSystem.SceneStackManager.LastSceneParam;
            InitializeCheckTrigger();
        }
        private BattleSceneInfo _sceneParam;
        private int _actionIndex = 0;
        private int _turnCount = 1;
        public int TurnCount => _turnCount;
        public void SeekTurnCount(){_turnCount++;}

        private List<SkillLogListInfo> _skillLogs = new ();
        public List<SkillLogListInfo> SkillLogs => _skillLogs;

        private SaveBattleInfo _saveBattleInfo = new SaveBattleInfo();

        private List<BattlerInfo> _battlers = new List<BattlerInfo>();
        public List<BattlerInfo> Battlers => _battlers;
        private Dictionary<int,BattleRecord> _battleRecords = new ();
        public Dictionary<int,BattleRecord> BattleRecords => _battleRecords;

        private UnitInfo _party = null;
        private UnitInfo _troop = null;


        private Dictionary<int,List<ActionInfo>> _turnActionInfos = new ();

        public void AddTurnActionInfos(ActionInfo actionInfo,bool Interrupt)
        {
            if (!_turnActionInfos.ContainsKey(_turnCount))
            {
                _turnActionInfos[_turnCount] = new List<ActionInfo>();
            }
            if (Interrupt)
            {
                _turnActionInfos[_turnCount].Insert(0,actionInfo);
            } else
            {
                _turnActionInfos[_turnCount].Add(actionInfo);
            }
        }

        private bool UsedTurnSameActionInfo(SkillInfo skillInfo,int subjectIndex)
        {
            return _turnActionInfos.ContainsKey(_turnCount) && _turnActionInfos[_turnCount].Find(a => a.Master.Id == skillInfo.Id && a.SubjectIndex == subjectIndex) != null;
        }

        private int UsedSameTurnActionInfo(SkillInfo skillInfo)
        {
            if (_turnActionInfos.ContainsKey(_turnCount))
            {
                var sameSkills = _turnActionInfos[_turnCount].FindAll(a => a.Master.Id == skillInfo.Id);
                if (sameSkills != null)
                {
                    return sameSkills.Count;
                }
            }
            return 0; 
        }

        private Dictionary<int,List<int>> _passiveSkillInfos = new ();

        public UniTask<List<AudioClip>> GetBattleBgm()
        {
            if (CurrentStage != null)
            {
                if (CurrentSelectRecord().SymbolType == SymbolType.Boss)
                {
                    var bgmData = DataSystem.Data.GetBGM(CurrentStage.Master.BossBGMId);
                    return GetBgmData(bgmData.Key);
                }
            }
            return GetBgmData("TACTICS2");
        }

        public GameObject BattleBackGroundObject()
        {
            return ResourceSystem.LoadBattleBackGround(CurrentStage.Master.BackGround);
        }

        public void CreateBattleData()
        {
            _actionIndex = 0;
            _battlers.Clear();
            _battleRecords.Clear();
            var actorInfos = _sceneParam.ActorInfos;
            foreach (var actorInfo in actorInfos)
            {
                var battlerInfo = new BattlerInfo(actorInfo,actorInfo.BattleIndex);
                _battlers.Add(battlerInfo);
                _battleRecords[battlerInfo.Index] = new BattleRecord(battlerInfo.Index);
            }
            var enemies = _sceneParam.EnemyInfos;
            foreach (var enemy in enemies)
            {
                enemy.ResetData(enemy.Level);
                enemy.InitParamInfos(enemy.EnemyData);
                //enemy.GainHp(-9999);
                _battlers.Add(enemy);
                _battleRecords[enemy.Index] = new BattleRecord(enemy.Index);
            }
            // アルカナ
            var alcana = new BattlerInfo(AlcanaSkillInfos(),true,1);
            _battlers.Add(alcana);

            foreach (var battlerInfo1 in _battlers)
            {
                _passiveSkillInfos[battlerInfo1.Index] = new ();
            }
            _party = new UnitInfo();
            _party.SetBattlers(BattlerActors());
            _troop = new UnitInfo();
            _troop.SetBattlers(BattlerEnemies());
            _saveBattleInfo.SetParty(_party.CopyData());
            _saveBattleInfo.SetTroop(_troop.CopyData());
        }

        public void CreateBattleData(SaveBattleInfo saveBattleInfo)
        {
            _actionIndex = 0;
            _battlers.Clear();
            _battlers.AddRange(saveBattleInfo.Party.BattlerInfos);
            _battlers.AddRange(saveBattleInfo.Troop.BattlerInfos);

            // アルカナ
            var alcana = new BattlerInfo(AlcanaSkillInfos(),true,1);
            _battlers.Add(alcana);

            foreach (var battlerInfo1 in _battlers)
            {
                _passiveSkillInfos[battlerInfo1.Index] = new ();
            }
            _party = new UnitInfo();
            _party.SetBattlers(BattlerActors());
            _troop = new UnitInfo();
            _troop.SetBattlers(BattlerEnemies());
        }

        public List<BattlerInfo> FieldBattlerInfos()
        {
            return _battlers.FindAll(a => a.isAlcana == false);
        }

        public List<StateInfo> UpdateAp()
        {
            var removeStateList = new List<StateInfo>();
            foreach (var battler in FieldBattlerInfos())
            {
                if (battler.IsAlive())
                {
                    battler.UpdateAp();
                    var removeStates = battler.UpdateState(RemovalTiming.UpdateAp);
                    if (removeStates.Count > 0)
                    {
                        removeStateList.AddRange(removeStates);
                    }
                }
            }
            CheckApCurrentBattler();
            return removeStateList;
        }

        public void UpdateApModify(BattlerInfo battlerInfo)
        {
            var minusAp = 0f;
            if (battlerInfo.Ap < 0)
            {
                minusAp = battlerInfo.Ap;
            }
            foreach (var battler in FieldBattlerInfos())
            {
                if (battler.IsAlive())
                {
                    battler.ChangeAp(minusAp * -1f);
                }
            }
        }

        public void WaitCommand(ActionInfo actionInfo)
        {
            actionInfo.ActionResults.Clear();
        }

        public void AssignWaitBattler()
        {
            /*
            if (_firstActionBattler != null && _firstActionBattler.IsActor)
            {
                var waitBattlerIndex = _party.AliveBattlerInfos.FindIndex(a => a.IsState(StateType.Wait));
                if (waitBattlerIndex > -1)
                {
                    _party.AliveBattlerInfos[waitBattlerIndex].SetAp(0);
                    _party.AliveBattlerInfos[waitBattlerIndex].EraseStateInfo(StateType.Wait);
                }
            }
            */
        }

        public void RemoveOneMemberWaitBattlers()
        {
            var partyWaitBattlers = _party.AliveBattlerInfos.FindAll(a => a.CanMove());
            if (partyWaitBattlers.Count < 1)
            {
                foreach (var battlerInfo in _party.AliveBattlerInfos)
                {
                    battlerInfo.EraseStateInfo(StateType.Wait);
                }
            }
            var troopWaitBattlers = _troop.AliveBattlerInfos.FindAll(a => a.CanMove());
            if (troopWaitBattlers.Count < 1)
            {
                foreach (var battlerInfo in _troop.AliveBattlerInfos)
                {
                    battlerInfo.EraseStateInfo(StateType.Wait);
                }
            }
        }


        public void SetActionBattler(int targetIndex)
        {
            var battlerInfo = GetBattlerInfo(targetIndex);
            if (battlerInfo != null)
            {
                _currentBattler = battlerInfo;
            }
        }

        public List<BattlerInfo> BattlerActors()
        {
            return FieldBattlerInfos().FindAll(a => a.IsActor == true);
        }

        public List<BattlerInfo> BattlerEnemies()
        {
            return FieldBattlerInfos().FindAll(a => a.IsActor == false);
        }

        public BattlerInfo GetBattlerInfo(int index)
        {
            return _battlers.Find(a => a.Index == index);
        }

        public List<SkillInfo> SkillActionList()
        {
            var skillInfos = _currentBattler.Skills.FindAll(a => a.Master.SkillType != SkillType.None && a.Master.Id > 100 && a.IsEnhanceSkill() == false);
            for (int i = 0; i < skillInfos.Count;i++)
            {
                skillInfos[i].SetEnable(CheckCanUse(skillInfos[i],_currentBattler));
            }
            var sortList1 = new List<SkillInfo>();
            var sortList2 = new List<SkillInfo>();
            var sortList3 = new List<SkillInfo>();
            skillInfos.Sort((a,b) => {return a.Master.Id > b.Master.Id ? 1 : -1;});
            foreach (var skillInfo in skillInfos)
            {
                if (skillInfo.Master.IconIndex >= MagicIconType.Elementarism && skillInfo.Master.IconIndex <= MagicIconType.Psionics)
                {
                    sortList1.Add(skillInfo);
                } else
                if (skillInfo.Master.IconIndex >= MagicIconType.Demigod && skillInfo.Master.IconIndex < MagicIconType.Other)
                {
                    sortList2.Add(skillInfo);
                } else
                {
                    sortList3.Add(skillInfo);
                }
            }
            skillInfos.Clear();
            skillInfos.AddRange(sortList1);
            skillInfos.AddRange(sortList2);
            skillInfos.AddRange(sortList3);
            return skillInfos;
        }

        public int SelectSkillIndex(List<SkillInfo> skillInfos)
        {
            int selectIndex = 0;
            if (_currentBattler != null && _currentBattler.IsActor == true)
            {
                var skillIndex = skillInfos.FindIndex(a => a.Id == _currentBattler.LastSelectSkillId);
                if (skillIndex > -1)
                {
                    selectIndex = skillIndex;
                }
            }
            return selectIndex;
        }

        private bool CheckCanUse(SkillInfo skillInfo,BattlerInfo battlerInfo)
        {
            if (skillInfo.CountTurn > 0)
            {
                return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Passive)
            {
                return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Relic)
            {
                return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Unique)
            {
                return false;
            }
            if (battlerInfo.IsState(StateType.Silence) && skillInfo.Master.CountTurn != 0)
            {
                return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Awaken)
            {
                if (!battlerInfo.IsState(StateType.Demigod))
                {
                    return false;
                }
            }
            if (skillInfo.IsUnison())
            {
                return FieldBattlerInfos().FindAll(a => a.IsAlive() && a.IsActor == battlerInfo.IsActor && a.CanMove()).Count > 1;
            }
            if (CanUseTrigger(skillInfo,battlerInfo) == false)
            {
                return false;
            }
            var targetIndexList = GetSkillTargetIndexList(skillInfo.Master.Id,battlerInfo.Index,true);
            if (targetIndexList.Count == 0)
            {
                return false;
            }
            return true;
        }

        private bool CheckCanUsePassive(SkillInfo skillInfo,BattlerInfo battlerInfo)
        {
            if (skillInfo.CountTurn > 0)
            {
                return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Passive)
            {
                //return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Unique)
            {
                return false;
            }
            if (battlerInfo.IsState(StateType.Silence) && skillInfo.Master.CountTurn != 0)
            {
                return false;
            }
            if (battlerInfo.IsState(StateType.NoPassive))
            {
                return false;
            }
            if (!battlerInfo.CanMove())
            {
                return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Awaken)
            {
                if (!battlerInfo.IsState(StateType.Demigod))
                {
                    return false;
                }
            }
            if (skillInfo.IsUnison())
            {
                return FieldBattlerInfos().FindAll(a => a.IsAlive() && a.IsActor == battlerInfo.IsActor && a.CanMove()).Count > 1;
            }
            if (CanUseTrigger(skillInfo,battlerInfo) == false)
            {
                return false;
            }
            return true;
        }

        public bool EnableCurrentBattler()
        {
            if (_currentBattler.CanMove() == false)
            {
                return false;
            }
            // 使用可能な魔法がない
            var skillInfos = _currentBattler.ActiveSkills().FindAll(a => CheckCanUse(a,_currentBattler));
            if (skillInfos.Count == 0)
            {
                return false;
            }
            return true;
        }


        // 選択可能なBattlerInfoを取得
        public List<ListData> TargetBattlerPartyInfos(ActionInfo actionInfo)
        {
            var targetBattlerInfos = new List<ListData>();
            foreach (var battlerInfo in _party.BattlerInfos)
            {
                var listData = new ListData(battlerInfo);
                listData.SetEnable(actionInfo.CandidateTargetIndexList.Contains(battlerInfo.Index));
                listData.SetSelected(actionInfo.LastTargetIndex == battlerInfo.Index);
                targetBattlerInfos.Add(listData);
            }
            return targetBattlerInfos;
        }

        // 選択可能なBattlerInfoを取得
        public List<ListData> TargetBattlerEnemyInfos(ActionInfo actionInfo)
        {
            var targetBattlerInfos = new List<ListData>();
            foreach (var battlerInfo in _troop.BattlerInfos)
            {
                var listData = new ListData(battlerInfo);
                listData.SetEnable(actionInfo.CandidateTargetIndexList.Contains(battlerInfo.Index));
                listData.SetSelected(actionInfo.LastTargetIndex == battlerInfo.Index);
                targetBattlerInfos.Add(listData);
            }
            return targetBattlerInfos;
        }

        // selectIndexを対象にした時の効果範囲を取得
        public List<int> ActionInfoTargetIndexes(ActionInfo actionInfo,int selectIndex,int counterSubjectIndex = -1,ActionInfo baseActionInfo = null,List<ActionResultInfo> baseActionResultInfos = null)
        {
            if (actionInfo == null)
            {
                return new List<int>();
            }
            var subject = GetBattlerInfo(actionInfo.SubjectIndex);
            var targetIndexList = GetSkillTargetIndexList(actionInfo.Master.Id,subject.Index,true,counterSubjectIndex,baseActionInfo,baseActionResultInfos);
            var scopeType = actionInfo.ScopeType;
            if (subject.IsState(StateType.EffectLine) && scopeType != ScopeType.All)
            {
                scopeType = ScopeType.Line;
            }
            if (subject.IsState(StateType.EffectAll))
            {
                scopeType = ScopeType.All;
            }
            var TargetBattler = GetBattlerInfo(selectIndex);
            switch (scopeType)
            {
                case ScopeType.All:
                case ScopeType.WithoutSelfAll:
                    break;
                case ScopeType.Line:
                case ScopeType.FrontLine:
                case ScopeType.WithoutSelfLine:
                    targetIndexList = targetIndexList.FindAll(a => GetBattlerInfo(a).LineIndex == TargetBattler.LineIndex);
                    break;
                case ScopeType.One:
                case ScopeType.WithoutSelfOne:
                case ScopeType.Self:
                    targetIndexList.Clear();
                    targetIndexList.Add(selectIndex);
                    break;
                case ScopeType.OneAndNeighbor:
                case ScopeType.Neighbor:
                    targetIndexList.Clear();
                    targetIndexList.Add(selectIndex);
                    // 両隣を追加
                    var targetUnit = subject.IsActor ? _party.AliveBattlerInfos : _troop.AliveBattlerInfos;
                    if (actionInfo.TargetType == TargetType.Opponent)
                    {
                        targetUnit = subject.IsActor ? _troop.AliveBattlerInfos : _party.AliveBattlerInfos;
                    }
                    var before = targetUnit.FindAll(a => a.Index < selectIndex);
                    if (before.Count > 0)
                    {
                        targetIndexList.Add(before[before.Count-1].Index);
                    }
                    var after = targetUnit.FindAll(a => a.Index > selectIndex);
                    if (after.Count > 0)
                    {
                        targetIndexList.Add(after[0].Index);
                    }
                    if (scopeType == ScopeType.Neighbor)
                    {
                        targetIndexList.Remove(subject.Index);
                    }
                    break;
            }
            // 挑発
            if (subject != null && subject.IsState(StateType.Substitute))
            {
                // 対象と挑発した対象が同じパーティなら有効
                var substituteState = subject.GetStateInfo(StateType.Substitute);
                if (targetIndexList.FindIndex(a => GetBattlerInfo(a).IsActor == GetBattlerInfo(substituteState.BattlerId).IsActor) > -1)
                {
                    targetIndexList.Clear();
                    int substituteId = subject.GetStateInfo(StateType.Substitute).BattlerId;
                    if (targetIndexList.Contains(substituteId))
                    {
                        targetIndexList.Add(substituteId);
                    } else
                    {
                        var tempIndexList = GetSkillTargetIndexList(actionInfo.Master.Id,actionInfo.SubjectIndex,false);
                        if (tempIndexList.Contains(substituteId))
                        {
                            targetIndexList.Add(substituteId);
                        }
                    }
                }
            }
            // かばう
            CalcCoverTargetIndexes(targetIndexList,GetBattlerInfo(actionInfo.SubjectIndex).IsActor);
            return targetIndexList;
        }

        public void CalcCoverTargetIndexes(List<int> targetIndexList,bool subjectIsActor)
        {
            var coverBattlerId = -1;
            var coveredBattlerId = -1;
            foreach (var coverTargetIndex in targetIndexList)
            {
                var friends = GetBattlerInfo(coverTargetIndex).IsActor ? _party : _troop;
                var coverBattlerInfo = friends.AliveBattlerInfos.Find(a => a.IsState(StateType.Cover));
                if (coverBattlerInfo != null && coverBattlerId == -1 && coverBattlerInfo.IsActor != subjectIsActor)
                {
                    if (coverBattlerInfo.Index != coverTargetIndex)
                    {
                        coverBattlerId = coverBattlerInfo.Index;
                        coveredBattlerId = coverTargetIndex;
                    }
                }
            }
            if (coverBattlerId > -1 && coveredBattlerId > -1)
            {
                if (!targetIndexList.Contains(coverBattlerId))
                {
                    targetIndexList.Add(coverBattlerId);
                }
                targetIndexList.Remove(coveredBattlerId);
            }
        }

        public bool CanUseCondition(int skillId,BattlerInfo subject,int targetIndex)
        {
            bool IsEnable = false;
            var skill = DataSystem.FindSkill(skillId);
            var target = GetBattlerInfo(targetIndex);
            foreach (var featureData in skill.FeatureDates)
            {
                switch (featureData.FeatureType)
                {
                    case FeatureType.HpDamage:
                    if (target.Hp > 0)
                    {
                        IsEnable = true;
                    }
                    break;
                    case FeatureType.HpConsumeDamage:
                    if (target.Hp > 0)
                    {
                        var needHp = 0;
                        // 割合
                        if (featureData.Param2 == 1)
                        {
                            needHp = (int)(subject.MaxHp * featureData.Param3 * 0.01f);
                        } else
                        {
                            needHp = featureData.Param3;
                        }
                        IsEnable = subject.Hp > needHp;
                    }
                    break;
                    case FeatureType.HpHeal:
                    case FeatureType.KindHeal:
                    if (subject != null && subject.IsActor)
                    {
                        {
                            if (!target.IsActor)
                            {
                                IsEnable = target.Kinds.Contains(KindType.Undead);
                            } else
                            {
                                IsEnable = true;
                            }
                        }
                    } else 
                    {
                        if (target.Hp < target.MaxHp)
                        {
                            if (!target.IsActor && !target.Kinds.Contains(KindType.Undead))
                            {
                                IsEnable = true;
                            }
                        }
                    }
                    break;
                    case FeatureType.CtHeal:
                    if (subject != null)
                    {
                        if (target.Skills.Find(a => a.CountTurn > 0) != null)
                        {
                            IsEnable = true;
                        }
                    }
                    break;
                    case FeatureType.CtDamage:
                    if (target.Skills.Find(a => a.CountTurn < a.Master.CountTurn) != null)
                    {
                        IsEnable = true;
                    }
                    break;
                    case FeatureType.AddState:
                    case FeatureType.AddStateNextTurn:
                    if ((StateType)featureData.Param1 == StateType.RemoveBuff)
                    {
                        // 消すバフがあれば有効
                        if (target.GetRemovalBuffStates().Count > 0)
                        {
                            IsEnable = true;
                        }
                    }
                    else
                    {
                        if (target != null)
                        {
                            var targetStateInfos = target.StateInfos;
                            var sameStateInfos = targetStateInfos.FindAll(a => a.Master.StateType == (StateType)featureData.Param1 && a.SkillId == skillId);
                            // 既にかかっているか
                            if (sameStateInfos.Count > 0)
                            {
                                // 重複できるか
                                var overLapCount = sameStateInfos[0].Master.OverLap;
                                if (overLapCount > sameStateInfos.Count)
                                {
                                    IsEnable = true;
                                }
                            } else
                            {
                                IsEnable = true;
                            }
                        }
                        /*
                        if (!target.IsState((StateType)featureData.Param1) && !target.IsState(StateType.Barrier))
                        {
                            IsEnable = true;
                        } else
                        if (subject != null && subject.IsActor || (StateType)featureData.Param1 == StateType.DamageUp)
                        {
                            IsEnable = true;
                        } else
                        if (subject != null && !subject.IsActor && !target.IsState((StateType)featureData.Param1) && target.IsState(StateType.Barrier))
                        {
                            if (UnityEngine.Random.Range(0,100) > 50)
                            {
                                IsEnable = true;
                            }
                        }
                        */
                    }
                    break;
                    case FeatureType.RemoveState:
                    if (target.IsState((StateType)featureData.Param1))
                    {
                        IsEnable = true;
                    }
                    break;
                    case FeatureType.RemoveAbnormalState:
                    if (target.StateInfos.Find(a => a.Master.Abnormal) != null)
                    {
                        IsEnable = true;
                    }
                    break;
                    case FeatureType.BreakUndead:
                    if (subject.IsActor)
                    {
                        IsEnable = true;
                    } else 
                    {
                        if (!target.IsActor && !target.Kinds.Contains(KindType.Undead))
                        {
                            IsEnable = true;
                        }
                    }
                    break;
                    case FeatureType.NoResetAp:
                        break;
                    default:
                        IsEnable = true;
                    break;
                }
            }
            return IsEnable;
        }

        public List<int> CheckScopeTriggers(List<int> targetIndexList,List<SkillData.TriggerData> scopeTriggers,ActionInfo actionInfo,List<ActionResultInfo> actionResultInfos)
        {
            for (int i = targetIndexList.Count-1;i >= 0;i--)
            {
                var target = GetBattlerInfo(targetIndexList[i]);
                var remove = false;
                foreach (var scopeTrigger in scopeTriggers)
                {
                    if (scopeTrigger.TriggerType == TriggerType.DemigodMagicAttribute)
                    {
                        if (target.Skills.Find(a => a.Master.SkillType == SkillType.Unique && a.Attribute == (AttributeType)scopeTrigger.Param1) == null)
                        {
                            remove = true;
                        }
                    } else
                    {
                        if (!IsTriggeredSkillInfo(target,scopeTriggers,actionInfo,actionResultInfos))
                        {
                            remove = true;
                        }
                        /*
                        if (!scopeTrigger.IsTriggeredSkillInfo(target,BattlerActors(),BattlerEnemies()))
                        {
                            remove = true;
                        }
                        */
                    }
                }
                if (remove)
                {
                    targetIndexList.RemoveAt(i);
                }
            }
            return targetIndexList;
        }


        private void SetActorLastTarget(ActionInfo actionInfo,BattlerInfo subject,List<int> indexList)
        {
            if (indexList.Count > 0)
            {
                if (subject.IsActor)
                {
                    if (actionInfo.Master.TargetType == TargetType.Opponent)
                    {
                        subject.SetLastTargetIndex(indexList[0]);
                    }
                }
                if (actionInfo.Master.TargetType == TargetType.All)
                {
                    if (indexList[0] > 100)
                    {
                        subject.SetLastTargetIndex(indexList[0]);
                    }
                }
            }
        }


        // indexListにActionを使ったときのリザルトを生成
        public void MakeActionResultInfo(ActionInfo actionInfo,List<int> indexList)
        {
            if (actionInfo.RepeatTime == 0)
            {
                return;
            }
            actionInfo.SetCandidateTargetIndexList(indexList);
            actionInfo.SetRepeatTime(actionInfo.RepeatTime - 1);
            var subject = GetBattlerInfo(actionInfo.SubjectIndex);
            // ターゲットの生死判定
            var aliveType = actionInfo.Master.AliveType;
            if (actionInfo != null)
            {
                indexList = CalcAliveTypeIndexList(indexList,aliveType);
            }
            SetActorLastTarget(actionInfo,subject,indexList);
            if (subject.IsState(StateType.Silence))
            {
                return;
            }
            if (subject.IsState(StateType.NoPassive) && actionInfo.Master.SkillType == SkillType.Passive)
            {
                return;
            }

            var actionResultInfos = new List<ActionResultInfo>();

            foreach (var targetIndex in indexList)
            {
                var target = GetBattlerInfo(targetIndex);
                var featureDates = new List<SkillData.FeatureData>();
                foreach (var featureData in actionInfo.SkillInfo.FeatureDates)
                {
                    featureDates.Add(featureData);
                }

                var actionResultInfo = new ActionResultInfo(subject,target,featureDates,actionInfo.Master.Id,actionInfo.ScopeType == ScopeType.One);
                
                // Hpダメージ分の回復計算
                var DamageHealPartyResultInfos = CalcDamageHealParty(subject,featureDates,actionResultInfo.HpDamage);
                actionResultInfos.AddRange(DamageHealPartyResultInfos);
                var DamageMpHealPartyResultInfos = CalcDamageCtHealParty(subject,featureDates,actionResultInfo.HpDamage);
                actionResultInfos.AddRange(DamageMpHealPartyResultInfos);

                if (actionResultInfo.RemoveAttackStateDamage())            
                {
                    // 攻撃を受けた時に外れるステートを管理
                }
                actionResultInfos.Add(actionResultInfo);
            }
            AdjustActionResultInfo(actionResultInfos);
            actionInfo.SetActionResult(actionResultInfos);
        }

        private List<ActionResultInfo> CalcDamageHealParty(BattlerInfo subject,List<SkillData.FeatureData> featureDates,int hpDamage)
        {
            var actionResultInfos = new List<ActionResultInfo>();
            var damageHealParty = featureDates.Find(a => a.FeatureType == FeatureType.DamageHpHealParty);
            if (damageHealParty != null)
            {
                var friends = subject.IsActor ? _party.AliveBattlerInfos : _troop.AliveBattlerInfos;
                var hpHeal = hpDamage * damageHealParty.Param3 * 0.01f;
                foreach (var friend in friends)
                {
                    var featureData = new SkillData.FeatureData
                    {
                        FeatureType = FeatureType.HpHeal,
                        Param1 = (int)hpHeal
                    };
                    var actionResultInfo = new ActionResultInfo(subject,GetBattlerInfo(friend.Index),new List<SkillData.FeatureData>(){featureData},-1);
                    actionResultInfos.Add(actionResultInfo);
                }
            }
            return actionResultInfos;
        }

        private List<ActionResultInfo> CalcDamageCtHealParty(BattlerInfo subject,List<SkillData.FeatureData> featureDates,int hpDamage)
        {
            var actionResultInfos = new List<ActionResultInfo>();
            var damageHealParty = featureDates.Find(a => a.FeatureType == FeatureType.DamageMpHealParty);
            if (damageHealParty != null)
            {
                var friends = subject.IsActor ? _party.AliveBattlerInfos : _troop.AliveBattlerInfos;
                var ctHeal = hpDamage * damageHealParty.Param3 * 0.01f;
                foreach (var friend in friends)
                {
                    var featureData = new SkillData.FeatureData
                    {
                        FeatureType = FeatureType.CtHeal,
                        Param1 = (int)ctHeal
                    };
                    var actionResultInfo = new ActionResultInfo(subject,GetBattlerInfo(friend.Index),new List<SkillData.FeatureData>(){featureData},-1);
                    actionResultInfos.Add(actionResultInfo);
                }
            }
            return actionResultInfos;
        }

        private int CalcHpCost(ActionInfo actionInfo)
        {
            int hpCost = 0;
            var featureDates = actionInfo.Master.FeatureDates.FindAll(a => a.FeatureType == FeatureType.HpConsumeDamage);
            foreach (var featureData in featureDates)
            {
                // 割合
                if (featureData.Param2 == 1)
                {
                    hpCost += (int)(GetBattlerInfo(actionInfo.SubjectIndex).MaxHp * featureData.Param3 * 0.01f);
                } else
                {
                    hpCost += featureData.Param3;
                }
            }
            return hpCost;
        }

        private int CalcMpCost(BattlerInfo battlerInfo,int mpCost)
        {
            return battlerInfo.CalcMpCost(mpCost);
        }

        private int CalcRepeatTime(BattlerInfo subject,ActionInfo actionInfo)
        {
            var repeatTime = actionInfo.Master.RepeatTime;
            // パッシブで回数増加を計算
            var addFeatures = subject.Skills.FindAll(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.ChangeSkillRepeatTime && actionInfo.Master.Id == b.Param1) != null);
            foreach (var addFeature in addFeatures)
            {
                foreach (var featureData in addFeature.FeatureDates)
                {
                    repeatTime = featureData.Param3;
                }
            }
            return repeatTime;
        }

        private ScopeType CalcScopeType(BattlerInfo subject,ActionInfo actionInfo)
        {
            var scopeType = actionInfo.Master.Scope;
            // パッシブで対象変更を計算
            var changeScopeFeature = subject.Skills.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.ChangeSkillScope && actionInfo.Master.Id == b.Param1) != null);
            if (changeScopeFeature != null)
            {
                scopeType = (ScopeType)changeScopeFeature.FeatureDates[0].Param3;
            }
            return scopeType;
        }

        public List<MakerEffectData.SoundTimings> SkillActionSoundTimings(string animationName)
        {
            var makerEffectPath = animationName.Replace("MakerEffect/","");
            var path = "Animations/AnimationData/" + makerEffectPath;
            var result = Resources.Load<MakerEffectAssetData>(path);
            if (result != null)
            {
                return result.AssetData.soundTimings;
            }
            return null;
        }

        public Effekseer.EffekseerEffectAsset AwakenEffect(int actorId)
        {
            var result = ResourceSystem.LoadResourceEffect("NA_Effekseer/NA_cut-in_" + DataSystem.FindActor(actorId).ImagePath);
            if (result != null)
            {
                return result;
            }
            return null;
        }

        public Sprite AwakenSprite(int actorId)
        {
            var result = ResourceSystem.LoadActorCutinSprite(DataSystem.FindActor(actorId).ImagePath);
            if (result != null)
            {
                return result;
            }
            return null;
        }

        public Sprite AwakenEnemySprite(int enemyId)
        {
            var result = ResourceSystem.LoadEnemySprite(DataSystem.Enemies.Find(a => a.Id == enemyId).ImagePath);
            if (result != null)
            {
                return result;
            }
            return null;
        }

        public void ExecCurrentAction(ActionInfo actionInfo,bool addSaveData)
        {
            if (actionInfo != null)
            {
                var subject = GetBattlerInfo(actionInfo.SubjectIndex);
                // 支払いは最後の1回
                if (actionInfo.RepeatTime == 0)
                {
                    // Hpの支払い
                    subject.GainHp(actionInfo.HpCost * -1);
                    // Mpの支払い
                    //subject.GainMp(actionInfo.MpCost * -1);
                    //subject.GainPayBattleMp(actionInfo.MpCost);
                    subject.InitCountTurn(actionInfo.SkillInfo.Id);
                    subject.SetLastSelectSkillId(actionInfo.SkillInfo.Id);
                    subject.GainUseCount(actionInfo.SkillInfo.Id);
                }
                if (actionInfo.Master.IsHpHealFeature())
                {
                    subject.GainHealCount(1);
                }
                if (addSaveData)
                {
                    _saveBattleInfo.AddActionData(actionInfo);
                }
                ExecActionResultInfos(actionInfo.ActionResults,false);
                if (actionInfo.Master.IsRevengeHpDamageFeature())
                {
                    // 受けたダメージをリセット
                    subject.SetDamagedValue(0);
                }
            }
        }

        // 複数のActionResultのreDamageとreHealを1つにまとめる
        public void AdjustActionResultInfo(List<ActionResultInfo> actionResultInfos)
        {
            // ドレイン回復をまとめる
            var reHealResults = actionResultInfos.FindAll(a => a.ReHeal > 0);
            if (reHealResults.Count > 1)
            {
                int reHeal = 0;
                foreach (var reHealResult in reHealResults)
                {
                    reHeal += reHealResult.ReHeal;
                    reHealResult.SetReHeal(0);
                }
                reHealResults[reHealResults.Count-1].SetReHeal(reHeal);
            }
            // カウンターダメージをまとめる
            var counterResults = actionResultInfos.FindAll(a => a.ReDamage > 0);
            if (counterResults.Count > 1)
            {
                int reDamage = 0;
                foreach (var counterResult in counterResults)
                {
                    reDamage += counterResult.ReDamage;
                    counterResult.SetReDamage(0);
                }
                counterResults[counterResults.Count-1].SetReDamage(reDamage);
            }
            if (reHealResults.Count > 0 && counterResults.Count > 0)
            {
                int heal = reHealResults[reHealResults.Count-1].ReHeal;
                int damage = counterResults[counterResults.Count-1].ReDamage;
                
                if (heal > damage)
                {
                    reHealResults[reHealResults.Count-1].SetReHeal(heal - damage);
                    counterResults[counterResults.Count-1].SetReDamage(0);
                } else
                {
                    reHealResults[reHealResults.Count-1].SetReHeal(0);
                    counterResults[counterResults.Count-1].SetReDamage(damage - heal);
                }
            }
            // ReDamageによってDeadIndexを変更
            if (counterResults.Count > 0)
            {
                var result = counterResults[counterResults.Count-1];
                if (result.ReDamage > GetBattlerInfo(result.SubjectIndex).Hp && !result.DeadIndexList.Contains(result.SubjectIndex))
                {
                    counterResults[counterResults.Count-1].DeadIndexList.Add(result.SubjectIndex);
                }
            }

            // Stateの重複をまとめる
            var addStates = new List<StateInfo>();
            var removeStates = new List<StateInfo>();
            var displayStates = new List<StateInfo>();
            foreach (var actionResultInfo in actionResultInfos)
            {
                for (var i = actionResultInfo.AddedStates.Count-1;i >= 0;i--)
                {
                    if (addStates.Find(a => a.CheckSameStateType(actionResultInfo.AddedStates[i]) == true) == null)
                    {
                        addStates.Add(actionResultInfo.AddedStates[i]);
                    } else
                    {
                        actionResultInfo.AddedStates.RemoveAt(i);
                    }
                }
                for (var i = actionResultInfo.RemovedStates.Count-1;i >= 0;i--)
                {
                    if (removeStates.Find(a => a.CheckSameStateType(actionResultInfo.RemovedStates[i]) == true) == null)
                    {
                        removeStates.Add(actionResultInfo.RemovedStates[i]);
                    } else
                    {
                        actionResultInfo.RemovedStates.RemoveAt(i);
                    }
                }
                for (var i = actionResultInfo.DisplayStates.Count-1;i >= 0;i--)
                {
                    if (displayStates.Find(a => a.CheckSameStateType(actionResultInfo.DisplayStates[i]) == true) == null)
                    {
                        displayStates.Add(actionResultInfo.DisplayStates[i]);
                    } else
                    {
                        actionResultInfo.DisplayStates.RemoveAt(i);
                    }
                }
            }
        }

        public void ExecActionResultInfos(List<ActionResultInfo> actionResultInfos,bool addSaveData = true)
        {
            foreach (var actionResultInfo in actionResultInfos)
            {
                ExecActionResultInfo(actionResultInfo,addSaveData);
            }
        }

        /// <summary>
        /// ダメージなどを適用
        /// </summary>
        /// <param name="actionResultInfo"></param>
        private void ExecActionResultInfo(ActionResultInfo actionResultInfo,bool addSaveData = true)
        {
            var subject = GetBattlerInfo(actionResultInfo.SubjectIndex);
            var target = GetBattlerInfo(actionResultInfo.TargetIndex);
            foreach (var addState in actionResultInfo.AddedStates)
            {
                var addTarget = GetBattlerInfo(addState.TargetIndex);
                addTarget.AddState(addState,true);
            }
            foreach (var removeState in actionResultInfo.RemovedStates)
            {
                var removeTarget = GetBattlerInfo(removeState.TargetIndex);
                removeTarget.RemoveState(removeState,true);
            }
            if (actionResultInfo.HpDamage != 0)
            {
                var hpDamage = actionResultInfo.HpDamage;
                target.GainHp(-1 * hpDamage);
                target.GainDamagedValue(hpDamage);
                _battleRecords[subject.Index].GainAttackValue(hpDamage);
                _battleRecords[target.Index].GainDamagedValue(hpDamage);
            }
            if (actionResultInfo.HpHeal != 0 && (!actionResultInfo.DeadIndexList.Contains(target.Index) || actionResultInfo.AliveIndexList.Contains(target.Index)))
            {
                target.GainHp(actionResultInfo.HpHeal);
            }
            if (actionResultInfo.CtDamage != 0)
            {
                target.SeekCountTurn(-1 * actionResultInfo.CtDamage);
            }
            if (actionResultInfo.CtHeal != 0)
            {
                target.SeekCountTurn(actionResultInfo.CtHeal,actionResultInfo.CtHealSkillId);
            }
            if (actionResultInfo.ApHeal != 0)
            {
                target.ChangeAp(actionResultInfo.ApHeal * -1);
            }
            if (actionResultInfo.ApDamage != 0)
            {
                target.ChangeAp(actionResultInfo.ApDamage);
            }
            if (actionResultInfo.ReHeal != 0)
            {
                subject.GainHp(actionResultInfo.ReHeal);
            }
            if (actionResultInfo.ReDamage != 0 || actionResultInfo.CurseDamage != 0)
            {
                var reDamage = 0;
                if (target.IsAlive())
                {
                    reDamage += actionResultInfo.ReDamage;
                }
                reDamage += actionResultInfo.CurseDamage;
                if (reDamage > 0)
                {
                    subject.GainHp(-1 * reDamage);
                    _battleRecords[target.Index].GainAttackValue(reDamage);
                    _battleRecords[subject.Index].GainDamagedValue(reDamage);
                }
            }
            if (actionResultInfo.Missed == true)
            {
                target.GainDodgeCount(1);
            }
            foreach (var targetIndex in actionResultInfo.ExecStateInfos)
            {
                var execTarget = GetBattlerInfo(targetIndex.Key);
                if (execTarget != null)
                {
                    foreach (var stateInfo in targetIndex.Value)
                    {
                        execTarget.UpdateStateCount(RemovalTiming.UpdateCount,stateInfo);
                    }
                }
            }
            
            actionResultInfo.SetTurnCount(_turnCount);
            if (addSaveData)
            {
                _saveBattleInfo.AddResultData(actionResultInfo);
            }
        }
        
        public List<int> DeathBattlerIndex(List<ActionResultInfo> actionResultInfos)
        {
            var deathBattlerIndex = new List<int>();
            foreach (var actionResultInfo in actionResultInfos)
            {
                foreach (var deadIndexList in actionResultInfo.DeadIndexList)
                {
                    // 例外
                    if (!GetBattlerInfo(deadIndexList).IsState(StateType.Death))
                    {

                    } else
                    {
                        deathBattlerIndex.Add(deadIndexList);
                    }
                }
            }
            return deathBattlerIndex;
        }

        public List<int> AliveBattlerIndex(List<ActionResultInfo> actionResultInfos)
        {
            var aliveBattlerIndex = new List<int>();
            foreach (var actionResultInfo in actionResultInfos)
            {
                foreach (var aliveIndexList in actionResultInfo.AliveIndexList)
                {
                    aliveBattlerIndex.Add(aliveIndexList);
                }
            }
            return aliveBattlerIndex;
        }

        public List<StateInfo> UpdateTurn()
        {
            var result = _firstActionBattler.UpdateState(RemovalTiming.UpdateTurn);
            _firstActionBattler.TurnEnd();
            var skillInfos = new List<SkillInfo>();
            foreach (var actionInfo in _turnActionInfos[_turnCount])
            {
                skillInfos.Add(actionInfo.SkillInfo);
            }
            _firstActionBattler.TurnEndSkillSeekCountTurn(skillInfos);
            return result;
        }

        public int CheckActionAfterGainAp(ActionInfo actionInfo)
        {
            var gainAp = actionInfo.SkillInfo.ActionAfterGainAp();
            return gainAp;
        }

        public void ActionAfterGainAp(int gainAp)
        {
            //_currentTurnBattler.GainMp(gainAp);
        }

        public List<StateInfo> UpdateNextSelfTurn()
        {
            var result = _currentBattler.UpdateState(RemovalTiming.NextSelfTurn);
            return result;
        }

        public bool CheckNoResetAp(ActionInfo actionInfo)
        {
            if (actionInfo.TriggeredSkill == false)
            {
                var subject = GetBattlerInfo(actionInfo.SubjectIndex);
                var noResetAp = actionInfo.SkillInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.NoResetAp);
                return subject.IsAlive() && noResetAp != null;
            }
            return false;
        }

        public bool CheckReaction(ActionInfo actionInfo)
        {
            var reAction = false;
            if (actionInfo.TriggeredSkill == false)
            {
                var subject = GetBattlerInfo(actionInfo.SubjectIndex);
                var noResetAp = actionInfo.SkillInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.NoResetAp);
                if (subject.IsAlive() && noResetAp != null)
                {
                    reAction = true;
                } else
                {
                    subject.ResetAp(false);
                }
            }
            return reAction;
        }

        public void TurnEnd(ActionInfo actionInfo)
        {
            if (actionInfo.TriggeredSkill == false)
            {
                var subject = GetBattlerInfo(actionInfo.SubjectIndex);

                var afterApHalf = actionInfo.SkillInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.SetAfterApHalf);
                if (afterApHalf != null)
                {
                    subject.ResetAp(false);
                    subject.SetAp((int)(subject.Ap * afterApHalf.Param1 * 0.01f));
                }
                var afterAp = actionInfo.SkillInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.SetAfterAp);
                if (afterAp != null)
                {
                    subject.SetAp(afterAp.Param1);
                }
                if (subject.IsState(StateType.Combo))
                {
                    var friends = subject.IsActor ? _party.AliveBattlerInfos : _troop.AliveBattlerInfos;
                    foreach (var friend in friends)
                    {
                        if (friend.Index != subject.Index)
                        {
                            friend.SetAp(0);
                        }
                    }
                }
            }
            var skillLog = new SkillLogListInfo
            {
                battlerInfo = GetBattlerInfo(actionInfo.SubjectIndex),
                skillInfo = actionInfo.SkillInfo
            };
            _skillLogs.Add(skillLog);
            PopActionInfo();
            _currentBattler = null;
        }

        public void CheckPlusSkill(ActionInfo actionInfo)
        {
            if (actionInfo != null)
            {
                var plusActionInfos = actionInfo.CheckPlusSkill();
                foreach (var plusActionInfo in plusActionInfos)
                {
                    plusActionInfo.SetRangeType(CalcRangeType(plusActionInfo.Master,GetBattlerInfo(actionInfo.SubjectIndex)));
                }
                foreach (var plusActionInfo in plusActionInfos)
                {
                    if (plusActionInfo.Master.SkillType == SkillType.Passive)
                    {
                        if (!_passiveSkillInfos[actionInfo.SubjectIndex].Contains(plusActionInfo.Master.Id))
                        {
                            _passiveSkillInfos[actionInfo.SubjectIndex].Add(plusActionInfo.Master.Id);
                        }
                    }
                    AddActionInfo(plusActionInfo,false);
                    AddTurnActionInfos(plusActionInfo,false);
                }

                var plusTriggerSkillInfos = actionInfo.CheckPlusSkillTrigger();
                foreach (var skillInfo in plusTriggerSkillInfos)
                {
                    var triggerDates = skillInfo.Master.TriggerDates;
                    if (IsTriggeredSkillInfo(GetBattlerInfo(actionInfo.SubjectIndex),triggerDates,actionInfo,actionInfo.ActionResults))
                    {
                        if (skillInfo.Master.SkillType == SkillType.Passive)
                        {
                            if (!_passiveSkillInfos[actionInfo.SubjectIndex].Contains(skillInfo.Master.Id))
                            {
                                _passiveSkillInfos[actionInfo.SubjectIndex].Add(skillInfo.Master.Id);
                            }
                        }
                        var plusTriggerActionInfo = new ActionInfo(skillInfo,_actionIndex,actionInfo.SubjectIndex,-1,null);
                        plusTriggerActionInfo.SetTriggerSkill(true);
                        AddActionInfo(plusTriggerActionInfo,false);
                        AddTurnActionInfos(plusTriggerActionInfo,false);
                        plusTriggerActionInfo.SetRangeType(CalcRangeType(plusTriggerActionInfo.Master,GetBattlerInfo(actionInfo.SubjectIndex)));
                    }
                }
            }
        }

        public List<ActionResultInfo> CheckRegenerate(ActionInfo actionInfo)
        {
            var firstActionBattler = _firstActionBattler;
            var actionResultInfos = new List<ActionResultInfo>();
            var regenerateHp = firstActionBattler.RegenerateHpValue();
            if (regenerateHp > 0)
            {
                var featureData = new SkillData.FeatureData
                {
                    FeatureType = FeatureType.HpHeal,
                    Param1 = regenerateHp
                };
                var actionResultInfo = new ActionResultInfo(firstActionBattler,firstActionBattler,new List<SkillData.FeatureData>(){featureData},-1);
                actionResultInfos.Add(actionResultInfo);
            }
            actionResultInfos.AddRange(AfterHealActionResults());
            if (actionInfo != null && actionInfo.ActionResults.Find(a => a.HpDamage > 0) != null)
            {
                actionResultInfos.AddRange(AssistHealActionResults());
            }
            return actionResultInfos;
        }

        private List<ActionResultInfo> AfterHealActionResults()
        {
            var firstActionBattler = _firstActionBattler;
            var afterHealResults = new List<ActionResultInfo>();
            var afterSkillInfo = firstActionBattler.Skills.Find(a => a.FeatureDates.Find(b => b.FeatureType == FeatureType.AddState && (StateType)b.Param1 == StateType.AfterHeal) != null);
            if (firstActionBattler.IsState(StateType.AfterHeal) && afterSkillInfo != null)
            {
                var stateInfo = firstActionBattler.GetStateInfo(StateType.AfterHeal);
                var skillInfo = new SkillInfo(afterSkillInfo.Id);
                var actionInfo = MakeActionInfo(firstActionBattler,skillInfo,false,false);
                
                if (actionInfo != null)
                {
                    var party = firstActionBattler.IsActor ? _party.AliveBattlerInfos : _troop.AliveBattlerInfos;
                    var targetIndexes = new List<int>();
                    foreach (var member in party)
                    {
                        if (firstActionBattler.Index != member.Index)
                        {
                            targetIndexes.Add(member.Index);
                        }   
                    }
                    foreach (var targetIndex in targetIndexes)
                    {
                        var featureData = new SkillData.FeatureData
                        {
                            FeatureType = FeatureType.HpHeal,
                            Param1 = stateInfo.Effect
                        };

                        var actionResultInfo = new ActionResultInfo(GetBattlerInfo(targetIndex),GetBattlerInfo(targetIndex),new List<SkillData.FeatureData>(){featureData},-1);
                        afterHealResults.Add(actionResultInfo);
                    }
                }
            }
            return afterHealResults;
        }

        private List<ActionResultInfo> AssistHealActionResults()
        {
            var assistHealResults = new List<ActionResultInfo>();
            if (_currentBattler == null)
            {
                return assistHealResults;
            }
            var afterSkillInfo = _currentBattler.Skills.Find(a => a.FeatureDates.Find(b => b.FeatureType == FeatureType.AddState && (StateType)b.Param1 == StateType.AssistHeal) != null);
            if (_currentBattler.IsState(StateType.AssistHeal) && afterSkillInfo != null)
            {
                var stateInfo = _currentBattler.GetStateInfo(StateType.AssistHeal);
                var skillInfo = new SkillInfo(afterSkillInfo.Id);
                var actionInfo = MakeActionInfo(_currentBattler,skillInfo,false,false);
                
                if (actionInfo != null)
                {
                    var party = _currentBattler.IsActor ? _party.AliveBattlerInfos : _troop.AliveBattlerInfos;
                    party = party.FindAll(a => a.IsAlive());
                    var targetIndexes = new List<int>();
                    foreach (var member in party)
                    {
                        targetIndexes.Add(member.Index);
                    }
                    var healValue = actionInfo.ActionResults.FindAll(a => a.HpDamage > 0).Count;
                    foreach (var targetIndex in targetIndexes)
                    {
                        var featureData = new SkillData.FeatureData
                        {
                            FeatureType = FeatureType.HpHeal,
                            Param1 = healValue * stateInfo.Effect
                        };

                        var actionResultInfo = new ActionResultInfo(GetBattlerInfo(targetIndex),GetBattlerInfo(targetIndex),new List<SkillData.FeatureData>(){featureData},-1);
                        assistHealResults.Add(actionResultInfo);
                    }
                }
            }
            return assistHealResults;
        }

        public List<ActionResultInfo> CheckSlipDamage()
        {
            var firstActionBattler = _firstActionBattler;
            var actionResultInfos = new List<ActionResultInfo>();
            var slipDamage = firstActionBattler.SlipDamage();
            if (slipDamage > 0)
            {
                var featureData = new SkillData.FeatureData
                {
                    FeatureType = FeatureType.HpSlipDamage,
                    Param1 = slipDamage
                };
                var actionResultInfo = new ActionResultInfo(firstActionBattler,firstActionBattler,new List<SkillData.FeatureData>(){featureData},-1);
                actionResultInfos.Add(actionResultInfo);
            }
            return actionResultInfos;
        }

        // リザルトから発生するトリガースキルを生成
        public List<ActionInfo> CheckTriggerActiveInfos(TriggerTiming triggerTiming,ActionInfo actionInfo,List<ActionResultInfo> actionResultInfos,bool makeResult = false)
        {
            var madeActionInfos = new List<ActionInfo>();
            var actionInfos = new List<ActionInfo>();
            var triggeredSkills = new List<SkillInfo>();
            foreach (var battlers in _battlers)
            {
                var checkBattler = battlers;
                triggeredSkills.Clear();
                foreach (var skillInfo in checkBattler.ActiveSkills())
                {
                    if (UsedTurnSameActionInfo(skillInfo,checkBattler.Index))
                    {
                        continue;
                    }
                    if (actionInfo == null || (actionInfo.Master.Id != skillInfo.Id))
                    {
                        if (skillInfo != null && skillInfo.Master != null)
                        {
                            var triggerDates = skillInfo.Master.TriggerDates?.FindAll(a => a.TriggerTiming == triggerTiming);
                            if (IsTriggeredSkillInfo(checkBattler,triggerDates,actionInfo,actionResultInfos))
                            {
                                triggeredSkills.Add(skillInfo);
                            }
                        }
                    }
                }
                if (triggeredSkills.Count > 0)
                {
                    foreach (var triggeredSkill in triggeredSkills)
                    {
                        var IsInterrupt = triggerTiming == TriggerTiming.Interrupt || triggerTiming == TriggerTiming.BeforeSelfUse || triggerTiming == TriggerTiming.BeforeOpponentUse || triggerTiming == TriggerTiming.BeforeFriendUse;
                        if (triggeredSkill.Master.SkillType == SkillType.Unique && checkBattler.IsAwaken == false)
                        {
                            checkBattler.SetAwaken();
                        }
                        var makeActionInfo = MakeActionInfo(checkBattler,triggeredSkill,IsInterrupt,true);
                        if (makeResult)
                        {
                            var counterSubjectIndex = actionInfo != null ? actionInfo.SubjectIndex : -1;
                            var selectIndexList = MakeAutoSelectIndex(makeActionInfo,-1,counterSubjectIndex);
                            if (selectIndexList.Count == 0 && triggeredSkill.Master.TargetType == TargetType.IsTriggerTarget)
                            {
                                var triggerDates = triggeredSkill.Master.TriggerDates.FindAll(a => a.TriggerTiming == triggerTiming);
                                selectIndexList = TriggerTargetList(checkBattler,triggerDates[0],actionInfo,actionResultInfos,makeActionInfo.Master.AliveType);
                            }
                            if (selectIndexList.Count == 0)
                            {
                                continue;
                            }
                            SetActionInfoParameter(makeActionInfo);
                            MakeActionResultInfo(makeActionInfo,ActionInfoTargetIndexes(makeActionInfo,selectIndexList[0],counterSubjectIndex,actionInfo,actionResultInfos));
                            AddActionInfo(makeActionInfo,IsInterrupt);
                        }
                        
                        madeActionInfos.Add(makeActionInfo);
                    }
                }
            }

            return madeActionInfos;
        }

        public void CheckTriggerPassiveInfos(List<TriggerTiming> triggerTimings,ActionInfo actionInfo = null, List<ActionResultInfo> actionResultInfos = null)
        {
            // 同時発動制限管理
            var checkedSkillIds = new List<int>();
            foreach (var battlerInfo in _battlers)
            {
                if (battlerInfo.IsState(StateType.NoPassive))
                {
                    continue;
                }
                foreach (var passiveInfo in battlerInfo.PassiveSkills())
                {
                    var triggerDates = passiveInfo.Master.TriggerDates.FindAll(a => triggerTimings.Contains(a.TriggerTiming));
                    
                    // バトル中〇回以下使用
                    var inBattleUseCountUnder = triggerDates.Find(a => a.TriggerType == TriggerType.InBattleUseCountUnder);
                    if (inBattleUseCountUnder != null)
                    {
                        if (inBattleUseCountUnder.Param1 <= passiveInfo.UseCount)
                        {
                            continue;
                        }
                    }
                    // ターン中〇回以下使用
                    var inTurnUseCountUnder = triggerDates.Find(a => a.TriggerType == TriggerType.InTurnUseCountUnder);
                    if (inTurnUseCountUnder != null)
                    {
                        if (inTurnUseCountUnder.Param1 <= UsedSameTurnActionInfo(passiveInfo))
                        {
                            continue;
                        }
                    }
                    if (_passiveSkillInfos[battlerInfo.Index].Contains(passiveInfo.Id))
                    {
                        continue;
                    }
                    if (passiveInfo.CountTurn > 0)
                    {
                        continue;
                    }
                    if (passiveInfo.Master.TimingOnlyCount > 0)
                    {
                        if (passiveInfo.Master.TimingOnlyCount <= UsedSameTurnActionInfo(passiveInfo))
                        {
                            continue;
                        }
                    }
                    if (IsTriggeredSkillInfo(battlerInfo,triggerDates,actionInfo,actionResultInfos))
                    {
                        //bool usable = CanUsePassiveCount(battlerInfo,passiveInfo.Id,triggerDates);
                        // 元の条件が成立
                        // 作戦で可否判定
                        var selectSkill = -1;
                        var selectTarget = -1;
                        var skillTriggerInfos = battlerInfo.SkillTriggerInfos;
                        var sameSkillTriggerInfo = skillTriggerInfos.Find(a => a.SkillId == passiveInfo.Id);
                        if (sameSkillTriggerInfo != null)
                        {
                            (selectSkill,selectTarget) = SelectSkillTargetBySkillTriggerDates(battlerInfo,new List<SkillTriggerInfo>(){sameSkillTriggerInfo},actionInfo,actionResultInfos);
                            if (selectSkill != passiveInfo.Id)
                            {
                                continue;
                            }
                            if (selectTarget == -1)
                            {
                                continue;
                            }
                        } else
                        {
                            // 制限なしトリガー
                            var skillTriggerInfo = new SkillTriggerInfo(battlerInfo.Index,passiveInfo);
                            skillTriggerInfo.UpdateTriggerDates(new List<SkillTriggerData>());
                            // 作戦に縛りがなければ使える
                            (selectSkill,selectTarget) = SelectSkillTargetBySkillTriggerDates(battlerInfo,new List<SkillTriggerInfo>(){skillTriggerInfo},actionInfo,actionResultInfos);
                            if (selectSkill != passiveInfo.Id)
                            {
                                continue;
                            }
                            if (selectTarget == -1)
                            {
                                continue;
                            }
                        }
                        var IsInterrupt = triggerDates[0].TriggerTiming == TriggerTiming.Interrupt || triggerDates[0].TriggerTiming == TriggerTiming.BeforeSelfUse || triggerDates[0].TriggerTiming == TriggerTiming.BeforeOpponentUse || triggerDates[0].TriggerTiming == TriggerTiming.BeforeFriendUse;
                        var result = MakePassiveSkillActionResults(battlerInfo,passiveInfo,IsInterrupt,selectTarget,actionInfo,actionResultInfos,triggerDates[0]);
                        if (result != null && result.ActionResults.Count > 0)
                        {
                            checkedSkillIds.Add(passiveInfo.Id);
                            // 継続パッシブは保存
                            var addPassive = passiveInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.AddState);
                            if (addPassive != null && addPassive.Param2 == 999)
                            {
                                var stateData = DataSystem.FindState(addPassive.Param1);
                                if (stateData.OverLap == 0)
                                {
                                    _passiveSkillInfos[battlerInfo.Index].Add(passiveInfo.Id);
                                } else
                                {
                                    var overLapCount = battlerInfo.GetStateInfoAll(stateData.StateType).Count;
                                    if (stateData.OverLap-1 <= overLapCount)
                                    {
                                        _passiveSkillInfos[battlerInfo.Index].Add(passiveInfo.Id);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private ActionInfo MakePassiveSkillActionResults(BattlerInfo battlerInfo,SkillInfo passiveInfo,bool IsInterrupt,int selectIndex,ActionInfo actionInfo = null,List<ActionResultInfo> actionResultInfos = null,SkillData.TriggerData triggerData = null)
        {
            if (!CheckCanUsePassive(passiveInfo,battlerInfo))
            {
                return null;
            }
            
            if (UsedTurnSameActionInfo(passiveInfo,battlerInfo.Index))
            {
                return null;
            }
            var makeActionInfo = MakeActionInfo(battlerInfo,passiveInfo,IsInterrupt,true);
            var counterSubjectIndex = actionInfo != null ? actionInfo.SubjectIndex : -1;
            if (selectIndex == -1)
            {
                // 対象を再取得
                var selectIndexList = MakeAutoSelectIndex(makeActionInfo,-1,counterSubjectIndex,actionInfo,actionResultInfos);
                if (selectIndexList.Count == 0 && passiveInfo.Master.TargetType == TargetType.IsTriggerTarget)
                {
                    selectIndexList = TriggerTargetList(battlerInfo,triggerData,actionInfo,actionResultInfos,makeActionInfo.Master.AliveType);
                }
                if (selectIndexList.Count == 0)
                {
                    return null;
                }
                selectIndex = selectIndexList[0];
            }
            SetActionInfoParameter(makeActionInfo);
            MakeActionResultInfo(makeActionInfo,ActionInfoTargetIndexes(makeActionInfo,selectIndex,counterSubjectIndex,actionInfo,actionResultInfos));
            AddActionInfo(makeActionInfo,IsInterrupt);
            passiveInfo.GainUseCount();
            passiveInfo.SetCountTurn(passiveInfo.Master.CountTurn);
            return makeActionInfo;
        }
        
        public List<ActionResultInfo> CheckRemovePassiveInfos()
        {
            var actionResultInfos = new List<ActionResultInfo>();
            foreach (var battlerInfo in _battlers)
            {
                var passiveSkillIds = _passiveSkillInfos[battlerInfo.Index];
                for (int i = 0;i < passiveSkillIds.Count;i++)
                {
                    var passiveSkillData = DataSystem.FindSkill(passiveSkillIds[i]);
                    bool IsRemove = false;
                    
                    foreach (var feature in passiveSkillData.FeatureDates)
                    {
                        if (feature.FeatureType == FeatureType.AddState)
                        {
                            var triggerDates = passiveSkillData.TriggerDates.FindAll(a => a.TriggerTiming == TriggerTiming.After || a.TriggerTiming == TriggerTiming.StartBattle || a.TriggerTiming == TriggerTiming.AfterAndStartBattle);
                            if (IsRemove == false && triggerDates.Count > 0 && !IsTriggeredSkillInfo(battlerInfo,triggerDates,null,new List<ActionResultInfo>()))
                            {
                                IsRemove = true;
                                var featureData = new SkillData.FeatureData
                                {
                                    FeatureType = FeatureType.RemoveStatePassive,
                                    Param1 = feature.Param1
                                };
                                if (passiveSkillData.Scope == ScopeType.Self)
                                {
                                    var actionResultInfo = new ActionResultInfo(battlerInfo,battlerInfo,new List<SkillData.FeatureData>(){featureData},passiveSkillData.Id);
                                    if (actionResultInfos.Find(a => a.RemovedStates.Find(b => b.Master.StateType == (StateType)featureData.FeatureType) != null) != null)
                                    {
                                        
                                    } else
                                    {
                                        var stateInfos = battlerInfo.GetStateInfoAll((StateType)feature.Param1);
                                        if (battlerInfo.IsAlive() && stateInfos.Find(a => a.SkillId == passiveSkillData.Id) != null)
                                        {
                                            actionResultInfos.Add(actionResultInfo);
                                        }
                                    }
                                } else
                                if (passiveSkillData.Scope == ScopeType.All)
                                {
                                    var partyMember = battlerInfo.IsActor ? BattlerActors() : BattlerEnemies();
                                    
                                    switch (passiveSkillData.AliveType)
                                    {
                                        case AliveType.DeathOnly:
                                            partyMember = partyMember.FindAll(a => !a.IsAlive());
                                            break;
                                        case AliveType.AliveOnly:                        
                                            partyMember = partyMember.FindAll(a => a.IsAlive());
                                            break;
                                        case AliveType.All:
                                            break;
                                    }
                                    foreach (var member in partyMember)
                                    {
                                        var actionResultInfo = new ActionResultInfo(battlerInfo,member,new List<SkillData.FeatureData>(){featureData},passiveSkillData.Id);
                                        if (actionResultInfos.Find(a => a.RemovedStates.Find(b => b.Master.StateType == (StateType)featureData.FeatureType) != null) != null)
                                        {
                                            
                                        } else
                                        {
                                            var stateInfos = battlerInfo.GetStateInfoAll((StateType)feature.Param1);
                                            if (member.IsAlive() && stateInfos.Find(a => a.SkillId == passiveSkillData.Id) != null)
                                            {
                                                actionResultInfos.Add(actionResultInfo);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return actionResultInfos;
        }

        private bool IsTriggeredSkillInfo(BattlerInfo battlerInfo,List<SkillData.TriggerData> triggerDates,ActionInfo actionInfo,List<ActionResultInfo> actionResultInfos)
        {
            var friends = battlerInfo.IsActor ? _party : _troop;            
            var opponents = battlerInfo.IsActor ? _troop : _party;
            bool IsTriggered = false;
            var checkTriggerInfo = new CheckTriggerInfo(_turnCount,battlerInfo,BattlerActors(),BattlerEnemies(),actionInfo,actionResultInfos);
            if (triggerDates.Count > 0)
            {
                foreach (var triggerData in triggerDates)
                {
                    // 自身の行動前判定
                    if (triggerData.TriggerTiming == TriggerTiming.BeforeSelfUse)
                    {
                        if (battlerInfo.Index != actionInfo.SubjectIndex)
                        {
                            continue;
                        }
                    }
                    // 相手の行動前判定
                    if (triggerData.TriggerTiming == TriggerTiming.BeforeOpponentUse)
                    {
                        if (battlerInfo.Index == actionInfo.SubjectIndex)
                        {
                            continue;
                        }
                        if (battlerInfo.IsActor == GetBattlerInfo(actionInfo.SubjectIndex).IsActor)
                        {
                            continue;
                        }
                    }
                    // 味方の行動前判定
                    if (triggerData.TriggerTiming == TriggerTiming.BeforeFriendUse)
                    {
                        if (battlerInfo.Index == actionInfo.SubjectIndex)
                        {
                            continue;
                        }
                        if (battlerInfo.IsActor != GetBattlerInfo(actionInfo.SubjectIndex).IsActor)
                        {
                            continue;
                        }
                    }
                    /*
                    if (triggerTiming == TriggerTiming.Use)
                    {
                        if (actionInfo == null)
                        {
                            IsTriggered = false;
                            break;
                        }
                        if (actionInfo != null && actionInfo.SubjectIndex != battlerInfo.Index)
                        {
                            IsTriggered = false;
                            break;
                        }
                    }
                    */
                    var key = (int)triggerData.TriggerType / 1000;
                    if (_checkTriggerDict.ContainsKey(key))
                    {
                        var checkTrigger = _checkTriggerDict[key];
                        IsTriggered = checkTrigger.CheckTrigger(triggerData,battlerInfo,checkTriggerInfo);
                    } else
                    {
                        // 個別判定
                        switch (triggerData.TriggerType)
                        {
                            case TriggerType.None:
                            case TriggerType.ExtendStageTurn: // 別処理で判定するためここではパス
                                IsTriggered = true;
                            break;
                            case TriggerType.SelfActionInfo:
                            if (battlerInfo.IsAlive() && actionInfo != null)
                            {
                                if (actionInfo.SubjectIndex == battlerInfo.Index)
                                {
                                    IsTriggered = true;
                                }
                            }
                            break;
                            case TriggerType.IsFriendBattler:
                            if (battlerInfo.IsAlive())
                            {
                                if (actionResultInfos.Find(a => friends.AliveBattlerInfos.Find(b => GetBattlerInfo(a.TargetIndex).IsActor == battlerInfo.IsActor) != null) != null)
                                {
                                    IsTriggered = true;
                                }
                            }
                            break;
                            case TriggerType.IsOpponentBattler:
                            if (battlerInfo.IsAlive())
                            {
                                if (actionResultInfos.Find(a => opponents.AliveBattlerInfos.Find(b => GetBattlerInfo(a.TargetIndex).IsActor != battlerInfo.IsActor) != null) != null)
                                {
                                    IsTriggered = true;
                                }
                            }
                            break;
                            case TriggerType.ActionResultDeath:
                            if (battlerInfo.IsAlive())
                            {
                                if (actionResultInfos.Find(a => opponents.AliveBattlerInfos.Find(b => a.DeadIndexList.Contains(b.Index)) != null) != null)
                                {
                                    IsTriggered = true;
                                }
                            }
                            break;
                            case TriggerType.DeadWithoutSelf:
                            var dWithoutSelfUnit = battlerInfo.IsActor ? _party : _troop;
                            int aliveCount = dWithoutSelfUnit.AliveBattlerInfos.Count;
                            if (battlerInfo.IsAlive() && aliveCount == 1)
                            {
                                IsTriggered = true;
                            }
                            break;
                            case TriggerType.SelfDead:
                            if (actionResultInfos.Find(a => a.DeadIndexList.Contains(battlerInfo.Index)) != null)
                            {
                                IsTriggered = true;
                                var stateInfos = battlerInfo.GetStateInfoAll(StateType.Death);
                                for (var i = 0;i < stateInfos.Count;i++)
                                {
                                    battlerInfo.RemoveState(stateInfos[i],true);
                                    battlerInfo.SetPreserveAlive(true);
                                }
                            }
                            break;
                            case TriggerType.AllEnemyCurseState:
                            /*
                            if (battlerInfo.IsAlive() && opponents.AliveBattlerInfos.Find(a => !a.IsState(StateType.DeBuffUpper)) == null && opponents.AliveBattlerInfos.FindAll(a => a.IsAlive()).Count > 0)
                            {
                                IsTriggered = true;
                            }
                            */
                            break;
                            case TriggerType.AllEnemyFreezeState:
                            if (battlerInfo.IsAlive() && opponents.AliveBattlerInfos.Find(a => !a.IsState(StateType.Freeze)) == null && opponents.AliveBattlerInfos.FindAll(a => a.IsAlive()).Count > 0)
                            {
                                IsTriggered = true;
                            }
                            break;
                            case TriggerType.DemigodMemberCount:
                            if (battlerInfo.IsAlive())
                            {
                                var demigodMember = opponents.AliveBattlerInfos.FindAll(a => a.IsState(StateType.Demigod));
                                if (demigodMember.Count >= triggerData.Param1)
                                {
                                    IsTriggered = true;
                                }
                            }
                            break;
                            case TriggerType.ActionResultAddState:
                            if (battlerInfo.IsAlive())
                            {
                                if (actionInfo != null && battlerInfo.IsActor != GetBattlerInfo(actionInfo.SubjectIndex).IsActor)
                                {
                                    var states = actionInfo.SkillInfo.FeatureDates.FindAll(a => a.FeatureType == FeatureType.AddState);
                                    foreach (var state in states)
                                    {
                                        if (state.Param1 == (int)StateType.Stun || state.Param1 == (int)StateType.BurnDamage || state.Param1 == (int)StateType.Freeze)
                                        {
                                            IsTriggered = true;
                                        }
                                    }
                                }
                            }
                            break;
                            case TriggerType.DefeatEnemyByAttack:
                            if (actionInfo != null && actionResultInfos != null)
                            {
                                var attackBattler = GetBattlerInfo(actionInfo.SubjectIndex);
                                if (battlerInfo.IsAlive() && attackBattler != null && battlerInfo.Index == attackBattler.Index)
                                {
                                    foreach (var actionResultInfo in actionResultInfos)
                                    {
                                        foreach (var deadIndex in actionResultInfo.DeadIndexList)
                                        {
                                            if (battlerInfo.IsActor != GetBattlerInfo(deadIndex).IsActor)
                                            {
                                                IsTriggered = true;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                            case TriggerType.AwakenDemigodAttribute:
                            var DemigodAttributes = friends.AliveBattlerInfos.FindAll(a => a.IsAwaken);
                            if (battlerInfo.IsAlive() && DemigodAttributes.Count > 0 && DemigodAttributes.Find(a => a.Skills.Find(b => b.Attribute == (AttributeType)triggerData.Param1 && b.Master.SkillType == SkillType.Unique) != null) != null)
                            {
                                IsTriggered = true;
                            }
                            break;
                            case TriggerType.ActionResultSelfDeath:
                            if (battlerInfo.IsAlive())
                            {
                                if (actionResultInfos.Find(a => a.DeadIndexList.Contains(battlerInfo.Index)) != null)
                                {
                                    IsTriggered = true;
                                }
                            }
                            break;
                            case TriggerType.InterruptAttackDodge:
                            if (battlerInfo.IsAlive())
                            {
                                if (actionInfo != null && battlerInfo.IsActor != GetBattlerInfo(actionInfo.SubjectIndex).IsActor)
                                {
                                    foreach (var actionResultInfo in actionInfo.ActionResults)
                                    {
                                        if (actionResultInfo.TargetIndex == battlerInfo.Index)
                                        {
                                            if (actionResultInfo.Missed)
                                            {
                                                IsTriggered = true;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }
                    // Param3をAnd条件フラグにする
                    if (triggerData.Param3 == 1)
                    {
                        if (IsTriggered)
                        {
                            IsTriggered = false;
                        } else
                        {
                            IsTriggered = false;
                            break;
                        }
                    }
                }
            }
            return IsTriggered;
        }

        private List<int> TriggerTargetList(BattlerInfo battlerInfo,SkillData.TriggerData triggerData,ActionInfo actionInfo, List<ActionResultInfo> actionResultInfos,AliveType aliveType)
        {
            var list = new List<int>();
            var opponents = battlerInfo.IsActor ? _troop : _party;
            var friends = battlerInfo.IsActor ? _party : _troop;
            var key = (int)triggerData.TriggerType / 1000;
            if (_checkTriggerDict.ContainsKey(key))
            {
                var checkTriggerInfo = new CheckTriggerInfo(_turnCount,battlerInfo,BattlerActors(),BattlerEnemies(),actionInfo,actionResultInfos);
                var checkTrigger = _checkTriggerDict[key];
                checkTrigger.AddTriggerTargetList(list,triggerData,battlerInfo,checkTriggerInfo);
            }
            switch (triggerData.TriggerType)
            {
                case TriggerType.ActionResultDeath:
                if (battlerInfo.IsAlive())
                {
                    var deathTarget = actionResultInfos.Find(a => friends.AliveBattlerInfos.Find(b => a.DeadIndexList.Contains(b.Index)) != null);
                    if (deathTarget != null)
                    {
                        var targetBattlerInfo = GetBattlerInfo(deathTarget.TargetIndex);
                        list.Add(targetBattlerInfo.Index);
                    }
                }
                break;
            }
            if (actionInfo != null)
            {
                list = CalcAliveTypeIndexList(list,aliveType);
            }
            return list;
        }

        private bool CanUseTrigger(SkillInfo skillInfo,BattlerInfo battlerInfo)
        {
            bool CanUse = true;
            if (skillInfo.TriggerDates.Count > 0)
            {
                CanUse = IsTriggeredSkillInfo(battlerInfo,skillInfo.TriggerDates,null,new List<ActionResultInfo>());
            }
            return CanUse;
        }

        private List<BattlerInfo> LineTargetBattlers(ScopeType scopeType,BattlerInfo targetBattler,List<BattlerInfo> targetBatterInfos)
        {
            var fronts = targetBatterInfos.FindAll(a => a.LineIndex == LineType.Front);
            var backs = targetBatterInfos.FindAll(a => a.LineIndex == LineType.Back);
            // この時点で有効なtargetIndexesが判定されているので人数で判定
            var lineTargets = new List<BattlerInfo>(){targetBattler};
            if (scopeType == ScopeType.Line)
            {
                lineTargets = targetBattler.LineIndex == LineType.Front ? fronts : backs;
            } else
            if (scopeType == ScopeType.All)
            {
                lineTargets = targetBatterInfos;
            } else
            if (scopeType == ScopeType.FrontLine)
            {
                lineTargets = fronts;
            } else
            if (scopeType == ScopeType.WithoutSelfAll)
            {
                lineTargets = targetBatterInfos;
                lineTargets.Remove(targetBattler);
            }
            return lineTargets;
        }

        public List<int> MakeAutoSelectIndex(ActionInfo actionInfo,int oneTargetIndex = -1,int counterSubjectIndex = -1,ActionInfo baseActionInfo = null,List<ActionResultInfo> baseActionResultInfos = null)
        {
            var indexList = new List<int>();
            // interruptされた行動の対象を引き継ぐ
            if (actionInfo.ActionResults.Count > 0)
            {
                foreach (var actionResultInfo in actionInfo.ActionResults)
                {
                    if (actionResultInfo.CursedDamage == false && !indexList.Contains(actionResultInfo.TargetIndex))
                    {
                        indexList.Add(actionResultInfo.TargetIndex);
                    } 
                }
                // かばう判定
                CalcCoverTargetIndexes(indexList,GetBattlerInfo(actionInfo.SubjectIndex).IsActor);
                return indexList;
            }
            var targetIndexList = GetSkillTargetIndexList(actionInfo.Master.Id,actionInfo.SubjectIndex,true,counterSubjectIndex,baseActionInfo,baseActionResultInfos);
            if (targetIndexList.Count == 0)
            {
                return targetIndexList;
            }
            var selectIndex = targetIndexList[0];
            if (oneTargetIndex > -1 && targetIndexList.Contains(oneTargetIndex))
            {
                selectIndex = oneTargetIndex;
            }
            return ActionInfoTargetIndexes(actionInfo,selectIndex,counterSubjectIndex,baseActionInfo,baseActionResultInfos);
        }

        private List<int> CalcAliveTypeIndexList(List<int> indexList,AliveType aliveType)
        {
            switch (aliveType)
            {
                case AliveType.DeathOnly:
                    indexList = indexList.FindAll(a => !_battlers.Find(b => a == b.Index).IsAlive());
                    break;
                case AliveType.AliveOnly:                        
                    indexList = indexList.FindAll(a => _battlers.Find(b => a == b.Index).IsAlive());
                    break;
                case AliveType.All:
                    break;
            }
            return indexList;
        }

        /// <summary>
        /// 繰り返し攻撃の時ターゲットを変える
        /// </summary>
        /// <param name="actionInfo"></param>
        public void ResetTargetIndexList(ActionInfo actionInfo)
        {
            var needReset = false;
            foreach (var targetIndex in actionInfo.CandidateTargetIndexList)
            {
                var target = GetBattlerInfo(targetIndex);
                if (!target.IsAlive() && actionInfo.Master.IsHpDamageFeature())
                {
                    needReset = true;
                }
            }
            if (needReset)
            {
                actionInfo.ActionResults.Clear();
                actionInfo.SetCandidateTargetIndexList(MakeAutoSelectIndex(actionInfo,-1,actionInfo.SubjectIndex));
            }
        }

        public (int,int) MakeAutoSkillTriggerSkillId(BattlerInfo battlerInfo)
        {
            var skillInfos = battlerInfo.ActiveSkills().FindAll(a => CheckCanUse(a,battlerInfo));
            
            // トリガーデータからスキル検索
            // 使用可能なものに絞る
            var skillTriggerInfos = battlerInfo.SkillTriggerInfos.FindAll(a => skillInfos.Find(b => b.Id == a.SkillId) != null);
            return SelectSkillTargetBySkillTriggerDates(battlerInfo,skillTriggerInfos);
        }

        public List<BattlerInfo> PreservedAliveEnemies()
        {
            var list = new List<BattlerInfo>();
            foreach (var battlerInfo in FieldBattlerInfos())
            {
                if (battlerInfo.PreserveAlive)
                {
                    list.Add(battlerInfo);
                    battlerInfo.SetPreserveAlive(false);
                }
            }
            return list;
        }

        public List<BattlerInfo> NotDeadMembers()
        {
            var list = new List<BattlerInfo>();
            foreach (var battlerInfo in FieldBattlerInfos())
            {
                if (battlerInfo.Hp > 0 && battlerInfo.IsState(StateType.Death))
                {
                    var states = battlerInfo.GetStateInfoAll(StateType.Death);
                    foreach (var state in states)
                    {                
                        battlerInfo.RemoveState(state,true);
                    }
                    list.Add(battlerInfo);
                }
            }
            return list;
        }

        // 戦闘不能の付与者のステート効果を解除する
        public List<StateInfo> EndRemoveState()
        {
            var removeStateInfos = new List<StateInfo>();
            foreach (var battler in FieldBattlerInfos())
            {
                if (battler.IsAlive() == false)
                {
                    for (var i = battler.StateInfos.Count-1;i >= 0;i--)
                    {
                        if (battler.StateInfos[i].Master.RemoveByDeath)
                        {
                            if (battler.StateInfos[i].IsStartPassive() == false)
                            {
                                removeStateInfos.Add(battler.StateInfos[i]);
                                battler.RemoveState(battler.StateInfos[i],true);
                            }
                        }
                    }
                }
            }
            return removeStateInfos;
        }

        // 戦闘不能に聖棺がいたら他の対象に移す
        public List<StateInfo> EndHolyCoffinState()
        {
            var addStateInfos = new List<StateInfo>();
            //var StateTypes = RemoveDeathStateTypes();
            foreach (var battler in FieldBattlerInfos())
            {
                if (battler.IsAlive() == false)
                {
                    for (var i = battler.StateInfos.Count-1;i >= 0;i--)
                    {
                        if (battler.StateInfos[i].StateType == StateType.HolyCoffin)
                        {
                            battler.RemoveState(battler.StateInfos[i],true);
                            var randTargets = battler.IsActor ? _party.AliveBattlerInfos : _troop.AliveBattlerInfos;
                            if (randTargets.Count > 0)
                            {
                                var rand = UnityEngine.Random.Range(0,randTargets.Count);
                                battler.StateInfos[i].SetTargetIndex(randTargets[rand].Index);
                                randTargets[rand].AddState(battler.StateInfos[i],true);
                                addStateInfos.Add(battler.StateInfos[i]);
                            }
                        }
                    }
                }
            }
            return addStateInfos;
        }
        
        // 味方に生存者がいなくなったら透明を外す
        public List<StateInfo> EndRemoveShadowState()
        {
            var units = new List<UnitInfo>();
            if (_party.AliveBattlerInfos.Count == 1)
            {
                units.Add(_party);
            }
            if (_troop.AliveBattlerInfos.Count == 1)
            {
                units.Add(_troop);
            }
            var removeStateInfos = new List<StateInfo>();
            foreach (var unit in units)
            {
                var aliveMember = unit.AliveBattlerInfos[0];
                if (aliveMember.IsState(StateType.Shadow))
                {
                    var shadowStates = aliveMember.GetStateInfoAll(StateType.Shadow);
                    for (int i = shadowStates.Count-1;i >= 0;i--)
                    {
                        removeStateInfos.Add(shadowStates[i]);
                        aliveMember.RemoveState(shadowStates[i],true);
                    }
                }
            }
            return removeStateInfos;
        }

        public void GainAttackedCount(int targetIndex)
        {
            GetBattlerInfo(targetIndex).GainAttackedCount(1);
        }

        public void GainBeCriticalCount(int targetIndex)
        {
            GetBattlerInfo(targetIndex).GainBeCriticalCount(1);
        }

        public void GainMaxDamage(int targetIndex,int damage)
        {
            GetBattlerInfo(targetIndex).GainMaxDamage(damage);
        }

        public bool CheckVictory()
        {
            bool isVictory = _troop.BattlerInfos.Find(a => a.IsAlive()) == null;
            return isVictory;
        }

        public int MakeBattleScore(bool isVictory,StrategySceneInfo strategySceneInfo)
        {
            if (isVictory)
            {
                var score = 100f;
                // ターン数の減点
                var turns = (5 * _troop.BattlerInfos.Count) - _turnCount;
                score += turns;
                score = Math.Max(0,score);
                score = Math.Min(100,score);
                // 与ダメージ - 被ダメージの加算
                //var attack = 0;
                //var damaged = 0;
                var remainHpPercent = 0f;
                var maxDamage = 0;
                var defeated = 0;
                var actorCount = 0;
                foreach (var battleRecord in _battleRecords)
                {
                    if (battleRecord.Key < 10)
                    {
                        var actorInfo = GetBattlerInfo(battleRecord.Key);
                        var actorMaxHp = actorInfo.MaxHp;
                        remainHpPercent += 1f - (float)(actorMaxHp - actorInfo.Hp) / actorMaxHp;
                        actorCount++;
                        if (battleRecord.Value.MaxDamage > maxDamage)
                        {
                            maxDamage = battleRecord.Value.MaxDamage;
                        }
                        if (!GetBattlerInfo(battleRecord.Key).IsAlive())
                        {
                            defeated += 1;
                        }
                    }
                }
                // 被ダメージ率の加算
                if (remainHpPercent > 0)
                {
                    score += (remainHpPercent/actorCount) * 100;
                }
                // 最大ダメージ値の加算
                if (maxDamage > 0)
                {
                    score += maxDamage / 10;
                }
                // 戦闘不能数の少なさで加算
                if (defeated == 0)
                {
                    score += 25;
                }
                strategySceneInfo.BattleRemainHpPercent = (int)((remainHpPercent/actorCount) * 100);
                strategySceneInfo.BattleMaxDamage = maxDamage;
                strategySceneInfo.BattleDefeatedCount = defeated;
                return (int)score;
            }
            return 0;
        }

        public List<GetItemInfo> MakeBattlerResult()
        {
            var list = new List<GetItemInfo>();
            list.AddRange(CurrentSelectRecord().SymbolInfo.GetItemInfos);
            return list;
        }

        public bool CheckDefeat()
        {
            bool isDefeat = _party.BattlerInfos.Find(a => a.IsAlive()) == null || _turnCount > 150;
            return isDefeat;
        }

        public bool EnableEscape()
        {
            return false;
        }

        public void EndBattle()
        {
            if (TempInfo.InReplay)
            {
                TempInfo.SetInReplay(false);
            } else
            {
                foreach (var battler in _party.BattlerInfos)
                {
                    var actorInfo = Actors().Find(a => a.ActorId == battler.CharaId);
                    actorInfo.ChangeHp(battler.MaxHp);
                    actorInfo.ChangeMp(battler.MaxMp);
                }
                foreach (var battlerInfo in _troop.BattlerInfos)
                {
                    battlerInfo.ResetData(battlerInfo.Level);
                }
                if (CurrentSelectRecord().SaveBattleReplayStage())
                {
                    var stageKey = CurrentStageKey();
                    var userId = CurrentData.PlayerInfo.UserId;
                    _saveBattleInfo.SetUserName(CurrentData.PlayerInfo.PlayerName);
                    _saveBattleInfo.SetVersion(GameSystem.Version);
                    SaveSystem.SaveReplay(stageKey,_saveBattleInfo);
                    FirebaseController.UploadReplayFile(stageKey,userId.ToString(),_saveBattleInfo);
                }
            }
            SaveSystem.SaveConfigStart(GameSystem.ConfigData);
        }

        public void AddEnemyInfoSkillId()
        {
            foreach (var battlerInfo in _battlers)
            {
                if (!battlerInfo.IsActor)
                {
                    foreach (var skillInfo in battlerInfo.Skills)
                    {
                        AddPlayerInfoSkillId(skillInfo.Id);
                    }
                }
            }
        }

        public List<SystemData.CommandData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var menuCommand = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(19700),
                Key = "Help"
            };
            list.Add(menuCommand);
            return list;
        }

        public SystemData.CommandData BattleAutoButton()
        {
            var menuCommand = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(706),
                Key = "BATTLE_AUTO"
            };
            return menuCommand;
        }

        public void ChangeBattleAuto()
        {
            ConfigUtility.ChangeBattleAuto(!GameSystem.ConfigData.BattleAuto);
        }

        public List<StateInfo> SelectCharacterConditions()
        {
            return _currentBattler.StateInfos;
        }

        public int WaitFrameTime(int time)
        {
            var waitFrame = GameSystem.ConfigData.BattleAnimationSkip ? 1 : time;
            return (int)(waitFrame / GameSystem.ConfigData.BattleSpeed);
        }

        public string BattleStartText()
        {
            var textId = 19611;
            if (CurrentSelectRecord().SymbolType == SymbolType.Boss)
            {
                textId = 19612;
            }
            return DataSystem.GetText(textId);
        }
        
    #if UNITY_EDITOR
        public List<TestActionData> testActionDates = new ();
        public int testActionIndex = 0;
        public void MakeTestBattleAction()
        {
            testActionDates.Clear();
            testActionDates = Resources.Load<TestBattleData>("Data/TestBattle").TestActionDates;
        }
        public BattlerInfo TestBattler()
        {
            if (testActionDates.Count > testActionIndex)
            {
                return GetBattlerInfo(testActionDates[testActionIndex].BattlerIndex);
            }
            return null;
        }
        
        public int TestSkillId()
        {
            if (testActionDates.Count > testActionIndex)
            {
                return testActionDates[testActionIndex].SkillId;
            }
            return 0;
        }

        public void SeekActionIndex()
        {
            testActionIndex++;
        }
    #endif
    }

    public class BattleSceneInfo
    {
        public List<ActorInfo> ActorInfos;
        public List<BattlerInfo> EnemyInfos;
    }
}