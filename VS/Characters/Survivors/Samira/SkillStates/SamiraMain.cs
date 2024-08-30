using EntityStates;
using SamiraMod.Survivors.Samira.SkillStates.Emotes;
using UnityEngine;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class SamiraMain : GenericCharacterMain
    {
        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void Update()
        {
            base.Update();

            if (isAuthority && characterMotor.isGrounded)
            {
                if (TryPlayEmote(Modules.Config.tauntKeybind.Value, new SamiraTaunt())) return;
                if (TryPlayEmote(Modules.Config.jokeKeybind.Value, new SamiraJoke())) return;
                if (TryPlayEmote(Modules.Config.laughKeybind.Value, new SamiraLaugh())) return;
                if (TryPlayEmote(Modules.Config.danceKeybind.Value, new SamiraDance())) return;
            }
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