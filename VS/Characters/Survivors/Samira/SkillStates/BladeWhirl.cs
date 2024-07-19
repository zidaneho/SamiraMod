using System.Collections.Generic;
using System.Linq;
using EntityStates;
using On.EntityStates.Huntress;
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
        public static float duration = SamiraStaticValues.bladeWhirlDuration;
        public static float baseJumpSpeed = 2f;
        public static float initialSpeedCoefficient = 1.3f;
        public static float finalSpeedCoefficient = .5f;
        public static int attackID = 2;
        public static float searchRadius = 15f;
        public static int projectileStack = 50;

        public static string dodgeSoundString = "HenryRoll";
        public static float dodgeFOV = global::EntityStates.Commando.DodgeState.dodgeFOV;
        
        private Animator animator;
        private float jumpSpeed;
        private SamiraComboManager _comboManager;
        private static readonly int InDashing = Animator.StringToHash("inDashing");

        #region Attack Members
        
        public static float procCoefficient = 1f;

        protected float attackStartPercentTime = 0.2f;
        protected float attackEndPercentTime = 0.8f;
        
        protected float attackRecoil = 0.75f;
        protected string meleeMuzzleString = "SwingCenter";
        protected string swingSoundString = "";
        protected string hitSoundString = "";
        protected string playbackRateParam = "Slash.playbackRate";
        protected GameObject swingEffectPrefab;
        
        private OverlapAttack attack;
        protected bool inHitPause;
        private bool hasFired;
        protected float stopwatch;
        private HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;
        private GameObject attackIndicatorInstance;
        

        #endregion

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            _comboManager = characterBody.GetComponent<SamiraComboManager>();
            
            base.characterMotor.Motor.ForceUnground();
            
            RecalculateJumpSpeed();

            characterMotor.disableAirControlUntilCollision = false;
            
            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = characterDirection.moveVector.normalized * moveSpeedStat;
                characterMotor.velocity.y = jumpSpeed;
            }

            PlayAnimation("FullBody, Override", "BladeWhirl", "BladeWhirl.playbackRate", duration);
            Util.PlaySound("Play_SamiraSFX_W", gameObject);

            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(SamiraBuffs.armorBuff, 3f * duration);
                characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 0.5f * duration);
            }
            
            if (!attackIndicatorInstance) CreateIndicator();
        }
        
       

        private void RecalculateJumpSpeed()
        {
            jumpSpeed = baseJumpSpeed * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, fixedAge / duration);
            
            if (!attackIndicatorInstance) CreateIndicator();
            UpdateIndicator();
            
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
            if ((fireStarted && !fireEnded || fireStarted && fireEnded) && !hasFired)
            {
                if (!hasFired)
                {
                    EnterAttack();
                }
                FireAttack();
            }
            else if (fireStarted && !fireEnded || fireStarted && fireEnded)
            {
                if (DeleteNearbyProjectiles() && Modules.Config.enableVoiceLines.Value)
                {
                    Util.PlaySound("Play_SamiraVO_W", gameObject);
                }
            }

            if (stopwatch >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            if (this.attackIndicatorInstance) EntityState.Destroy(this.attackIndicatorInstance);

            base.OnExit();

            characterMotor.disableAirControlUntilCollision = false;
            bool hitEnemy = FireAttack();
            if (hitEnemy) Util.PlaySound("Play_samira_w_hit", gameObject);
        }
        
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader); 
        }
        
        private void EnterAttack()
        {
            hasFired = true;
            Util.PlayAttackSpeedSound(swingSoundString, gameObject, attackSpeedStat);

            PlaySwingEffect();

            if (isAuthority)
            {
                AddRecoil(-1f * attackRecoil, -2f * attackRecoil, -0.5f * attackRecoil, 0.5f * attackRecoil);
            }
        }
        

        private bool FireAttack()
        {
            List<HurtBox> HurtBoxes = new List<HurtBox>();
            HurtBoxes = new SphereSearch
            {
                radius = searchRadius,
                mask = LayerIndex.entityPrecise.mask,
                origin = this.attackIndicatorInstance.transform.position
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(base.teamComponent.teamIndex)).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().ToList();
            

            bool hitEnemy = false;
            foreach (HurtBox hurtbox in HurtBoxes)
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
                hitEnemy = true;
            }

            if (hitEnemy)
            {
                _comboManager.AddCombo(attackID);
            }
            
            

            return hitEnemy;
        }

        protected virtual void PlaySwingEffect()
        {
            EffectManager.SimpleMuzzleFlash(swingEffectPrefab, gameObject, meleeMuzzleString, false);
        }

        void CreateIndicator()
        {
            if (EntityStates.Huntress.ArrowRain.areaIndicatorPrefab)
            {
                Vector3 spawnPos = characterBody.corePosition;
                
                attackIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab);
                attackIndicatorInstance.transform.localScale = Vector3.one * 15f;
                attackIndicatorInstance.transform.position = spawnPos;
            }
        }

        void UpdateIndicator()
        {
            attackIndicatorInstance.transform.position = characterBody.corePosition;
        }

        bool DeleteNearbyProjectiles()
        {
            Vector3 vector = base.characterBody ? base.characterBody.corePosition : Vector3.zero;
            TeamIndex teamIndex = base.characterBody ? base.characterBody.teamComponent.teamIndex : TeamIndex.None;
            float num = searchRadius * searchRadius;
            int num2 = 0;
            bool result = false;
            List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
            List<ProjectileController> list = new List<ProjectileController>();
            int num3 = 0;
            int count = instancesList.Count;
            while (num3 < count && num2 < projectileStack)
            {
                ProjectileController projectileController = instancesList[num3];
                if (!projectileController.cannotBeDeleted && projectileController.teamFilter.teamIndex != teamIndex && (projectileController.transform.position - vector).sqrMagnitude < num)
                {
                    list.Add(projectileController);
                    num2++;
                }
                num3++;
            }
            int i = 0;
            int count2 = list.Count;
            while (i < count2)
            {
                ProjectileController projectileController2 = list[i];
                if (projectileController2)
                {
                    result = true;
                    Vector3 position = projectileController2.transform.position;
                    Vector3 start = vector;
                    EntityState.Destroy(projectileController2.gameObject);
                }
                i++;
            }
            return result;
        }
        
    }
}