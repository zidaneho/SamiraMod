using System;
using UnityEngine;

namespace SamiraMod.Survivors.Samira
{
    public static class SamiraStaticValues
    {
        public const float flairBaseDamage = 20;
        public const float flairDamageMult = 1.4f;
        public const float flairDamageGrowthPerLevel = .4f;
        public const float bladeWhirlBaseDamage = 10f;
        public const float bladeWhirlDamageMult = 0.8f;
        public const float bladeWhirlDamageGrowthPerLevel = .2f;
        public const float bladeWhirlDuration = 0.5f;
        public const float wildRushBaseDamage = 15f;
        public const float wildRushDamageMult = .7f;
        public const float wildRushDamageGrowthPerLevel = 1;
        public const float wildRushAttackSpeedMult = 0.3f;
        public const float wildRushAttackSpeedDuration = 4.5f;
        public const float infernoTriggerBaseDamage = 20;
        public const float infernoTriggerDamageMult = 1.5f;
        public const float infernoTriggerDamageGrowthPerLevel = .4f; // Assuming a growth per level
        public const float infernoTriggerDuration = 3f;

        public static float GetFlairDamage(float damageStat, float currentLevel)
        {
            return GetDamage(flairBaseDamage, flairDamageMult, flairDamageGrowthPerLevel, damageStat, currentLevel);
        }

        public static float GetBladeWhirlDamage(float damageStat, float currentLevel)
        {
            return GetDamage(bladeWhirlBaseDamage, bladeWhirlDamageMult, bladeWhirlDamageGrowthPerLevel, damageStat, currentLevel);
        }
    
        public static float GetWildRushDamage(float damageStat, float currentLevel)
        {
            return GetDamage(wildRushBaseDamage, wildRushDamageMult, wildRushDamageGrowthPerLevel, damageStat, currentLevel);
        }

        public static float GetInfernoTriggerDamage(float damageStat, float currentLevel)
        {
            return GetDamage(infernoTriggerBaseDamage, infernoTriggerDamageMult, infernoTriggerDamageGrowthPerLevel, damageStat, currentLevel);
        }

        static float GetDamage(float baseDamage, float damageMultiplier, float levelMultiplier, float damageStat,
            float currentLevel)
        {
            float damage = baseDamage * (1 + (currentLevel - 1)) * levelMultiplier + damageStat * damageMultiplier;
            return damage;
        }
    }
    
}