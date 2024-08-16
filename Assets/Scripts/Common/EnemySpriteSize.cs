using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class EnemySpriteSize : MonoBehaviour
    {
        [SerializeField] private Image enemySprite;
        private float _width = -1;
        private float _height = -1;
        private string _textureName = null;

        private void Update() 
        {
            UpdateEnemy();
        }

        private void UpdateEnemy() 
        {
            if (enemySprite != null && enemySprite.mainTexture != null && _textureName != enemySprite.mainTexture.name)
            {
                if (_width == -1)
                {
                    var rect = enemySprite.GetComponent<RectTransform>();
                    _width = rect.sizeDelta.x;
                    _height = rect.sizeDelta.y;
                }
                UpdateSizeDelta();
                _textureName = enemySprite.mainTexture.name;
            }
        }

        public void UpdateSizeDelta()
        {
            float scaleX = _width / enemySprite.mainTexture.width;
            float scaleY = _height / enemySprite.mainTexture.height;
            float scale = scaleX < scaleY ? scaleX : scaleY;
            var width = scale * enemySprite.mainTexture.width;
            var height = scale * enemySprite.mainTexture.height;
            var objectRect = enemySprite.GetComponent<RectTransform>();
            objectRect.sizeDelta = new Vector2(width,height);
        }
    }
}
