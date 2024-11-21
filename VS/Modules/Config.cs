using System;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using UnityEngine;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace SamiraMod.Modules
{
    public static class Config
    {
        public static ConfigFile MyConfig = SamiraPlugin.instance.Config;
        
        public static ConfigEntry<bool> enableCharacter;
        public static ConfigEntry<bool> enableVoiceLines;
        public static ConfigEntry<float> soundEffectVolume;
        public static ConfigEntry<float> voiceEffectVolume;
        
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
            // General Settings
            enableCharacter = MyConfig.Bind<bool>(
                new ConfigDefinition("00 - Other", "Enable Character"),
                true,
                new ConfigDescription("Enable Character", null, Array.Empty<object>())
            );
            TryRegisterOption(enableCharacter, 0, 1, false);

            enableVoiceLines = MyConfig.Bind<bool>(
                new ConfigDefinition("00 - Other", "Voice Lines"),
                true,
                new ConfigDescription("Enable Voice Lines", null, Array.Empty<object>())
            );
            TryRegisterOption(enableVoiceLines, 0, 1, false);

            // Character Stats
            baseHealth = MyConfig.Bind<float>(
                new ConfigDefinition("01 - Character Stats", "Base Health"),
                110f,
                new ConfigDescription("", null, Array.Empty<object>())
            );
            TryRegisterOption(baseHealth, 0f, 500f, false); // Slider for health: range 0 to 500

            healthGrowth = MyConfig.Bind<float>(
                new ConfigDefinition("01 - Character Stats", "Health Growth"),
                30f,
                new ConfigDescription("", null, Array.Empty<object>())
            );
            TryRegisterOption(healthGrowth, 0f, 100f, false); // Slider for health growth

            baseRegen = MyConfig.Bind<float>(
                new ConfigDefinition("01 - Character Stats", "Base Health Regen"),
                1f,
                new ConfigDescription("", null, Array.Empty<object>())
            );
            TryRegisterOption(baseRegen, 0f, 10f, false);

            regenGrowth = MyConfig.Bind<float>(
                new ConfigDefinition("01 - Character Stats", "Health Regen Growth"),
                0.2f,
                new ConfigDescription("", null, Array.Empty<object>())
            );
            TryRegisterOption(regenGrowth, 0f, 5f, false);

            baseArmor = MyConfig.Bind<float>(
                new ConfigDefinition("01 - Character Stats", "Base Armor"),
                20f,
                new ConfigDescription("", null, Array.Empty<object>())
            );
            TryRegisterOption(baseArmor, 0f, 100f, false);

            armorGrowth = MyConfig.Bind<float>(
                new ConfigDefinition("01 - Character Stats", "Armor Growth"),
                0f,
                new ConfigDescription("", null, Array.Empty<object>())
            );
            TryRegisterOption(armorGrowth, 0f, 10f, false);

            baseDamage = MyConfig.Bind<float>(
                new ConfigDefinition("01 - Character Stats", "Base Damage"),
                12f,
                new ConfigDescription("", null, Array.Empty<object>())
            );
            TryRegisterOption(baseDamage, 0f, 50f, false);

            damageGrowth = MyConfig.Bind<float>(
                new ConfigDefinition("01 - Character Stats", "Damage Growth"),
                2.4f,
                new ConfigDescription("", null, Array.Empty<object>())
            );
            TryRegisterOption(damageGrowth, 0f, 10f, false);

            baseMovementSpeed = MyConfig.Bind<float>(
                new ConfigDefinition("01 - Character Stats", "Base Movement Speed"),
                7f,
                new ConfigDescription("", null, Array.Empty<object>())
            );
            TryRegisterOption(baseMovementSpeed, 0f, 20f, false);

            baseCrit = MyConfig.Bind<float>(
                new ConfigDefinition("01 - Character Stats", "Base Crit"),
                1f,
                new ConfigDescription("", null, Array.Empty<object>())
            );
            TryRegisterOption(baseCrit, 0f, 100f, false);

            jumpCount = MyConfig.Bind<int>(
                new ConfigDefinition("01 - Character Stats", "Jump Count"),
                1,
                new ConfigDescription("", null, Array.Empty<object>())
            );
            TryRegisterOption(jumpCount, 1, 5, false);

            // Emote Keybinds
            tauntKeybind = SamiraPlugin.instance.Config.Bind<KeyCode>(
                new ConfigDefinition("02 - Emotes", "Taunt"),
                KeyCode.Alpha2,
                new ConfigDescription("Keybind used to perform the Taunt emote", null, Array.Empty<object>())
            );
            TryRegisterOption(tauntKeybind, 0, 0, false); // No min/max for keybinds

            jokeKeybind = SamiraPlugin.instance.Config.Bind<KeyCode>(
                new ConfigDefinition("02 - Emotes", "Joke"),
                KeyCode.Alpha1,
                new ConfigDescription("Keybind used to perform the Joke emote", null, Array.Empty<object>())
            );
            TryRegisterOption(jokeKeybind, 0, 0, false);

            laughKeybind = SamiraPlugin.instance.Config.Bind<KeyCode>(
                new ConfigDefinition("02 - Emotes", "Laugh"),
                KeyCode.Alpha4,
                new ConfigDescription("Keybind used to perform the Laugh emote", null, Array.Empty<object>())
            );
            TryRegisterOption(laughKeybind, 0, 0, false);

            danceKeybind = SamiraPlugin.instance.Config.Bind<KeyCode>(
                new ConfigDefinition("02 - Emotes", "Dance"),
                KeyCode.Alpha3,
                new ConfigDescription("Keybind used to perform the Dance emote", null, Array.Empty<object>())
            );
            TryRegisterOption(danceKeybind, 0, 0, false);
            
            
            //Risk of Options
            soundEffectVolume = MyConfig.Bind<float>(new ConfigDefinition("02 - Volume", "Sound Effect Volume"), 50f,
                new ConfigDescription("", null, Array.Empty<object>()));
            voiceEffectVolume = MyConfig.Bind<float>(new ConfigDefinition("02 - Volume", "Voice Effect Volume"), 50f,
                new ConfigDescription("", null, Array.Empty<object>()));
            TryRegisterOption(soundEffectVolume,0,100,false, "{0:F0}");
            TryRegisterOption(voiceEffectVolume,0,100,false,"{0:F0}");
            

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
        private static void TryRegisterOption<T>(ConfigEntry<T> entry, float min, float max, bool restartRequired, String formatString = "{0:0.00}")
        {
            if (entry is ConfigEntry<float> floatEntry)
            {
                ModSettingsManager.AddOption(new SliderOption(floatEntry, new SliderConfig
                {
                    min = min,
                    max = max,
                    FormatString = formatString,
                    restartRequired = restartRequired
                }));
            }
            else if (entry is ConfigEntry<int> intEntry)
            {
                ModSettingsManager.AddOption(new IntSliderOption(intEntry, new IntSliderConfig
                {
                    min = (int)min,
                    max = (int)max,
                    restartRequired = restartRequired
                }));
            }
            else if (entry is ConfigEntry<bool> boolEntry)
            {
                ModSettingsManager.AddOption(new CheckBoxOption(boolEntry, restartRequired));
            }
            else if (entry is ConfigEntry<KeyboardShortcut> keybindEntry)
            {
                ModSettingsManager.AddOption(new KeyBindOption(keybindEntry, restartRequired));
            }
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
