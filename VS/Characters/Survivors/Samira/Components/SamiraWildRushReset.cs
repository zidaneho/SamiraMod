using System;
using EntityStates;
using On.RoR2.Mecanim;
using R2API.Networking.Interfaces;
using RoR2;
using SamiraMod.Survivors.Samira.Networking;
using SamiraMod.Survivors.Samira.SkillStates;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.Components
{
    internal class SamiraWildRushReset : MonoBehaviour
    {
        void OnEnable()
        {
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeath;
        }

        void OnDisable()
        {
            GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeath;
        }

        private void OnCharacterDeath(DamageReport damageReport)
        {
            // Check if the killer is a player
            
            if (damageReport.attacker != null && damageReport.attacker.GetComponent<CharacterBody>() != null)
            {
                CharacterBody attackerBody = damageReport.attacker.GetComponent<CharacterBody>();
                
                var networkBody = attackerBody.GetComponent<NetworkIdentity>();
                if (networkBody != null)
                {
                    new SyncWildRushReset(networkBody.netId).Send(R2API.Networking.NetworkDestination.Server);
                }
            }
        }
        
    }
}