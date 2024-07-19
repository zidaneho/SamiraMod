using System.Collections.Generic;
using System.Linq;
using EntityStates;
using EntityStates.ArtifactShell;
using RoR2;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class InfernoTrigger : BaseSkillState
    {
        private SamiraComboManager _comboManager;
        private Animator animator;
        private static int attackID = 4;
        private float duration = SamiraStaticValues.infernoTriggerDuration;
        private static float durationExtendOnKill = 0.25f;
        private static readonly int InDashing = Animator.StringToHash("inDashing");
        private float attackSpeedMultiplier = 1f;
        private static float baseAttackInterval = 0.2f;
        private static float lifeStealPercentage = 0.1f;
        private int muzzleIndexer = 0;
        #region Attack Members

        public float attackRadius = 25f;
        public static float procCoefficient = 1f;
        
        private OverlapAttack attack;
        private float timer = 0f;
        private HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;
        private GameObject attackIndicatorInstance;
        private static readonly int InInfernoTrigger = Animator.StringToHash("inInfernoTrigger");
        public static GameObject tracerEffectPrefab =
            LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

        private ParticleSystem revolverMuzzleParticle;
        private ParticleSystem pistolMuzzleParticle;

        private string playbackRateParam = "InfernoTrigger.playbackRate";

        #endregion

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            _comboManager = characterBody.GetComponent<SamiraComboManager>();
            duration = SamiraStaticValues.infernoTriggerDuration;
            
            var childLocator = GetModelChildLocator();
            if (childLocator)
            {
                var pistolMuzzleTransform = childLocator.FindChild("PistolMuzzle");
                if (pistolMuzzleTransform)
                {
                    pistolMuzzleParticle = pistolMuzzleTransform.GetComponentInChildren<ParticleSystem>();
                }

                var revolverMuzzleTransform = childLocator.FindChild("RevolverMuzzle");
                if (revolverMuzzleTransform)
                {
                    revolverMuzzleParticle = revolverMuzzleTransform.GetComponentInChildren<ParticleSystem>();
                }

            }
            
            
            animator.SetBool(InInfernoTrigger, true);
            PlayAnimation("FullBody, Override", "InfernoTrigger");

            Util.PlaySound("Play_SamiraSFX_R",gameObject);
            if (!attackIndicatorInstance) CreateIndicator();
            FireAttack();
        }

        public override void OnExit()
        {
            if (this.attackIndicatorInstance) EntityState.Destroy(this.attackIndicatorInstance);
            _comboManager.ResetCombo(true);
            base.OnExit();
        }
        
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            //to guarantee attack comes out if at high attack speed the stopwatch skips past the firing duration between frames
            float currentAttackSpeed = attackSpeedMultiplier * attackSpeedStat;
            float currentInterval = baseAttackInterval / currentAttackSpeed;
            timer += Time.fixedDeltaTime;
            if (timer >= currentInterval)
            {
                FireAttack();
                timer = 0f;
            }

            if (fixedAge >= duration && isAuthority)
            {
                animator.SetBool(InInfernoTrigger,false);
                if (this.attackIndicatorInstance != null) EntityState.Destroy(this.attackIndicatorInstance);
                outer.SetNextStateToMain();
            }
            else
            {
                if (attackIndicatorInstance == null) CreateIndicator();
                UpdateIndicator();
            }
        }
        private void FireAttack()
        {
            
            List<HurtBox> HurtBoxes = new List<HurtBox>();
            HurtBoxes = new SphereSearch
            {
                radius = attackRadius,
                mask = LayerIndex.entityPrecise.mask,
                origin = this.attackIndicatorInstance.transform.position
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(base.teamComponent.teamIndex)).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().ToList();

            if (HurtBoxes.Count > 0)
            {
                if (pistolMuzzleParticle) pistolMuzzleParticle.Play();
                if (revolverMuzzleParticle) revolverMuzzleParticle.Play();
            }

            bool usePistol = muzzleIndexer % 2 == 0;
            muzzleIndexer += 1;
            
            string muzzleName = usePistol ? "Pistol_Muzzle" : "Revolver_Muzzle";
            if (usePistol && HurtBoxes.Count > 0)
            {
                if (pistolMuzzleParticle) pistolMuzzleParticle.Play();
            }
            else if (HurtBoxes.Count > 0)
            {
                if (revolverMuzzleParticle) revolverMuzzleParticle.Play();
            }
            float range = 250f;
            foreach (HurtBox hurtbox in HurtBoxes)
            {
                var bulletAttack = new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = hurtbox.transform.position - transform.position,
                    origin = characterBody.corePosition,
                    damage = SamiraStaticValues.GetInfernoTriggerDamage(damageStat,characterBody.level),
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.None,
                    maxDistance = range,
                    force = 0f,
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
                    muzzleName = muzzleName,
                    hitEffectPrefab = SamiraAssets.bulletHitEffect,
                    hitCallback = OnBulletHit
                };
                bulletAttack.Fire();
                
            }
            
        }

        bool OnBulletHit(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            // Custom logic when a bullet hits something
            if (hitInfo.hitHurtBox)
            {
                HealthComponent healthComponent = hitInfo.hitHurtBox.healthComponent;
                if (healthComponent != null)
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        damage = bulletAttack.damage,
                        attacker = bulletAttack.owner,
                        crit = bulletAttack.isCrit,
                        procChainMask = bulletAttack.procChainMask,
                        procCoefficient = bulletAttack.procCoefficient,
                        position = hitInfo.point,
                        force = bulletAttack.force * bulletAttack.aimVector,
                        damageType = bulletAttack.damageType
                    };
                    
                    healthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, hitInfo.hitHurtBox.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, hitInfo.hitHurtBox.gameObject);
                    
                    //additional logic
                    Util.PlayAttackSpeedSound("Play_SamiraSFX_BulletHit", hitInfo.hitHurtBox.gameObject, attackSpeedMultiplier);
                    float lifeSteal = SamiraStaticValues.GetInfernoTriggerDamage(damageStat,characterBody.level) * lifeStealPercentage;
                    healthComponent.Heal(lifeSteal, default(ProcChainMask), true);

                    if (!healthComponent.alive)
                    {
                        duration += durationExtendOnKill;
                        Debug.Log("Inferno Trigger duration extended because enemy was killed");   
                    }
                    
                    return true; // Indicate that the hit was processed
                }
            }

            return false; // Indicate that the hit was not processed
        }
        
        void CreateIndicator()
        {
            if (EntityStates.Huntress.ArrowRain.areaIndicatorPrefab)
            {
                Vector3 spawnPos = characterBody.corePosition;
                
                attackIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab);
                attackIndicatorInstance.transform.localScale = Vector3.one * attackRadius;
                attackIndicatorInstance.transform.position = spawnPos;
            }
        }

        void UpdateIndicator()
        {
            attackIndicatorInstance.transform.position = characterBody.corePosition;
        }
        
    }
}