using RoR2;
using R2API.Networking.Interfaces;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

namespace SamiraMod.Survivors.Samira.Networking
{
    public class SyncWildRushReset : INetMessage
    {
        private float healValue;
        private NetworkInstanceId bodyID;
        public SyncWildRushReset()
        {
        }

        public SyncWildRushReset(NetworkInstanceId bodyID)
        {
            this.bodyID = bodyID;
        }
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.bodyID);
        }

        public void Deserialize(NetworkReader reader)
        {
            bodyID = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            var playerGameObject = RoR2.Util.FindNetworkObject(bodyID);
            if (playerGameObject)
            {
                var body = playerGameObject.GetComponent<RoR2.CharacterBody>();

                if (body != null)
                {
                    var skill = GetUtilitySkill(body);
                    if (skill && skill.skillDef.skillName == "SamiraWildRush")
                    {
                        skill.rechargeStopwatch = 0f;
                        skill.stock = skill.maxStock;
                        skill.RunRecharge(float.MaxValue);
                    }
                }
            }
        }
        GenericSkill GetUtilitySkill(RoR2.CharacterBody characterBody)
        {
            return characterBody?.skillLocator?.utility;
        }
    }
}