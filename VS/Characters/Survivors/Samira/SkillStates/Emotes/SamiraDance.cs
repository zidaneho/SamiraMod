using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Util = IL.RoR2.Util;

namespace SamiraMod.Survivors.Samira.SkillStates.Emotes
{
    public class SamiraDance : SamiraBaseEmote
    {
        private string startSoundString = "Play_SamiraSFX_Dance";
        private string loopSoundString = "Play_SamiraSFX_Dance_Loop";

        private float startLoopDuration = 2.523f;
        private bool startedPlayingLoop;
        private uint loopSoundID;
        private uint startSoundID;

        private float buffDuration = 30f;
        private int buffMaxStacks = 5;
        private float durationBetweenBuffs = 4f;
        private float stopwatch;
        public override void OnEnter()
        {
            animString = "Dance";
            duration = -1f; // infinite until canceled
            startSoundID = 0;
            loopSoundID = 0;

            var animator = GetModelAnimator();
            animator.SetBool("inDance",true);
            base.OnEnter();

            startedPlayingLoop = false;
            startSoundID = RoR2.Util.PlaySound(startSoundString, gameObject);
            
            stopwatch = 0f;
        }

        public override void Update()
        {
            base.Update();

            if (!startedPlayingLoop && fixedAge >= startLoopDuration)
            {
                startedPlayingLoop=true;
                loopSoundID = RoR2.Util.PlaySound(loopSoundString, gameObject);
            }
        }

        public override void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= durationBetweenBuffs)
            {
                stopwatch = 0f;
                RoR2.Util.PlaySound("Play_SamiraSFX_CoinHit", gameObject);
                if (NetworkServer.active) characterBody.AddTimedBuff(SamiraBuffs.danceBuff, buffDuration, buffMaxStacks);
            }
            base.FixedUpdate();
        }
        

        public override void OnExit()
        {
            var animator = GetModelAnimator();
            animator.SetBool("inDance",false);
            base.OnExit();

            if (loopSoundID != 0) AkSoundEngine.StopPlayingID(loopSoundID);
            if (startSoundID != 0) AkSoundEngine.StopPlayingID(startSoundID);
        }
    }
}