using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class GuideModel : BaseModel
    {
        private List<HelpData> _guideDates = null;
        private int _currentIndex = 0;
        public HelpData GuideData => _guideDates.Count > _currentIndex ? _guideDates[_currentIndex] : null;
        public void SetGuideDates(string guideKey)
        {
            _guideDates = DataSystem.Helps.FindAll(a => a.Key == guideKey);
        }

        public Sprite GuideSprite()
        {
            return ResourceSystem.LoadGuideSprite(GuideData?.GuideImagePath);
        }

        public List<ListData> GuideTextList()
        {
            return DataSystem.HelpText(GuideData.Id);
        }

        public bool NeedLeftPage()
        {
            return _currentIndex > 0;
        }

        public bool NeedRightPage()
        {
            return _currentIndex != _guideDates.Count-1 && _guideDates.Count != 1;
        }

        public void PageLeft()
        {
            _currentIndex--;
            _currentIndex = Math.Max(_currentIndex,0);
        }

        public void PageRight()
        {
            _currentIndex++;
            _currentIndex = Math.Min(_guideDates.Count-1,_currentIndex);
        }

        public int CallHelpId()
        {
            return GuideData.CommonHelpId;
        }
    }
}
