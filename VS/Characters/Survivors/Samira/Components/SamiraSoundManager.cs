using RoR2;
using RoR2.WwiseUtils;
using UnityEngine;
using UnityEngine.Serialization;

namespace SamiraMod.Survivors.Samira.Components
{
    internal class SamiraSoundManager : MonoBehaviour
    {
        private ModelSkinController modelSkinController;

        public static SamiraSoundManager instance;



        private void Awake()
        {
            if (instance == null) instance = this;
            
            modelSkinController = GetComponentInChildren<ModelSkinController>();
        }

        public uint PlaySoundBySkin(string soundName, GameObject source)
        {
            if (modelSkinController == null)
            {
                return 0;
            }
            
            return Util.PlaySound(GetSoundName(soundName), source);
        }

        public uint PlayAttackSpeedSoundBySkin(string soundName, GameObject source, float attackSpeedStat)
        {
            if (modelSkinController == null)
            {
                return 0;
            }
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