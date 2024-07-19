using RoR2;
using SamiraMod.Survivors.Samira.Achievements;
using UnityEngine;

namespace SamiraMod.Survivors.Samira
{
    public static class SamiraUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
                SamiraMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(SamiraMasteryAchievement.identifier),
                SamiraSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));
        }
    }
}
