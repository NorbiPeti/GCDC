using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RobocraftX.Blocks.GUI;
using RobocraftX.Common;
using RobocraftX.Common.Input;
using RobocraftX.Common.Utilities;
using RobocraftX.SimulationModeState;
using RobocraftX.StateSync;
using Svelto.ECS;
using Svelto.ECS.Experimental;
using Unity.Jobs;
using UnityEngine.Diagnostics;
using uREPL;

namespace GCDC
{
    public class TextBlockUpdateEngine : IQueryingEntitiesEngine, IDeterministicSim, IInitializeOnBuildStart
    {
        private DiscordSocketClient _client;
        public void Ready()
        {
            try
            {
                /*Console.WriteLine("Current directory: " + Environment.CurrentDirectory);
                Console.WriteLine(Assembly.LoadFrom("Discord.Net.WebSocket.dll")?.DefinedTypes?.Select(t => t.FullName)
                    .Aggregate((a, b) => a + "\n" + b));*/ //Spent a couple hours trying to figure out why it doesn't even load anymore - somehow I got a completely different System.Collections.Immutable.dll
                if (!RuntimeCommands.HasRegistered("dc"))
                    RuntimeCommands.Register<string>("dc", AddMessage);
                Start();
            }
            catch (TypeLoadException e)
            {
                Console.WriteLine("Type load exception for type: "+e.TypeName);
                Console.WriteLine(e);
            }
        }

        private void Start()
        {
            _client=new DiscordSocketClient(new DiscordSocketConfig()
            {
               LogLevel = LogSeverity.Debug,
               DefaultRetryMode = RetryMode.RetryRatelimit
            });
            _client.Log += msg=>
            {
                Log.Output(msg.Message);
                //Window.selected.OutputLog(new Log.Data(msg.Message, "Discord", Log.Level.Verbose));
                return Task.CompletedTask;
            };
            Setup();
        }
        private async void Setup()
        {
            try
            {
                const string path = "discordToken.json";
                const string notoken =
                    "Please add your bot token to the discordToken.json file in game files and run this again.";
                if (!File.Exists(path))
                {
                    var obj = new JObject {["token"] = "Put your token here"};
                    File.WriteAllText(path, obj.ToString());
                    Log.Error(notoken);
                    return;
                }

                string token;
                try
                {
                    token = (string) JObject.Parse(File.ReadAllText(path))["token"];
                    if (token.Contains(" "))
                    {
                        Log.Error(notoken);
                        return;
                    }

                }
                catch (JsonReaderException exception)
                {
                    Log.Error("Failed to read token! " + exception);
                    return;
                }

                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        public IEntitiesDB entitiesDB { get; set; }
        public string name { get; } = "GCDC-TextUpdate";
        private volatile Queue<string> messages = new Queue<string>();
        private volatile bool updatedTextBlock;
        
        public JobHandle SimulatePhysicsStep(
            in float deltaTime,
            in PhysicsUtility utility,
            in PlayerInput[] playerInputs) //Gamecraft.Blocks.ConsoleBlock.dll
        {
            if (updatedTextBlock)
                return new JobHandle();
            var txt = messages.Count > 0 ? messages.Aggregate((current, msg) => current + "\n" + msg) : "<No messages yet>";
            RuntimeCommands.Call("ChangeTextBlockCommand", "Discord", txt);
            updatedTextBlock = true;

            return new JobHandle();
        }

        public void AddMessage(string message)
        {
            messages.Enqueue(message);
            if (messages.Count > 10)
                messages.Dequeue();
            updatedTextBlock = false;
        }

        public JobHandle OnInitializeBuildMode()
        {
            updatedTextBlock = false; //Update text block
            return new JobHandle();
        }
    }
}