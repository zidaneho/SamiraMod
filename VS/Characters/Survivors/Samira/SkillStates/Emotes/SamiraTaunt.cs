using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.Projectile;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;

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

            voID = SamiraSoundManager.instance.PlaySoundBySkin("PlayVO_Taunt", gameObject);
            sfxID = RoR2.Util.PlaySound("Play_SamiraSFX_Taunt",gameObject);
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
            TrackNearestEnemy();
        }

        void FireCoin()
        {
            Util.PlaySound("Play_SamiraSFX_Coinflip",gameObject);
            if (!isAuthority) return;
            DisableCoinVisual();

            Transform muzzle = childLocator.FindChild("CoinMuzzle");
            ProjectileManager.instance.FireProjectile(SamiraAssets.coinProjectile, // The projectile prefab
                muzzle.position, // Origin of the projectile
                Util.QuaternionSafeLookRotation(muzzle.forward), // Direction the projectile will travel
                base.gameObject, // The owner of the projectile
                1f, // Damage of the projectile
                0f, // Force applied to the projectile
                false, // Whether the projectile is a critical hit
                DamageColorIndex.Default, // The color of the damage numbers
                null, // No special target filter
                -1f
                // No speed override, use the prefab's speed
            ); 
        
        }
        void TrackNearestEnemy()
        {
            List<HurtBox> HurtBoxes = new List<HurtBox>();
            HurtBoxes = new SphereSearch
            {
                radius = 50f,
                mask = LayerIndex.entityPrecise.mask,
                origin = characterBody.corePosition,
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(base.teamComponent.teamIndex)).FilterCandidatesByDistinctHurtBoxEntities().OrderCandidatesByDistance().GetHurtBoxes().ToList();
            if (HurtBoxes.Count > 0)
            {
                HurtBox nearestHurtBox = HurtBoxes[0];
                Debug.Log(nearestHurtBox.healthComponent.body);
                // Calculate the direction to the nearest enemy
                Vector3 directionToEnemy = nearestHurtBox.transform.position - characterBody.corePosition;
                directionToEnemy.y = 0; // Ignore the y-axis to keep the rotation on the horizontal plane

                // Calculate the target rotation
                Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);
                Transform modelTransform = modelLocator.modelTransform;
                modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, targetRotation, Time.deltaTime * 10f);
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