using System.Collections;
using System.Collections.Generic;
using EntityStates;
using EntityStates.GoldGat;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Projectile;
using SamiraMod.Survivors.Samira.Networking;
using SamiraMod.Survivors.Samira.SkillStates;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.Components
{
    //should be handled only be server, not client, because take damage hook is only seen on server
    internal class ProjectileDamageTracker : MonoBehaviour
    {
        private SamiraComboManager _comboManager;
        private CharacterBody _characterBody;
        private ChildLocator childLocator;

        private HashSet<CharacterBody> targetedBodies = new HashSet<CharacterBody>();

        void Awake()
        {
            _comboManager = GetComponent<SamiraComboManager>();
            _characterBody = GetComponent<CharacterBody>();
            childLocator = _characterBody.modelLocator.modelTransform.GetComponent<ChildLocator>();
        }
        private void OnEnable()
        {
            // Hook into the TakeDamage method
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
           
        }



        private void OnDisable()
        {
            // Unhook to avoid memory leaks
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
        }

   

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            // Call the original method to ensure the damage is applied
            orig(self, damageInfo);
            
            if (!NetworkServer.active || damageInfo.attacker == null || damageInfo.attacker.gameObject != _characterBody.gameObject) return;
            
            var receiverTeamComponent = self.body.teamComponent;
            if (receiverTeamComponent == null) return;
            if (_characterBody.teamComponent.teamIndex == receiverTeamComponent.teamIndex) return;
            
            #region Passive Barrage
            

            var setStateOnHurt = self.body.GetComponent<SetStateOnHurt>();
            bool inStunState = setStateOnHurt != null && (setStateOnHurt.targetStateMachine.state is StunState || setStateOnHurt.targetStateMachine.state is ShockState || setStateOnHurt.targetStateMachine.state is FrozenState);
            
            if (inStunState && !targetedBodies.Contains(self.body) )
            {
                targetedBodies.Add(self.body);
                SendSoundToClient(_characterBody, "Play_SamiraSFX_Stun");
                StartCoroutine(PlayFireBarrage(self.body));
            }
            #endregion
        }

        IEnumerator PlayFireBarrage(CharacterBody targetBody)
        {
            Vector3 cachedPosition = targetBody.corePosition;
            for (int i = 0; i < SamiraStaticValues.barrageFireCount; i++)
            {
                string muzzle = i % 2 == 0 ? "PistolMuzzle" : "RevolverMuzzle";
                Vector3 origin = childLocator.FindChild(muzzle).position;
                var bulletAttack = new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = (cachedPosition - origin).normalized,
                    origin = origin,
                    damage = SamiraStaticValues.GetBarrageDamage(_characterBody.damage, _characterBody.level) ,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.None,
                    maxDistance = 500,
                    force = 0,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0f,
                    maxSpread = 0f,
                    isCrit = Util.CheckRoll(_characterBody.crit,_characterBody.master),
                    owner = gameObject,
                    smartCollision = true,
                    procChainMask = default,
                    procCoefficient = 1,
                    radius = 1f,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = Flair.tracerEffectPrefab,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    muzzleName = muzzle,
                    hitEffectPrefab = SamiraAssets.bulletHitEffect,
                    hitCallback = HitCallback,
                };
                bulletAttack.Fire();
                yield return new WaitForSeconds(SamiraStaticValues.delayBetweenShots);
            }


            yield return new WaitForSeconds(SamiraStaticValues.barrageCooldownPerTarget);
            targetedBodies.Remove(targetBody);
        }
        private bool HitCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {

            var result = BulletAttack.defaultHitCallback(bulletAttack, ref hitInfo);
            
            HealthComponent healthComponent = hitInfo.hitHurtBox ? hitInfo.hitHurtBox.healthComponent : null;
            if (healthComponent&& hitInfo.hitHurtBox.teamIndex != _characterBody.teamComponent.teamIndex)
            {
                _comboManager.AddCombo(SamiraStaticValues.passiveID);
                SendSoundToClient(_characterBody,"Play_SamiraSFX_BulletHit");
            }
            return result;
        }

        void SendSoundToClient(CharacterBody characterBody, string soundString)
        {
            var netBody = characterBody.GetComponent<NetworkIdentity>();
            if (netBody != null)
            {
                new SyncSound(soundString, netBody.netId).Send(NetworkDestination.Clients);
            }
        }
    }
}