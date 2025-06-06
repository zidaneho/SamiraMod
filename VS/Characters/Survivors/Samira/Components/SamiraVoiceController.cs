using System;
using On.EntityStates.SurvivorPod;
using RoR2;
using UnityEngine;
using GlobalEventManager = On.RoR2.GlobalEventManager;

namespace SamiraMod.Survivors.Samira.Components
{
    internal class SamiraVoiceController : MonoBehaviour
    {
        
        private float moveTimer;
        private const float moveVoiceTime = 15f;

        private CharacterBody characterBody;
        private RoR2.CharacterMotor characterMotor;
        private RoR2.CharacterDirection characterDirection;

        private uint currentVoiceID;
        
        private SamiraSoundManager _soundManager;

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            characterMotor = GetComponent<RoR2.CharacterMotor>();
            characterDirection = GetComponent<RoR2.CharacterDirection>();
            _soundManager = GetComponent<SamiraSoundManager>();

            On.EntityStates.SurvivorPod.ReleaseFinished.OnEnter += ReleaseFinishedOnOnEnter;
            BossGroup.onBossGroupDefeatedServer += BossGroup_OnBossGroupDefeatedServer;
        }
        


        private void OnDestroy()
        {
            On.EntityStates.SurvivorPod.ReleaseFinished.OnEnter -= ReleaseFinishedOnOnEnter;
            BossGroup.onBossGroupDefeatedServer -= BossGroup_OnBossGroupDefeatedServer;
            
        }
        private void ReleaseFinishedOnOnEnter(ReleaseFinished.orig_OnEnter orig, EntityStates.SurvivorPod.ReleaseFinished self)
        {
            orig(self);

            if (!characterBody.isLocalPlayer) return;
            
            characterBody = GetComponent<CharacterBody>();
            if (characterBody.isPlayerControlled && Modules.Config.enableVoiceLines.Value)
            {
                PlaySound("PlayVO_MoveFirst");   
            }
        }
        private void BossGroup_OnBossGroupDefeatedServer(BossGroup obj)
        {
            if (!characterBody.isLocalPlayer) return;
            
            if (Modules.Config.enableVoiceLines.Value)
            {
                PlaySound("PlayVO_ChampionKill");
            }
        }


        private void Update()
        {
            if (!Modules.Config.enableVoiceLines.Value || !characterBody.isLocalPlayer) return;
            
            
            if (characterBody.outOfCombat && characterDirection.moveVector != Vector3.zero)
            {
                moveTimer += Time.deltaTime;
                if (moveTimer >= moveVoiceTime)
                {
                    moveTimer = 0f;
                    PlaySound("PlayVO_Move");
                }
            }
            else
            {
                moveTimer = 0f;
            }
        }

       

        void PlaySound(string soundString)
        {
            if (currentVoiceID != 0) AkSoundEngine.StopPlayingID(currentVoiceID);
            if (Modules.Config.enableVoiceLines.Value)
            {
                currentVoiceID = _soundManager.PlaySoundBySkin(soundString, gameObject);
            }
        }
    }
}