using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.Networking
{
    public class SyncResetWildRush : INetMessage
    {
        NetworkInstanceId netId;

        public SyncResetWildRush() { }

        public SyncResetWildRush(NetworkInstanceId netId)
        {
            this.netId = netId;
        }

        public void OnReceived()
        {
            var obj = ClientScene.FindLocalObject(netId);
            if (obj)
            {
                var body = obj.GetComponent<CharacterBody>();
                if (body && body.skillLocator && body.skillLocator.utility)
                {
                    body.skillLocator.utility.Reset();
                    Debug.Log($"[Client] Wild Rush skill reset for {body.GetUserName()}");
                }
            }
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(netId);
        }

        public void Deserialize(NetworkReader reader)
        {
            netId = reader.ReadNetworkId();
        }
    }
}