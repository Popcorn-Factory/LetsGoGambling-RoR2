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
using MonoMod.RuntimeDetour;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;

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
        public const string MODVERSION = "1.2.2";

        public const string DEVELOPER_PREFIX = "POPCORN";
        public static PluginInfo PInfo { get; private set; }
        public static LetsGoGamblingPlugin instance;

        private static Hook AddBankAfterAKSoundEngineInit;
        public static bool hasSucceeded = false;

        private void Awake()
        {
            instance = this;
            PInfo = Info;
            Log.Init(Logger);
            Modules.PluginAssets.Initialize(); // load assets and read config
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
            AddBankAfterAKSoundEngineInit = new Hook(
                typeof(AkSoundEngineInitialization).GetMethodCached(nameof(AkSoundEngineInitialization.InitializeSoundEngine)),
                typeof(LetsGoGamblingPlugin).GetMethodCached(nameof(UpdateAfterInit)));
        }

        private static bool UpdateAfterInit(Func<AkSoundEngineInitialization, bool> orig, AkSoundEngineInitialization self)
        {
            var res = orig(self);

            if (AkSoundEngine.IsInitialized())
            {
                AkSoundEngine.SetRTPCValue("Volume_Gambling_SFX", Modules.Config.volumeSlider.Value);
            }

            return res;
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

            if (!self.pingTarget) 
            {
                return;
            }

            if (master.hasEffectiveAuthority && Modules.Config.playOnPing.Value && self.pingTarget.gameObject.name.Contains("ShrineChance")) 
            {
                ShrineChanceBehavior shrineBehaviour = self.pingTarget.GetComponent<ShrineChanceBehavior>();

                if (Modules.Config.playEvenIfExpended.Value || shrineBehaviour.successfulPurchaseCount < shrineBehaviour.maxPurchaseCount)
                {
                    new PlaySoundNetworkRequest(master.GetBody().netId, "gambling_letsgogambling").Send(NetworkDestination.Clients);
                }
               
                //Component[] components = self.pingTarget.GetComponents<Component>();
                //foreach (Component c in components)
                //{
                //    Chat.AddMessage(c.ToString());
                //}
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