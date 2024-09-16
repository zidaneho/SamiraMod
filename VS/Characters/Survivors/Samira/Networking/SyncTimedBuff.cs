using System.Net.NetworkInformation;
using RoR2;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.Networking
{
    public class SyncTimedBuff : INetMessage
    {
        private int buffIndex;
        private float duration;
        private int maxStacks;
        private NetworkInstanceId bodyId;
        public SyncTimedBuff() 
        {
        }
        public SyncTimedBuff(BuffIndex buffIndex, float duration, int maxStacks, NetworkInstanceId bodyId)
        {
            this.buffIndex = (int) buffIndex;
            this.duration = duration;
            this.maxStacks = maxStacks;
            this.bodyId = bodyId;
        }
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.buffIndex);
            writer.Write(this.duration);
            writer.Write(this.maxStacks);
            writer.Write(this.bodyId);
        }

        public void Deserialize(NetworkReader reader)
        {
            buffIndex = reader.ReadInt32();
            duration = reader.ReadSingle();
            maxStacks = reader.ReadInt32();
            bodyId = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            var playerGameObject = RoR2.Util.FindNetworkObject(bodyId);
            if (playerGameObject)
            {
                var body = playerGameObject.GetComponent<RoR2.CharacterBody>();
                if (body)
                {
                    var buffDef = BuffCatalog.GetBuffDef((BuffIndex) buffIndex);
                    body.AddTimedBuff(buffDef, duration, maxStacks);
                }
            }
        }
    }
}