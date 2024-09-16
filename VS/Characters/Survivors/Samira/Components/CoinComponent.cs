using System;
using EntityStates.GoldGat;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Projectile;
using SamiraMod.Survivors.Samira.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.Components
{
    //ID for damage checker
    //Projectile is always handled by the server
    internal class CoinComponent : MonoBehaviour
    {
        private ProjectileController projectileController;
        private ProjectileImpactExplosion _impactExplosion;

        private bool playedImpact;

        private void Awake()
        {
            projectileController = GetComponent<ProjectileController>();
            _impactExplosion = GetComponent<ProjectileImpactExplosion>();
            
        }

        private void OnEnable()
        {
            playedImpact = false;
        }


        private void Update()
        {
            if (!NetworkServer.active) return;
            
            if (!_impactExplosion.alive && !playedImpact && projectileController != null && projectileController.owner != null)
            {
                playedImpact = true;
                CharacterBody player = projectileController.owner.GetComponent<CharacterBody>();
                if (player == null) return;
                SamiraComboManager comboManager = player.GetComponent<SamiraComboManager>();
                if (comboManager == null) return;
                player.AddTimedBuff(SamiraBuffs.coinOnHitBuff, 10f, 1);
                comboManager.AddCombo(SamiraStaticValues.coinID);
                EffectManager.SimpleEffect(GoldGatFire.impactEffectPrefab, transform.position, Quaternion.identity, true);
                
                var networkBody = player.networkIdentity;
                if (networkBody != null)
                {
                    new SyncSound("Play_SamiraSFX_CoinHit", networkBody.netId).Send(R2API.Networking.NetworkDestination.Clients);
                }
            }
        }
    }
}