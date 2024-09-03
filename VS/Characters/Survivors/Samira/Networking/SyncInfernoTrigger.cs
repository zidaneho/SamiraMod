using RoR2;
using R2API.Networking.Interfaces;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

namespace SamiraMod.Survivors.Samira.Networking
{
    public class SyncInfernoTrigger : INetMessage
    {
        private float healValue;
        private NetworkInstanceId bodyID;
        public SyncInfernoTrigger()
        {
        }

        public SyncInfernoTrigger(float healValue, NetworkInstanceId bodyID)
        {
            this.healValue = healValue;
            this.bodyID = bodyID;
        }
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.healValue);
            writer.Write(this.bodyID);
        }

        public void Deserialize(NetworkReader reader)
        {
            healValue = reader.ReadSingle();
            bodyID = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            var playerGameObject = RoR2.Util.FindNetworkObject(bodyID);
            if (playerGameObject)
            {
                var body = playerGameObject.GetComponent<RoR2.CharacterBody>();
                if (body)
                {
                    var healthComponent = body.GetComponent<HealthComponent>();
                    if (healthComponent)
                    {
                        healthComponent.Heal(healValue, new ProcChainMask());
                    }
                }
            }
        }
    }
}