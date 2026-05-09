using UnityEngine;

namespace GodMachine.DamageSystem
{
    public class DamageDealer : MonoBehaviour
    {
        [SerializeField] private int _damage = 10;
        [SerializeField] private bool _destroyOnHit = false;

        public void SetDamage(int value)
        {
            _damage = value;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();

            if (damageable == null) return;

            damageable.TakeDamage(new DamageData(_damage, this));

            if (_destroyOnHit)
                Destroy(gameObject);
        }
    }
}