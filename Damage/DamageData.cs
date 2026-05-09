using UnityEngine;

namespace GodMachine.DamageSystem
{
    public readonly struct DamageData
    {
        public readonly int Amount;
        public readonly Component Source;

        public DamageData(int amount, Component source)
        {
            Amount = amount;
            Source = source;
        }
    }
}
