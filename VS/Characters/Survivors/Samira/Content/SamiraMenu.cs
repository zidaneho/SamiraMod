using System;
using System.Collections;
using HG;
using On.RoR2.SurvivorMannequins;
using On.RoR2.UI;
using R2API.Networking;
using RoR2;
using RoR2.Skills;
using SamiraMod.Survivors.Samira;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine;
using UnityEngine.Networking;
using CharacterBody = RoR2.CharacterBody;
using ModelSkinController = RoR2.ModelSkinController;
using SkinCatalog = RoR2.SkinCatalog;
using UserProfile = RoR2.UserProfile;

namespace SamiraMod.Modules.Characters
{
    public class SamiraMenu : MonoBehaviour
    {
        private static uint playID;
        public string soundPrefix = "DefaultSkin";

        private static bool finishedGracePeriod;

        
        

        void OnEnable()
        {
            //StartCoroutine(PlayEffect());
            Invoke(nameof(GracePeriod), 0.5f);
        }
        void OnDisable() {
            finishedGracePeriod = false;
        }

        private void OnDestroy()
        {
            if (playID != 0) AkSoundEngine.StopPlayingID(playID);
        }

        public void SetAndPlaySoundPrefix(string prefix)
        {
            if (finishedGracePeriod && prefix == soundPrefix) return;
            if (playID != 0) AkSoundEngine.StopPlayingID(playID);
            this.soundPrefix = prefix;
            
            AkSoundEngine.SetRTPCValue("Samira_Voice_Volume",Config.voiceEffectVolume.Value);
            AkSoundEngine.SetRTPCValue("Samira_SFX_Volume",Config.soundEffectVolume.Value);
            playID = Util.PlaySound(soundPrefix +"_PlayVO_Menu", base.gameObject);
        
            
        }

        void GracePeriod()
        {
           
            finishedGracePeriod = true;
        }
        
    }
}