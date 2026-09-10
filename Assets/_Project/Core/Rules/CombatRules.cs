using Game.Core.Rules.Mathematics;

namespace Game.Core.Rules
{
    public static class CombatRules
    {
        public static float ApplyDamage(float currentHealth, float damageAmount)
        {
            return Math3D.Max(0, currentHealth - Math3D.Max(0, damageAmount));
        }

        public static bool IsDefeated(float currentHealth)
        {
            return currentHealth <= 0;
        } 
    }
}
