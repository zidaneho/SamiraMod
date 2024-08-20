
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using SamiraMod.Survivors.Samira.SkillStates;
using UnityEngine;

namespace SamiraMod.Survivors.Samira.Components
{
    internal class SamiraBladeWhirlHandler : MonoBehaviour
    {
        private GameObject _bladeWhirlInstance;
        private CharacterBody _characterBody;
        private BladeWhirl _bladeWhirlState;

        private float duration = SamiraStaticValues.bladeWhirlDuration;
        private float radius = SamiraStaticValues.bladeWhirlRadius;
        private int destroyLimit = 50;

        private float damageStat;
        private bool canUpdate;
        

        private void Update()
        {
            if (canUpdate && _bladeWhirlInstance &&  _characterBody)
            {
                UpdateIndicator();
                if (DeleteNearbyProjectiles() && Modules.Config.enableVoiceLines.Value)
                {
                    Util.PlaySound("Play_SamiraVO_W", gameObject);
                }
            }
        }

        public void SpawnInstance(CharacterBody characterBody, BladeWhirl bladeWhirl)
        {
            _bladeWhirlInstance = CreateIndicator(characterBody.corePosition, radius);
            _characterBody = characterBody;
            _bladeWhirlState = bladeWhirl;

            StartCoroutine(UpdateInstanceTimer());
        }
        
        void UpdateIndicator()
        {
            _bladeWhirlInstance.transform.position = _characterBody.corePosition;
        }
        
        bool DeleteNearbyProjectiles()
        {
            Vector3 vector = _characterBody ? _characterBody.corePosition : Vector3.zero;
            TeamIndex teamIndex = _characterBody ? _characterBody.teamComponent.teamIndex : TeamIndex.None;
            float num = radius * radius;
            int num2 = 0;
            bool result = false;
            List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
            List<ProjectileController> list = new List<ProjectileController>();
            int num3 = 0;
            int count = instancesList.Count;
            while (num3 < count && num2 < destroyLimit)
            {
                ProjectileController projectileController = instancesList[num3];
                if (!projectileController.cannotBeDeleted && projectileController.teamFilter.teamIndex != teamIndex && (projectileController.transform.position - vector).sqrMagnitude < num)
                {
                    list.Add(projectileController);
                    num2++;
                }
                num3++;
            }
            int i = 0;
            int count2 = list.Count;
            while (i < count2)
            {
                ProjectileController projectileController2 = list[i];
                if (projectileController2)
                {
                    result = true;
                    Vector3 position = projectileController2.transform.position;
                    Vector3 start = vector;
                    EntityState.Destroy(projectileController2.gameObject);
                }
                i++;
            }
            return result;
        }
        
        GameObject CreateIndicator(Vector3 position, float radius)
        {
            if (EntityStates.Huntress.ArrowRain.areaIndicatorPrefab)
            {
                Vector3 spawnPos = position;
                
                var indicator = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab);
                indicator.transform.localScale = Vector3.one * radius;
                indicator.transform.position = spawnPos;

                return indicator;
            }

            return null;
        }

        IEnumerator UpdateInstanceTimer()
        {
            canUpdate = true;
            _bladeWhirlState.BeginWhirl();
            yield return new WaitForSeconds(duration);
            canUpdate = false;
            if (_bladeWhirlInstance) EntityState.Destroy(_bladeWhirlInstance);
            _bladeWhirlState.EndWhirl();
        }
    }
}