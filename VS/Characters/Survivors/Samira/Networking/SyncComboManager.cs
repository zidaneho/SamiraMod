using IL.RoR2;
using R2API.Networking.Interfaces;
using RoR2;
using SamiraMod.Survivors.Samira.Components;
using UnityEngine.Networking;
using BuffCatalog = RoR2.BuffCatalog;
using CharacterBody = On.RoR2.CharacterBody;
using GenericSkill = RoR2.GenericSkill;

namespace SamiraMod.Survivors.Samira.Networking
{
    public class SyncComboManager : INetMessage
    {
        private int comboIndex;
        private int buffIndex;
        private NetworkInstanceId bodyId;
        public SyncComboManager()
        {
        }

        public SyncComboManager(int comboIndex, BuffIndex buffIndex, NetworkInstanceId bodyId)
        {
            this.comboIndex = comboIndex;
            this.buffIndex = (int)buffIndex;
            this.bodyId = bodyId;
        }
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(comboIndex);
            writer.Write(buffIndex);
            writer.Write(bodyId);
        }

        public void Deserialize(NetworkReader reader)
        {
            comboIndex = reader.ReadInt32();
            buffIndex = reader.ReadInt32();
            bodyId = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            var playerGameObject = RoR2.Util.FindNetworkObject(bodyId);
            if (playerGameObject)
            {
                var samiraComboManager = playerGameObject.GetComponent<SamiraComboManager>();
                if (samiraComboManager != null)
                {
                    samiraComboManager.ComboIndex = comboIndex;
                    samiraComboManager.currentBuff = BuffCatalog.GetBuffDef((BuffIndex)buffIndex);
                }
            }
        }
        
    }
}