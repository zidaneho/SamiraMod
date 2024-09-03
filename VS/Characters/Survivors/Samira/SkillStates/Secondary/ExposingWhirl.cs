using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityStates;
using RoR2;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class ExposingWhirl : BladeWhirl
    {
        private SamiraBladeWhirlHandler _bladeWhirlHandler;
        private bool hitFirstEnemy = false;

        private float force = 40f;
        public override void FireAttack()
        {
            if (!base.isAuthority) return;
            
            Util.PlayAttackSpeedSound("Play_SamiraSFX_Shoot", gameObject,attackSpeedStat);
            if (Modules.Config.enableVoiceLines.Value)
            {
                soundManager.PlaySoundBySkin("PlayVO_BasicAttackRanged", gameObject);
            }
            
            List<HurtBox> HurtBoxes = new List<HurtBox>();
            HurtBoxes = new SphereSearch
            {
                radius = searchRadius,
                mask = LayerIndex.entityPrecise.mask,
                origin = characterBody.corePosition,
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(base.teamComponent.teamIndex)).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().ToList();
            
            Vector3 origin = characterBody.corePosition;
            float damage = SamiraStaticValues.GetExposingWhirlDamage(damageStat, characterBody.level);
            float range = 500f;
            foreach (HurtBox hurtbox in HurtBoxes)
            {
                var bulletAttack = new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = hurtbox.transform.position - origin,
                    origin = origin,
                    damage = damage,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.None,
                    maxDistance = range,
                    force = force,
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
                    tracerEffectPrefab = Flair.tracerEffectPrefab,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    muzzleName = "PistolMuzzle",
                    hitEffectPrefab = SamiraAssets.bulletHitEffect,
                    hitCallback = HitCallback,
                };
                bulletAttack.Fire();
            }
        }
        private bool HitCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            var result = BulletAttack.defaultHitCallback(bulletAttack, ref hitInfo);
            
            HealthComponent healthComponent = hitInfo.hitHurtBox ? hitInfo.hitHurtBox.healthComponent : null;

            var hitEnemy = healthComponent && healthComponent.alive &&
                           hitInfo.hitHurtBox.teamIndex != base.teamComponent.teamIndex;
            if (hitEnemy)
            {
                hitFirstEnemy = true;
                _comboManager.AddCombo(attackID);
            }
            
            if (hitEnemy)
            {
                healthComponent.body.AddTimedBuff(SamiraBuffs.bladeWhirlArmorShredDebuff, SamiraStaticValues.exposeDebuffDuration, 1);
                Util.PlayAttackSpeedSound("Play_SamiraSFX_BulletHit", hitInfo.hitHurtBox.gameObject,attackSpeedStat);
            }
            return result;
        }
        
        

        public override void EndWhirl()
        {
        }
    }
}