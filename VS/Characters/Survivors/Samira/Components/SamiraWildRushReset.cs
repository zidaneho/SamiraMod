using System;
using EntityStates;
using On.RoR2.Mecanim;
using RoR2;
using SamiraMod.Survivors.Samira.SkillStates;
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
                
                    // Reset skill cooldowns for the player
                    ResetSkillCooldowns(attackerBody);
                }
            }
        }
        // We only want the regular Wild Rush to reset cooldown;
        // We do not want Quick Steps to reset cooldown.
        private void ResetSkillCooldowns(CharacterBody player)
        {
            var skill = GetUtilitySkill(player);
            Debug.Log(skill + skill.skillDef.skillName);
            if (skill && skill.skillDef.skillName == "SamiraWildRush")
            { 
                skill.Reset();
            }
        }

        GenericSkill GetUtilitySkill(CharacterBody characterBody)
        {
            return characterBody?.skillLocator?.utility;
        }
    }
}