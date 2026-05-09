namespace GodMachine.DamageSystem
{
    public static class DamageResolver
    {
        public static int Resolve(DamageData damage, IDefensable defense)
        {
            if (defense == null)
                return damage.Amount;

            if (defense.IsParrying)
                return 0;

            if (defense.IsBlocking)
            {
                float reduction = defense.DamageReduction / 100f;
                return UnityEngine.Mathf.Max(0, UnityEngine.Mathf.RoundToInt(damage.Amount * (1f - reduction)));
            }

            return damage.Amount;
        }
    }
}