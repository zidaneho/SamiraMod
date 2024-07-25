using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using BodyCatalog = On.RoR2.BodyCatalog;
using Path = RoR2.Path;

namespace SamiraMod.Survivors.Samira.Components
{
    internal class SamiraComboManager : MonoBehaviour
    {
        public int ComboIndex { get; private set; }

        public readonly int minimumCombo = 0;
        public readonly int maximumCombo = 6;

        private int previousAttackID;

        private float timer;
        private float comboResetInterval = 7f;

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
            ResetCombo();
        }

        private void OnDisable()
        {
            specialSkill.skillDef.icon = comboSprites[maximumCombo];
        }

        //AutoAttack - 1, BladeWhirl - 2, Wild Rush - 3, Flair - 4
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
            
            string soundString = "Play_SamiraSFX_comboM"+ComboIndex;
            Util.PlaySound(soundString, gameObject);

            if (Modules.Config.enableVoiceLines.Value && ComboIndex >= maximumCombo)
            {
                Util.PlaySound("Play_SamiraVO_R_ReadyBuff", gameObject);
            }
        }

        private void FixedUpdate()
        {
            timer += Time.deltaTime;
            if (timer >= comboResetInterval && characterBody.outOfCombat)
            {
                ResetCombo(true);
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


        public void ResetCombo(bool removeBuff = false)
        {
            Debug.Log("reset combo");

            if (ComboIndex > 0)
            {
                Util.PlaySound("Play_SamiraSFX_ComboReset", gameObject);
            }
            SetComboIndex(0);
            timer = 0f;
            previousAttackID = 0;

            if (removeBuff)
            {
                characterBody.RemoveBuff(currentBuff);
                currentBuff = null;
                characterBody.RecalculateStats();
            }
        }
    }
}