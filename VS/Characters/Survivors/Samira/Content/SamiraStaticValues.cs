using System;
using SamiraMod.Survivors.Samira.SkillStates;
using UnityEngine;

namespace SamiraMod.Survivors.Samira
{
    public static class SamiraStaticValues
    {
        public const float flairBaseDamage = 17;
        public const float flairDamageMult = 1.3f;
        public const float flairDamageGrowthPerLevel = .4f;
        public const float flairMeleeBonusMultiplier = 0.1f;
        public const int attacksPerFlair = 5;
        public const float flairUniqueBonusMultiplier = 0.4f;
        public const float bladeWhirlBaseDamage = 10f;
        public const float bladeWhirlDamageMult = 0.8f;
        public const float bladeWhirlDamageGrowthPerLevel = .2f;
        public const float bladeWhirlDuration = 0.5f;
        public const float wildRushBaseDamage = 15f;
        public const float wildRushDamageMult = .7f;
        public const float wildRushDamageGrowthPerLevel = 1;
        public const float wildRushAttackSpeedMult = 0.3f;
        public const float wildRushAttackSpeedDuration = 3f;
        public const float infernoTriggerBaseDamage = 20;
        public const float infernoTriggerDamageMult = 1.6f;
        public const float infernoTriggerDamageGrowthPerLevel = .4f; // Assuming a growth per level
        public const float infernoTriggerDuration = 3f;

        public static float GetFlairDamage(float damageStat, float currentLevel, bool isMelee = false, int swingIndex = 0)
        {
            float damage = GetDamage(flairBaseDamage, flairDamageMult, flairDamageGrowthPerLevel, damageStat,
                currentLevel);

            float bonusMultiplier = 0f;

            if (swingIndex >= attacksPerFlair - 1)
            {
                bonusMultiplier += flairUniqueBonusMultiplier;
            }

            if (isMelee)
            {
                bonusMultiplier += flairMeleeBonusMultiplier;
            }

            damage += damage * bonusMultiplier;
            
            return damage;
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