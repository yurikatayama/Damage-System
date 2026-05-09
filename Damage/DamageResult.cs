using UnityEngine;

namespace GodMachine.DamageSystem
{
    public readonly struct DamageResult
    {
        public readonly int FinalDamage;
        public readonly DamageHitState HitState;
        public readonly Component Source;

        public DamageResult(int finalDamage, DamageHitState hitState, Component source)
        {
            FinalDamage = finalDamage;
            HitState = hitState;
            Source = source;
        }
    }
}