using System;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.WwiseUtils;
using SamiraMod.Modules;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace SamiraMod.Survivors.Samira.Components
{
    public class SamiraSoundManager : MonoBehaviour
    {
        private ModelSkinController modelSkinController;
        private CharacterBody characterBody;

    


        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            modelSkinController = GetComponentInChildren<ModelSkinController>();

        }

    
        

        public uint PlaySoundBySkin(string soundName, GameObject source)
        {
            AkSoundEngine.SetRTPCValue("Samira_Voice_Volume",Config.voiceEffectVolume.Value);
            AkSoundEngine.SetRTPCValue("Samira_SFX_Volume",Config.soundEffectVolume.Value);
            
            
            if (modelSkinController == null)
            {
                return 0;
            }

            if (!characterBody.hasAuthority) return 0;
            Debug.Log("playing sound: " + soundName + " voice : " + Config.voiceEffectVolume.Value + " sfx : " + Config.soundEffectVolume.Value);
            return Util.PlaySound(GetSoundName(soundName), source);
        }

        public uint PlayAttackSpeedSoundBySkin(string soundName, GameObject source, float attackSpeedStat)
        {
            if (modelSkinController == null)
            {
                return 0;
            }
            if (!characterBody.hasAuthority) return 0;
            return Util.PlayAttackSpeedSound(GetSoundName(soundName), source,attackSpeedStat);
        }

        string GetSoundName(string soundName)
        {
            if (modelSkinController == null) return "";
            string prefix = modelSkinController.skins[modelSkinController.currentSkinIndex].name + "_";
            return prefix + soundName;
        }

        
    }
}