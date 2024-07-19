using System;
using System.Linq;
using EntityStates;
using RoR2;
using RoR2.Audio;
using RoR2.Skills;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class Flair : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        public enum AttackType
        {
            Melee,
            Ranged
        }

        public static int attackID = 1;
        private static float maxDistance = 5f;
        private static float searchAngle = 45f;
        protected DamageType damageType = DamageType.Generic;
        public static float damageCoefficient = SamiraStaticValues.bladeWhirlDamageMult;
        public static float procCoefficient = 1f;


        private float duration;
        private bool hasFired;
        private AttackType _attackType;
        private bool crit;
        private SamiraComboManager _comboManager;

        #region Ranged Members

        public static float rangedBaseDuration = 0.9f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;

        public static GameObject tracerEffectPrefab =
            LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");
        private ParticleSystem muzzleParticle;
        private float fireTime;

        #endregion

        #region Melee Members

        public int swingIndex;

        protected string hitboxGroupName = "AAHitbox";
        
        protected float pushForce = 300f;
        protected Vector3 bonusForce = Vector3.zero;
        protected float meleeBaseDuration = 1.5f;

        protected float attackStartPercentTime = 0.2f;
        protected float attackEndPercentTime = 0.8f;

        protected float earlyExitPercentTime = 0.4f;

        protected float hitStopDuration = 0.012f;
        protected float attackRecoil = 0.75f;
        protected float hitHopVelocity = 4f;
        protected string meleeMuzzleString = "SwingCenter";
        protected string playbackRateParam = "Slash.playbackRate";
        protected GameObject swingEffectPrefab;
        protected GameObject hitEffectPrefab;
        protected NetworkSoundEventIndex impactSound = NetworkSoundEventIndex.Invalid;

        private float hitPauseTimer;
        private OverlapAttack attack;
        protected bool inHitPause;
        private bool hasHopped;
        protected float stopwatch;
        protected Animator animator;
        private HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;
        private static readonly int InDashing = Animator.StringToHash("inDashing");

        #endregion


        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            _comboManager = characterBody.GetComponent<SamiraComboManager>();
            crit = RollCrit();

            var childLocator = GetModelChildLocator();
            if (childLocator)
            {
                var muzzleTransform = childLocator.FindChild("PistolMuzzle");
                if (muzzleTransform)
                {
                    muzzleParticle = muzzleTransform.GetComponentInChildren<ParticleSystem>();
                }
            }

            if (IsEnemyInFront())
            {
                _attackType = AttackType.Melee;
                SetupMeleeAttack();
            }
            else
            {
                _attackType = AttackType.Ranged;
                SetupRangedAttack();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (_attackType == AttackType.Ranged)
            {
                if (fixedAge >= fireTime)
                {
                    Fire();
                }

                if (fixedAge >= duration && isAuthority)
                {
                    outer.SetNextStateToMain();
                    return;
                }
            }

            if (_attackType == AttackType.Melee)
            {
                hitPauseTimer -= Time.fixedDeltaTime;

                if (hitPauseTimer <= 0f && inHitPause)
                {
                    RemoveHitstop();
                }

                if (!inHitPause)
                {
                    stopwatch += Time.fixedDeltaTime;
                }
                else
                {
                    if (characterMotor) characterMotor.velocity = Vector3.zero;
                    if (animator) animator.SetFloat(playbackRateParam, 0f);
                }

                bool fireStarted = stopwatch >= duration * attackStartPercentTime;
                bool fireEnded = stopwatch >= duration * attackEndPercentTime;

                //to guarantee attack comes out if at high attack speed the stopwatch skips past the firing duration between frames
                if (fireStarted && !fireEnded || fireStarted && fireEnded && !hasFired)
                {
                    if (!hasFired)
                    {
                        EnterAttack();
                    }

                    FireAttack();
                }

                if (stopwatch >= duration && isAuthority)
                {
                    outer.SetNextStateToMain();
                    return;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            switch (_attackType)
            {
                case AttackType.Melee:
                    if (stopwatch >= duration * earlyExitPercentTime)
                    {
                        return InterruptPriority.Any;
                    }

                    return InterruptPriority.Skill;
                case AttackType.Ranged:
                    return InterruptPriority.Skill;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void SetupRangedAttack()
        {
            duration = rangedBaseDuration / attackSpeedStat;
            fireTime = firePercentTime * duration;
            characterBody.SetAimTimer(0.5f + duration);
            
            PlayShootAnimation();
        }

        void SetupMeleeAttack()
        {
            hitboxGroupName = "AAHitbox";

            damageType = DamageType.Generic;
            damageCoefficient = SamiraStaticValues.wildRushDamageMult;
            procCoefficient = 1f;
            pushForce = 300f;
            bonusForce = Vector3.zero;
            meleeBaseDuration = 1f;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0.2f;
            attackEndPercentTime = 0.4f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.6f;

            hitStopDuration = 0.012f;
            attackRecoil = 0.5f;
            hitHopVelocity = 4f;
            
            //swingEffectPrefab = SamiraAssets.swordSwingEffect;
            //hitEffectPrefab = SamiraAssets.swordHitImpactEffect;

            impactSound = SamiraAssets.swordHitSoundEvent.index;

            //BaseMeleeAttack OnEnter
            duration = meleeBaseDuration / attackSpeedStat;
            StartAimMode(0.5f + duration, false);

            attack = new OverlapAttack();
            attack.damageType = damageType;
            attack.attacker = gameObject;
            attack.inflictor = gameObject;
            attack.teamIndex = GetTeam();
            attack.damage = SamiraStaticValues.GetFlairDamage(damageStat, characterBody.level);
            attack.procCoefficient = procCoefficient;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.forceVector = bonusForce;
            attack.pushAwayForce = pushForce;
            attack.hitBoxGroup = FindHitBoxGroup(hitboxGroupName);
            attack.isCrit = crit;
            attack.impactSound = impactSound;

            PlayAttackAnimation();
        }


        private void Fire()
        {
            if (!hasFired)
            {
                hasFired = true;
                
                Ray aimRay = GetAimRay();
                characterBody.AddSpreadBloom(1.5f);
                EffectManager.SimpleMuzzleFlash(muzzleParticle.gameObject,
                    gameObject, "Pistol_Muzzle", false);
                if (muzzleParticle) muzzleParticle.Play();

                if (base.isAuthority)
                {
                    AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);
                    Util.PlayAttackSpeedSound("Play_SamiraSFX_Shoot", gameObject,attackSpeedStat);
                    if (Modules.Config.enableVoiceLines.Value) Util.PlaySound("Play_SamiraVO_BasicAttackRanged", gameObject);
                    
                    var bulletAttack = new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = SamiraStaticValues.GetFlairDamage(damageStat,characterBody.level),
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic,
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
                        muzzleName = "Pistol_Muzzle",
                        hitEffectPrefab = SamiraAssets.bulletHitEffect,
                        hitCallback = HitCallback,
                    };
                    bulletAttack.Fire();
                }
            }
        }

        private bool HitCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            if (hitInfo.hitHurtBox && hitInfo.hitHurtBox.teamIndex != teamComponent.teamIndex)
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

        protected virtual void PlayAttackAnimation()
        {
            var animString = "Slash" + (1 + swingIndex);
            if (crit) animString = "Slash1Crit";

            playbackRateParam = "Slash.playbackRate";
            PlayAnimation("FullBody, Override", animString);
        }

        protected virtual void PlayShootAnimation()
        {
            var animString = "Shoot" + (1 + swingIndex);
            if (crit) animString = "Shoot1Crit";
            playbackRateParam = "Shoot.playbackRate";
            if (crit) playbackRateParam = "ShootCrit.playbackRate";
            
            Debug.Log("Played animation");
            PlayAnimation("FullBody, Override", animString, playbackRateParam, duration);
        }

        private void EnterAttack()
        {
            hasFired = true;

            PlaySwingEffect();

            if (isAuthority)
            {
                AddRecoil(-1f * attackRecoil, -2f * attackRecoil, -0.5f * attackRecoil, 0.5f * attackRecoil);
            }
        }

        private void FireAttack()
        {
            if (isAuthority)
            {   
                if (attack.Fire())
                {
                    OnHitEnemyAuthority();
                }
            }
        }

        protected virtual void OnHitEnemyAuthority()
        {
            if (Modules.Config.enableVoiceLines.Value) Util.PlayAttackSpeedSound("Play_SamiraVO_BasicAttackMelee", gameObject,attackSpeedStat);
            Util.PlaySound("Play_SamiraSFX_SwordHit", gameObject);
            _comboManager.AddCombo(attackID);

            if (!hasHopped)
            {
                if (characterMotor && !characterMotor.isGrounded && hitHopVelocity > 0f)
                {
                    SmallHop(characterMotor, hitHopVelocity);
                }

                hasHopped = true;
            }

            ApplyHitstop();
        }

        protected void ApplyHitstop()
        {
            if (!inHitPause && hitStopDuration > 0f)
            {
                storedVelocity = characterMotor.velocity;
                hitStopCachedState = CreateHitStopCachedState(characterMotor, animator, playbackRateParam);
                hitPauseTimer = hitStopDuration / attackSpeedStat;
                inHitPause = true;
            }
        }

        protected virtual void PlaySwingEffect()
        {
            //EffectManager.SimpleMuzzleFlash(swingEffectPrefab, gameObject, meleeMuzzleString, false);
        }

        private void RemoveHitstop()
        {
            ConsumeHitStopCachedState(hitStopCachedState, characterMotor, animator);
            inHitPause = false;
            characterMotor.velocity = storedVelocity;
        }

        private bool IsEnemyInFront()
        {
            // Create a new BullseyeSearch instance
            BullseyeSearch search = new BullseyeSearch
            {
                searchOrigin = characterBody.corePosition,
                searchDirection = characterDirection.forward,
                maxDistanceFilter = maxDistance,
                teamMaskFilter = TeamMask.GetEnemyTeams(TeamComponent.GetObjectTeam(gameObject)),
                filterByLoS = true,
                sortMode = BullseyeSearch.SortMode.DistanceAndAngle,
                maxAngleFilter = searchAngle
                
            };
            search.RefreshCandidates();
            return search.GetResults().Any();
        }
    }
}