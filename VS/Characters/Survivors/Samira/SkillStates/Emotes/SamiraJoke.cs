using SamiraMod.Survivors.Samira.Components;
using UnityEngine;

namespace SamiraMod.Survivors.Samira.SkillStates.Emotes
{
    public class SamiraJoke : SamiraBaseEmote
    {
        private float spawnShellDuration = 2.2f;
        private GameObject shellInstance;
        private Transform muzzleTransform;

        private uint loopSoundID;
        private uint startSoundID;
        private uint voID;
        public override void OnEnter()
        {
            animString = "Joke";
            duration = -1f; // infinite until canceled
            var animator = GetModelAnimator();
            animator.SetBool("inJoke",true);
            base.OnEnter();
            
            shellInstance = null;
            var childLocator = GetModelChildLocator();
            muzzleTransform = childLocator.FindChild("TauntMuzzle");

            startSoundID = RoR2.Util.PlaySound("Play_SamiraSFX_JokeA", gameObject);
            voID = SamiraSoundManager.instance.PlaySoundBySkin("PlayVO_Joke", gameObject);


        }
        public override void Update()
        {
            base.Update();

            if (shellInstance == null && fixedAge >= spawnShellDuration)
            {
                shellInstance = UnityEngine.Object.Instantiate(SamiraAssets.shellParticlePrefab, muzzleTransform);
                loopSoundID = RoR2.Util.PlaySound("Play_SamiraSFX_JokeB",gameObject);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!isAuthority) return;

            

            if (shellInstance != null)
            {
                shellInstance.transform.position = muzzleTransform.position;
            }
        }
        public override void OnExit()
        {
            var animator = GetModelAnimator();
            animator.SetBool("inJoke",false);
            base.OnExit();
            if (shellInstance) UnityEngine.Object.Destroy(shellInstance);

            if (loopSoundID != 0) AkSoundEngine.StopPlayingID(loopSoundID);
            if (startSoundID != 0) AkSoundEngine.StopPlayingID(startSoundID);
            if (voID != 0) AkSoundEngine.StopPlayingID(voID);
        }
    }
}