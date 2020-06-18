using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GamecraftModdingAPI.App;
using GamecraftModdingAPI.Commands;
using IllusionPlugin;
using RobocraftX.Schedulers;
using Svelto.Tasks.ExtraLean;
using UnityEngine;
using uREPL;

namespace GCDC
{
    public class GCDCPlugin : IPlugin
    {
        public string Name { get; } = Assembly.GetExecutingAssembly().GetName().Name;
        public string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public void OnApplicationStart()
        {
            GamecraftModdingAPI.Main.Init();
            var client = new DiscordClient(this);
            CommandBuilder.Builder("dc", "Send messages to Discord.")
                .Action<string>(client.SendMessage).Build();
            CommandBuilder.Builder("dcsetup", "Initial setup for GCDC. The argument is the channel ID first. For example: dcsetup \"420159832423923714\"")
                .Action<string>(client.Setup).Build();
            Game.Enter += (sender, e) =>
                client.Ready();
            Game.Edit += (sender, e) =>
                client.Update(); //Update text block
            Game.Exit += (sender, e) =>
                client.Stop();
            Debug.Log("GCDC loaded");
        }

        public void Update(Queue<string> messages)
        {
            UpdateEnum(messages).RunOn(ExtraLean.EveryFrameStepRunner_RUNS_IN_TIME_STOPPED_AND_RUNNING);
        }

        private IEnumerator UpdateEnum(Queue<string> messages)
        {
            var txt = messages.Count > 0
                ? messages.Aggregate((current, msg) => current + "\n" + msg)
                : "<No messages yet>";
            RuntimeCommands.Call("ChangeTextBlockCommand", "Discord", txt);
            yield break;
        }

        public void OnApplicationQuit()
        {
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