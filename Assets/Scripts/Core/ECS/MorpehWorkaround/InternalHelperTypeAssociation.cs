using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
#if MORPEH_BURST
using Scellecs.Morpeh.Native;
#endif
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Prototypes.Core.ECS.MorpehWorkaround
{
    internal static class InternalHelperTypeAssociation
    {
        private static Dictionary<Type, InternalAPIHelper> typeAssociations = new Dictionary<Type, InternalAPIHelper>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static InternalAPIHelper Get(Type type)
        {
            if (typeAssociations.ContainsKey(type) == false)
            {
                if ((typeof(IComponent).IsAssignableFrom(type) && type.IsValueType) == false)
                {
                    throw new ArgumentException($"The specified type {type} is not valid. Please ensure that the type you are trying to assign is a value type and implements IComponent interface.");
                }

                var typeId = typeof(InternalAPIHelper<>).MakeGenericType(type);
                var method = typeId.GetMethod("Warmup", BindingFlags.Static | BindingFlags.NonPublic);
                method.Invoke(null, null);
            }

            return typeAssociations[type];
        }

        internal static void Set<T>(InternalAPIHelper<T> helper) where T : unmanaged, IComponent
        {
            typeAssociations.Add(typeof(T), helper);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Cleanup() => typeAssociations?.Clear();
    }

    internal abstract class InternalAPIHelper
    {
        internal abstract void SetComponentBoxed(Entity entity, object component);
        internal abstract void RemoveComponentBoxed(Entity entity);
#if MORPEH_BURST
        internal abstract NativeUnmanagedStash<TUnmanaged> CreateUnmanagedStash<TUnmanaged>(World world) where TUnmanaged : unmanaged;
#endif
    }

    internal sealed class InternalAPIHelper<T> : InternalAPIHelper where T : unmanaged, IComponent
    {
        private InternalAPIHelper() { }

        [Preserve]
        private static void Warmup() => InternalHelperTypeAssociation.Set(new InternalAPIHelper<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void SetComponentBoxed(Entity entity, object component) => entity.world.GetStash<T>().Set(entity, (T)component);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void RemoveComponentBoxed(Entity entity) => entity.world.GetStash<T>().Remove(entity);
#if MORPEH_BURST
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe override NativeUnmanagedStash<TUnmanaged> CreateUnmanagedStash<TUnmanaged>(World world)
        {
            var stash = world.GetStash<T>();
            var hashMap = stash.components;
            var nativeIntHashMap = new NativeIntHashMap<TUnmanaged>();

            fixed (int* lengthPtr = &hashMap.length)
            fixed (int* capacityPtr = &hashMap.capacity)
            fixed (int* capacityMinusOnePtr = &hashMap.capacityMinusOne)
            fixed (int* lastIndexPtr = &hashMap.lastIndex)
            fixed (int* freeIndexPtr = &hashMap.freeIndex)
            fixed (void* dataPtr = &hashMap.data[0])
            fixed (int* bucketsPtr = &hashMap.buckets[0])
            fixed (IntHashMapSlot* slotsPtr = &hashMap.slots[0])
            {
                nativeIntHashMap.lengthPtr = lengthPtr;
                nativeIntHashMap.capacityPtr = capacityPtr;
                nativeIntHashMap.capacityMinusOnePtr = capacityMinusOnePtr;
                nativeIntHashMap.lastIndexPtr = lastIndexPtr;
                nativeIntHashMap.freeIndexPtr = freeIndexPtr;
                nativeIntHashMap.data = (TUnmanaged*)dataPtr;
                nativeIntHashMap.buckets = bucketsPtr;
                nativeIntHashMap.slots = slotsPtr;
            }

            return new NativeUnmanagedStash<TUnmanaged>()
            {
                componentsAsUnmanagedType = nativeIntHashMap,
                world = world.AsNative()
            };
        }
#endif
    }
}
