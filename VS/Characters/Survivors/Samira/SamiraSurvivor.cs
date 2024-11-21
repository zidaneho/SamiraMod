using BepInEx.Configuration;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using HG;
using On.RoR2.SurvivorMannequins;
using SamiraMod.Modules;
using SamiraMod.Modules.Characters;
using SamiraMod.Survivors.Samira.Components;
using SamiraMod.Survivors.Samira.SkillStates;
using UnityEngine;
using GlobalEventManager = On.RoR2.GlobalEventManager;
using Loadout = On.RoR2.Loadout;
using SamiraMain = SamiraMod.Survivors.Samira.SkillStates.SamiraMain;
using R2API.Networking;
using RiskOfOptions;
using SamiraMod.Survivors.Samira.Networking;

namespace SamiraMod.Survivors.Samira
{
    public class SamiraSurvivor : SurvivorBase<SamiraSurvivor>
    {
        //used to load the assetbundle for this character. must be unique
        public override string assetBundleName => "samiraassetbundle"; //if you do not change this, you are giving permission to deprecate the mod

        //the name of the prefab we will create. conventionally ending in "Body". must be unique
        public override string bodyName => "SamiraBody"; //if you do not change this, you get the point by now

        //name of the ai master for vengeance and goobo. must be unique
        public override string masterName => "SamiraMonsterMaster"; //if you do not

        //the names of the prefabs you set up in unity that we will use to build your character
        public override string modelPrefabName => "mdlSamira";
        public override string displayPrefabName => "SamiraDisplay";

        public const string SAMIRA_PREFIX = SamiraPlugin.DEVELOPER_PREFIX + "_SAMIRA_";

        //used when registering your survivor's language tokens
        public override string survivorTokenPrefix => SAMIRA_PREFIX;
        
        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = SAMIRA_PREFIX + "NAME",
            subtitleNameToken = SAMIRA_PREFIX + "SUBTITLE",
            

            characterPortrait = assetBundle.LoadAsset<Texture>("texSamiraIcon"),
            bodyColor = new Color(0.7607843f,0.2313726f,0.1333333f),
            sortPosition = 100,

            crosshair = Modules.Assets.LoadCrosshair("Standard"),
            podPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = Modules.Config.baseHealth.Value,
            healthRegen = Modules.Config.baseRegen.Value,
            armor = Modules.Config.baseArmor.Value,
            damage = Modules.Config.baseDamage.Value,
            moveSpeed = Modules.Config.baseMovementSpeed.Value,
            crit = Modules.Config.baseCrit.Value,
            jumpCount = Modules.Config.jumpCount.Value,
            armorGrowth = Modules.Config.armorGrowth.Value,
            healthGrowth = Modules.Config.healthGrowth.Value,
            damageGrowth = Modules.Config.damageGrowth.Value,
            regenGrowth = Modules.Config.regenGrowth.Value,
            
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
            new CustomRendererInfo()
            {
                childName = "BodyMesh",
                material = assetBundle.LoadMaterial("mat_base_Body")
            },
            new CustomRendererInfo()
            {
                childName = "PistolMesh",
                material = assetBundle.LoadMaterial("mat_base_Pistol")
            },
            new CustomRendererInfo()
            {
            childName = "RevolverMesh",
            material = assetBundle.LoadMaterial("mat_base_Revolver")
            },
            new CustomRendererInfo()
            {
                childName = "SwordMesh",
                material = assetBundle.LoadMaterial("mat_base_Sword")
            },
        };

        public override UnlockableDef characterUnlockableDef => SamiraUnlockables.characterUnlockableDef;
        
        public override ItemDisplaysBase itemDisplays => new SamiraItemDisplays();

