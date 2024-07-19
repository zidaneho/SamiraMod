using System;
using SamiraMod.Modules;
using SamiraMod.Survivors.Samira.Achievements;

namespace SamiraMod.Survivors.Samira
{
    public static class SamiraTokens
    {
        public const string colorPrefix = "<color=#C23B22>";
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
            #endregion

            #region Passive

            
            Language.Add(prefix + "PASSIVE_NAME", colorPrefix +"Daredevil Impulse</color>");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", "Samira's unique attacks generate a stack of Style for 6 seconds. For each stack, Samira gains bonus movement speed. At maximum stacks, Samira can cast Inferno Trigger.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_FLAIR_NAME", colorPrefix+"Flair</color>");
            Language.Add(prefix + "PRIMARY_FLAIR_DESCRIPTION", $"Samira fires a shot or swings her sword, dealing <style=cIsDamage>({SamiraStaticValues.flairBaseDamage} + {100f * SamiraStaticValues.flairDamageMult}%) damage</style>.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_BLADEWHIRL_NAME", colorPrefix+"Blade Whirl</color>");
            Language.Add(prefix + "SECONDARY_BLADEWHIRL_DESCRIPTION", $"Samira slashes around her for {SamiraStaticValues.bladeWhirlDuration} seconds, damaging enemies twice for <style=cIsDamage>({SamiraStaticValues.bladeWhirlBaseDamage} + {100f * SamiraStaticValues.bladeWhirlDamageMult}%) damage</style> while destroying incoming enemy projectiles");
            #endregion

            #region Utility
            Language.Add(prefix + "UTILITY_WILDRUSH_NAME", colorPrefix+"Wild Rush</color>");
            Language.Add(prefix + "UTILITY_WILDRUSH_DESCRIPTION", $"Samira dashes forward slashing through any enemy in her path, dealing <style=cIsDamage>({SamiraStaticValues.wildRushBaseDamage} + {100f * SamiraStaticValues.wildRushDamageMult}%) damage</style>. She also gains <style=cIsDamage>{100f * SamiraStaticValues.wildRushAttackSpeedMult}% Attack Speed</style> for {SamiraStaticValues.wildRushAttackSpeedDuration} seconds. <style=cIsUtility>Getting a takedown against an enemy resets Wild Rush's cooldown.</style>");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_INFERNOTRIGGER_NAME", colorPrefix+"Inferno Trigger</color>");
            Language.Add(prefix + "SPECIAL_INFERNOTRIGGER_DESCRIPTION", $"Samira unleashes a torrent of shots for {SamiraStaticValues.infernoTriggerDuration} seconds, dealing <style=cIsDamage>({SamiraStaticValues.infernoTriggerBaseDamage} + {100f * SamiraStaticValues.infernoTriggerDamageMult}%) damage</style> per shot. The number of shots fired is scaled with <style=cKeywordName>Attack Speed</style>");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(SamiraMasteryAchievement.identifier), "Samira: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(SamiraMasteryAchievement.identifier), "As Samira, beat the game or obliterate on Monsoon.");
            #endregion
        }
    }
}
