using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SamiraMod.Survivors.Samira.SkillStates
{
    public class QuickSteps : BaseWildRush
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(SamiraBuffs.wildRushAttackSpeedBuff, SamiraStaticValues.wildRushAttackSpeedDuration);
            }
        }
        
        protected override void SetupAttack()
        {
            hitboxGroupName = "WildRushHitbox";
            damageType = DamageType.Generic;
            procCoefficient = 1f;
            pushForce = 0f;
            bonusForce = Vector3.zero;
            
            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0f;
            attackEndPercentTime = 1f;

          

            hitStopDuration = 0.012f;
            attackRecoil = 0.5f;
            meleeMuzzleString = "SwingLeft";
            //swingEffectPrefab = SamiraAssets.swordSwingEffect;
            //hitEffectPrefab = SamiraAssets.swordHitImpactEffect;
            
            attack = new OverlapAttack();
            attack.damageType = damageType;
            attack.attacker = gameObject;
            attack.inflictor = gameObject;
            attack.teamIndex = GetTeam();
            attack.damage = SamiraStaticValues.GetQuickStepsDamage(damageStat, characterBody.level);
            attack.procCoefficient = procCoefficient;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.forceVector = bonusForce;
            attack.pushAwayForce = pushForce;
            attack.hitBoxGroup = FindHitBoxGroup(hitboxGroupName);
            attack.isCrit = RollCrit();
            attack.impactSound = impactSound;
        }
    }
}