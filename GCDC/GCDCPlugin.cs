using System.Reflection;
using HarmonyLib;
using IllusionPlugin;
using UnityEngine;

namespace GCDC
{
    public class GCDCPlugin : IPlugin
    {
        public string Name { get; } = "GCDC";
        public string Version { get; } = "v0.0.1";
        public static Harmony harmony { get; protected set; }
        public const string HarmonyID = "io.github.norbipeti.GCDC";
        
        public void OnApplicationStart()
        {
            if (harmony == null)
            {
                harmony = new Harmony(HarmonyID);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            Debug.Log("GCDC loaded");
        }

        public void OnApplicationQuit()
        {
            harmony?.UnpatchAll(HarmonyID);
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }
    }
}