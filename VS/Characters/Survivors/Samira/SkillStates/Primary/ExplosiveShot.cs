using System;
using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using RoR2.Skills;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class ExplosiveShot : BaseSkillState, SteppedSkillDef.IStepSetter
    {

        public static int autoAttackID = SamiraStaticValues.autoAttackID;
        public virtual int attacksPerFlair => SamiraStaticValues.attacksPerExplosiveShot;
        private static float maxDistance = 10f;
        private static float searchAngle = 45f;
        public static float procCoefficient = 1f;


        private float duration;
        private bool hasFired;
        private bool crit;
        private SamiraComboManager _comboManager;
        private ChildLocator childLocator;

        #region Ranged Members

        public static DamageType damageType = DamageType.Generic;
        public static float rangedBaseDuration = 0.85f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public float firePercentTime = 0.025f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;

        public static GameObject tracerEffectPrefab =
            LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");
        private float fireTime;
        private GameObject muzzleEffectPrefab;

        #endregion

   

        public int swingIndex;


        protected string playbackRateParam;
        
    
        protected Animator animator;
        protected SamiraSoundManager soundManager;

        private bool canUseFlair => swingIndex >= attacksPerFlair - 1;
  




        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            _comboManager = characterBody.GetComponent<SamiraComboManager>();
            crit = RollCrit();
            this.childLocator = GetModelChildLocator();
            muzzleEffectPrefab = SamiraAssets.bulletMuzzleEffect;
            soundManager = characterBody.GetComponent<SamiraSoundManager>();

            
            SetupRangedAttack();
            
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            
                if (fixedAge >= fireTime)
                {
                    if (canUseFlair) FireExplosiveShot();
                    else FireBullet();
                }

                if (fixedAge >= duration && isAuthority)
                {
                    outer.SetNextStateToMain();
                    return;
                }
                
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        void SetupRangedAttack()
        {
            duration = rangedBaseDuration / attackSpeedStat;
            fireTime = firePercentTime * duration;
            
            PlayShootAnimation();
            characterBody.SetAimTimer(0.5f * duration);
        }

        private void FireExplosiveShot()
        {
            if (!hasFired)
            {
                hasFired = true;
                
                

                if (base.isAuthority)
                {
                    PlayFireBulletSound();

                    EffectManager.SimpleMuzzleFlash(SamiraAssets.explosiveMuzzle,
                        gameObject, "RevolverMuzzle", true);

                    Vector3 origin = childLocator.FindChild("RevolverMuzzle").position;
                    FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                    fireProjectileInfo.position = origin;
                    fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(GetAimRay().direction);
                    fireProjectileInfo.crit = crit;
                    fireProjectileInfo.damage = SamiraStaticValues.GetFlairDamage(damageStat, characterBody.level) * SamiraStaticValues.explosiveShotBonusMultiplier;
                    fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                    fireProjectileInfo.owner = base.gameObject;
                    fireProjectileInfo.force = force;
                    fireProjectileInfo.useFuseOverride = false;
                    fireProjectileInfo.useSpeedOverride = false;
                    fireProjectileInfo.target = null;
                    fireProjectileInfo.projectilePrefab = SamiraAssets.explosiveProjectile;
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);

                }
                
                
                
            }
        }

        void PlayFireBulletSound()
        {
            if (base.isAuthority)
            {
                Util.PlayAttackSpeedSound("Play_SamiraSFX_Shoot", gameObject,attackSpeedStat);
                if (Modules.Config.enableVoiceLines.Value)
                {
                    soundManager.PlaySoundBySkin("PlayVO_BasicAttackRanged", gameObject);
                }   
            }
        }

        private void FireBullet()
        {
            if (!hasFired)
            {
                hasFired = true;
                
                Ray aimRay = GetAimRay();
                characterBody.AddSpreadBloom(1.5f);

                string rangedMuzzleString = canUseFlair ? "RevolverMuzzle" : "PistolMuzzle";
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab,
                    gameObject, rangedMuzzleString, false);

                if (base.isAuthority)
                {
                    AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);
                    PlayFireBulletSound();

                    Vector3 origin = childLocator.FindChild(rangedMuzzleString).position;
                    
                    float damage = SamiraStaticValues.GetFlairDamage(damageStat, characterBody.level);
                    
                    var bulletAttack = new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = origin,
                        damage = damage,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = damageType,
                        falloffModel = BulletAttack.FalloffModel.None,
                        maxDistance = range,
                        force = force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        minSpread = 0f,
                        maxSpread = 0f,
                        isCrit = crit,
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
                        muzzleName = "PistolMuzzle",
                        hitEffectPrefab = SamiraAssets.bulletHitEffect,
                        hitCallback = HitCallback,
                    };
                    bulletAttack.Fire();
                }
            }
        }

        private bool HitCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {

            var result = BulletAttack.defaultHitCallback(bulletAttack, ref hitInfo);
            
            HealthComponent healthComponent = hitInfo.hitHurtBox ? hitInfo.hitHurtBox.healthComponent : null;
            if (healthComponent && healthComponent.alive && hitInfo.hitHurtBox.teamIndex != base.teamComponent.teamIndex)
            {
                _comboManager.AddCombo(autoAttackID);
                if (base.isAuthority) Util.PlayAttackSpeedSound("Play_SamiraSFX_BulletHit", hitInfo.hitHurtBox.gameObject,attackSpeedStat);
            }
            return result;
        }

        public void SetStep(int i)
        {
            swingIndex = i;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(swingIndex);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            swingIndex = reader.ReadInt32();
        }
        
        protected virtual void PlayShootAnimation()
        {
            var animString = swingIndex % 2 == 0 ? "Shoot1" : "Shoot2";
            if (crit) animString = "Shoot1Crit";
            playbackRateParam = "Shoot.playbackRate";
            if (crit) playbackRateParam = "ShootCrit.playbackRate";

            if (canUseFlair)
            {
                animString = "FlairRanged";
            }
                

            bool moving = animator.GetBool("isMoving");
            bool grounded = animator.GetBool("isGrounded");

            bool useGesture = (moving || !grounded) && swingIndex < attacksPerFlair-1 && !crit;

            if (useGesture)
            {
                PlayAnimation("Gesture, Override",animString,playbackRateParam,duration);
            }
            else
            {
                PlayAnimation("FullBody, Override", animString, playbackRateParam, duration);   
            }
        }
    }
}