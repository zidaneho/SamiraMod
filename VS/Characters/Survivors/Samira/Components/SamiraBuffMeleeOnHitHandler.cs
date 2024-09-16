using R2API.Networking.Interfaces;
using RoR2;
using SamiraMod.Survivors.Samira.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.Components
{
    internal class SamiraBuffMeleeOnHitHandler : MonoBehaviour
    {
        private static float buffDuration = 5f;
        private static float fadeOutDuration = 0.5f;
        private float timer;
        private CharacterBody _characterBody;

        private bool canPlay;
        private bool isFadingOut;

        private void Update()
        {
            if (!canPlay) return;

            timer += Time.deltaTime;

            if (isFadingOut && timer >= fadeOutDuration)
            {
                timer = 0f;
                _characterBody.RemoveBuff(SamiraBuffs.meleeOnHitBuff);
            }
            else if (timer >= buffDuration)
            {
                timer = 0f;
                _characterBody.RemoveBuff(SamiraBuffs.meleeOnHitBuff);
                isFadingOut = true;
            }
        }

        public void ApplyBuff(CharacterBody characterBody)
        {
            _characterBody = characterBody;
            _characterBody.AddBuff(SamiraBuffs.meleeOnHitBuff);
            timer = 0f;
            canPlay = true;
            isFadingOut = false;
        }
    }
}