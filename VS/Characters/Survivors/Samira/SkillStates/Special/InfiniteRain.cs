using System.Collections.Generic;
using System.Linq;
using EntityStates;
using RoR2;
using UnityEngine;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class InfiniteRain : BaseInfernoTrigger
    {
        private float durationExtendOnKill = SamiraStaticValues.infiniteRainDurationExtend;
        
        public override void FixedUpdate()
        {
            attackSpeedMultiplier = SamiraStaticValues.infiniteRainASMultiplier;
            base.FixedUpdate();
        }
        protected override void FireAttack()
        {
            
            List<HurtBox> HurtBoxes = new List<HurtBox>();
            HurtBoxes = new SphereSearch
            {
                radius = attackRadius,
                mask = LayerIndex.entityPrecise.mask,
                origin = this.attackIndicatorInstance.transform.position
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(base.teamComponent.teamIndex)).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().ToList();

            
            
            float range = 250f;
            foreach (HurtBox hurtbox in HurtBoxes)
            {
                bool usePistol = muzzleIndexer % 2 == 0;
                muzzleIndexer += 1;
                string muzzleName = usePistol ? "PistolMuzzle" : "RevolverMuzzle";
                EffectManager.SimpleMuzzleFlash(SamiraAssets.bulletMuzzleEffect,gameObject,muzzleName,false);
                
                var bulletAttack = new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = hurtbox.transform.position - transform.position,
                    origin = childLocator.FindChild(muzzleName).position,
                    damage = SamiraStaticValues.GetInfiniteRainDamage(damageStat,characterBody.level),
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.None,
                    maxDistance = range,
                    force = 0f,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0f,
                    maxSpread = 0f,
                    isCrit = RollCrit(),
                    owner = gameObject,
                    smartCollision = true,
                    procChainMask = default,
                    procCoefficient = procCoefficient,
                    radius = 0.75f,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = tracerEffectPrefab,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    muzzleName = muzzleName,
                    hitEffectPrefab = SamiraAssets.bulletHitEffect,
                    hitCallback = OnBulletHit
                };
                bulletAttack.Fire();
                
            }
            
        }

        protected override bool OnBulletHit(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            var result = base.OnBulletHit(bulletAttack, ref hitInfo);
            
            HealthComponent enemyHealthComponent = hitInfo.hitHurtBox ? hitInfo.hitHurtBox.healthComponent : null;
            
            if (enemyHealthComponent && !enemyHealthComponent.alive)
            {
                duration += durationExtendOnKill;
                Debug.Log("Infinite Rain duration extended to " + duration);
            }

            return result;
        }
    }
}