using RoR2;
using UnityEngine;
using System;
using RoR2.Projectile;
using SamiraMod.Modules;

namespace SamiraMod.Survivors.Samira
{
    public static class SamiraAssets
    {
        // particle effects
        public static GameObject bulletHitEffect;

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
            CreateBulletImpact();
        }
        
        private static void CreateProjectiles()
        {
        }

        private static void CreateBulletImpact()
        {
            bulletHitEffect = _assetBundle.LoadEffect("Bullet_GoldFire_Small_Impact_Template");
            
        }
        
        
    }
}
