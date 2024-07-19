using RoR2;
using SamiraMod.Modules.Achievements;

namespace SamiraMod.Survivors.Samira.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, null)]
    public class SamiraMasteryAchievement : BaseMasteryAchievement
    {
        public const string identifier = SamiraSurvivor.SAMIRA_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = SamiraSurvivor.SAMIRA_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => SamiraSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}