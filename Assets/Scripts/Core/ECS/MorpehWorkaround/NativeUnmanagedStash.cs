#if MORPEH_BURST
using Scellecs.Morpeh.Native;

namespace Prototypes.Core.ECS.MorpehWorkaround
{
    public struct NativeUnmanagedStash<T> where T : unmanaged
    {
        internal NativeIntHashMap<T> componentsAsUnmanagedType;
        internal NativeWorld world;
    }
}
#endif
