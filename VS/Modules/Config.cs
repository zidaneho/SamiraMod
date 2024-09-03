using System;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using UnityEngine;

namespace SamiraMod.Modules
{
    public static class Config
    {
        public static ConfigFile MyConfig = SamiraPlugin.instance.Config;
        
        public static ConfigEntry<bool> enableCharacter;
        public static ConfigEntry<bool> enableVoiceLines;
        
        public static ConfigEntry<float> armorGrowth;
        public static ConfigEntry<float> baseArmor;
        public static ConfigEntry<float> baseCrit;
        public static ConfigEntry<float> baseDamage;
        public static ConfigEntry<float> baseHealth;
        public static ConfigEntry<float> baseMovementSpeed;
        public static ConfigEntry<float> baseRegen;
        public static ConfigEntry<float> bonusHealthCoefficient;
        public static ConfigEntry<float> healthGrowth;
        public static ConfigEntry<int> jumpCount;
        
        public static ConfigEntry<float> damageGrowth;
        public static ConfigEntry<float> regenGrowth;
        
        
        public static ConfigEntry<KeyCode> tauntKeybind;
        public static ConfigEntry<KeyCode> jokeKeybind;
        public static ConfigEntry<KeyCode> laughKeybind;
        public static ConfigEntry<KeyCode> danceKeybind;

        public static void ReadConfig()
        {
            enableCharacter = MyConfig.Bind<bool>(new ConfigDefinition("00 - Other", "Enable Character"), true,
                new ConfigDescription("Enable Character", null, Array.Empty<object>()));
            enableVoiceLines = MyConfig.Bind<bool>(
                new ConfigDefinition("00 - Other", "Voice Lines"), true,
                new ConfigDescription("Enable Voice Lines", null, Array.Empty<object>()));
            baseHealth = MyConfig.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Health"), 110f, new ConfigDescription("", null, Array.Empty<object>()));
            healthGrowth = MyConfig.Bind<float>(new ConfigDefinition("01 - Character Stats", "Health Growth"), 30f, new ConfigDescription("", null, Array.Empty<object>()));

            baseRegen = MyConfig.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Health Regen"), 1f, new ConfigDescription("", null, Array.Empty<object>()));
            regenGrowth = MyConfig.Bind<float>(new ConfigDefinition("01 - Character Stats", "Health Regen Growth"), 0.2f, new ConfigDescription("", null, Array.Empty<object>()));

            baseArmor = MyConfig.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Armor"), 20f, new ConfigDescription("", null, Array.Empty<object>()));
            armorGrowth = MyConfig.Bind<float>(new ConfigDefinition("01 - Character Stats", "Armor Growth"), 0f, new ConfigDescription("", null, Array.Empty<object>()));

            baseDamage = MyConfig.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Damage"), 12f, new ConfigDescription("", null, Array.Empty<object>()));
            damageGrowth = MyConfig.Bind<float>(new ConfigDefinition("01 - Character Stats", "Damage Growth"), 2.4f, new ConfigDescription("", null, Array.Empty<object>()));

            baseMovementSpeed = MyConfig.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Movement Speed"), 7f, new ConfigDescription("", null, Array.Empty<object>()));

            baseCrit = MyConfig.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Crit"), 1f, new ConfigDescription("", null, Array.Empty<object>()));

            jumpCount = MyConfig.Bind<int>(new ConfigDefinition("01 - Character Stats", "Jump Count"), 1, new ConfigDescription("", null, Array.Empty<object>()));
            
            
            tauntKeybind = SamiraPlugin.instance.Config.Bind<KeyCode>(new ConfigDefinition("02 - Emotes", "Taunt"), KeyCode.Alpha2, new ConfigDescription("Keybind used to perform the Taunt emote", null, Array.Empty<object>()));
            jokeKeybind = SamiraPlugin.instance.Config.Bind<KeyCode>(new ConfigDefinition("02 - Emotes", "Joke"), KeyCode.Alpha1, new ConfigDescription("Keybind used to perform the Joke emote", null, Array.Empty<object>()));
            laughKeybind = SamiraPlugin.instance.Config.Bind<KeyCode>(new ConfigDefinition("02 - Emotes", "Laugh"), KeyCode.Alpha4, new ConfigDescription("Keybind used to perform the Laugh emote", null, Array.Empty<object>()));
            danceKeybind = SamiraPlugin.instance.Config.Bind<KeyCode>(new ConfigDefinition("02 - Emotes", "Dance"), KeyCode.Alpha3, new ConfigDescription("Keybind used to perform the Dance emote", null, Array.Empty<object>()));
        }

