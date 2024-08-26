using System;
using SamiraMod.Modules;
using SamiraMod.Survivors.Samira.Achievements;

namespace SamiraMod.Survivors.Samira
{
    public static class SamiraTokens
    {
        public const string colorPrefix = "<color=#C23B22>";
        public const string damageColorPrefix = "<color=#FFD700>";
        public const string levelColorPrefix = "<color=#F5F5F5>";
        public const string characterLore = 
            "After her Shuriman home was destroyed as a child, Samira found her true calling in Noxus, where she built a reputation as a stylish daredevil taking on dangerous missions of the highest caliber. Wielding black-powder pistols and a custom-engineered blade, Samira thrives in life-or-death circumstances, eliminating any who stand in her way with flash and flair.";

        public const string characterName = "Samira";
        public static void Init()
        {
            AddSamiraTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Henry.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddSamiraTokens()
        {
            string prefix = SamiraSurvivor.SAMIRA_PREFIX;

            string desc = "Samira is a melee and ranged marksman who specializes in decimating enemies in style.>" + Environment.NewLine + Environment.NewLine
             + "< ! > Flair is a comfort button to press for consistent damage and combo stacking." + Environment.NewLine + Environment.NewLine
             + "< ! > Blade Whirl is a great skill for blocking all damage. Pair usage with Wild Rush to achieve more combo points." + Environment.NewLine + Environment.NewLine
             + "< ! > Wild Rush is Samira's main method of transportation. Play aggressive to kill enemies and reset the cooldown." + Environment.NewLine + Environment.NewLine
             + "< ! > Inferno Trigger unleashes Samira's highest damage output, wiping crowds of enemies at ease." + Environment.NewLine + Environment.NewLine;

            string outro = "..and so she left, searching for a new identity.";
            string outroFailure = "..and so she vanished, forever a blank slate.";

            Language.Add(prefix + "NAME", characterName);
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "The Desert Rose");
            Language.Add(prefix + "LORE", characterLore);
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            Language.Add(prefix + "DANTE_SKIN","Dante");
            #endregion

            #region Passive

            
            Language.Add(prefix + "PASSIVE_NAME", "Daredevil Impulse");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", $"Chaining different attacks generate a stack of Style for {SamiraStaticValues.styleDuration} seconds. For each stack, Samira gains bonus movement speed. At maximum stacks, Samira can cast Inferno Trigger.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_FLAIR_NAME", "Flair");
            Language.Add(prefix + "PRIMARY_FLAIR_DESCRIPTION", $"Samira fires a shot or swings her sword, dealing {damageColorPrefix}{100f * SamiraStaticValues.flairDamageMult}% damage</color>. Every {SamiraStaticValues.attacksPerFlair} uses triggers a unique attack, dealing {damageColorPrefix}{100f * SamiraStaticValues.flairUniqueBonusMultiplier}% additional damage</color>. Melee attacks deal an additional {damageColorPrefix}{100f * SamiraStaticValues.flairMeleeBonusMultiplier}% damage</color>.");
            
            Language.Add(prefix + "PRIMARY_EXPLOSIVESHOT_NAME", "Explosive Shot");
            Language.Add(prefix + "PRIMARY_EXPLOSIVESHOT_DESCRIPTION", $"Instead of swinging her sword, Samira only uses her guns, firing an explosive shot every 4 attacks, dealing {damageColorPrefix}{SamiraStaticValues.explosiveShotBonusMultiplier * 100}% additional damage</color>. The explosive attack does not grant style");
            
            Language.Add(prefix + "PRIMARY_SLASHINGMANIAC_NAME", "Slashing Maniac");
            Language.Add(prefix + "PRIMARY_SLASHINGMANIAC_DESCRIPTION", $"Instead of firing her guns, Samira swings her sword for {damageColorPrefix}{100 * (SamiraStaticValues.slashBonusMultiplier + SamiraStaticValues.flairMeleeBonusMultiplier)}% additional damage</color>. On hit, she gains {damageColorPrefix}{100 * SamiraStaticValues.slashBonusMS}% Movement Speed and {100 * SamiraStaticValues.slashBonusAS}% Attack Speed.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_BLADEWHIRL_NAME", "Blade Whirl");
            Language.Add(prefix + "SECONDARY_BLADEWHIRL_DESCRIPTION", $"Samira slashes around her for {SamiraStaticValues.bladeWhirlDuration} seconds, damaging enemies twice for {damageColorPrefix}{100f * SamiraStaticValues.bladeWhirlDamageMult}% damage</color> while destroying incoming enemy projectiles.");
            
            Language.Add(prefix + "SECONDARY_EXPOSINGWHIRL_NAME", "Exposing Whirl");
            Language.Add(prefix + "SECONDARY_EXPOSINGWHIRL_DESCRIPTION", $"Samira slashes around her, damaging enemies once for {damageColorPrefix}{100 * SamiraStaticValues.exposingWhirlDamageMult}% damage</color>. Samira will have {damageColorPrefix}{SamiraStaticValues.exposeDebuffArmorPen} armor penetration against  hit enemies.");
            #endregion

            #region Utility
            Language.Add(prefix + "UTILITY_WILDRUSH_NAME", "Wild Rush");
            Language.Add(prefix + "UTILITY_WILDRUSH_DESCRIPTION", $"Samira dashes forward slashing through any enemy in her path, dealing {damageColorPrefix}{100f * SamiraStaticValues.wildRushDamageMult}% damage</color>.  <style=cIsUtility>Getting a takedown against an enemy resets Wild Rush's cooldown.</style>");
            
            Language.Add(prefix + "UTILITY_QUICKSTEPS_NAME", "Quick Steps");
            Language.Add(prefix + "UTILITY_QUICKSTEPS_DESCRIPTION", $"Samira dashes forward, dealing {damageColorPrefix}{100 * SamiraStaticValues.quickStepsDamageMult}% damage</color> to hit enemies, while gaining {damageColorPrefix}{100f * SamiraStaticValues.wildRushAttackSpeedMult}% Attack Speed</color> for {SamiraStaticValues.wildRushAttackSpeedDuration} seconds. This ability does not reset on takedown.");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_INFERNOTRIGGER_NAME", "Inferno Trigger");
            Language.Add(prefix + "SPECIAL_INFERNOTRIGGER_DESCRIPTION", $"Samira unleashes a torrent of shots for {SamiraStaticValues.infernoTriggerDuration} seconds, dealing {damageColorPrefix}{100f * SamiraStaticValues.infernoTriggerDamageMult}% damage</color> per shot. The number of shots fired is scaled by {damageColorPrefix}{100 * SamiraStaticValues.infernoTriggerAttackSpeedMult}% attack speed</color>.");
            
            Language.Add(prefix + "SPECIAL_INFINITERAIN_NAME", "Infinite Rain");
            Language.Add(prefix + "SPECIAL_INFINITERAIN_DESCRIPTION", $"Samira unleashes a torrent of shots for {damageColorPrefix}{100 * SamiraStaticValues.infiniteRainDMGMultiplier}% damage and {100 * SamiraStaticValues.infiniteRainASMultiplier}% attack speed</color>. However, killing an enemy will increase the duration of the ability by {SamiraStaticValues.infiniteRainDurationExtend} seconds.");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(SamiraMasteryAchievement.identifier), "Samira: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(SamiraMasteryAchievement.identifier), "As Samira, beat the game or obliterate on Monsoon.");
            #endregion
        }
    }
}
