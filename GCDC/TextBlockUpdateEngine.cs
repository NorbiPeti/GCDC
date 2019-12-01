using System.Collections.Generic;
using System.Linq;
using RobocraftX.Blocks.GUI;
using RobocraftX.Common;
using RobocraftX.Common.Input;
using RobocraftX.Common.Utilities;
using RobocraftX.SimulationModeState;
using RobocraftX.StateSync;
using Svelto.ECS;
using Svelto.ECS.Experimental;
using Unity.Jobs;
using uREPL;

namespace GCDC
{
    public class TextBlockUpdateEngine : IQueryingEntitiesEngine, IEngine, IDeterministicSim
    {
        public void Ready()
        {
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
    }
}