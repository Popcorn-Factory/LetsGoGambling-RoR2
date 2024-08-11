using BepInEx.Configuration;
using RiskOfOptions.Options;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;

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
    }
}