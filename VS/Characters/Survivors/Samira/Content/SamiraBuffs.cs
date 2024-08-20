using RoR2;
using RoR2.UI;
using SamiraMod.Modules;
using UnityEngine;

namespace SamiraMod.Survivors.Samira
{
    public static class SamiraBuffs
    {
        // armor buff gained during roll
        public static BuffDef armorBuff;
        public static BuffDef comboBuff1;
        public static BuffDef comboBuff2;
        public static BuffDef comboBuff3;
        public static BuffDef comboBuff4;
        public static BuffDef comboBuff5;
        public static BuffDef comboBuff6;

        public static BuffDef wildRushAttackSpeedBuff;

        public static BuffDef meleeOnHitBuff;

        public static BuffDef bladeWhirlArmorShredDebuff;

        public static void Init(AssetBundle assetBundle)
        {
            armorBuff = Modules.Content.CreateAndAddBuff("HenryArmorBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.white,
                false,
                false);
            comboBuff1 = CreateComboBuff("ComboBuff1",assetBundle.LoadAsset<Sprite>("texSamiraR1"));
            comboBuff2 = CreateComboBuff("ComboBuff2",assetBundle.LoadAsset<Sprite>("texSamiraR2"));
            comboBuff3 = CreateComboBuff("ComboBuff3",assetBundle.LoadAsset<Sprite>("texSamiraR3"));
            comboBuff4 = CreateComboBuff("ComboBuff4",assetBundle.LoadAsset<Sprite>("texSamiraR4"));
            comboBuff5 = CreateComboBuff("ComboBuff5",assetBundle.LoadAsset<Sprite>("texSamiraR5"));
            comboBuff6 = CreateComboBuff("ComboBuff6",assetBundle.LoadAsset<Sprite>("texSamiraR6"));
            
            wildRushAttackSpeedBuff = Content.CreateAndAddBuff("WildRushAttackSpeedBuff",assetBundle.LoadAsset<Sprite>("texSamiraE"),Color.white,false,false);
            
            meleeOnHitBuff = Content.CreateAndAddBuff("SamiraMeleeOnHitBuff", assetBundle.LoadAsset<Sprite>("texSamiraQ"),Color.white,true,false);
            
            bladeWhirlArmorShredDebuff = Content.CreateAndAddBuff("SamiraBladeWhirlDefenseShredDebuff", assetBundle.LoadAsset<Sprite>("texSamiraW"),Color.white,false,true);
        }

        static BuffDef CreateComboBuff(string name, Sprite sprite)
        {
            return Modules.Content.CreateAndAddBuff(name, sprite, Color.white, false, false);
        }
    }
}
