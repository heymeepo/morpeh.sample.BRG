using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Prototypes.BRG
{
    public static class BRGHelpers
    {
        public const int SIZE_OF_UINT = sizeof(uint);
        public const int SIZE_OF_FLOAT = sizeof(float);
        public const int SIZE_OF_FLOAT4 = SIZE_OF_FLOAT * 4;
        public const int SIZE_OF_MATRIX = SIZE_OF_FLOAT4 * 4;
        public const int SIZE_OF_PACKED_MATRIX = SIZE_OF_FLOAT4 * 3;
        public const int OFFSET = 32;
        public const int EXTRA_BYTES = SIZE_OF_MATRIX + OFFSET;
        public const uint MSB = 0x80000000;

        public static readonly float4x4[] ZERO_MATRIX_HEADER = new float4x4[1];

        public static readonly int OBJECT_TO_WORLD_ID = Shader.PropertyToID("unity_ObjectToWorld");
        public static readonly int WORLD_TO_OBJECT_ID = Shader.PropertyToID("unity_WorldToObject");

        public static int BufferSizeForInstances(int bytesPerInstance, int numInstances, int alignment, int extraBytes = 0)
        {
            bytesPerInstance = (bytesPerInstance + alignment - 1) / alignment * alignment;
            extraBytes = (extraBytes + alignment - 1) / alignment * alignment;
            return bytesPerInstance * numInstances + extraBytes;
        }

        public static int AsInt(this BatchID batchId)
        {
            return unchecked((int)batchId.value);
        }
    }
}
