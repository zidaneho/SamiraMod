using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Skills;
using SamiraMod.Modules;
using SamiraMod.Survivors.Samira.Networking;
using UnityEngine;
using UnityEngine.Networking;
using BodyCatalog = On.RoR2.BodyCatalog;
using Path = RoR2.Path;

namespace SamiraMod.Survivors.Samira.Components
{
    public class SamiraComboManager : MonoBehaviour
    {
        public int ComboIndex;
        public BuffDef currentBuff;

        public readonly int minimumCombo = 0;
        public readonly int maximumCombo = 6;

        private int previousAttackID;
        private uint previousSoundID;

        private float timer;
        private float comboResetInterval = SamiraStaticValues.styleDuration;

        private CharacterBody characterBody;
        private SkillLocator skillLocator;
        private GenericSkill specialSkill;
        protected SamiraSoundManager soundManager;

        public List<Sprite> comboSprites;

        private HashSet<BuffIndex> buffIndexes;
        
        

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            soundManager = GetComponent<SamiraSoundManager>();
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
            
            buffIndexes = new HashSet<BuffIndex>();
            buffIndexes.Add(SamiraBuffs.comboBuff1.buffIndex);
            buffIndexes.Add(SamiraBuffs.comboBuff2.buffIndex);
            buffIndexes.Add(SamiraBuffs.comboBuff3.buffIndex);
            buffIndexes.Add(SamiraBuffs.comboBuff4.buffIndex);
            buffIndexes.Add(SamiraBuffs.comboBuff5.buffIndex);
            buffIndexes.Add(SamiraBuffs.comboBuff6.buffIndex);
        }
        

        private void Start()
        {
            ResetCombo(true, false);
        }

        private void OnDisable()
        {
            specialSkill.skillDef.icon = comboSprites[maximumCombo];
        }

        private void FixedUpdate()
        {
            if (!characterBody.hasAuthority) return;
            
            if (ComboIndex <= 0 || !characterBody.outOfCombat)
                timer = 0f;
            else
                timer += Time.deltaTime;

            if (timer >= comboResetInterval && characterBody.outOfCombat)
            {
                ResetCombo(true);
                if (Config.enableVoiceLines.Value)
                {
                    if (previousSoundID != 0) AkSoundEngine.StopPlayingID(previousSoundID);
                    previousSoundID = soundManager.PlaySoundBySkin("PlayVO_ComboReset", gameObject);
                }
            }
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
            
            // if (!NetworkServer.active)
            // {
            //     var netBody = characterBody.GetComponent<NetworkIdentity>();
            //     if (netBody != null)
            //     {
            //         new SyncComboManager(ComboIndex, currentBuff.buffIndex, netBody.netId).Send(NetworkDestination.Server);
            //     }   
            // }
            

            string soundString = "PlaySFX_comboM" + ComboIndex;
            previousSoundID = soundManager.PlaySoundBySkin(soundString, gameObject);

            if (Modules.Config.enableVoiceLines.Value && ComboIndex >= maximumCombo)
            {
                soundManager.PlaySoundBySkin("PlayVO_R_ReadyBuff", gameObject);
            }
        }
        

       

        void SetComboIndex(int newIndex)
        {
            newIndex = Mathf.Clamp(newIndex, 0, maximumCombo);
            ComboIndex = newIndex;
            if (characterBody.hasAuthority) specialSkill.skillDef.icon = comboSprites[ComboIndex];
        }

        void AddComboBuff(int index)
        {
            ResetCurrentBuff();
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
            if (currentBuff != null)
            {
                var netBody = characterBody.GetComponent<NetworkIdentity>();
                if (netBody != null)
                {
                    new SyncAddBuff(currentBuff.buffIndex, netBody.netId).Send(NetworkDestination.Server);
                }
            }
            characterBody.RecalculateStats();
        }

        public void ResetCurrentBuff()
        {
            if (currentBuff != null)
            {
                var netBody = characterBody.GetComponent<NetworkIdentity>();
                if (netBody != null)
                {
                    new SyncRemoveBuff(currentBuff.buffIndex, netBody.netId).Send(NetworkDestination.Server);
                    
                    //Checking for any other loose buffs
                    List<BuffIndex> buffIndexesToRemove = new List<BuffIndex>();
                    foreach (var buff in characterBody.buffs)
                    {
                        if (buffIndexes.Contains((BuffIndex)buff))
                        {
                            buffIndexesToRemove.Add((BuffIndex)buff);
                        }
                    }

                    if (buffIndexesToRemove.Count > 0)
                    {
                        foreach (var index in buffIndexesToRemove)
                        {
                            new SyncRemoveBuff(index, netBody.netId).Send(NetworkDestination.Server);
                        }
                    }
                    
                }
                currentBuff = null;
            }
            
        }


        public void ResetCombo(bool removeBuff = false, bool playSound = true)
        {
            if (characterBody.hasAuthority && playSound) Util.PlaySound("Play_SamiraSFX_ComboReset", gameObject);
            SetComboIndex(0);
            timer = 0f;
            previousAttackID = 0;

            if (removeBuff)
            {
                ResetCurrentBuff();
               
                currentBuff = null;
                characterBody.RecalculateStats();
            }
            
            // if (!NetworkServer.active)
            // {
            //     var netBody = characterBody.GetComponent<NetworkIdentity>();
            //     if (netBody != null)
            //     {
            //         var buffIndex = currentBuff != null ? currentBuff.buffIndex : BuffIndex.None;
            //         new SyncComboManager(ComboIndex, buffIndex, netBody.netId).Send(NetworkDestination.Server);
            //     }   
            // }
        }
        
    }
}