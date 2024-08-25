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
        public const string MODUID = "com.PopcornFactory.LetsGoGambling";
        public const string MODNAME = "LetsGoGambling";
        public const string MODVERSION = "1.1.0";

        public const string DEVELOPER_PREFIX = "POPCORN";

        public static LetsGoGamblingPlugin instance;

        public static bool hasSucceeded = false;

        private void Awake()
        {
            instance = this;

            Log.Init(Logger);
            Modules.Assets.Initialize(); // load assets and read config
            Modules.Config.ReadConfig();
            if (Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                Modules.Config.SetupRiskOfOptions();
                Modules.Config.OnChangeHooks();
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
            On.RoR2.UI.PingIndicator.RebuildPing += PingIndicator_RebuildPing;
        }

        private void PingIndicator_RebuildPing(On.RoR2.UI.PingIndicator.orig_RebuildPing orig, RoR2.UI.PingIndicator self)
        {
            orig(self);

            CharacterMaster master = self.pingOwner.GetComponent<CharacterMaster>();
            if (!master) 
            {
                //Chat.AddMessage($"NoBody on {self.pingOwner}");

                //Component[] components = self.pingOwner.GetComponents<Component>();
                //foreach (Component c in components) 
                //{
                //    Chat.AddMessage(c.ToString());
                //}
                return;
            }

            if (master.hasEffectiveAuthority && Modules.Config.playOnPing.Value && self.pingTarget.gameObject.name.Contains("ShrineChance")) 
            {
                new PlaySoundNetworkRequest(master.GetBody().netId, "gambling_letsgogambling").Send(NetworkDestination.Clients);
            }


        }


        private void ShrineChanceBehavior_AddShrineStack(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, RoR2.ShrineChanceBehavior self, RoR2.Interactor activator)
        {
            //Observe before and after and play if the success changes/does not change.
            int currentSuccessAmount = self.successfulPurchaseCount;
            orig(self, activator);
            int afterAmount = self.successfulPurchaseCount;

            if (NetworkServer.active) 
            {
                if (hasSucceeded && currentSuccessAmount != afterAmount)
                {
                    new EmitSoundAtPoint(444218535, self.gameObject.transform.position).Send(NetworkDestination.Clients);
                    return;
                }

                if (currentSuccessAmount != afterAmount)
                {
                    hasSucceeded = true;
                    new EmitSoundAtPoint(853644935, self.gameObject.transform.position).Send(NetworkDestination.Clients);
                    return;
                }

                hasSucceeded = false;
                new EmitSoundAtPoint(51628376, self.gameObject.transform.position).Send(NetworkDestination.Clients);
            }
        }

        private void SetupNetworkMessages() 
        {
            NetworkingAPI.RegisterMessageType<EmitSoundAtPoint>();
            NetworkingAPI.RegisterMessageType<PlaySoundNetworkRequest>();
        }
    }
}