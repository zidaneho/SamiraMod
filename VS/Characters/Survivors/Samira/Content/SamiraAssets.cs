using RoR2;
using UnityEngine;
using System;
using EntityStates.Commando.CommandoWeapon;
using RoR2.Audio;
using RoR2.Projectile;
using SamiraMod.Modules;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
using R2API;
using SamiraMod.Survivors.Samira.Components;

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

        public static GameObject shellParticlePrefab;
        public static GameObject coinProjectile;
        

        //projectiles

        private static AssetBundle _assetBundle;

        public static void Init(AssetBundle assetBundle)
        {

            _assetBundle = assetBundle;

            CreateEffects();

            CreateProjectiles();
        }

     
        private static void CreateEffects()
        {
            bulletHitEffect = _assetBundle.LoadEffect("Bullet_GoldFire_Small_Impact_Template");
            autoSwingEffect = _assetBundle.LoadEffect("SamiraAutoSlashEffect");
            autoCritSwingEffect = _assetBundle.LoadEffect("SamiraAutoCritSlashEffect");
            cleaveSwingEffect = _assetBundle.LoadEffect("SamiraCleaveSlashEffect");
            
            CreateShells();
        }
        
        private static void CreateProjectiles()
        {
            bulletMuzzleEffect = _assetBundle.LoadEffect("Bullet_GoldFire_Small_MuzzleFlare_Template");
            CreateExplosiveBullet();
            CreateCoinProjectile();

        }

        static void CreateExplosiveBullet()
        {
            explosiveMuzzle = _assetBundle.LoadEffect("Plasma_RagingRed_Big_Impact");
            var ghost = _assetBundle.CreateProjectileGhostPrefab("Plasma_RagingRed_Big_Projectile");
            explosiveProjectile = Modules.Assets.CloneProjectilePrefab("CommandoGrenadeProjectile", "SamiraExplosiveBullet");
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
            
            var projectileDamage = explosiveProjectile.GetComponent<ProjectileDamage>();
            projectileDamage.force = 100f;


        }

        static void CreateShells()
        {
            shellParticlePrefab = _assetBundle.LoadAsset<GameObject>("samiraTaunt_ShellsParticleSystem");
            
            shellParticlePrefab.AddComponent<NetworkIdentity>();
        }

        static void CreateCoinProjectile()
        {
            var ghost = _assetBundle.CreateProjectileGhostPrefab("CoinProjectile");
            
            coinProjectile = Modules.Assets.CloneProjectilePrefab("CommandoGrenadeProjectile", "SamiraCoinProjectile");

            coinProjectile.AddComponent<CoinComponent>();
            
            var projController = coinProjectile.GetComponent<ProjectileController>();
            projController.ghostPrefab = ghost;

         
            
           
            
            var projSimple = coinProjectile.GetComponent<ProjectileSimple>();
            projSimple.desiredForwardSpeed = 25f;
            
            
        
            var projImpactExplosion = coinProjectile.GetComponent<ProjectileImpactExplosion>();
            projImpactExplosion.impactEffect = null;
            projImpactExplosion.destroyOnEnemy = true;
            projImpactExplosion.destroyOnWorld = false;
            projImpactExplosion.blastRadius = 5f;
            projImpactExplosion.falloffModel = BlastAttack.FalloffModel.SweetSpot;
           

            var torqueOnStart = coinProjectile.GetComponent<ApplyTorqueOnStart>();
            torqueOnStart.randomize = false;
            torqueOnStart.localTorque = new Vector3(100000f, 0f, 0f);
        }
        

        

    }
}
