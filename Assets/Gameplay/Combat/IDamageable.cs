namespace Cipher.Gameplay.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        void ApplyDamage(float amount, UnityEngine.GameObject source);
    }
}
