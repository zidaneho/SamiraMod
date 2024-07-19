using EntityStates;
using RoR2;
using UnityEngine;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class SamiraDeathState : GenericCharacterDeath
    {
        private Animator animator;
        private float velocityMagnitude = 10f;
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        public override void OnEnter() 
        {
            base.OnEnter();
            Util.PlaySound("Play_SamiraVO_Death", gameObject);
            animator = GetModelAnimator();
            if (animator)
            {
                animator.CrossFadeInFixedTime("Death",0.1f);
            }
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}