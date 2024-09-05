using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.Projectile;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;
using static UnityEngine.UI.Image;

namespace SamiraMod.Survivors.Samira.SkillStates.Emotes
{
    public class SamiraTaunt : SamiraBaseEmote
    {
        private float throwCoinDuration = 2.91f;
        private bool hasFired;
        
        private ChildLocator childLocator;

        private uint sfxID;
        private uint voID;
        public override void OnEnter()
        {
            animString = "Taunt";
            duration = 4.91f; // 24 fps, 118 frames

            childLocator = GetModelChildLocator();
            EnableCoinVisual();
            
            base.OnEnter();

            if (isAuthority)
            {
                voID = soundManager.PlaySoundBySkin("PlayVO_Taunt", gameObject);
                sfxID = RoR2.Util.PlaySound("Play_SamiraSFX_Taunt",gameObject);   
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (voID != 0) AkSoundEngine.StopPlayingID(voID); 
            if (sfxID != 0) AkSoundEngine.StopPlayingID(sfxID);
        }

        public override void Update()
        {
            base.Update();

            if (fixedAge >= throwCoinDuration && !hasFired)
            {
                hasFired = true;
                FireCoin();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        void FireCoin()
        {
            DisableCoinVisual();
            if (base.isAuthority)
            {
                Util.PlaySound("Play_SamiraSFX_Coinflip",gameObject);
                Transform muzzle = childLocator.FindChild("CoinMuzzle");
                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.position = muzzle.position;
                fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(muzzle.forward);
                fireProjectileInfo.crit = false;
                fireProjectileInfo.damage = 1f;
                fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                fireProjectileInfo.owner = base.gameObject;
                fireProjectileInfo.force = 50f;
                fireProjectileInfo.useFuseOverride = false;
                fireProjectileInfo.useSpeedOverride = false;
                fireProjectileInfo.target = null;
                fireProjectileInfo.projectilePrefab = SamiraAssets.coinProjectile;
            
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);

                
                
            }
            
        
        }

        void DisableCoinVisual()
        {
            childLocator.FindChild("CoinMesh").gameObject.SetActive(false);
        }

        void EnableCoinVisual()
        {
            childLocator.FindChild("CoinMesh").gameObject.SetActive(true);
        }
        
        

        
        
    }
}