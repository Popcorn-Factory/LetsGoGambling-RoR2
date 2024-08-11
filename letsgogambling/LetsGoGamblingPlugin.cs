using BepInEx;
using BepInEx.Bootstrap;
using LetsGoGambling.Modules.Networking;
using R2API.Networking;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;
using UnityEngine.Networking;
using RoR2;
using UnityEngine;
using R2API.Networking.Interfaces;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace LetsGoGambling
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.sound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.networking", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]

    public class LetsGoGamblingPlugin : BaseUnityPlugin
    {
        // if you don't change these you're giving permission to deprecate the mod-
        //  please change the names to your own stuff, thanks
        //   this shouldn't even have to be said
        public const string MODUID = "com.PopcornFactory.LetsGoGambling";
        public const string MODNAME = "LetsGoGambling";
        public const string MODVERSION = "1.0.0";

        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string DEVELOPER_PREFIX = "POPCORN";

        public static LetsGoGamblingPlugin instance;

        private void Awake()
        {
            instance = this;

            Log.Init(Logger);
            Modules.Assets.Initialize(); // load assets and read config
            Modules.Config.ReadConfig();
            if (Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                Modules.Config.SetupRiskOfOptions();
            }

            Hook();
            SetupNetworkMessages();
        }

        private void Start() 
        {
            if (AkSoundEngine.IsInitialized())
            {
                AkSoundEngine.SetRTPCValue("Volume_Gambling_SFX", Modules.Config.volumeSlider.Value);
            }
        }

        private void Hook()
        {
            On.RoR2.ShrineChanceBehavior.AddShrineStack += ShrineChanceBehavior_AddShrineStack;
        }

        private void ShrineChanceBehavior_AddShrineStack(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, RoR2.ShrineChanceBehavior self, RoR2.Interactor activator)
        {
            //Observe before and after and play if the success changes/does not change.
            int currentSuccessAmount = self.successfulPurchaseCount;
            orig(self, activator);
            int afterAmount = self.successfulPurchaseCount;

            if (NetworkServer.active) 
            {
                if (currentSuccessAmount != afterAmount)
                {
                    new EmitSoundAtPoint(853644935, self.gameObject.transform.position).Send(NetworkDestination.Clients);
                }
                else 
                {
                    new EmitSoundAtPoint(51628376, self.gameObject.transform.position).Send(NetworkDestination.Clients);
                }
            }
        }

        private void SetupNetworkMessages() 
        {
            NetworkingAPI.RegisterMessageType<EmitSoundAtPoint>();
        }
    }
}