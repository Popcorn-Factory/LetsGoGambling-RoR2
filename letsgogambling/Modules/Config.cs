using BepInEx.Configuration;
using RiskOfOptions.Options;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using UnityEngine;

namespace LetsGoGambling.Modules
{
    public static class Config
    {
        public static ConfigEntry<float> volumeSlider;

        public static void ReadConfig()
        {
            volumeSlider = LetsGoGamblingPlugin.instance.Config.Bind<float>
            (
                new ConfigDefinition("01 - Volume", "Volume Slider"),
                50f,
                new ConfigDescription("Changes the volume on the SFX. In-game Volume SFX affects this too!")
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