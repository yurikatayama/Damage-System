namespace GodMachine.DamageSystem
{
    public interface IDefensable
    {
        bool IsParrying { get; }
        bool IsBlocking { get; }
        float DamageReduction { get; }
    }
}