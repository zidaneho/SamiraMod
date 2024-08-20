using RoR2;
using RoR2.Skills;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;

namespace SamiraMod.Survivors.Samira
{
    public class InfernoTriggerSkillDef : SkillDef
    {
        public override bool IsReady(GenericSkill skillSlot)
        {
            if (base.IsReady(skillSlot))
            {
                return HasRequiredCombo(skillSlot);
            }

            return false;
        }

        bool HasRequiredCombo(GenericSkill skillSlot)
        {
            var comboManager = skillSlot.characterBody.GetComponent<SamiraComboManager>();
            return comboManager.ComboIndex >= comboManager.maximumCombo;
        }
    }
}