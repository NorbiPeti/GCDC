using RobocraftX.Common.Input;
using RobocraftX.Common.Utilities;
using RobocraftX.StateSync;
using Svelto.ECS;
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
        public JobHandle SimulatePhysicsStep(in float deltaTime, in PhysicsUtility utility, in PlayerInput[] playerInputs)
        {
            //TODO
            return new JobHandle();
        }

        public string name { get; } = "GCDC-TextUpdate";
    }
}