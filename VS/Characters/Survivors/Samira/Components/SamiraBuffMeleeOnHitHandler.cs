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
                var networkBody = _characterBody.GetComponent<NetworkIdentity>();
                if (networkBody != null)
                {
                    new SyncRemoveBuff(SamiraBuffs.meleeOnHitBuff.buffIndex, networkBody.netId).Send(R2API.Networking.NetworkDestination.Server);
                }
            }
            else if (timer >= buffDuration)
            {
                timer = 0f;
                var networkBody = _characterBody.GetComponent<NetworkIdentity>();
                if (networkBody != null)
                {
                    new SyncRemoveBuff(SamiraBuffs.meleeOnHitBuff.buffIndex, networkBody.netId).Send(R2API.Networking.NetworkDestination.Server);
                }
                isFadingOut = true;
            }
        }

        public void ApplyBuff(CharacterBody characterBody)
        {
            _characterBody = characterBody;
            var networkBody = _characterBody.GetComponent<NetworkIdentity>();
            if (networkBody != null)
            {
                new SyncBuff(SamiraBuffs.meleeOnHitBuff.buffIndex, networkBody.netId).Send(R2API.Networking.NetworkDestination.Server);
            }
          
            timer = 0f;
            canPlay = true;
            isFadingOut = false;
        }
    }
}