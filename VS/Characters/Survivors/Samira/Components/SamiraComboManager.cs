using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using RoR2;
using RoR2.Skills;
using SamiraMod.Modules;
using UnityEngine;
using BodyCatalog = On.RoR2.BodyCatalog;
using Path = RoR2.Path;

namespace SamiraMod.Survivors.Samira.Components
{
    public class SamiraComboManager : MonoBehaviour
    {
        public int ComboIndex { get; private set; }

        public readonly int minimumCombo = 0;
        public readonly int maximumCombo = 6;

        private int previousAttackID;
        private uint previousSoundID;

        private float timer;
        private float comboResetInterval = SamiraStaticValues.styleDuration;

        private CharacterBody characterBody;
        private SkillLocator skillLocator;
        private GenericSkill specialSkill;

        public List<Sprite> comboSprites;

        private BuffDef currentBuff;
        

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            if (characterBody != null)
            {
                skillLocator = characterBody.skillLocator;
                if (skillLocator != null)
                {
                    specialSkill = skillLocator.special;
                }
                else
                {
                    Debug.Log("Skill Locator not found.");
                }
            }
        }
        

        private void Start()
        {
            ResetCombo(true, false);
        }

        private void OnDisable()
        {
            specialSkill.skillDef.icon = comboSprites[maximumCombo];
        }

        //AutoAttack - 1, BladeWhirl - 2, Wild Rush - 3, Flair - 4, FlairDash - 5
        public void AddCombo(int attackID)
        {
            if (ComboIndex >= maximumCombo)
            {
                return;
            }
            if (attackID == previousAttackID)
            {
                timer = 0f;
                return;
            }

            timer = 0f;
            previousAttackID = attackID;
            SetComboIndex(ComboIndex + 1);
            AddComboBuff(ComboIndex);
            
            string soundString = "PlaySFX_comboM"+ComboIndex;
            previousSoundID = SamiraSoundManager.instance.PlaySoundBySkin(soundString, gameObject);

            if (Modules.Config.enableVoiceLines.Value && ComboIndex >= maximumCombo)
            {
                SamiraSoundManager.instance.PlaySoundBySkin("PlayVO_R_ReadyBuff", gameObject);
            }
        }

        private void FixedUpdate()
        {
            if (ComboIndex <= 0 || !characterBody.outOfCombat)
            {
                timer = 0f;
            }
            else
            {
                timer += Time.deltaTime;
            }
            
            if (timer >= comboResetInterval && characterBody.outOfCombat)
            {
                ResetCombo(true);
                if (Config.enableVoiceLines.Value)
                {
                    if (previousSoundID != 0 ) AkSoundEngine.StopPlayingID(previousSoundID);
                    previousSoundID = SamiraSoundManager.instance.PlaySoundBySkin("PlayVO_ComboReset", gameObject);
                }
            }
            
        }

        void SetComboIndex(int newIndex)
        {
            newIndex = Mathf.Clamp(newIndex, 0, maximumCombo);
            ComboIndex = newIndex;
            specialSkill.skillDef.icon = comboSprites[ComboIndex];
            
        }

        void AddComboBuff(int index)
        {
            if (currentBuff != null)
            {
                characterBody.RemoveBuff(currentBuff);
                currentBuff = null;
            }
            switch (index)
            {
                case 1:
                    currentBuff = SamiraBuffs.comboBuff1;
                    break;
                case 2:
                    currentBuff = SamiraBuffs.comboBuff2;
                    break;
                case 3:
                    currentBuff = SamiraBuffs.comboBuff3;
                    break;
                case 4:
                    currentBuff = SamiraBuffs.comboBuff4;
                    break;
                case 5:
                    currentBuff = SamiraBuffs.comboBuff5;
                    break;
                case 6:
                    currentBuff = SamiraBuffs.comboBuff6;
                    break;
            }
            if (currentBuff != null) characterBody.AddBuff(currentBuff);
            characterBody.RecalculateStats();
        }


        public void ResetCombo(bool removeBuff = false, bool playSound = true)
        {
            SetComboIndex(0);
            timer = 0f;
            previousAttackID = 0;

            if (removeBuff)
            {
                characterBody.RemoveBuff(currentBuff);
                currentBuff = null;
                characterBody.RecalculateStats();
            }

            if (!playSound) return;
            
            if (ComboIndex > 0)
            {
                Util.PlaySound("Play_SamiraSFX_ComboReset", gameObject);
            }
            
        }
    }
}