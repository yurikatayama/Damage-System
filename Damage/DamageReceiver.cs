using UnityEngine;

namespace GodMachine.DamageSystem
{
    public class DamageReceiver : MonoBehaviour, IDamageable
    {
        private Health _health;
        private Invulnerability _invulnerability;
        private IDefensable _defensable;

        public event System.Action<DamageResult> OnDamageResolved;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _invulnerability = GetComponent<Invulnerability>();
            _defensable = GetComponent<IDefensable>();
        }

        public void TakeDamage(DamageData damage)
        {
            if (_health == null) return;
            if (_health.IsDead) return;

            if (_invulnerability != null && _invulnerability.IsInvulnerable)
            {
                OnDamageResolved?.Invoke(new DamageResult(0, DamageHitState.Invulnerable, damage.Source));
                return;
            }

            int finalDamage = DamageResolver.Resolve(damage, _defensable);

            DamageHitState hitState = DamageHitState.Damaged;

            if (_defensable != null)
            {
                if (_defensable.IsParrying)
                    hitState = DamageHitState.Parried;
                else if (_defensable.IsBlocking)
                    hitState = DamageHitState.Blocked;
            }

            DamageResult result = new DamageResult(finalDamage, hitState, damage.Source);

            OnDamageResolved?.Invoke(result);

            _health.ApplyDamage(finalDamage);
        }
    }
}