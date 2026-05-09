using UnityEngine;

namespace GodMachine.DamageSystem
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int _maxHP = 100;

        [Header("Regen")]
        [SerializeField] private bool _hasRegen = false;
        [SerializeField] private float _regenPerSecond = 0f;

        public int CurrentHP { get; private set; }
        public int MaxHP { get; private set; }
        public bool IsDead => CurrentHP <= 0;

        public event System.Action<int, int> OnHealthChanged;
        public event System.Action<int> OnDamageTaken;
        public event System.Action OnDeath;

        private void Awake()
        {
            SetMaxHP(_maxHP);
        }

        private void Update()
        {
            TickRegen();
        }

        public void SetMaxHP(int value)
        {
            _maxHP = value;
            MaxHP = value;
            CurrentHP = value;
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        }

        public void SetRegen(float perSecond)
        {
            _regenPerSecond = perSecond;
            _hasRegen = perSecond > 0f;
        }

        public void ApplyDamage(int amount)
        {
            if (IsDead) return;
            if (amount <= 0) return;

            CurrentHP -= amount;
            CurrentHP = Mathf.Max(CurrentHP, 0);

            OnHealthChanged?.Invoke(CurrentHP, MaxHP);
            OnDamageTaken?.Invoke(amount);

            if (CurrentHP == 0)
                Die();
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            if (amount <= 0) return;

            CurrentHP += amount;
            CurrentHP = Mathf.Min(CurrentHP, MaxHP);

            OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        }

        private void TickRegen()
        {
            if (!_hasRegen) return;
            if (IsDead) return;
            if (CurrentHP >= MaxHP) return;

            Heal(Mathf.RoundToInt(_regenPerSecond * Time.deltaTime));
        }

        private void Die()
        {
            OnDeath?.Invoke();
        }
    }
}