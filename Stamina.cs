using UnityEngine;

namespace GodMachine.DamageSystem
{
    public class Stamina : MonoBehaviour
    {
        [SerializeField] private float _max = 100f;
        [SerializeField] private float _regenPerSecond = 15f;
        [SerializeField] private float _respiteDuration = 0.5f;

        public float Current { get; private set; }
        public float Max => _max;
        public bool IsExhausted => Current <= 0f;

        public event System.Action<float, float> OnStaminaChanged;

        private float _respiteTimer;
        private bool _regenPaused;

        private void Awake()
        {
            Current = _max;
        }

        private void Update()
        {
            TickRegen();
        }

        public void SetMax(float value)
        {
            _max = value;
            Current = Mathf.Min(Current, _max);
            OnStaminaChanged?.Invoke(Current, Max);
        }

        public void SetRegen(float perSecond)
        {
            _regenPerSecond = perSecond;
        }

        public void SetRespite(float seconds)
        {
            _respiteDuration = seconds;
        }

        public void Consume(float amount)
        {
            if (amount <= 0f) return;

            TriggerRespite();

            Current -= amount;
            Current = Mathf.Max(Current, 0f);

            OnStaminaChanged?.Invoke(Current, Max);
        }

        public void ConsumeOverTime(float amount)
        {
            if (amount <= 0f) return;

            TriggerRespite();

            Current -= amount * Time.deltaTime;
            Current = Mathf.Max(Current, 0f);

            OnStaminaChanged?.Invoke(Current, Max);
        }

        public void Recover(float amount)
        {
            if (amount <= 0f) return;
            if (Current >= Max) return;

            Current += amount;
            Current = Mathf.Min(Current, Max);

            OnStaminaChanged?.Invoke(Current, Max);
        }

        public void SetRegenPaused(bool paused)
        {
            _regenPaused = paused;
        }

        private void TriggerRespite()
        {
            _respiteTimer = Mathf.Max(_respiteTimer, _respiteDuration);
        }

        private void TickRegen()
        {
            if (_regenPaused) return;

            if (_respiteTimer > 0f)
            {
                _respiteTimer -= Time.deltaTime;
                return;
            }

            if (Current >= Max) return;

            RegenOverTime();
        }

        private void RegenOverTime()
        {
            Current += _regenPerSecond * Time.deltaTime;
            Current = Mathf.Min(Current, Max);

            OnStaminaChanged?.Invoke(Current, Max);
        }
    }
}