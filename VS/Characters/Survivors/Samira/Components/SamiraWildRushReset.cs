using RoR2;
using UnityEngine;

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

                // Check if the attackerBody is a player
                if (attackerBody.isPlayerControlled)
                {
                    Debug.Log($"Player {attackerBody.GetUserName()} killed {damageReport.victim.name}");
                
                    // Reset skill cooldowns for the player
                    ResetSkillCooldowns(attackerBody);
                }
            }
        }

        private void ResetSkillCooldowns(CharacterBody player)
        {
            // Get the player's skill locators
            SkillLocator skillLocator = player.skillLocator;

            if (skillLocator != null)
            {
                if (skillLocator.utility != null)
                {
                    skillLocator.utility.Reset();
                    Debug.Log("Utility skill cooldown reset.");
                }
                // If you only want to reset a specific skill, adjust the logic accordingly
                // Example: Resetting only the utility skill
                // if (skillLocator.utility != null)
                // {
                //     skillLocator.utility.Reset();
                //     Debug.Log("Utility skill cooldown reset.");
                // }
            }
        }
    }
}