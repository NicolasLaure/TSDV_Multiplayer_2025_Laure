using System;
using UnityEngine;
using UnityEngine.Events;

namespace Health
{
    public class HealthPoints : MonoBehaviour, ITakeDamage
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int initHealth = 100;
        [SerializeField] private bool canTakeDamage = true;

        [Header("Internal events")]
        public Action onDeathEvent;
        [SerializeField] private UnityEvent<int> onTakeDamageEvent;

        public int MaxHealth
        {
            get { return maxHealth; }
        }

        private bool _isInvincible = false;
        private bool _hasBeenDead = false;

        public int CurrentHp { get; private set; }

        void Start()
        {
            CurrentHp = initHealth;
            _hasBeenDead = false;
        }

        private void OnEnable()
        {
            _hasBeenDead = false;
        }

        public void SetCanTakeDamage(bool value)
        {
            canTakeDamage = value;
        }

        // Is invincible =/= can take damage.
        // isInvincible is used for attacks That can be avoidable.
        // canTakeDamage is used if the entity just cant take damage in any way.
        public void SetIsInvincible(bool value)
        {
            _isInvincible = value;
        }

        public bool TryTakeDamage(int damage)
        {
            CurrentHp -= damage;

            if (IsDead() && !_hasBeenDead)
            {
                _hasBeenDead = true;
                onTakeDamageEvent?.Invoke(0);
                onDeathEvent?.Invoke();
            }
            else
                onTakeDamageEvent?.Invoke(CurrentHp);

            return true;
        }

        public bool IsDead()
        {
            return CurrentHp <= 0;
        }

        public void TryTakeAvoidableDamage(int damage)
        {
            if (_isInvincible) return;
            TryTakeDamage(damage);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void ToggleInvulnerability()
        {
            canTakeDamage = !canTakeDamage;
        }

        public void ToggleInvulnerability(bool value)
        {
            canTakeDamage = value;
        }
#endif
    }
}