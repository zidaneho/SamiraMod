using RoR2;
using UnityEngine;
using System;
using RoR2.Projectile;
using SamiraMod.Modules;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira
{
    public static class SamiraAssets
    {
        // particle effects
        public static GameObject bulletHitEffect;
        public static GameObject bulletMuzzleEffect;

        public static GameObject autoSwingEffect;
        public static GameObject cleaveSwingEffect;
        public static GameObject autoCritSwingEffect;

        // networked hit sounds
        public static NetworkSoundEventDef swordHitSoundEvent;

        //projectiles

        private static AssetBundle _assetBundle;

        public static void Init(AssetBundle assetBundle)
        {

            _assetBundle = assetBundle;

            swordHitSoundEvent = Content.CreateAndAddNetworkSoundEventDef("Play_SamiraSFX_SwordHit");

            CreateEffects();

            CreateProjectiles();
        }

     
        private static void CreateEffects()
        {
            bulletHitEffect = _assetBundle.LoadEffect("Bullet_GoldFire_Small_Impact_Template");
            autoSwingEffect = _assetBundle.LoadEffect("SamiraAutoSlashEffect");
            autoCritSwingEffect = _assetBundle.LoadEffect("SamiraAutoCritSlashEffect");
            cleaveSwingEffect = _assetBundle.LoadEffect("SamiraCleaveSlashEffect");
        }
        
        private static void CreateProjectiles()
        {
            bulletMuzzleEffect = _assetBundle.LoadEffect("Bullet_GoldFire_Small_MuzzleFlare_Template");
            
        }

        

    }
}
