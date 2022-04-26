
using UnityEngine;

namespace Game.Stamina
{
    public class Stamina
    {
        private readonly float MaxStaminaAmount = 100f;

        private float _currentStamina = 0f;
        public float _staminaRegenRate = 20f;

        public float CurrentStamina { get { return _currentStamina; } }

        public Stamina()
        {
            _currentStamina = 0f;
        }

        public void Update()
        {
            _currentStamina += _staminaRegenRate * Time.deltaTime;
            _currentStamina = Mathf.Clamp(_currentStamina, 0f, MaxStaminaAmount);
        }

        public void TryConsumeStamina(float amount)
        {
            if (_currentStamina > 0f)
            {
                _currentStamina -= amount * Time.deltaTime;
            }
        }

        public float NormalizedStamina
        {
            get
            {
                return _currentStamina / MaxStaminaAmount;
            }
        }
    }

}
