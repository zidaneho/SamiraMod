using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using SamiraMod.Survivors.Samira.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.Components
{
    public class SamiraWildRushReset : MonoBehaviour
    {
        private void OnEnable()
        {
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
        }

        private void OnDisable()
        {
            GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
        }

        private void OnCharacterDeathGlobal(DamageReport damageReport)
        {
   
            // Ensure this runs only on the server
            if (!NetworkServer.active) return;
   
            if (damageReport == null) return;
            


            // Check if the killer exists and is a player
            var attackerBody = damageReport.attackerBody;
            if (attackerBody && attackerBody.teamComponent.teamIndex == TeamIndex.Player)
            {
                var skillLocator = attackerBody.skillLocator;
                if (skillLocator && skillLocator.utility && skillLocator.utility.skillDef.skillName == "SamiraWildRush")
                {
                    skillLocator.utility.Reset(); // Reset for host
                    // Sync with client
                    var netId = attackerBody.gameObject.GetComponent<NetworkIdentity>().netId;
                    new SyncResetWildRush(netId).Send(NetworkDestination.Clients);
                }
            }
        }
    }
}