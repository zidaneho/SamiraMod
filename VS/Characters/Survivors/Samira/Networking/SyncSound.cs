using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.Networking
{
    public class SyncSound : INetMessage
    {
        private string soundString;
        private NetworkInstanceId bodyId;
        
        public SyncSound()
        {
            
        }

        public SyncSound(string soundString, NetworkInstanceId bodyId)
        {
            this.soundString = soundString;
            this.bodyId = bodyId;
        }
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(soundString);
            writer.Write(bodyId);
        }

        public void Deserialize(NetworkReader reader)
        {
            soundString = reader.ReadString();
            bodyId = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            var playerGameObject = RoR2.Util.FindNetworkObject(bodyId);
            if (playerGameObject)
            {
                if (playerGameObject.GetComponent<CharacterBody>().hasAuthority)
                {
                    Util.PlaySound(soundString, playerGameObject);
                }
            }
        }
    }
}