using System.Reflection;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using System.IO;
using System;

namespace LetsGoGambling.Modules
{
    internal static class Assets
    {
        private const string csProjName = "letsgogambling";
        internal static AssetBundle mainAssetBundle;

        internal static void Initialize()
        {
            LoadAssetBundle();
            LoadSoundbank();
        }

        internal static void LoadAssetBundle()
        {
            try
            {
                if (mainAssetBundle == null)
                {
                    using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{csProjName}.letsgogamblingassets"))
                    {
                        mainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Failed to load assetbundle. Make sure your assetbundle name is setup correctly\n" + e);
                return;
            }
        }

        internal static void LoadSoundbank()
        {                                                                
            //soundbank currently broke, but this is how you should load yours
            using (Stream manifestResourceStream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{csProjName}.letsgogamblingSoundbank.bnk"))
            {
                byte[] array = new byte[manifestResourceStream2.Length];
                manifestResourceStream2.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }
        }

    }
}