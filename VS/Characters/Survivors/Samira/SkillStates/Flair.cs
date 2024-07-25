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

        public static int autoAttackID = 1;
        public static int flairAttackID = 4;
        public int attacksPerFlair => SamiraStaticValues.attacksPerFlair;
        private static float maxDistance = 10f;
        private static float searchAngle = 45f;
        protected DamageType damageType = DamageType.Generic;
        public static float damageCoefficient = SamiraStaticValues.bladeWhirlDamageMult;
        public static float procCoefficient = 1f;


        private float duration;
        private bool hasFired;
        public AttackType attackType;
        private bool crit;
        private SamiraComboManager _comboManager;
        private ChildLocator childLocator;

        #region Ranged Members

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
            this.childLocator = GetModelChildLocator();
            muzzleEffectPrefab = SamiraAssets.bulletMuzzleEffect;

            if (IsEnemyInFront())
            {
                attackType = AttackType.Melee;
                SetupMeleeAttack();
            }
            else
            {
                attackType = AttackType.Ranged;
                SetupRangedAttack();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (attackType == AttackType.Ranged)
            {
                if (fixedAge >= fireTime)
                {
                    FireBullet();
                }

                if (fixedAge >= duration && isAuthority)
                {
                    outer.SetNextStateToMain();
                    return;
                }
            }

            if (attackType == AttackType.Melee)
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
            switch (attackType)
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
            //characterBody.SetAimTimer(0.5f * duration);
            //PlayShootAnimation();
        }

        void SetupMeleeAttack()
        {
            hitboxGroupName = "AAHitbox";

            damageType = DamageType.Generic;
            damageCoefficient = SamiraStaticValues.wildRushDamageMult;
            procCoefficient = 1f;
            pushForce = 300f;
            bonusForce = Vector3.zero;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0.2f;
            attackEndPercentTime = 0.4f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.6f;

            hitStopDuration = 0.012f;
            attackRecoil = 0.5f;
            hitHopVelocity = 4f;

            impactSound = SamiraAssets.swordHitSoundEvent.index;

            //BaseMeleeAttack OnEnter
            duration = meleeBaseDuration / attackSpeedStat;
            StartAimMode(0.5f + duration, false);

            attack = new OverlapAttack();
            attack.damageType = damageType;
            attack.attacker = gameObject;
            attack.inflictor = gameObject;
            attack.teamIndex = GetTeam();
            attack.damage = SamiraStaticValues.GetFlairDamage(damageStat, characterBody.level,true,swingIndex);
            attack.procCoefficient = procCoefficient;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.forceVector = bonusForce;
            attack.pushAwayForce = pushForce;
            attack.hitBoxGroup = FindHitBoxGroup(hitboxGroupName);
            attack.isCrit = crit;
            attack.impactSound = impactSound;
            
            if (swingIndex >= attacksPerFlair - 1)
            {
                swingEffectPrefab = SamiraAssets.cleaveSwingEffect;
                meleeMuzzleString = "CleaveSlashMuzzle";
            }
            else
            {
                swingEffectPrefab = crit ? SamiraAssets.autoCritSwingEffect : SamiraAssets.autoSwingEffect;
                meleeMuzzleString = swingIndex % 2 == 0 ? "Auto1SlashMuzzle" : "Auto2SlashMuzzle";
            }

            PlayAttackAnimation();
        }


        private void FireBullet()
        {
            if (!hasFired)
            {
                hasFired = true;
                
                Ray aimRay = GetAimRay();
                characterBody.AddSpreadBloom(1.5f);
                
                PlayShootAnimation();
                characterBody.SetAimTimer(0.5f * duration);

                string rangedMuzzleString = swingIndex >= attacksPerFlair - 1 ? "RevolverMuzzle" : "PistolMuzzle";
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab,
                    gameObject, rangedMuzzleString, false);

                if (base.isAuthority)
                {
                    AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);
                    Util.PlayAttackSpeedSound("Play_SamiraSFX_Shoot", gameObject,attackSpeedStat);
                    if (Modules.Config.enableVoiceLines.Value) Util.PlaySound("Play_SamiraVO_BasicAttackRanged", gameObject);

                    Vector3 origin = childLocator.FindChild("PistolMuzzle").position;
                    
                    var bulletAttack = new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = origin,
                        damage = SamiraStaticValues.GetFlairDamage(damageStat,characterBody.level,false,swingIndex),
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
                _comboManager.AddCombo(swingIndex >= attacksPerFlair - 1 ? flairAttackID : autoAttackID);
                Util.PlayAttackSpeedSound("Play_SamiraSFX_BulletHit", hitInfo.hitHurtBox.gameObject,attackSpeedStat);
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
            writer.Write((int)attackType);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            swingIndex = reader.ReadInt32();
            attackType = (AttackType)reader.ReadInt32();
        }

        protected virtual void PlayAttackAnimation()
        {
            var animString = swingIndex % 2 == 0 ?"Slash1" : "Slash2";
            if (crit) animString = "Slash1Crit";

            if (swingIndex >= attacksPerFlair - 1)
            {
                animString = "FlairMelee";
                attack.hitBoxGroup = FindHitBoxGroup("FlairMeleeHitbox");
            }

            playbackRateParam = "Slash.playbackRate";
            PlayAnimation("FullBody, Override", animString);
        }

        protected virtual void PlayShootAnimation()
        {
            var animString = swingIndex % 2 == 0 ? "Shoot1" : "Shoot2";
            if (crit) animString = "Shoot1Crit";
            playbackRateParam = "Shoot.playbackRate";
            if (crit) playbackRateParam = "ShootCrit.playbackRate";

            if (swingIndex >= attacksPerFlair - 1)
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
            _comboManager.AddCombo(swingIndex >= attacksPerFlair - 1 ? flairAttackID : autoAttackID);

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
            swingEffectPrefab.transform.localScale = childLocator.FindChild(meleeMuzzleString).localScale;
            EffectManager.SimpleMuzzleFlash(swingEffectPrefab, gameObject, meleeMuzzleString, false);
        }

        private void RemoveHitstop()
        {
            ConsumeHitStopCachedState(hitStopCachedState, characterMotor, animator);
            inHitPause = false;
            characterMotor.velocity = storedVelocity;
        }

        private bool IsEnemyInFront()
        {
            string hitboxName = swingIndex >= attacksPerFlair - 1 ? "FlairMeleeHitbox" : "AAHitbox";
            Transform hitboxTransform = childLocator.FindChild(hitboxName);

            Collider[] colliders = Physics.OverlapBox(hitboxTransform.position, hitboxTransform.localScale / 2,
                hitboxTransform.rotation);
            foreach (var collider in colliders)
            {
                HurtBox hurtbox = collider.GetComponent<HurtBox>();
                if (hurtbox != null && hurtbox.teamIndex != teamComponent.teamIndex && hurtbox.healthComponent != null && hurtbox.healthComponent.alive)
                {
                    return true;
                }
            }

            return false;
        }
    }
}