using System;
using System.Collections.Generic;
using System.Linq;
using EntityStates;
using On.EntityStates.Huntress;
using Rewired.ComponentControls;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class BladeWhirl : BaseSkillState
    {
        public static float duration = 0.6f;
        public static float baseJumpSpeed = 2f;
        public static float initialSpeedCoefficient = 1.3f;
        public static float finalSpeedCoefficient = .5f;
        public static int attackID = SamiraStaticValues.bladeWhirlID;
        public static float searchRadius = SamiraStaticValues.bladeWhirlRadius;
        public static float dodgeFOV = global::EntityStates.Commando.DodgeState.dodgeFOV;
        
        protected Animator animator;
        protected SamiraSoundManager soundManager;
        protected float jumpSpeed;
        protected SamiraComboManager _comboManager;
        private SamiraBladeWhirlHandler _bladeWhirlHandler;
        protected static readonly int InDashing = Animator.StringToHash("inDashing");

        #region Attack Members
        
        public static float procCoefficient = 1f;
        
        protected bool hasFired;
        protected float stopwatch;
        

        #endregion

        public override void OnEnter()
        {
            base.OnEnter();
            soundManager = characterBody.GetComponent<SamiraSoundManager>();
            animator = GetModelAnimator();
            _comboManager = characterBody.GetComponent<SamiraComboManager>();
            _bladeWhirlHandler = characterBody.GetComponent<SamiraBladeWhirlHandler>();
            
            base.characterMotor.Motor.ForceUnground();
            
            RecalculateJumpSpeed();

            characterMotor.disableAirControlUntilCollision = false;
            
            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = characterDirection.moveVector.normalized * moveSpeedStat;
                characterMotor.velocity.y = jumpSpeed;
            }

            PlayAnimation("FullBody, Override", "BladeWhirl", "BladeWhirl.playbackRate", duration);
            if (base.isAuthority) Util.PlaySound("Play_SamiraSFX_W", gameObject);
            
            _bladeWhirlHandler.SpawnInstance(this);
            
        }

        public virtual void RecalculateJumpSpeed()
        {
            jumpSpeed = baseJumpSpeed * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, fixedAge / duration);
            
            if (characterMotor && characterDirection && !animator.GetBool(InDashing))
            {
                //float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                //vector = forwardDirection * d;
                //vector.y = 0f;

                characterMotor.velocity = characterDirection.moveVector.normalized * moveSpeedStat;
                characterMotor.velocity.y = jumpSpeed;
            }

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }

            
            stopwatch += Time.deltaTime;
            

            if (stopwatch >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }

            if (CancelByAbility(skillLocator.special, SkillSlot.Special)) return;
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            base.OnExit();
            characterMotor.disableAirControlUntilCollision = false;
        }
        
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
        
        public virtual bool CancelByAbility(GenericSkill skill, SkillSlot slot)
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
                    bool executed = skill.IsReady();
                    if (executed)
                    {
                        skill.OnExecute();
                        EntityState state = EntityStateCatalog.InstantiateState(skill.activationState._stateType);
                        outer.SetNextState(state);
                    }
                    return executed;
                }
                
            }

            return false;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader); 
        }
        
        

        public virtual void FireAttack()
        {
            //
            List<HurtBox> HurtBoxes = new List<HurtBox>();
            HurtBoxes = new SphereSearch
            {
                radius = searchRadius,
                mask = LayerIndex.entityPrecise.mask,
                origin = characterBody.corePosition,
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(base.teamComponent.teamIndex)).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().ToList();
            

            bool hitEnemy = false;
            foreach (HurtBox hurtbox in HurtBoxes)
            {

                if (NetworkServer.active)
                {
                    Vector3 force = hurtbox.transform.position - transform.position;
                    force.Normalize();
                    force *= 3f;
                    
                    DamageInfo damageInfo = new DamageInfo();
                    damageInfo.damage = SamiraStaticValues.GetBladeWhirlDamage(damageStat, characterBody.level);
                    damageInfo.attacker = base.gameObject;
                    damageInfo.inflictor = base.gameObject;
                    damageInfo.force = force;
                    damageInfo.crit = base.RollCrit();
                    damageInfo.procCoefficient = procCoefficient;
                    damageInfo.position = hurtbox.gameObject.transform.position;
                    damageInfo.damageType = DamageType.Generic;

                    hurtbox.healthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, hurtbox.healthComponent.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, hurtbox.healthComponent.gameObject);   
                }
                hitEnemy = true;
            }

            if (hitEnemy)
            {
                _comboManager.AddCombo(attackID);
                if (base.isAuthority) Util.PlaySound("Play_SamiraSFX_W_Hit", gameObject);
                soundManager.PlaySoundBySkin("PlayVO_W",gameObject);
            }
        }

        public virtual void BeginWhirl()
        {
            FireAttack();
        }

        public virtual void EndWhirl()
        {
            FireAttack();
        }
        
        
        
        
    }
}