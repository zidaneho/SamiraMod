using System;
using SamiraMod.Survivors.Samira.SkillStates;
using UnityEngine;

namespace SamiraMod.Survivors.Samira
{
    public static class SamiraStaticValues
    {
        #region Primary
        public const float flairBaseDamage = 17;
        public const float flairDamageMult = 1.3f;
        public const float flairDamageGrowthPerLevel = .4f;
        public const float flairMeleeBonusMultiplier = 0.1f;
        public const int attacksPerFlair = 5;
        public const int attacksPerExplosiveShot = 4;
        public const float explosiveShotBonusMultiplier = 4f;
        public const float flairUniqueBonusMultiplier = 0.4f;
        public const float slashBonusMultiplier = 0.2f;
        public const float slashBonusMS = 0.02f;
        public const float slashBonusAS = 0.01f;
        #endregion
        #region Secondary
        public const float bladeWhirlBaseDamage = 12f;
        public const float bladeWhirlBonusDamageMult = 0.15f;
        public const float bladeWhirlDamageGrowthPerLevel = .2f;
        public const float exposingWhirlDamageMult = 0.5f;
        public const float exposeDebuffArmorPen = 12f;
        public const float exposeDebuffDuration = 5f;
        public const float bladeWhirlDuration = 0.4f;
        public const float bladeWhirlRadius = 15f;
        #endregion
        #region Utility
        public const float wildRushBaseDamage = 15f;
        public const float wildRushDamageMult = .25f;
        public const float wildRushDamageGrowthPerLevel = 1;
        public const float wildRushAttackSpeedMult = 0.3f;
        public const float wildRushAttackSpeedDuration = 6f;
        #endregion
        #region Special
        public const float infernoTriggerBaseDamage = 17;
        public const float infernoTriggerDamageMult = 1.3f;
        public const float infernoTriggerAttackSpeedMult = 0.9f;
        public const float infernoTriggerDamageGrowthPerLevel = .4f; // Assuming a growth per level
        public const float infernoTriggerDuration = 3f;
        public const float infiniteRainDurationExtend = 0.25f;
        public const float infiniteRainDMGMultiplier = 1f;
        public const float infiniteRainASMultiplier = 0.7f;
        #endregion

        public const int flairID = 4;
        public const int bladeWhirlID = 2;
        public const int wildRushID = 3;
        public const int autoAttackID = 1;
        public const int flairDashID = 5;

        public static float GetFlairDamage(float damageStat, float currentLevel)
        {
            float damage = GetDamage(flairBaseDamage, flairDamageMult, flairDamageGrowthPerLevel, damageStat,
                currentLevel);
            
            return damage;
        }

        public static float GetBladeWhirlDamage(float damageStat, float currentLevel)
        {
            return GetDamage(bladeWhirlBaseDamage, bladeWhirlBonusDamageMult, bladeWhirlDamageGrowthPerLevel, damageStat, currentLevel);
        }
    
        public static float GetWildRushDamage(float damageStat, float currentLevel)
        {
            return GetDamage(wildRushBaseDamage, wildRushDamageMult, wildRushDamageGrowthPerLevel, damageStat, currentLevel);
        }

        public static float GetInfernoTriggerDamage(float damageStat, float currentLevel)
        {
            return GetDamage(infernoTriggerBaseDamage, infernoTriggerDamageMult, infernoTriggerDamageGrowthPerLevel, damageStat, currentLevel);
        }

        public static float GetInfiniteRainDamage(float damageStat, float currentLevel)
        {
            return GetDamage(infernoTriggerBaseDamage, infiniteRainDMGMultiplier, infernoTriggerDamageGrowthPerLevel, damageStat, currentLevel);
        }

        static float GetDamage(float baseDamage, float damageMultiplier, float levelMultiplier, float damageStat,
            float currentLevel)
        {
            float damage = baseDamage * (1 + (currentLevel - 1)) * levelMultiplier + damageStat * damageMultiplier;
            return damage;
        }
    }
    
}