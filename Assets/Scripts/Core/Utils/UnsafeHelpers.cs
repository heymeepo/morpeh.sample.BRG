using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace Prototypes.Core.Utils
{
    public static unsafe class UnsafeHelpers
    {
        public static unsafe T* Malloc<T>(int count, Allocator allocator = Allocator.TempJob) where T : unmanaged
        {
            return (T*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * count, UnsafeUtility.AlignOf<T>(), allocator);
        }
    }
}
