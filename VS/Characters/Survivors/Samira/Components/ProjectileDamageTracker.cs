using EntityStates.GoldGat;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace SamiraMod.Survivors.Samira.Components
{
    internal class ProjectileDamageTracker : MonoBehaviour
    {
        private SamiraComboManager _comboManager;
        private void Awake()
        {
            // Hook into the TakeDamage method
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            _comboManager = GetComponent<SamiraComboManager>();
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

            // Check if the damage was caused by a projectile
            if (damageInfo.inflictor && damageInfo.inflictor.GetComponent<ProjectileController>() && damageInfo.inflictor.GetComponent<CoinComponent>())
            {
                // The enemy was hit by a projectile
                
                Util.PlaySound("Play_SamiraSFX_CoinHit",self.body.gameObject);
                _comboManager.AddCombo(SamiraStaticValues.coinID);
                damageInfo.attacker.GetComponent<CharacterBody>().AddTimedBuff(SamiraBuffs.coinOnHitBuff, 10f);
                EffectManager.SimpleEffect(GoldGatFire.impactEffectPrefab, damageInfo.position, Quaternion.identity, false);

                // Additional logic can be added here
            }
        }
    }
}