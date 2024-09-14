using System.Collections.Generic;
using System.Linq;
using EntityStates;
using EntityStates.ArtifactShell;
using R2API.Networking.Interfaces;
using RoR2;
using SamiraMod.Modules;
using SamiraMod.Survivors.Samira.Components;
using SamiraMod.Survivors.Samira.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class InfernoTrigger : BaseInfernoTrigger
    {
    }
    public abstract class BaseInfernoTrigger : BaseSkillState
    {
        protected SamiraComboManager _comboManager;
        protected Animator animator;
        protected ChildLocator childLocator;
        protected float duration = SamiraStaticValues.infernoTriggerDuration;
        protected float attackSpeedMultiplier = 0.9f;
        protected static float baseAttackInterval = 0.2f;
        protected static float lifeStealPercentage = 0.10f;
        protected int muzzleIndexer = 0;
        protected SamiraSoundManager soundManager;
        #region Attack Members

        public DamageType damageType = DamageType.Generic;
        public float attackRadius = 25f;
        public static float procCoefficient = 1f;
        protected float timer = 0f;
        protected GameObject attackIndicatorInstance;
        public static readonly int InInfernoTrigger = Animator.StringToHash("inInfernoTrigger");
        public static GameObject tracerEffectPrefab =
            LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");
        

        #endregion

        public override void OnEnter()
        {
            base.OnEnter();
            soundManager = characterBody.GetComponent<SamiraSoundManager>();
            animator = GetModelAnimator();
            _comboManager = characterBody.GetComponent<SamiraComboManager>();
            duration = SamiraStaticValues.infernoTriggerDuration;
            
            childLocator = GetModelChildLocator();
            
            
            animator.SetBool(InInfernoTrigger, true);
            PlayAnimation("FullBody, Override", "InfernoTrigger");

            if (base.isAuthority) Util.PlaySound("Play_SamiraSFX_R",gameObject);
            if (Config.enableVoiceLines.Value)
            {
                soundManager.PlaySoundBySkin("PlayVO_R",gameObject);
            }
            if (!attackIndicatorInstance) CreateIndicator();
            FireAttack();
        }

        public override void OnExit()
        {
            if (this.attackIndicatorInstance) EntityState.Destroy(this.attackIndicatorInstance);
            _comboManager.ResetCombo(true);
            animator.SetBool(InInfernoTrigger, false);
            base.OnExit();
            
        }
        
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (base.isAuthority)
            {
                float currentAttackSpeed = attackSpeedMultiplier * attackSpeedStat;
                float currentInterval = baseAttackInterval / currentAttackSpeed;
                timer += Time.deltaTime;


                if (timer >= currentInterval)
                {
                    FireAttack();
                    timer = 0f;
                }
            }
            
            

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
            else
            {
                if (attackIndicatorInstance == null) CreateIndicator();
                UpdateIndicator();
            }
            
        }
        protected virtual void FireAttack()
        {

            List<HurtBox> HurtBoxes = new List<HurtBox>();
            var sphereSearch = new SphereSearch
            {
                radius = attackRadius,
                mask = LayerIndex.entityPrecise.mask,
                origin = this.attackIndicatorInstance.transform.position
            };
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(base.teamComponent.teamIndex));
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            HurtBoxes = sphereSearch.GetHurtBoxes().ToList();



            float range = 250f;
            foreach (HurtBox hurtbox in HurtBoxes)
            {
                bool usePistol = muzzleIndexer % 2 == 0;
                muzzleIndexer += 1;
                string muzzleName = usePistol ? "PistolMuzzle" : "RevolverMuzzle";
                EffectManager.SimpleMuzzleFlash(SamiraAssets.bulletMuzzleEffect,gameObject,muzzleName,true);
                
                var bulletAttack = new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = hurtbox.transform.position - transform.position,
                    origin = childLocator.FindChild(muzzleName).position,
                    damage = SamiraStaticValues.GetInfernoTriggerDamage(damageStat,characterBody.level),
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = damageType,
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

        protected virtual bool OnBulletHit(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            var result = BulletAttack.defaultHitCallback(bulletAttack, ref hitInfo);
            
            HealthComponent enemyHealthComponent = hitInfo.hitHurtBox ? hitInfo.hitHurtBox.healthComponent : null;
            if (hitInfo.hitHurtBox != null && hitInfo.hitHurtBox.teamIndex != base.teamComponent.teamIndex)
            {
                if (base.isAuthority) Util.PlayAttackSpeedSound("Play_SamiraSFX_BulletHit", hitInfo.hitHurtBox.gameObject,attackSpeedStat);
            }
            if (enemyHealthComponent && enemyHealthComponent.alive && hitInfo.hitHurtBox.teamIndex != base.teamComponent.teamIndex)
            {
                float lifeSteal = SamiraStaticValues.GetInfernoTriggerDamage(damageStat,characterBody.level) * lifeStealPercentage;

                var bodyID = characterBody.GetComponent<NetworkIdentity>();
                if (bodyID)
                {
                    new SyncInfernoTrigger(lifeSteal, bodyID.netId).Send(R2API.Networking.NetworkDestination.Server);
                }
            }
            
            return result;
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