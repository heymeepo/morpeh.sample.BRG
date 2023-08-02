using Unity.Mathematics;

namespace Prototypes.SamplesBRG.PrimitivesSample.ECS.Graph
{
    public interface IGraphFunction
    {
        public float3 Perform(float u, float v, float time);
    }
}
