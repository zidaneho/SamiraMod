using EntityStates.GoldGat;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Projectile;
using SamiraMod.Survivors.Samira.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.Components
{
    internal class ProjectileDamageTracker : MonoBehaviour
    {
        private SamiraComboManager _comboManager;
        private CharacterBody _characterBody;
        private void Awake()
        {
            // Hook into the TakeDamage method
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            _comboManager = GetComponent<SamiraComboManager>();
            _characterBody = GetComponent<CharacterBody>();
        }

        private void OnDestroy()
        {
            // Unhook to avoid memory leaks
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            // Call the original method to ensure the damage is applied
            orig(self, damageInfo);

            if (!_characterBody.hasAuthority) return;

            // Check if the damage was caused by a projectile
            if (damageInfo.inflictor && damageInfo.inflictor.GetComponent<ProjectileController>() && damageInfo.inflictor.GetComponent<CoinComponent>())
            {
                // The enemy was hit by a projectile
                
                if (self.body.hasAuthority) Util.PlaySound("Play_SamiraSFX_CoinHit",self.body.gameObject);
                _comboManager.AddCombo(SamiraStaticValues.coinID);

              

                var networkBody = damageInfo.attacker.GetComponent<NetworkIdentity>();
                if (networkBody != null)
                {
                    new SyncTimedBuff(SamiraBuffs.coinOnHitBuff.buffIndex, 10f, 1, networkBody.netId).Send(R2API.Networking.NetworkDestination.Server);
                }

                EffectManager.SimpleEffect(GoldGatFire.impactEffectPrefab, damageInfo.position, Quaternion.identity, false);

                // Additional logic can be added here
            }
        }
    }
}