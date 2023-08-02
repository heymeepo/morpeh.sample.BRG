using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Prototypes.SamplesBRG.PrimitivesSample.ECS.Graph
{
    public struct TorusFunc : IGraphFunction
    {
        public float3 Perform(float u, float v, float time)
        {
            float r1 = 0.7f + 0.1f * sin(PI * (6f * u + 0.5f * time));
            float r2 = 0.15f + 0.05f * sin(PI * (8f * u + 4f * v + 2f * time));
            float s = r1 + r2 * cos(PI * v);
            float3 p;
            p.x = s * sin(PI * u);
            p.y = r2 * sin(PI * v);
            p.z = s * cos(PI * u);
            return p;
        }
    }
}
