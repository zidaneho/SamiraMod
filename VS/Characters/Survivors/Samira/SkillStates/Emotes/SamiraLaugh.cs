using SamiraMod.Survivors.Samira.Components;

namespace SamiraMod.Survivors.Samira.SkillStates.Emotes
{
    public class SamiraLaugh : SamiraBaseEmote
    {
        private uint sfxID;
        private uint voID;
        public override void OnEnter()
        {
            animString = "Laugh";
            duration = 3.91f; //24 fps, anim is 91 frames
            base.OnEnter();

            if (base.isAuthority)
            {
                sfxID = RoR2.Util.PlaySound("Play_SamiraSFX_Laugh", gameObject);
                voID = soundManager.PlaySoundBySkin("PlayVO_Laugh", gameObject);   
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (sfxID != 0) AkSoundEngine.StopPlayingID(sfxID);
            if (voID != 0) AkSoundEngine.StopPlayingID(voID);
        }
    }
}