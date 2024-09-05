using System;
using EntityStates;
using RoR2;
using RoR2.Audio;
using RoR2.Skills;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class SlashingManiac : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        public static int autoAttackID = SamiraStaticValues.autoAttackID;
        public static int flairAttackID = SamiraStaticValues.flairID;
        public virtual int attacksPerFlair => SamiraStaticValues.attacksPerFlair;
        private static float maxDistance = 10f;
        private static float searchAngle = 45f;
        protected DamageType damageType = DamageType.Generic;
        public static float procCoefficient = 1f;

        private static float buffDuration = 3f;

        private float duration;
        private bool hasFired;
        private bool crit;
        private SamiraComboManager _comboManager;
        private ChildLocator childLocator;
        private SamiraBuffMeleeOnHitHandler buffHandler;
        protected SamiraSoundManager soundManager;

        private float fireTime;
        private GameObject muzzleEffectPrefab;

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
        protected string playbackRateParam;
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

        #endregion

        private bool canUseFlair => swingIndex >= attacksPerFlair - 1;

        public override void OnEnter()
        {
            base.OnEnter();
            soundManager = characterBody.GetComponent<SamiraSoundManager>();
            animator = GetModelAnimator();
            _comboManager = characterBody.GetComponent<SamiraComboManager>();
            buffHandler = characterBody.GetComponent<SamiraBuffMeleeOnHitHandler>();
            crit = RollCrit();
            this.childLocator = GetModelChildLocator();
            
            SetupMeleeAttack();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            hitPauseTimer -= Time.deltaTime;

            if (hitPauseTimer <= 0f && inHitPause)
            {
                RemoveHitstop();
            }

            if (!inHitPause)
            {
                stopwatch += Time.deltaTime;
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (stopwatch >= duration * earlyExitPercentTime) 
            {
                return InterruptPriority.Any;
            }
            return InterruptPriority.Skill;
        }

        void SetupMeleeAttack()
        {
            hitboxGroupName = "AAHitbox";

            damageType = DamageType.Generic;
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

            //BaseMeleeAttack OnEnter
            duration = meleeBaseDuration / attackSpeedStat;
            StartAimMode(0.5f + duration, false);
            
            float damage = SamiraStaticValues.GetFlairDamage(damageStat, characterBody.level) ;
            damage += damage * SamiraStaticValues.flairMeleeBonusMultiplier +
                      damage * SamiraStaticValues.slashBonusMultiplier;
            if (canUseFlair) damage += damage * SamiraStaticValues.flairUniqueBonusMultiplier;

            attack = new OverlapAttack();
            attack.damageType = damageType;
            attack.attacker = gameObject;
            attack.inflictor = gameObject;
            attack.teamIndex = GetTeam();
            attack.damage = damage;
            attack.procCoefficient = procCoefficient;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.forceVector = bonusForce;
            attack.pushAwayForce = pushForce;
            attack.hitBoxGroup = FindHitBoxGroup(hitboxGroupName);
            attack.isCrit = crit;
            attack.impactSound = impactSound;

            if (canUseFlair)
            {
                swingEffectPrefab = SamiraAssets.cleaveSwingEffect;
                meleeMuzzleString = "CleaveSlashMuzzle";
            }
            else
            {
                swingEffectPrefab = crit ? SamiraAssets.autoCritSwingEffect : SamiraAssets.autoSwingEffect;
                meleeMuzzleString = swingIndex % 2 == 0 ? "Auto1SlashMuzzle" : "Auto2SlashMuzzle";
                if (crit) meleeMuzzleString = "CritSlashMuzzle";
            }

            PlayAttackAnimation();
            if (base.isAuthority)
            {
                if (!canUseFlair) Util.PlayAttackSpeedSound("Play_SamiraSFX_MeleeAuto", gameObject, attackSpeedStat);
                else  Util.PlayAttackSpeedSound("Play_SamiraSFX_Cleave", gameObject, attackSpeedStat);   
            }
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
            var animString = swingIndex % 2 == 0 ? "Slash1" : "Slash2";
            if (crit) animString = "Slash1Crit";

            if (canUseFlair)
            {
                animString = "FlairMelee";
                attack.hitBoxGroup = FindHitBoxGroup("FlairMeleeHitbox");
            }
            PlayAnimation("FullBody, Override", animString);
        }
        

        private void EnterAttack()
        {
            hasFired = true;

            if (Modules.Config.enableVoiceLines.Value)
            {
                soundManager.PlayAttackSpeedSoundBySkin("PlayVO_BasicAttackMelee", gameObject, attackSpeedStat);
            }
            
            
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
            if (base.isAuthority) Util.PlaySound("Play_SamiraSFX_SwordHit", gameObject);
            
            _comboManager.AddCombo(canUseFlair ? flairAttackID : autoAttackID);
            
            if (buffHandler) buffHandler.ApplyBuff(characterBody);

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
    }
}