using BepInEx.Configuration;
using RiskOfOptions.Options;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using UnityEngine;

namespace LetsGoGambling.Modules
{
    public static class Config
    {
        public static ConfigEntry<bool> playOnPing;
        public static ConfigEntry<float> volumeSlider;

        public static void ReadConfig()
        {
            volumeSlider = LetsGoGamblingPlugin.instance.Config.Bind<float>
            (
                new ConfigDefinition("01 - Volume", "Volume Slider"),
                50f,
                new ConfigDescription("Changes the volume on the SFX. In-game Volume SFX affects this too!")
            );

            playOnPing = LetsGoGamblingPlugin.instance.Config.Bind<bool>
            (
                new ConfigDefinition("02 - Misc", "Play on ping"),
                true,
                new ConfigDescription("Plays the \"Let's go gambling!\" SFX on ping on a shrine.")
            );
        }

        public static void SetupRiskOfOptions()
        {
            ModSettingsManager.SetModIcon(Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("rooIcon"));
            ModSettingsManager.SetModDescription("Plays the gamblecore SFX on Shrine of Chance attempts.");
            //Risk of Options intialization
            ModSettingsManager.AddOption(
                new StepSliderOption(
                    volumeSlider,
                    new StepSliderConfig
                    {
                        min = 0f,
                        max = 100f,
                        increment = 1f
                    }
                ));

            ModSettingsManager.AddOption(
                new CheckBoxOption(playOnPing)
            );
        }

        public static void OnChangeHooks() 
        {
            volumeSlider.SettingChanged += VolumeSlider_SettingChanged;
        }

        private static void VolumeSlider_SettingChanged(object sender, System.EventArgs e)
        {
            if (AkSoundEngine.IsInitialized())
            {
                AkSoundEngine.SetRTPCValue("Volume_Gambling_SFX", volumeSlider.Value);
            }
        }
    }
}