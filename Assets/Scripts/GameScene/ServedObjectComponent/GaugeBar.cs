using System;
using GameScene.Dto;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.ServedObjectComponent
{
    public class GaugeBar : MonoBehaviour
    {
        [SerializeField] private ServedObject servedObject;
        private Slider slider;
        [SerializeField] private string targetCategory;

        private void Awake()
        {
            slider = GetComponentInChildren<Slider>();
        }

        private void OnEnable()
        {
            servedObject.OnGaugeChanged += OnGaugeUpdated;
        }

        private void OnDisable()
        {
            servedObject.OnGaugeChanged -= OnGaugeUpdated;
        }

        private void OnGaugeUpdated(Gauge gauge)
        {
            if (gauge.category.Equals(targetCategory))
            {
                slider.value = gauge.value;
                slider.maxValue = gauge.maxValue;
            }
        }
    }
}
