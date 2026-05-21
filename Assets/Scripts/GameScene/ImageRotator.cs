using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameScene
{
    public class ImageRotator : MonoBehaviour
    {
        
        [SerializeField] private Image image;
        [SerializeField] private List<Sprite> sprites;
        [SerializeField] private float rotateInterval = 0.5f;

        private float _timer = 0;
        private float _index = 0;

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer > rotateInterval)
            {
                _timer = 0;
                _index++;
                if (_index >= sprites.Count)
                {
                    _index = 0;
                }
                image.sprite = sprites[(int)_index];
            }
        }
    }
}