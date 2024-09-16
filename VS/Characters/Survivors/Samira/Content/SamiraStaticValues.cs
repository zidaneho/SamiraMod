using System;
using SamiraMod.Survivors.Samira.SkillStates;
using UnityEngine;

namespace SamiraMod.Survivors.Samira
{
    public static class SamiraStaticValues
    {
        #region Passive

        public const float styleDuration = 4f;
        public const int barrageFireCount = 7;
        public const float delayBetweenShots = 0.1f;
        public const float barrageCooldownPerTarget = 1f;
        public const float barrageDamageMult = 0.2f;
        public const float barrageDamageGrowthPerLevel = 1f;
        #endregion
        #region Primary
        public const float flairDamageMult = 1.3f;
        public const float flairDamageGrowthPerLevel = .1f;
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
        public const float bladeWhirlDamageMult = 0.3f;
        public const float bladeWhirlDamageGrowthPerLevel = .2f;
        public const float exposingWhirlDamageMult = .1f;
        public const float exposeDebuffArmorPen = 12f;
        public const float exposeDebuffDuration = 5f;
        public const float bladeWhirlDuration = 0.4f;
        public const float bladeWhirlRadius = 15f;
        #endregion
        #region Utility
        public const float wildRushDamageMult = .25f;
        public const float wildRushDamageGrowthPerLevel = 1;
        public const float wildRushAttackSpeedMult = 0.3f;
        public const float wildRushAttackSpeedDuration = 6f;
        public const float quickStepsDamageMult = .10f;
        #endregion
        #region Special
        public const float infernoTriggerDamageMult = 1.3f;
        public const float infernoTriggerAttackSpeedMult = 0.9f;
        public const float infernoTriggerDamageGrowthPerLevel = .1f; // Assuming a growth per level
        public const float infernoTriggerDuration = 3f;
        public const float infiniteRainDurationExtend = 0.25f;
        public const float infiniteRainDMGMultiplier = 1f;
        public const float infiniteRainASMultiplier = 0.7f;
        #endregion
        
        public const int autoAttackID = 1;
        public const int bladeWhirlID = 2;
        public const int wildRushID = 3;
        public const int flairID = 4;
        public const int flairDashID = 5;
        public const int coinID = 6;
        public const int passiveID = 7;

        public static float GetBarrageDamage(float damageStat, float currentLevel)
        {
            float damage = GetDamage( barrageDamageMult, barrageDamageGrowthPerLevel, damageStat,
                currentLevel);
            
            return damage;
        }

        public static float GetFlairDamage(float damageStat, float currentLevel)
        {
            float damage = GetDamage( flairDamageMult, flairDamageGrowthPerLevel, damageStat,
                currentLevel);
            
            return damage;
        }

        public static float GetBladeWhirlDamage(float damageStat, float currentLevel)
        {
            return GetDamage( bladeWhirlDamageMult, bladeWhirlDamageGrowthPerLevel, damageStat, currentLevel);
        }

        public static float GetExposingWhirlDamage(float damageStat, float currentLevel)
        {
            return GetDamage( exposingWhirlDamageMult, bladeWhirlDamageGrowthPerLevel, damageStat, currentLevel);
        }
        public static float GetWildRushDamage(float damageStat, float currentLevel)
        {
            return GetDamage( wildRushDamageMult, wildRushDamageGrowthPerLevel, damageStat, currentLevel);
        }

        public static float GetQuickStepsDamage(float damageStat, float currentLevel)
        {
            return GetDamage( quickStepsDamageMult, wildRushDamageGrowthPerLevel, damageStat, currentLevel);
        }

        public static float GetInfernoTriggerDamage(float damageStat, float currentLevel)
        {
            return GetDamage( infernoTriggerDamageMult, infernoTriggerDamageGrowthPerLevel, damageStat, currentLevel);
        }

        public static float GetInfiniteRainDamage(float damageStat, float currentLevel)
        {
            return GetDamage(infiniteRainDMGMultiplier, infernoTriggerDamageGrowthPerLevel, damageStat, currentLevel);
        }

        static float GetDamage(float damageMultiplier, float levelMultiplier, float damageStat,
            float currentLevel)
        {
            float damage = (damageStat + GetLevelDamage(currentLevel, levelMultiplier)) * damageMultiplier;
            return damage;
        }

        static float GetLevelDamage(float currentLevel, float levelMultiplier)
        {
            return 1 + (currentLevel - 1) * levelMultiplier;
        }
    }
    
}