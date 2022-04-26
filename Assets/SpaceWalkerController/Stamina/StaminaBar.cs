using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Stamina
{
    public class StaminaBar : MonoBehaviour
    {
        private Stamina _stamina = new Stamina();
        private Slider _slider;

        public bool HasStamina => _stamina.NormalizedStamina > 0.05f;
        public bool HasStaminaToInitialLaunch => _stamina.NormalizedStamina > 0.2f;

        private float _rechargeTimer = 0f;

        public void UpdateStamina()
        {
            if (_rechargeTimer > 0f)
                _rechargeTimer -= Time.deltaTime;

            if (_rechargeTimer <= 0f)
            {
                _rechargeTimer = 0f;

                _stamina.Update();
            }
        }

        public void TrySpendStamina(float amount)
        {
            _stamina.TryConsumeStamina(amount);

            if (_stamina.CurrentStamina < 5)
            {
                _rechargeTimer = 2f;
            }
        }

        private void Start()
        {
            _slider = GetComponent<Slider>();
        }

        private void Update()
        {
            InternalUpdateSliderValue();
        }

        private void InternalUpdateSliderValue()
        {
            _slider.value = _stamina.NormalizedStamina;
        }
    }

}
