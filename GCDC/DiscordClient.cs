using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using GamecraftModdingAPI.Engines;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RobocraftX.Common.Input;
using RobocraftX.Common.Utilities;
using RobocraftX.StateSync;
using Svelto.ECS;
using Unity.Jobs;
using uREPL;

namespace GCDC
{
    public class DiscordClient
    {
        private string _token;
        private bool _running;
        private Thread _rect;
        private readonly Queue<string> messages = new Queue<string>();
        private readonly GCDCPlugin plugin;
        public DiscordClient(GCDCPlugin plugin) => this.plugin = plugin;
        public void Ready()
        {
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
                    if (JObject.Parse(WebUtils.Request("users/get?token=" + tokenOrChannel))["response"]?.Value<string>() == "OK")
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
                if (resp["response"]?.Value<string>() == "OK")
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

        public void AddMessage(string message)
        {
            messages.Enqueue(message);
            if (messages.Count > 10)
                messages.Dequeue();
            plugin.Update(messages);
        }

        public void Stop()
        {
            _running = false;
            _rect.Interrupt();
        }

        public void Update() => plugin.Update(messages);
    }
}