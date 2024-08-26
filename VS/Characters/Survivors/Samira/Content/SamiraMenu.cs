using System;
using On.RoR2.UI;
using RoR2;
using RoR2.Skills;
using SamiraMod.Survivors.Samira;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;
using CharacterBody = RoR2.CharacterBody;
using ModelSkinController = RoR2.ModelSkinController;
using SkinCatalog = RoR2.SkinCatalog;
using UserProfile = RoR2.UserProfile;

namespace SamiraMod.Modules.Characters
{
    public class SamiraMenu : MonoBehaviour
    {
        private uint playID;
        private uint playID2;
        public string soundPrefix = "DefaultSkin";
        

        private void OnDestroy()
        {
            if (this.playID != 0) AkSoundEngine.StopPlayingID(this.playID);
            if (this.playID2 != 0) AkSoundEngine.StopPlayingID(this.playID2);
        }

        private void OnEnable()
        {
            this.Invoke("PlayEffect", 0.05f);
        }
        

        private void PlayEffect()
        {
            if (Modules.Config.enableVoiceLines.Value)
            {
                this.playID = Util.PlaySound(Config.lastSkinName.Value +"_PlayVO_Menu", base.gameObject);
            }
            //this.playID2 = Util.PlaySound("SettMenuSFX", base.gameObject);
        }
        
    }
}