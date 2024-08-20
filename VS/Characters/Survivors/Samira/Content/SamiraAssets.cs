using RoR2;
using UnityEngine;
using System;
using RoR2.Audio;
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
        public static GameObject explosiveProjectile;
        public static GameObject explosiveMuzzle;

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
            CreateExplosiveBullet();
            

        }

        static void CreateExplosiveBullet()
        {
            explosiveMuzzle = _assetBundle.LoadEffect("Plasma_RagingRed_Big_Impact");
            var ghost = _assetBundle.CreateProjectileGhostPrefab("Plasma_RagingRed_Big_Projectile");
            explosiveProjectile = Assets.CloneProjectilePrefab("CommandoGrenadeProjectile", "SamiraExplosiveBullet");
            
            GameObject.Destroy(explosiveProjectile.GetComponent<ApplyTorqueOnStart>());
            
            var projController = explosiveProjectile.GetComponent<ProjectileController>();
            projController.ghostPrefab = ghost;
            
            var rb = explosiveProjectile.GetComponent<Rigidbody>();
            rb.useGravity = false;
            
            var projSimple = explosiveProjectile.GetComponent<ProjectileSimple>();
            projSimple.desiredForwardSpeed = 150f;
            
            //Commando's grenade blast radius is 11
            var projImpactExplosion = explosiveProjectile.GetComponent<ProjectileImpactExplosion>();
            projImpactExplosion.destroyOnEnemy = true;
            projImpactExplosion.destroyOnWorld = true;
            projImpactExplosion.blastRadius = 15f;

        }
        

        

    }
}
