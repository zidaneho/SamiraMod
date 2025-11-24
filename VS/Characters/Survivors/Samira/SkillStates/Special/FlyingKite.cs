using System;
using System.Collections.Generic;
using System.Linq;
using EntityStates;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Audio;
using RoR2.Skills;
using SamiraMod.Survivors.Samira.Components;
using SamiraMod.Survivors.Samira.Networking;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class FlyingKite : BaseSkillState
    {
        public virtual int attacksPerFlair => SamiraStaticValues.attacksPerFlair;
        private static float maxDistance = 10f;
        private static float searchAngle = 45f;
        protected DamageType damageType = DamageType.Generic;
        public static float procCoefficient = 1f;

        private static float buffDuration = 3f;

        private float duration;
        private bool hasFired;
        private bool crit;
        private ChildLocator childLocator;
        private SamiraBuffMeleeOnHitHandler buffHandler;
        protected SamiraSoundManager soundManager;
        protected SamiraComboManager _comboManager;

        #region Melee Members

        private int swingIndex;

        private string hitboxGroupName = "AAHitbox";

        private float pushForce = 300f;
        private Vector3 bonusForce = Vector3.zero;
        private float meleeBaseDuration = 0.5f;

        private float attackStartPercentTime = 0.2f;
        private float attackEndPercentTime = 0.8f;

        private float earlyExitPercentTime = 0.4f;
        private float attackRadius = 100f;
        private float hitStopDuration = 0.012f;
        private float attackRecoil = 0.75f;
        private float hitHopVelocity = 4f;
        private string meleeMuzzleString = "SwingCenter";
        protected string playbackRateParam;
        private GameObject swingEffectPrefab;
        protected GameObject hitEffectPrefab;
        private NetworkSoundEventIndex impactSound = NetworkSoundEventIndex.Invalid;

        private float hitPauseTimer;
        private OverlapAttack attack;
        private bool inHitPause;
        private bool hasHopped;
        private float stopwatch;
        private Animator animator;
        private HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;
        private GameObject attackIndicatorInstance;

        private int flyingKiteHitCounter;

        #endregion

        private bool canUseFlair => swingIndex >= attacksPerFlair - 1;

        public override void OnEnter()
        {
            base.OnEnter();
            soundManager = characterBody.GetComponent<SamiraSoundManager>();
            _comboManager = characterBody.GetComponent<SamiraComboManager>();
            animator = GetModelAnimator();
            buffHandler = characterBody.GetComponent<SamiraBuffMeleeOnHitHandler>();
            crit = RollCrit();
            this.childLocator = GetModelChildLocator();
            
            SetupMeleeAttack();
            if (base.characterBody)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            if (characterMotor)
            {
                characterMotor.useGravity = false;
            }

            flyingKiteHitCounter = SamiraStaticValues.flyingKiteHits;

            if (!TeleportToFarthestEnemy())
            {
                outer.SetNextStateToMain();
            }
           
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (characterDirection)
            {
                characterDirection.moveVector = Vector3.zero;
            }

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
            
            if (attackIndicatorInstance == null) CreateIndicator();
            UpdateIndicator();

            if (fireEnded && flyingKiteHitCounter > 0)
            {
                crit = RollCrit();
                SetupMeleeAttack();
                stopwatch = 0;
                hasFired = false;
           
                flyingKiteHitCounter -= 1;
                if (!TeleportToFarthestEnemy())
                {
                    outer.SetNextStateToMain();
                }
            }
            else if (flyingKiteHitCounter <= 0)
            {
                outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            if (this.attackIndicatorInstance) EntityState.Destroy(this.attackIndicatorInstance);
            if (base.characterBody)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
            if (characterMotor)
            {
                characterMotor.useGravity = true;
            }
            _comboManager.ResetCombo(true);
            base.OnExit();
            
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

        bool TeleportToFarthestEnemy()
        {
            List<HurtBox> HurtBoxes = new List<HurtBox>();
            var sphereSearch = new SphereSearch
            {
                radius = attackRadius,
                mask = LayerIndex.entityPrecise.mask,
                origin = this.characterBody.coreTransform.position,
            };
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(base.teamComponent.teamIndex));
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            sphereSearch.OrderCandidatesByDistance();
            HurtBoxes = sphereSearch.GetHurtBoxes().ToList();

            for (int i = HurtBoxes.Count - 1; i >= 0; i--)
            {
                if (!HurtBoxes[i].healthComponent.alive)
                {
                    continue;
                }
                Transform target = HurtBoxes[i].healthComponent.body.coreTransform;

                int radius = 3;
                Vector3 center =  target.position;
                float angle = Random.Range(0, 2 * Mathf.PI);

                Vector3 teleportPos = new Vector3(center.x + radius * Mathf.Cos(angle), center.y, center.z + radius * Mathf.Sin(angle));
                teleportPos.y -= 1.5f; //account for feet position
                
               
                TeleportHelper.TeleportBody(characterBody,teleportPos,false);
                characterDirection.forward = (target.position - teleportPos).normalized;
                return true;
                
            }
            

            

            return false;
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
            PlayAnimation("FullBody, Override", animString, playbackRateParam, duration );
            swingIndex = (swingIndex + 1) % 2;
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
            
            float lifeSteal = SamiraStaticValues.GetInfernoTriggerDamage(damageStat,characterBody.level) * SamiraStaticValues.lifeStealPercentage;

            var bodyID = characterBody.GetComponent<NetworkIdentity>();
            if (bodyID)
            {
                new SyncInfernoTrigger(lifeSteal, bodyID.netId).Send(R2API.Networking.NetworkDestination.Server);
            }
            
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
        protected void CreateIndicator()
        {
            if (EntityStates.Huntress.ArrowRain.areaIndicatorPrefab)
            {
                Vector3 spawnPos = characterBody.corePosition;
                
                attackIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab);
                attackIndicatorInstance.transform.localScale = Vector3.one * attackRadius;
                attackIndicatorInstance.transform.position = spawnPos;
            }
        }

        protected void UpdateIndicator()
        {
            attackIndicatorInstance.transform.position = characterBody.corePosition;
        }
    }
}