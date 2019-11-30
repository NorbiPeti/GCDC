using System;
using System.Reflection;
using Harmony;
using RobocraftX;
using RobocraftX.GUI.CommandLine;
using RobocraftX.Multiplayer;
using Svelto.Context;
using Svelto.ECS;
using Unity.Entities;
using UnityEngine;

namespace GCDC
{
    [HarmonyPatch]
    public class DiscordEngineInjectionPatch
    {
        static void Postfix(UnityContext<FullGameCompositionRoot> contextHolder, EnginesRoot enginesRoot, World physicsWorld, Action reloadGame, MultiplayerInitParameters multiplayerParameters)
        {
            enginesRoot.AddEngine(new TextBlockUpdateEngine());
            Debug.Log($"Added text block update engine");
        }

        static MethodBase TargetMethod(HarmonyInstance instance)
        {
            return _ComposeMethodInfo(CommandLineCompositionRoot.Compose<UnityContext<FullGameCompositionRoot>>);
        }

        private static MethodInfo _ComposeMethodInfo(Action<UnityContext<FullGameCompositionRoot>, EnginesRoot, World, Action, MultiplayerInitParameters> a)
        {
            return a.Method;
        }

    }
}