using System;
using BepInEx;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using R2API.Utils;
using SamiraMod.Survivors.Samira;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

//rename this namespace
namespace SamiraMod
{
    //[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.prefab", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.language", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.sound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.networking", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.recalculatestats", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    public class SamiraPlugin : BaseUnityPlugin
    {
        // if you do not change this, you are giving permission to deprecate the mod-
        //  please change the names to your own stuff, thanks
        //   this shouldn't even have to be said
        public const string MODUID = "com.zidane.SamiraMod";
        public const string MODNAME = "SamiraMod";
        public const string MODVERSION = "1.3.0";

        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string DEVELOPER_PREFIX = "ZIDANE";

        public static SamiraPlugin instance;
        
        void Awake()
        {
            instance = this;

            try
            {
                //easy to use logger
                Log.Init(Logger);

                Modules.Config.ReadConfig();

                // used when you want to properly set up language folders
                Modules.Language.Init();

                // character initialization
                new SamiraSurvivor().Initialize();

                // make a content pack and add it. this has to be last
                new Modules.ContentPacks().Initialize();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            }

            
            
        }
    }
}