        //set in base classes
        public override AssetBundle assetBundle { get; protected set; }

        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }

        public override void Initialize()
        {
            if (!Config.enableCharacter.Value)
                return;

            base.Initialize();
        }

        public override void InitializeCharacter()
        {
            //need the character unlockable before you initialize the survivordef
            SamiraUnlockables.Init();
            SamiraConfig.Init();
            
            

            base.InitializeCharacter();
            
            SamiraStates.Init();
            SamiraTokens.Init();

            SamiraAssets.Init(assetBundle);
            SamiraBuffs.Init(assetBundle);

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();

            AddHooks();
            SetupRiskOfOptions();
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            var comboManager = bodyPrefab.AddComponent<SamiraComboManager>();
            bodyPrefab.AddComponent<SamiraWildRushReset>();
            bodyPrefab.AddComponent<SamiraVoiceController>();
            bodyPrefab.AddComponent<SamiraBladeWhirlHandler>();
            bodyPrefab.AddComponent<SamiraBuffMeleeOnHitHandler>();
            bodyPrefab.AddComponent<SamiraSoundManager>();
            bodyPrefab.AddComponent<ProjectileDamageTracker>();
           
            //bodyPrefab.AddComponent<HuntressTrackerComopnent>();
            //anything else here
            
            var r0 = assetBundle.LoadAsset<Sprite>("texSamiraR0");
            var r1 = assetBundle.LoadAsset<Sprite>("texSamiraR1");
            var r2 = assetBundle.LoadAsset<Sprite>("texSamiraR2");
            var r3 = assetBundle.LoadAsset<Sprite>("texSamiraR3");
            var r4 = assetBundle.LoadAsset<Sprite>("texSamiraR4");
            var r5 = assetBundle.LoadAsset<Sprite>("texSamiraR5");
            var r6 = assetBundle.LoadAsset<Sprite>("texSamiraR6");

            comboManager.comboSprites = new List<Sprite>()
            {
                r0, r1, r2, r3, r4, r5, r6
            };
            
            displayPrefab.AddComponent<SamiraMenu>();
        }

        public void AddHitboxes()
        {
            //example of how to create a HitBoxGroup. see summary for more details
            Prefabs.SetupHitBoxGroup(characterModelObject, "AAHitbox", "AAHitbox");
            Prefabs.SetupHitBoxGroup(characterModelObject, "WildRushHitbox", "WildRushHitbox");
            Prefabs.SetupHitBoxGroup(characterModelObject, "FlairMeleeHitbox","FlairMeleeHitbox");
        }

        public override void InitializeEntityStateMachines() 
        {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(SamiraMain), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
                //don't forget to register custom entitystates in your HenryStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon2");
        }

        #region skills
        public override void InitializeSkills()
        {
            //remove the genericskills from the commando body we cloned
            Skills.ClearGenericSkills(bodyPrefab);
            //add our own
            AddPassiveSkill();
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtiitySkills();
            AddSpecialSkills();
        }

        //skip if you don't have a passive
        //also skip if this is your first look at skills
        private void AddPassiveSkill()
        {
            //option 1. fake passive icon just to describe functionality we will implement elsewhere
            bodyPrefab.GetComponent<SkillLocator>().passiveSkill = new SkillLocator.PassiveSkill
            {
                enabled = true,
                skillNameToken = SAMIRA_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = SAMIRA_PREFIX + "PASSIVE_DESCRIPTION",
                icon = assetBundle.LoadAsset<Sprite>("texSamiraP"),
            };

            /*//option 2. a new SkillFamily for a passive, used if you want multiple selectable passives
            GenericSkill passiveGenericSkill = Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, "PassiveSkill");
            SkillDef passiveSkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "SamiraPassive",
                skillNameToken = SAMIRA_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = SAMIRA_PREFIX + "PASSIVE_DESCRIPTION",
                keywordTokens = new string[] { "KEYWORD_AGILE" },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSamiraP"),

                //unless you're somehow activating your passive like a skill, none of the following is needed.
                //but that's just me saying things. the tools are here at your disposal to do whatever you like with

                //activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Shoot)),
                //activationStateMachineName = "Weapon1",
                //interruptPriority = EntityStates.InterruptPriority.Skill,

                //baseRechargeInterval = 1f,
                //baseMaxStock = 1,

                //rechargeStock = 1,
                //requiredStock = 1,
                //stockToConsume = 1,

                //resetCooldownTimerOnUse = false,
                //fullRestockOnAssign = true,
                //dontAllowPastMaxStocks = false,
                //mustKeyPress = false,
                //beginSkillCooldownOnSkillEnd = false,

                //isCombatSkill = true,
                //canceledFromSprinting = false,
                //cancelSprintingOnActivation = false,
                //forceSprintDuringState = false,

            });
            Skills.AddSkillsToFamily(passiveGenericSkill.skillFamily, passiveSkillDef1);*/
        }

        //if this is your first look at skilldef creation, take a look at Secondary first
        private void AddPrimarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Primary);

            //the primary skill is created using a constructor for a typical primary
            //it is also a SteppedSkillDef. Custom Skilldefs are very useful for custom behaviors related to casting a skill. see ror2's different skilldefs for reference
            FlairSkillDef primarySkillDef1 = Skills.CreateSkillDef<FlairSkillDef>(new SkillDefInfo
                {
                    skillName = "SamiraFlair",
                    skillNameToken = SAMIRA_PREFIX + "PRIMARY_FLAIR_NAME",
                    skillDescriptionToken = SAMIRA_PREFIX + "PRIMARY_FLAIR_DESCRIPTION",
                    skillIcon = assetBundle.LoadAsset<Sprite>("texSamiraQ"),
                    activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Flair)),
                    activationStateMachineName = "Weapon",
                });
            //custom Skilldefs can have additional fields that you can set manually
            //step count translates to swing index, index starts at 0
            primarySkillDef1.stepCount = SamiraStaticValues.attacksPerFlair;
            primarySkillDef1.stepGraceDuration = 1f;
            
            //the primary skill is created using a constructor for a typical primary
            //it is also a SteppedSkillDef. Custom Skilldefs are very useful for custom behaviors related to casting a skill. see ror2's different skilldefs for reference
            FlairSkillDef primarySkillDef2 = Skills.CreateSkillDef<FlairSkillDef>(new SkillDefInfo
            {
                skillName = "SamiraExplosiveShot",
                skillNameToken = SAMIRA_PREFIX + "PRIMARY_EXPLOSIVESHOT_NAME",
                skillDescriptionToken = SAMIRA_PREFIX + "PRIMARY_EXPLOSIVESHOT_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSamiraQ"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.ExplosiveShot)),
                activationStateMachineName = "Weapon",
            });
            //custom Skilldefs can have additional fields that you can set manually
            //step count translates to swing index, index starts at 0
            primarySkillDef2.stepCount = SamiraStaticValues.attacksPerExplosiveShot;
            primarySkillDef2.stepGraceDuration = 1f;
            
            //the primary skill is created using a constructor for a typical primary
            //it is also a SteppedSkillDef. Custom Skilldefs are very useful for custom behaviors related to casting a skill. see ror2's different skilldefs for reference
            FlairSkillDef primarySkillDef3 = Skills.CreateSkillDef<FlairSkillDef>(new SkillDefInfo
            {
                skillName = "SamiraSlashingManiac",
                skillNameToken = SAMIRA_PREFIX + "PRIMARY_SLASHINGMANIAC_NAME",
                skillDescriptionToken = SAMIRA_PREFIX + "PRIMARY_SLASHINGMANIAC_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSamiraQ"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.SlashingManiac)),
                activationStateMachineName = "Weapon",
            });
            //custom Skilldefs can have additional fields that you can set manually
            //step count translates to swing index, index starts at 0
            primarySkillDef3.stepCount = SamiraStaticValues.attacksPerFlair;
            primarySkillDef3.stepGraceDuration = 1f;
            

            Skills.AddPrimarySkills(bodyPrefab, primarySkillDef1, primarySkillDef2, primarySkillDef3);
        }

        private void AddSecondarySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Secondary);

            //here is a basic skill def with all fields accounted for
            BladeWhirlSkillDef secondarySkillDef1 = Skills.CreateSkillDef<BladeWhirlSkillDef>(new SkillDefInfo
            {
                skillName = "SamiraBladeWhirl",
                skillNameToken = SAMIRA_PREFIX + "SECONDARY_BLADEWHIRL_NAME",
                skillDescriptionToken = SAMIRA_PREFIX + "SECONDARY_BLADEWHIRL_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSamiraW"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.BladeWhirl)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 10f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });
            BladeWhirlSkillDef secondarySkillDef2 = Skills.CreateSkillDef<BladeWhirlSkillDef>(new SkillDefInfo
            {
                skillName = "SamiraExposingWhirl",
                skillNameToken = SAMIRA_PREFIX + "SECONDARY_EXPOSINGWHIRL_NAME",
                skillDescriptionToken = SAMIRA_PREFIX + "SECONDARY_EXPOSINGWHIRL_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSamiraW"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.ExposingWhirl)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.Skill,

                baseRechargeInterval = 12f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            Skills.AddSecondarySkills(bodyPrefab, secondarySkillDef1, secondarySkillDef2);
        }

        private void AddUtiitySkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Utility);

            //here's a skilldef of a typical movement skill.
            SkillDef utilitySkillDef1 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "SamiraWildRush",
                skillNameToken = SAMIRA_PREFIX + "UTILITY_WILDRUSH_NAME",
                skillDescriptionToken = SAMIRA_PREFIX + "UTILITY_WILDRUSH_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSamiraE"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(WildRush)),
                activationStateMachineName = "Body",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 6f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });
            
            SkillDef utilitySkillDef2 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "SamiraQuickSteps",
                skillNameToken = SAMIRA_PREFIX + "UTILITY_QUICKSTEPS_NAME",
                skillDescriptionToken = SAMIRA_PREFIX + "UTILITY_QUICKSTEPS_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSamiraE"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(QuickSteps)),
                activationStateMachineName = "Body",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 8f,
                baseMaxStock = 2,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });

            Skills.AddUtilitySkills(bodyPrefab, utilitySkillDef1, utilitySkillDef2);
        }

        private void AddSpecialSkills()
        {
            Skills.CreateGenericSkillWithSkillFamily(bodyPrefab, SkillSlot.Special);

            //a basic skill. some fields are omitted and will just have default values
            InfernoTriggerSkillDef specialSkillDef1 = Skills.CreateSkillDef<InfernoTriggerSkillDef>(new SkillDefInfo
            {
                skillName = "SamiraInfernoTrigger",
                skillNameToken = SAMIRA_PREFIX + "SPECIAL_INFERNOTRIGGER_NAME",
                skillDescriptionToken = SAMIRA_PREFIX + "SPECIAL_INFERNOTRIGGER_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSamiraR6"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.InfernoTrigger)),
                //setting this to the "weapon2" EntityStateMachine allows us to cast this skill at the same time primary, which is set to the "weapon" EntityStateMachine
                activationStateMachineName = "Weapon2", interruptPriority = EntityStates.InterruptPriority.Skill,

                baseMaxStock = 1,
                requiredStock = 1,
                rechargeStock = 1,
                stockToConsume = 1,
                baseRechargeInterval = 7f,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                mustKeyPress = false
                
            });
            
            InfernoTriggerSkillDef specialSkillDef2 = Skills.CreateSkillDef<InfernoTriggerSkillDef>(new SkillDefInfo
            {
                skillName = "SamiraInfiniteRain",
                skillNameToken = SAMIRA_PREFIX + "SPECIAL_INFINITERAIN_NAME",
                skillDescriptionToken = SAMIRA_PREFIX + "SPECIAL_INFINITERAIN_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSamiraR6"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.InfiniteRain)),
                //setting this to the "weapon2" EntityStateMachine allows us to cast this skill at the same time primary, which is set to the "weapon" EntityStateMachine
                activationStateMachineName = "Weapon2", interruptPriority = EntityStates.InterruptPriority.Skill,

                baseMaxStock = 1,
                requiredStock = 1,
                rechargeStock = 1,
                stockToConsume = 1,
                baseRechargeInterval = 7f,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                mustKeyPress = false
            });
            
            Skills.AddSpecialSkills(bodyPrefab, specialSkillDef1, specialSkillDef2);
        }
        #endregion skills
        
        #region skins
        public override void InitializeSkins()
        {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texSamiraIcon"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);
            defaultSkin.name = "DefaultSkin";
            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
                //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
                //uncomment this when you have another skin
            defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
                "baseSamira_bodyMesh",
                "baseSamira_pistolMesh",
                "baseSamira_revolverMesh",
                "baseSamira_swordMesh");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            #region MasterySkin
            
            ////creating a new skindef as we did before
            //SkinDef masterySkin = Modules.Skins.CreateSkinDef(HENRY_PREFIX + "MASTERY_SKIN_NAME",
            //    assetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
            //    defaultRendererinfos,
            //    prefabCharacterModel.gameObject,
            //    HenryUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            //masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
            //    "meshHenrySwordAlt",
            //    null,//no gun mesh replacement. use same gun mesh
            //    "meshHenryAlt");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            //masterySkin.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[1].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");
            //masterySkin.rendererInfos[2].defaultMaterial = assetBundle.LoadMaterial("matHenryAlt");

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            //masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            //{
            //    new SkinDef.GameObjectActivation
            //    {
            //        gameObject = childLocator.FindChildGameObject("GunModel"),
            //        shouldActivate = false,
            //    }
            //};
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            //skins.Add(masterySkin);
            
            #endregion
            
            #region DanteSkin
            
            SkinDef danteSkin = Skins.CreateSkinDef(SAMIRA_PREFIX +"DANTE_SKIN",assetBundle.LoadAsset<Sprite>("texDanteIcon"),defaultRendererinfos,prefabCharacterModel.gameObject);
            danteSkin.name = "DanteSkin";
            danteSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos, "dante_bodyMesh", "dante_pistolMesh","dante_revolverMesh","dante_swordMesh");
            danteSkin.rendererInfos[0].defaultMaterial = assetBundle.LoadMaterial("mat_dante_body");
            danteSkin.rendererInfos[1].defaultMaterial = assetBundle.LoadMaterial("mat_dante_gun");
            danteSkin.rendererInfos[2].defaultMaterial = assetBundle.LoadMaterial("mat_dante_gun");
            danteSkin.rendererInfos[3].defaultMaterial = assetBundle.LoadMaterial("mat_dante_sword");
            skins.Add(danteSkin);
            #endregion

            skinController.skins = skins.ToArray();
        }

        
        #endregion skins

        //Character Master is what governs the AI of your character when it is not controlled by a player (artifact of vengeance, goobo)
        public override void InitializeCharacterMaster()
        {
            //you must only do one of these. adding duplicate masters breaks the game.

            //if you're lazy or prototyping you can simply copy the AI of a different character to be used
            //Modules.Prefabs.CloneDopplegangerMaster(bodyPrefab, masterName, "Merc");

            //how to set up AI in code
            SamiraAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);
        }
        

        private void AddHooks()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.SurvivorMannequins.SurvivorMannequinSlotController.ApplyLoadoutToMannequinInstance += SurvivorMannequinSlotController_OnApplyLoadoutToMannequinInstance;
            
        }

        private void SetupRiskOfOptions()
        {
            Sprite icon = assetBundle.LoadAsset<Sprite>("texSamiraIcon");

            ModSettingsManager.SetModIcon(icon);
        }

       

        private void SurvivorMannequinSlotController_OnApplyLoadoutToMannequinInstance(SurvivorMannequinSlotController.orig_ApplyLoadoutToMannequinInstance orig, RoR2.SurvivorMannequins.SurvivorMannequinSlotController self)
        {
            
            orig(self);

            if (self.currentLoadout != null && self.currentLoadout.bodyLoadoutManager != null && self.currentSurvivorDef != null)
            {
                BodyIndex bodyIndexFromSurvivorIndex = SurvivorCatalog.GetBodyIndexFromSurvivorIndex(self.currentSurvivorDef.survivorIndex);
                if (bodyIndexFromSurvivorIndex == BodyIndex.None) return;
                int skinIndex = (int)self.currentLoadout.bodyLoadoutManager.GetSkinIndex(bodyIndexFromSurvivorIndex);
                SkinDef safe = ArrayUtils.GetSafe(BodyCatalog.GetBodySkins(bodyIndexFromSurvivorIndex), skinIndex);
                if (!safe) return;
                if (self.currentSurvivorDef.bodyPrefab.gameObject != bodyPrefab.gameObject) return;
               
                var menu = self.currentSurvivorDef.displayPrefab.GetComponent<SamiraMenu>();
                menu.SetAndPlaySoundPrefix(safe.name);
            }
            
           
        }
        
        


        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {

            if (sender.HasBuff(SamiraBuffs.armorBuff))
            {
                args.armorAdd += 300;
            }

            if (sender.HasBuff(SamiraBuffs.comboBuff1))
            {
                args.moveSpeedMultAdd += 0.04f;
            }
            if (sender.HasBuff(SamiraBuffs.comboBuff2))
            {
                args.moveSpeedMultAdd += 0.08f;
            }
            if (sender.HasBuff(SamiraBuffs.comboBuff3))
            {
                args.moveSpeedMultAdd += 0.12f;
            }
            if (sender.HasBuff(SamiraBuffs.comboBuff4))
            {
                args.moveSpeedMultAdd += 0.16f;
            }
            if (sender.HasBuff(SamiraBuffs.comboBuff5))
            {
                args.moveSpeedMultAdd += 0.20f;
            }
            if (sender.HasBuff(SamiraBuffs.comboBuff6))
            {
                args.moveSpeedMultAdd += 0.25f;
            }

            if (sender.HasBuff(SamiraBuffs.wildRushAttackSpeedBuff))
            {
                args.attackSpeedMultAdd += 0.30f;
            }

            if (sender.HasBuff(SamiraBuffs.meleeOnHitBuff))
            {
                args.moveSpeedMultAdd += SamiraStaticValues.slashBonusMS;
                args.attackSpeedMultAdd += SamiraStaticValues.slashBonusAS;
            }

            if (sender.HasBuff(SamiraBuffs.bladeWhirlArmorShredDebuff))
            {
                args.armorAdd -= SamiraStaticValues.exposeDebuffArmorPen;
            }

            int comboIndex = GetComboIndex(sender);

            if (sender.HasBuff(SamiraBuffs.coinOnHitBuff))
            {
                args.damageMultAdd += 0.06f * comboIndex;
                args.moveSpeedMultAdd += 0.06f * comboIndex;
            }

            if (sender.HasBuff(SamiraBuffs.danceBuff))
            {
                args.healthMultAdd +=  0.03f;
                args.armorAdd += 0.02f;
            }
            
        }

        private int GetComboIndex(CharacterBody sender)
        {
            if (sender.HasBuff(SamiraBuffs.comboBuff6))
            {
                return 6;
            }
            if (sender.HasBuff(SamiraBuffs.comboBuff5))
            {
                return 5;
            }
            if (sender.HasBuff(SamiraBuffs.comboBuff4))
            {
                return 4;
            }
            if (sender.HasBuff(SamiraBuffs.comboBuff3))
            {
                return 3;
            }
            if (sender.HasBuff(SamiraBuffs.comboBuff2))
            {
                return 2;
            }
            if (sender.HasBuff(SamiraBuffs.comboBuff1))
            {
                return 1;
            }

            // There is no combo buff
            return 0;
        }
    }
}