using System.Collections.Generic;
using EntityStates;
using RoR2;
using RoR2.Audio;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class FlairDash : BaseSkillState
    {
        public List<HurtBox> hurtboxesToCheck = new List<HurtBox>();
        private SamiraComboManager _comboManager;
        private Animator animator;
        private ChildLocator childLocator;
        

        private string playbackRateParam = "FlairDash.playbackRate";
        public static int attackID = SamiraStaticValues.flairDashID;
        public static float baseDuration = 0.5f;
        private float duration; // remember, flair does not have a cooldown
        protected float attackStartPercentTime = 0.0f;
        protected float attackEndPercentTime = 1f;
        
        protected float stopwatch;
        private bool hasFired;
        private GameObject muzzleEffectPrefab;
        
        #region Attack Members

        private string hitboxGroupName = "FlairDashHitbox";
        public float procCoefficient = 1f;
        
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

        #endregion

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            stopwatch = 0f;
            animator = GetModelAnimator();
            _comboManager = characterBody.GetComponent<SamiraComboManager>();
            
            childLocator = GetModelChildLocator();

            muzzleEffectPrefab = SamiraAssets.bulletMuzzleEffect;
            

            PlayAnimation("FullBody, Override", "FlairDash", playbackRateParam, duration);
        }

        public override void OnExit()
        {
            base.OnExit();
            hurtboxesToCheck.Clear();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }

            stopwatch += Time.fixedDeltaTime;
            
            bool fireStarted = stopwatch >= duration * attackStartPercentTime;
            bool fireEnded = stopwatch >= duration * attackEndPercentTime;

            // to guarantee attack comes out if at high attack speed the stopwatch skips past the firing duration between frames
            if (fireStarted && !fireEnded && !hasFired)
            {
                EnterAttack();
                FireAttack();
            }

            if (stopwatch >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void EnterAttack()
        {
            hasFired = true;
            Util.PlaySound("Play_SamiraSFX_EQ", gameObject);
        }

        private void FireAttack()
        {
            if (isAuthority)
            {
                FireBullets();
            }
        }

        private void FireBullets()
        {
            EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, "PistolMuzzle",false);
            EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, "RevolverMuzzle",false);
            
            if (hurtboxesToCheck == null || hurtboxesToCheck.Count <= 0)
            {
                Fire(-characterBody.inputBank.aimDirection);
                return;
            }
            
            int numberShot = 0;
            foreach (var hurtbox in hurtboxesToCheck)
            {
                if (hurtbox != null && hurtbox.healthComponent && hurtbox.healthComponent.alive)
                {
                    numberShot++;
                    Fire(hurtbox.transform.position - characterBody.corePosition);
                }
            }

            if (numberShot <= 0)
            {
                Fire(-characterBody.inputBank.aimDirection);
            }
        }

        void Fire(Vector3 direction)
        {
            float damage = SamiraStaticValues.GetFlairDamage(damageStat, characterBody.level);
            
            var bulletAttack = new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = direction,
                    origin = characterBody.corePosition,
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
                    tracerEffectPrefab = tracerEffectPrefab,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    muzzleName = "PistolMuzzle",
                    hitEffectPrefab = SamiraAssets.bulletHitEffect,
                    hitCallback = HitCallback
                };
                bulletAttack.Fire();
        }

        private bool HitCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            var result = BulletAttack.defaultHitCallback(bulletAttack, ref hitInfo);
            
            HealthComponent healthComponent = hitInfo.hitHurtBox ? hitInfo.hitHurtBox.healthComponent : null;
            if (healthComponent && healthComponent.alive && hitInfo.hitHurtBox.teamIndex != base.teamComponent.teamIndex)
            {
                _comboManager.AddCombo(attackID);
                Util.PlayAttackSpeedSound("Play_SamiraSFX_BulletHit", hitInfo.hitHurtBox.gameObject,attackSpeedStat);
            }
            return result;
        }
    }
}