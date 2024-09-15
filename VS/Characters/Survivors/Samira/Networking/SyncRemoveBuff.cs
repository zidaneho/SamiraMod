using RoR2;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.Networking
{
    public class SyncRemoveBuff : INetMessage
    {
        private int buffIndex;
        private NetworkInstanceId bodyId;

        public SyncRemoveBuff() { }

        public SyncRemoveBuff(BuffIndex buffIndex, NetworkInstanceId bodyId)
        {
            this.buffIndex = (int)buffIndex;
            this.bodyId = bodyId;
        }
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.buffIndex);
            writer.Write(this.bodyId);
        }

        public void Deserialize(NetworkReader reader)
        {
            buffIndex = reader.ReadInt32();
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
                    var buffDef = BuffCatalog.GetBuffDef((BuffIndex)buffIndex);
                    body.RemoveBuff(buffDef);
                }
            }
        }
    }
}