using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GamecraftModdingAPI.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class TextBlockUpdateEngine : IDeterministicSim, IInitializeOnBuildStart, IApiEngine
    {
        private string _token;
        private bool _running;
        private Thread _rect;
        public void Ready()
        {
            if (!RuntimeCommands.HasRegistered("dc"))
                RuntimeCommands.Register<string>("dc", SendMessage);
            if (!RuntimeCommands.HasRegistered("dcsetup"))
                RuntimeCommands.Register<string>("dcsetup", Setup,
                    "Initial setup for GCDC. The argument is the channel ID first.");
            if (File.Exists("gcdc.json"))
            {
                var jo = JObject.Load(new JsonTextReader(File.OpenText("gcdc.json")));
                _token = jo["token"]?.Value<string>();
            }

            if (_token != null)
                Start();
        }

        public void Setup(string tokenOrChannel)
        {
            if (!tokenOrChannel.Contains("-"))
            {
                if (!long.TryParse(tokenOrChannel, out _))
                {
                    Log.Error("Bad format for channel ID.");
                    return;
                }

                Process.Start(
                    "https://discordapp.com/oauth2/authorize?client_id=680138144812892371&redirect_uri=https%3A%2F%2Fgcdc.herokuapp.com%2Fapi%2Fusers%2Fregister&response_type=code&scope=identify&state=" +
                    tokenOrChannel);
                Log.Output(
                    "Please authorize the GCDC app on the page that should open. This connection is only used to avoid account spam and to display your Discord name.");
            }
            else
            {
                try
                {
                    if (JObject.Parse(WebUtils.Request("users/get?token=" + tokenOrChannel))["response"].Value<string>() == "OK")
                    {
                        _token = tokenOrChannel;
                        var jo = new JObject {["token"] = tokenOrChannel};
                        File.WriteAllText("gcdc.json", jo.ToString());
                        Start();
                        Log.Output(
                            "Successfully logged in. You can now use a text block named Discord and the dc command.");
                    }
                    else
                        Log.Error("Failed to verify login. Please try again.");
                }
                catch (Exception e)
                {
                    Log.Error("Failed to verify login. Please try again. (Error logged.)");
                    Console.WriteLine(e);
                }
            }
        }

        public void SendMessage(string message)
        {
            if (!_running)
            {
                Log.Error("Run dcsetup first.");
                return;
            }

            try
            {
                var parameters = "token=" + _token + "&message=" + message;
                var resp = JObject.Parse(WebUtils.Request("messages/send?" + parameters, ""));
                if (resp["response"]
                    .Value<string>() == "OK")
                {
                    AddMessage("<nobr><" + resp["username"] + "> " + message);
                    Log.Output("Message sent");
                }
                else
                    Log.Error("Failed to send message");
            }
            catch (Exception e)
            {
                Log.Error("Failed to send message (error logged).");
                Console.WriteLine(e);
            }
        }

        public void Start()
        {
            if (_running) return;
            _running = true;
            _rect = new Thread(() =>
            {
                Console.WriteLine("Starting DC receiver thread...");
                while (_running)
                {
                    try
                    {
                        string resp = WebUtils.Request("messages/get?token=" + _token);
                        var jo = JObject.Parse(resp);
                        AddMessage("<nobr><" + jo["username"] + "> " + jo["message"]);
                    }
                    catch (WebException)
                    {
                        // ignored
                    }
                    catch (ThreadInterruptedException)
                    {
                        break;
                    }
                }
            }) {Name = "DC Receiver Thread"};
            _rect.Start();
        }

        public EntitiesDB entitiesDB { get; set; }
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

        public void Dispose()
        {
            _running = false;
            _rect.Interrupt();
        }

        public string Name { get; } = "GCDCEngine";
    }
}