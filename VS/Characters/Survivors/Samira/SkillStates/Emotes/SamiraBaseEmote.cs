using EntityStates;
using RoR2;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.SkillStates.Emotes
{
    public class SamiraBaseEmote : BaseSkillState
    {
        public string animString;
        public float duration; // if the duration is infinite, set it below zero
        public float playbackRate = 1f;
        public string playbackRateString = "Emote.playbackRate";

        public float cancelTimeByMoveVector = 0.5f;

        public override void OnEnter()
        {
            base.OnEnter();

            var animator = GetModelAnimator();
            animator.SetFloat(playbackRateString, playbackRate);
            if (duration >= 0) PlayAnimation("FullBody, Override", animString, playbackRateString, duration);
            else
            {
                PlayAnimation("FullBody, Override", animString);
            }

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && characterMotor.isGrounded)
            {
                if (TryPlayEmote(Modules.Config.tauntKeybind.Value, new SamiraTaunt())) return;
                if (TryPlayEmote(Modules.Config.laughKeybind.Value, new SamiraLaugh())) return;
                if (TryPlayEmote(Modules.Config.danceKeybind.Value, new SamiraDance())) return;
                if (TryPlayEmote(Modules.Config.jokeKeybind.Value, new SamiraJoke())) return;
            }


            if (ShouldExitState())
            {
                outer.SetNextStateToMain();
            }
        }


        public override void OnExit()
        {
            base.OnExit();
            PlayCrossfade("FullBody, Override", "BufferEmpty",0.25f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }

        bool ShouldExitState()
        {
            return inputBank.skill1.down || inputBank.skill2.down || inputBank.skill3.down || inputBank.skill4.down ||
                   inputBank.jump.down || 
                   (inputBank.moveVector != Vector3.zero && fixedAge >= cancelTimeByMoveVector) ||
                   (duration >= 0 && fixedAge >= duration);
        }
        bool TryPlayEmote(KeyCode keybind, BaseSkillState state)
        {
            if (Input.GetKeyDown(keybind))
            {
                outer.SetInterruptState(state, InterruptPriority.Any);
                return true;
            }

            return false;
        }
    }
}