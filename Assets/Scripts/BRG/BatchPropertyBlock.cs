using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;

namespace Prototypes.BRG
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct BatchPropertyBlock : IDisposable
    {
        private NativeArray<byte> data;
        private NativeArray<MetadataValueBPB> metadata;
        private int capacity;

        public BatchPropertyBlock(NativeArray<byte> data, NativeArray<MetadataValueBPB> metadata, int capacity)
        {
            this.data = data;
            this.metadata = metadata;
            this.capacity = capacity;
        }

        public NativeArray<byte> GetRawData() => data;

        public NativeSlice<T> AccessProperty<T>(int propertyId) where T : unmanaged
        {
            for (int i = 0; i < metadata.Length; i++)
            {
                var current = metadata[i];
                int id = current.id;

                if (id == propertyId)
                {
                    int pSize = current.size;
                    int tSize = UnsafeUtility.SizeOf<T>();

                    if (pSize != tSize)
                    {
                        throw new InvalidOperationException($"The type {typeof(T)} is not valid for propertyId {propertyId}. Expected size: {pSize}, Actual size: {tSize}");
                    }

                    int offset = current.propertyBlockOffset;
                    var slice = data.Slice(offset, capacity * pSize);
                    return slice.SliceConvert<T>();
                }
            }

            throw new ArgumentException($"PropertyId not found: {propertyId}.");
        }

        public unsafe T* AccessPropertyUnsafe<T>(int propertyId) where T : unmanaged
        {
            for (int i = 0; i < metadata.Length; i++)
            {
                var current = metadata[i];
                int id = current.id;

                if (id == propertyId)
                {
                    int pSize = current.size;
                    int tSize = sizeof(T);

                    if (pSize != tSize)
                    {
                        throw new InvalidOperationException($"The type {typeof(T)} is not valid for propertyId {propertyId}. Expected size: {pSize}, Actual size: {tSize}");
                    }

                    int offset = current.propertyBlockOffset;
                    return (T*)((byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(data) + offset);
                }
            }

            throw new ArgumentException($"PropertyId not found: {propertyId}.");
        }

        public void Dispose()
        {
            data.Dispose();
            metadata.Dispose();

            data = default;
            metadata = default;
        }
    }
}
