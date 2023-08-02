using Prototypes.BRG.Unity;
using Scellecs.Morpeh;
using System;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Rendering;

namespace Prototypes.BRG
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct SrpBatch : IDisposable
    {
        public BatchID batchId;
        public DrawKey batchDrawKey;
        public AABB localBounds;
        public BatchPropertyBlock batchPropertyBlock;
        public NativeArray<EntityId> entities;
        public int instancesCount;
        public int capacity;

        public void Dispose()
        {
            batchPropertyBlock.Dispose();
            entities.Dispose();

            entities = default;
        }
    }
}
