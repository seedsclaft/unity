using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class LearnSkillView : BaseView
    {
        [SerializeField] private TextMeshProUGUI evaluateText = null;
        [SerializeField] private TextMeshProUGUI afterEvaluateText = null;
        [SerializeField] private SkillInfoComponent skillInfoComponent = null;
        [SerializeField] private CanvasGroup canvasGroup = null;

        public override void Initialize() 
        {
            base.Initialize();
        }
        
        public void SetLearnSkillInfo(LearnSkillInfo learnSkillInfo)
        {
            evaluateText?.SetText(learnSkillInfo.From.ToString());
            afterEvaluateText?.SetText(learnSkillInfo.To.ToString());
            skillInfoComponent.UpdateSkillInfo(learnSkillInfo.SkillInfo);
        }


    }

    public class LearnSkillInfo
    {
        private int _from = 0;
        public int From => _from;
        private int _to = 0;
        public int To => _to;
        private SkillInfo _skillInfo;
        public SkillInfo SkillInfo => _skillInfo;
        public LearnSkillInfo(int from,int to,SkillInfo skillInfo)
        {
            _from = from;
            _to = to;
            _skillInfo = skillInfo;
        }

    }
}