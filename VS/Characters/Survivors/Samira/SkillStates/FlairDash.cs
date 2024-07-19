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

        private string playbackRateParam = "FlairDash.playbackRate";
        
        public static int attackID = 4;
        public static float duration = 0.7f;
        protected float attackStartPercentTime = 0.2f;
        protected float attackEndPercentTime = 0.8f;
        
        protected float stopwatch;
        private bool hasFired;
        
        #region Attack Members

        private string hitboxGroupName = "FlairDashHitbox";
        public float procCoefficient = 1f;
        
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");
        private ParticleSystem muzzleParticle;

        #endregion

        public override void OnEnter()
        {
            base.OnEnter();
            stopwatch = 0f;
            animator = GetModelAnimator();
            _comboManager = characterBody.GetComponent<SamiraComboManager>();
            
            var childLocator = GetModelChildLocator();
            if (childLocator)
            {
                var muzzleTransform = childLocator.FindChild("PistolMuzzle");
                if (muzzleTransform)
                {
                    muzzleParticle = muzzleTransform.GetComponentInChildren<ParticleSystem>();
                }
            }

            PlayAnimation("FullBody, Override", "FlairDash", playbackRateParam, duration);
        }

        public override void OnExit()
        {
            base.OnExit();
            hurtboxesToCheck.Clear();
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
            if (muzzleParticle) muzzleParticle.Play();
            
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
                    Fire(hurtbox.transform.position - muzzleParticle.transform.position);
                }
            }

            if (numberShot <= 0)
            {
                Fire(-characterBody.inputBank.aimDirection);
            }
        }

        void Fire(Vector3 direction)
        {
            Vector3 origin = characterBody.corePosition;
            var bulletAttack = new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = direction,
                    origin = origin,
                    damage = SamiraStaticValues.GetFlairDamage(damageStat, characterBody.level),
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
                    muzzleName = "Pistol_Muzzle",
                    hitEffectPrefab = SamiraAssets.bulletHitEffect,
                    hitCallback = HitCallback
                };
                bulletAttack.Fire();
        }

        private bool HitCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            if (hitInfo.hitHurtBox&& hitInfo.hitHurtBox.teamIndex != teamComponent.teamIndex)
            {
                _comboManager.AddCombo(attackID);
                Util.PlaySound("Play_SamiraSFX_BulletHit", gameObject);

                HealthComponent healthComponent = hitInfo.hitHurtBox.healthComponent;
                if (healthComponent != null)
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        damage = bulletAttack.damage,
                        attacker = gameObject,
                        crit = false,
                        procChainMask = default,
                        procCoefficient = 1f,
                        position = hitInfo.hitHurtBox.transform.position
                    };
                    healthComponent.TakeDamage(damageInfo);
                }
            }
            return true;
        }
    }
}