        /// <summary>
        /// automatically makes config entries for disabling survivors
        /// </summary>
        /// <param name="section"></param>
        /// <param name="characterName"></param>
        /// <param name="description"></param>
        /// <param name="enabledByDefault"></param>
        public static ConfigEntry<bool> CharacterEnableConfig(string section, string characterName, string description = "", bool enabledByDefault = true)
        {

            if (string.IsNullOrEmpty(description))
            {
                description = "Set to false to disable this character and as much of its code and content as possible";
            }
            return BindAndOptions<bool>(section,
                                        "Enable " + characterName,
                                        enabledByDefault,
                                        description,
                                        true);
        }

        public static ConfigEntry<T> BindAndOptions<T>(string section, string name, T defaultValue, string description = "", bool restartRequired = false) =>
            BindAndOptions<T>(section, name, defaultValue, 0, 20, description, restartRequired);
        public static ConfigEntry<T> BindAndOptions<T>(string section, string name, T defaultValue, float min, float max, string description = "", bool restartRequired = false)
        {
            if (string.IsNullOrEmpty(description))
            {
                description = name;
            }

            if (restartRequired)
            {
                description += " (restart required)";
            }
            ConfigEntry<T> configEntry = MyConfig.Bind(section, name, defaultValue, description);

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                //TryRegisterOption(configEntry, min, max, restartRequired);
            }

            return configEntry;
        }

        //back compat
        public static ConfigEntry<float> BindAndOptionsSlider(string section, string name, float defaultValue, string description, float min = 0, float max = 20, bool restartRequired = false) =>
            BindAndOptions<float>(section, name, defaultValue, min, max, description, restartRequired);

        //add risk of options dll to your project libs and uncomment this for a soft dependency
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void TryRegisterOption<T>(ConfigEntry<T> entry, float min, float max, bool restartRequired)
        {
            //if (entry is ConfigEntry<float>)
            //{
            //    ModSettingsManager.AddOption(new SliderOption(entry as ConfigEntry<float>, new SliderConfig() { min = min, max = max, formatString = "{0:0.00}", restartRequired = restartRequired }));
            //}
            //if (entry is ConfigEntry<int>)
            //{
            //    ModSettingsManager.AddOption(new IntSliderOption(entry as ConfigEntry<int>, new IntSliderConfig() { min = (int)min, max = (int)max, restartRequired = restartRequired }));
            //}
            //if (entry is ConfigEntry<bool>)
            //{
            //    ModSettingsManager.AddOption(new CheckBoxOption(entry as ConfigEntry<bool>, restartRequired));
            //}
            //if (entry is BepInEx.Configuration.ConfigEntry<KeyboardShortcut>)
            //{
            //    ModSettingsManager.AddOption(new KeyBindOption(entry as ConfigEntry<KeyboardShortcut>, restartRequired));
            //}
        }

        //Taken from https://github.com/ToastedOven/CustomEmotesAPI/blob/main/CustomEmotesAPI/CustomEmotesAPI/CustomEmotesAPI.cs
        public static bool GetKeyPressed(KeyboardShortcut entry)
        {
            foreach (var item in entry.Modifiers)
            {
                if (!Input.GetKey(item))
                {
                    return false;
                }
            }
            return Input.GetKeyDown(entry.MainKey);
        }
        
    }
}
