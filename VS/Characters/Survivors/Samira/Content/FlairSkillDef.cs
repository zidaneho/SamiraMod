using RoR2;
using RoR2.Skills;
using SamiraMod.Survivors.Samira.Components;
using SamiraMod.Survivors.Samira.SkillStates;
using UnityEngine;

namespace SamiraMod.Survivors.Samira
{
    public class FlairSkillDef : SteppedSkillDef
    {
        public override bool IsReady(GenericSkill skillSlot)
        {
            if (base.IsReady(skillSlot))
            {
                return !UsingInfernoTrigger(skillSlot);
            }

            return false;
        }

        bool UsingInfernoTrigger(GenericSkill skillSlot)
        {
            var component = skillSlot.characterBody.modelLocator.modelTransform.GetComponent<Animator>();
            if (component) return component.GetBool(BaseInfernoTrigger.InInfernoTrigger);
            return false;
        }
    }
}