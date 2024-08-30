using System;
using System.Collections.Generic;
using System.Linq;
using EntityStates;
using RoR2;
using RoR2.Audio;
using SamiraMod.Survivors.Samira;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class WildRush : BaseWildRush
    {
    }
    public abstract class BaseWildRush : BaseSkillState
    {
        public static float duration = 0.3f;
        public static float delayDuration = 0.2f;
        public static float initialSpeedCoefficient = 16f;
        public static float finalSpeedCoefficient = 2f;
        
        public static float dodgeFOV = global::EntityStates.Commando.DodgeState.dodgeFOV;
        public static int attackID = SamiraStaticValues.wildRushID;

        private float rollSpeed;
        private Vector3 forwardDirection;
        private Animator animator;
        private static readonly int InDashing = Animator.StringToHash("inDashing");
        private SamiraComboManager _comboManager;
        
        #region Attack Members
      
        
        protected string hitboxGroupName = "WildRushHitbox";
        
        protected DamageType damageType = DamageType.Generic;
        public static float procCoefficient = 1f;
        
        protected float pushForce = 300f;
        protected Vector3 bonusForce = Vector3.zero;

        protected float attackStartPercentTime = 0.2f;
        protected float attackEndPercentTime = 0.8f;
        private float cancelAttackPercentTime = 0.5f;

        protected float hitStopDuration = 0.012f;
        protected float attackRecoil = 0.75f;
        protected string meleeMuzzleString = "SwingCenter";
        protected string playbackRateParam = "Slash.playbackRate";
        protected GameObject muzzleEffectPrefab;
        protected GameObject hitEffectPrefab;
        protected NetworkSoundEventIndex impactSound = NetworkSoundEventIndex.Invalid;
        
        protected OverlapAttack attack;
        private float hitPauseTimer;
        protected bool inHitPause;
        private bool hasFired;
        protected float stopwatch;
        private HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;
        private static readonly int InInfernoTrigger = Animator.StringToHash("inInfernoTrigger");
        private List<HurtBox> hitEnemies = new List<HurtBox>();
        private bool usedFlairDash;

        #endregion

        public override void OnEnter()
        {
            base.OnEnter();
            usedFlairDash = false;
            stopwatch = 0f;
            base.gameObject.layer = LayerIndex.fakeActor.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();
            animator = GetModelAnimator();
            _comboManager = characterBody.GetComponent<SamiraComboManager>();
            muzzleEffectPrefab = SamiraAssets.bulletMuzzleEffect;

            if (isAuthority && inputBank && characterDirection)
            {
                forwardDirection = GetAimRay().direction;
            }

            RecalculateRollSpeed();

            if (characterMotor && characterDirection)
            {
                characterMotor.velocity.y = 0f;
                characterMotor.velocity = forwardDirection * rollSpeed;
            }
            
            SetupAttack();

            bool inInfernoTrigger = animator.GetBool(InInfernoTrigger);
            if (inInfernoTrigger)
            {
                PlayAnimation("FullBody, Override", "InfernoTriggerDash");
            }
            else
            {
                PlayAnimation("FullBody, Override", "WildRush", "WildRush.playbackRate", duration);
            }
            
            animator.SetBool(InDashing,true);
            Util.PlaySound("Play_SamiraSFX_E_Start", gameObject);
            if (Modules.Config.enableVoiceLines.Value)
            {
                SamiraSoundManager.instance.PlaySoundBySkin("PlayVO_E",gameObject);
            }
            
            //disables fall damage, enables it back in onexit
            characterBody.bodyFlags = CharacterBody.BodyFlags.IgnoreFallDamage;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        protected virtual void SetupAttack()
        {
            hitboxGroupName = "WildRushHitbox";
            damageType = DamageType.Generic;
            procCoefficient = 1f;
            pushForce = 0f;
            bonusForce = Vector3.zero;
            
            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0f;
            attackEndPercentTime = 1f;

          

            hitStopDuration = 0.012f;
            attackRecoil = 0.5f;
            meleeMuzzleString = "SwingLeft";
            //swingEffectPrefab = SamiraAssets.swordSwingEffect;
            //hitEffectPrefab = SamiraAssets.swordHitImpactEffect;
            
            attack = new OverlapAttack();
            attack.damageType = damageType;
            attack.attacker = gameObject;
            attack.inflictor = gameObject;
            attack.teamIndex = GetTeam();
            attack.damage = SamiraStaticValues.GetWildRushDamage(damageStat, characterBody.level);
            attack.procCoefficient = procCoefficient;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.forceVector = bonusForce;
            attack.pushAwayForce = pushForce;
            attack.hitBoxGroup = FindHitBoxGroup(hitboxGroupName);
            attack.isCrit = RollCrit();
            attack.impactSound = impactSound;
        }

        protected virtual void RecalculateRollSpeed()
        {
            rollSpeed = moveSpeedStat * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RecalculateRollSpeed();

            if (characterDirection) characterDirection.forward = forwardDirection;
            
            if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, fixedAge / duration);
            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = forwardDirection.normalized * rollSpeed;
            }
            
            bool fireStarted = stopwatch >= duration * attackStartPercentTime;
            bool fireEnded = stopwatch >= duration * attackEndPercentTime;
            bool cancelStarted = stopwatch >= duration * cancelAttackPercentTime;
            
            

            if (isAuthority && fixedAge >= duration + delayDuration)
            {
                outer.SetNextStateToMain();
                return;
            }
            
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
            

            //to guarantee attack comes out if at high attack speed the stopwatch skips past the firing duration between frames
            if (fireStarted && !fireEnded || fireStarted && fireEnded && !hasFired)
            {
                if (!hasFired)
                {
                    EnterAttack();
                }

                FireAttack();
            }
            
            if (cancelStarted)
            {
                if (inputBank.skill1.down)
                {
                    var flairDashState = new FlairDash();
                    flairDashState.hurtboxesToCheck = hitEnemies;
                    usedFlairDash = true;

                    var weaponStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Weapon");
                    weaponStateMachine.SetNextState(flairDashState);
                    outer.SetNextStateToMain();
                    return;
                }

                if (CancelByAbility(skillLocator.secondary, SkillSlot.Secondary)) return;
                if (CancelByAbility(skillLocator.special, SkillSlot.Special)) return;

            }

        }

        protected virtual bool CancelByAbility(GenericSkill skill, SkillSlot slot)
        {
            bool skillPressed = false;
            switch (slot)
            {
                case SkillSlot.None:
                    break;
                case SkillSlot.Primary:
                    skillPressed = inputBank.skill1.down;
                    break;
                case SkillSlot.Secondary:
                    skillPressed = inputBank.skill2.down;
                    break;
                case SkillSlot.Utility:
                    skillPressed = inputBank.skill3.down;
                    break;
                case SkillSlot.Special:
                    skillPressed = inputBank.skill4.down;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
            if (inputBank && skillPressed)
            {
                if (skillLocator && skill && skill.stock > 0)
                {
                    // Dynamically set the next state to whatever ability is tied to the second slot
                    return skill.ExecuteIfReady();
                }
            }

            return false;
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            base.OnExit();
            rollSpeed = moveSpeedStat * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, 1);
            characterMotor.velocity = forwardDirection.normalized * rollSpeed;
            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();
            animator.SetBool(InDashing,false);

            characterMotor.disableAirControlUntilCollision = false;

            if (!usedFlairDash)
            {
                hitEnemies.Clear();
            }
            
            //disables fall damage, enables it back in onexit
            characterBody.bodyFlags = CharacterBody.BodyFlags.None;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(forwardDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            forwardDirection = reader.ReadVector3();
        }
        
        protected virtual void EnterAttack()
        {
            hasFired = true;

            if (isAuthority)
            {
                AddRecoil(-1f * attackRecoil, -2f * attackRecoil, -0.5f * attackRecoil, 0.5f * attackRecoil);
            }
        }

        protected virtual void FireAttack()
        {
            if (isAuthority)
            {
                if (attack.Fire(hitEnemies))
                {
                    OnHitEnemyAuthority();
                }
            }
        }

        protected virtual void OnHitEnemyAuthority()
        {
            Util.PlaySound("Play_SamiraSFX_E_Hit", gameObject);
            _comboManager.AddCombo(attackID);
            ApplyHitstop();
        }

        protected virtual void ApplyHitstop()
        {
            if (!inHitPause && hitStopDuration > 0f)
            {
                storedVelocity = characterMotor.velocity;
                hitStopCachedState = CreateHitStopCachedState(characterMotor, animator, playbackRateParam);
                hitPauseTimer = hitStopDuration / attackSpeedStat;
                inHitPause = true;
            }
        }
        

        protected virtual void RemoveHitstop()
        {
            ConsumeHitStopCachedState(hitStopCachedState, characterMotor, animator);
            inHitPause = false;
            characterMotor.velocity = storedVelocity;
        }

        
    }
}