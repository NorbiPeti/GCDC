using System;
using System.Reflection;
using Gamecraft.Blocks.ConsoleBlock;
using Harmony;
using RobocraftX;
using RobocraftX.GUI.CommandLine;
using RobocraftX.Multiplayer;
using RobocraftX.Services.MultiplayerNetworking;
using RobocraftX.StateSync;
using Svelto.ECS;
using Unity.Entities;
using UnityEngine;

namespace GCDC
{
    [HarmonyPatch]
    public class DiscordEngineInjectionPatch
    {
        static void Postfix(EnginesRoot enginesRoot, in StateSyncRegistrationHelper stateSyncReg, bool isAuthoritative)
        {
            if (isAuthoritative)
            {
                stateSyncReg.AddDeterministicEngine(new TextBlockUpdateEngine());
                Debug.Log($"Added Discord text block update engine");
            }
            else
                Debug.Log("Not authoritative, not adding Discord engine");
        }

        static MethodBase TargetMethod(HarmonyInstance instance)
        {
            return _ComposeMethodInfo(ConsoleBlockCompositionRoot.Compose);
        }

        private delegate void ComposeAction(EnginesRoot er, in StateSyncRegistrationHelper ssrh,
            NetworkReceivers networkReceivers, NetworkSender networkSende, bool isAuthoritative);
        private static MethodInfo _ComposeMethodInfo(ComposeAction a)
        {
            return a.Method;
        }

    }
}