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

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            characterMotor = GetComponent<RoR2.CharacterMotor>();
            characterDirection = GetComponent<RoR2.CharacterDirection>();

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
            characterBody = GetComponent<CharacterBody>();
            if (characterBody.isPlayerControlled && Modules.Config.enableVoiceLines.Value)
            {
                PlaySound("Play_SamiraVO_MoveFirst");   
            }
        }
        private void BossGroup_OnBossGroupDefeatedServer(BossGroup obj)
        {
            if (Modules.Config.enableVoiceLines.Value)
            {
                PlaySound("Play_SamiraVO_ChampionKill");
            }
        }


        private void Update()
        {
            if (!Modules.Config.enableVoiceLines.Value) return;
            

            if (characterBody.outOfCombat && characterDirection.moveVector != Vector3.zero)
            {
                moveTimer += Time.deltaTime;
                if (moveTimer >= moveVoiceTime)
                {
                    moveTimer = 0f;
                    PlaySound("Play_SamiraVO_Move");
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
            if (Modules.Config.enableVoiceLines.Value) currentVoiceID = Util.PlaySound(soundString, gameObject);
        }
    }
}