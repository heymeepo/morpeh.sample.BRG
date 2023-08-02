using Prototypes.BRG.Unity;
using Prototypes.Core.ECS;
using Prototypes.Core.ECS.Destroy;
using Prototypes.Core.ECS.MorpehWorkaround;
using Prototypes.Core.ECS.Transform;
using Prototypes.Core.Utils;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Scellecs.Morpeh.Native;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Prototypes.BRG.BRGHelpers;
using static Prototypes.Core.Utils.UnsafeHelpers;
using FrustumPlanes = Prototypes.BRG.Unity.FrustumPlanes;

namespace Prototypes.BRG.ECS
{
    public sealed class BatchRendererSystem : UpdateBaseSystem
    {
        private BatchRendererGroup brg;

        private Dictionary<DrawKey, FastList<MaterialPropertyData>> propertiesPerDrawKey;
        private Dictionary<DrawKey, AABB> boundsPerDrawKey;
        private Dictionary<DrawKey, BatchesIndices> batchesIndicesPerDrawKey;
        private IntHashMap<GraphicsBuffer> instanceDataMap;
        private IntHashMap<SrpBatch> batchMap;

        private IntFastList batchesIndices;

        private Filter registerInstanceFilter;
        private Filter unregisterInstanceFilter;

        private Stash<TransformComponent> transformStash;
        private Stash<RenderComponent> renderStash;
        private Stash<BRGInstanceComponent> brgStash;

        private readonly int batchCapacity = 1024;

        public override void OnAwake()
        {
            propertiesPerDrawKey = new Dictionary<DrawKey, FastList<MaterialPropertyData>>();
            boundsPerDrawKey = new Dictionary<DrawKey, AABB>();
            batchesIndicesPerDrawKey = new Dictionary<DrawKey, BatchesIndices>();
            instanceDataMap = new IntHashMap<GraphicsBuffer>(64);
            batchMap = new IntHashMap<SrpBatch>(64);
            batchesIndices = new IntFastList(64);

            InitializeBatchRenderer();

            registerInstanceFilter = World.Filter
                .With<RenderComponent>()
                .With<TransformComponent>()
                .Without<BRGInstanceComponent>();

            unregisterInstanceFilter = World.Filter
                .With<RenderComponent>()
                .With<TransformComponent>()
                .With<BRGInstanceComponent>()
                .With<DestroyMarker>();

            transformStash = World.GetStash<TransformComponent>();
            renderStash = World.GetStash<RenderComponent>();
            brgStash = World.GetStash<BRGInstanceComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            UnregisterInstances();
            RegisterInstances();
            UpdateBatchesIndices();

            JobHandle dependency = UpdateMatrices();
            dependency = UpdateDefinedProperties(dependency);
            WriteInstanceData(dependency);
        }

        public override void Dispose()
        {
            foreach (var idx in batchMap)
            {
                ref var batch = ref batchMap.GetValueRefByIndex(idx);
                brg.UnregisterMaterial(batch.batchDrawKey.materialID);
                brg.UnregisterMesh(batch.batchDrawKey.meshID);
                batch.Dispose();
            }

            foreach (var idx in instanceDataMap)
            {
                var buffer = instanceDataMap.GetValueByIndex(idx);
                buffer.Dispose();
            }

            brg.Dispose();
        }

        private void InitializeBatchRenderer()
        {
            brg = new BatchRendererGroup(OnPerformCulling, IntPtr.Zero);
            brg.SetGlobalBounds(new Bounds(new Vector3(), new Vector3(1048576f, 1048576f, 1048576f)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RegisterInstances()
        {
            foreach (var entity in registerInstanceFilter)
            {
                ref var renderData = ref renderStash.Get(entity).sharedData;
                var drawKey = GetOrCreateDrawKey(renderData);
                var batchIndex = GetOrCreateBatch(drawKey);
                ref var batch = ref batchMap.GetValueRefByIndex(batchIndex);
                var instanceId = batch.instancesCount;
                batch.entities[instanceId] = entity.ID;
                batch.instancesCount++;

                if (batch.instancesCount == batch.capacity)
                {
                    batchesIndicesPerDrawKey[drawKey].availableIndices.RemoveSwapBackChecked(batchIndex);
                }

                var properties = renderData.materialProperties;

                for (int i = 0; i < properties.Count; i++)
                {
                    entity.SetComponentBoxed(properties[i]);
                }

                brgStash.Set(entity, new BRGInstanceComponent()
                {
                    batchIndex = batchIndex,
                    instanceId = instanceId
                });
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UnregisterInstances()
        {
            uint cachedBatchId = uint.MaxValue;

            foreach (var entity in unregisterInstanceFilter)
            {
                ref var registredInstanceData = ref brgStash.Get(entity);
                int batchIndex = registredInstanceData.batchIndex;
                ref var batch = ref batchMap.GetValueRefByIndex(batchIndex);

                var last = batch.entities[--batch.instancesCount];

                if (World.TryGetEntity(last, out var lastEntity))
                {
                    ref var lastInstanceData = ref brgStash.Get(lastEntity);
                    lastInstanceData.instanceId = registredInstanceData.instanceId;
                }

                batch.entities[registredInstanceData.instanceId] = last;
                batch.entities[batch.instancesCount] = default;

                if (batch.instancesCount == 0)
                {
                    RemoveBatch(batchIndex);
                }
                else if (cachedBatchId != batch.batchId.value)
                {
                    cachedBatchId = batch.batchId.value;
                    var avaliableIndices = batchesIndicesPerDrawKey[batch.batchDrawKey].availableIndices;

                    if (avaliableIndices.IndexOf(batchIndex) < 0)
                    {
                        avaliableIndices.Add(batchIndex);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateBatchesIndices()
        {
            batchesIndices.Clear();

            foreach (var indices in batchesIndicesPerDrawKey.Values)
            {
                batchesIndices.AddListRange(indices.indices);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DrawKey GetOrCreateDrawKey(RenderSharedData renderData)
        {
            if (renderData.TryGetDrawKey(out var drawKey))
            {
                return drawKey;
            }

            return InitializeDrawKey(renderData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DrawKey InitializeDrawKey(RenderSharedData renderData)
        {
            var drawKey = new DrawKey()
            {
                meshID = brg.RegisterMesh(renderData.sharedMesh),
                materialID = brg.RegisterMaterial(renderData.sharedMaterial),
                submeshIndex = 0
            };

            if (batchesIndicesPerDrawKey.ContainsKey(drawKey) == false)
            {
                batchesIndicesPerDrawKey.Add(drawKey, new BatchesIndices()
                {
                    indices = new IntFastList(),
                    availableIndices = new IntFastList()
                });
            }

            if (propertiesPerDrawKey.TryGetValue(drawKey, out var properties) == false)
            {
                properties = new FastList<MaterialPropertyData>();

                var definedProperties = renderData.materialProperties;

                for (int i = 0; i < definedProperties.Count; i++)
                {
                    var propertyComponent = definedProperties[i];
                    var componentType = propertyComponent.GetType();
                    var attribute = componentType.GetAttribute<BatchMaterialPropertyAttribute>();

                    properties.Add(new MaterialPropertyData()
                    {
                        propertyId = Shader.PropertyToID(attribute.MaterialPropertyId),
                        format = attribute.Format,
                        associatedComponentType = componentType
                    });
                }

                propertiesPerDrawKey[drawKey] = properties;
            }
            else
            {
                CheckPropertiesForRenderData(properties, renderData);
            }

            if (boundsPerDrawKey.ContainsKey(drawKey) == false)
            {
                boundsPerDrawKey.Add(drawKey, renderData.sharedMesh.bounds.ToAABB());
            }

            renderData.Initialize(drawKey);
            return drawKey;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckPropertiesForRenderData(FastList<MaterialPropertyData> properties, RenderSharedData renderData)
        {
            var definedProperties = renderData.materialProperties;

            if (definedProperties.Count != properties.length)
            {
                throw new NotSupportedException($"Different set of overriden properties detected for the multiple identical DrawKeys at {renderData.name}");
            }

            bool result = false;

            for (int i = 0; i < properties.length; i++)
            {
                for (int j = 0; j < definedProperties.Count; j++)
                {
                    result |= properties.data[i].associatedComponentType == definedProperties[j].GetType();
                }

                if (result == false)
                {
                    throw new NotSupportedException($"Different set of overriden properties detected for the multiple identical DrawKeys at {renderData.name}");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetOrCreateBatch(DrawKey drawKey)
        {
            if (TryGetAvailableBatch(drawKey, out int batchIndex) == false)
            {
                batchIndex = CreateBatch(drawKey);
            }

            return batchIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetAvailableBatch(DrawKey drawKey, out int batchIndex)
        {
            var avaliableIndices = batchesIndicesPerDrawKey[drawKey].availableIndices;

            if (avaliableIndices.length == 0)
            {
                batchIndex = -1;
                return false;
            }
            else
            {
                batchIndex = avaliableIndices.data[avaliableIndices.length - 1];
                return true;
            }
        }

        private int CreateBatch(DrawKey drawKey)
        {
            var overridenProperties = propertiesPerDrawKey[drawKey];

            int defaultPropertiesCount = 2;
            int propertiesCount = defaultPropertiesCount + overridenProperties.length;
            int bytesPerInstance = 0;

            NativeArray<MetadataValueBPB> propertyBlockMetadata = new NativeArray<MetadataValueBPB>(propertiesCount, Allocator.Persistent);
            NativeArray<MetadataValue> metadata = new NativeArray<MetadataValue>(propertiesCount, Allocator.Temp);

            void CreateMetadata(int index, int propertyId, int size)
            {
                int offset = index > 0 ? propertyBlockMetadata[index - 1].propertyBlockOffset + propertyBlockMetadata[index - 1].size * batchCapacity : 0;

                propertyBlockMetadata[index] = new MetadataValueBPB(propertyId, size, offset);

                metadata[index] = new MetadataValue()
                {
                    NameID = propertyId,
                    Value = MSB | (uint)offset
                };

                bytesPerInstance += size;
            }

            CreateMetadata(0, OBJECT_TO_WORLD_ID, SIZE_OF_PACKED_MATRIX);
            CreateMetadata(1, WORLD_TO_OBJECT_ID, SIZE_OF_PACKED_MATRIX);

            for (int i = defaultPropertiesCount; i < propertiesCount; i++)
            {
                var propertyData = overridenProperties.data[i - defaultPropertiesCount];
                CreateMetadata(i, propertyData.propertyId, propertyData.format.GetSizeFormat());
            }

            int bufferSizeForInstances = BufferSizeForInstances(bytesPerInstance, batchCapacity, SIZE_OF_UINT);
            int bufferCountForInstances = bufferSizeForInstances / SIZE_OF_UINT;
            GraphicsBuffer instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, GraphicsBuffer.UsageFlags.LockBufferForWrite, bufferCountForInstances, SIZE_OF_UINT);
            instanceBuffer.SetData(ZERO_MATRIX_HEADER, 0, 0, 1);

            var batchId = brg.AddBatch(metadata, instanceBuffer.bufferHandle);

            NativeArray<byte> propertyBlockData = new NativeArray<byte>(bufferSizeForInstances, Allocator.Persistent, NativeArrayOptions.ClearMemory);

            var batch = new SrpBatch()
            {
                batchId = batchId,
                batchDrawKey = drawKey,
                localBounds = boundsPerDrawKey[drawKey],
                batchPropertyBlock = new BatchPropertyBlock(propertyBlockData, propertyBlockMetadata, batchCapacity),
                entities = new NativeArray<EntityId>(batchCapacity, Allocator.Persistent),
                instancesCount = 0,
                capacity = batchCapacity
            };

            int batchKey = batchId.AsInt();

            batchMap.Add(batchKey, batch, out int index);
            instanceDataMap.Add(batchKey, instanceBuffer, out _);

            var batchIndices = batchesIndicesPerDrawKey[drawKey];
            batchIndices.indices.Add(index);
            batchIndices.availableIndices.Add(index);

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveBatch(int batchIndex)
        {
            ref var batch = ref batchMap.GetValueRefByIndex(batchIndex);
            int batchKey = batch.batchId.AsInt();

            if (batchesIndicesPerDrawKey.TryGetValue(batch.batchDrawKey, out var batchIndices))
            {
                batchIndices.indices.RemoveSwapBackChecked(batchIndex);
                batchIndices.availableIndices.RemoveSwapBackChecked(batchIndex);
            }

            instanceDataMap.Remove(batchKey, out var instanceData);
            instanceData.Dispose();

            batchMap.Remove(batchKey, out _);
            brg.RemoveBatch(batch.batchId);
            batch.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe JobHandle UpdateMatrices()
        {
            JobHandle* handles = stackalloc JobHandle[batchesIndices.length];

            for (int i = 0; i < batchesIndices.length; i++)
            {
                ref var batch = ref batchMap.GetValueRefByIndex(batchesIndices.data[i]);

                handles[i] = new UpdateBatchMatricesJob()
                {
                    entities = batch.entities,
                    stash = transformStash.AsNative(),
                    objectToWorld = batch.batchPropertyBlock.AccessPropertyUnsafe<float3x4>(OBJECT_TO_WORLD_ID),
                    worldToObject = batch.batchPropertyBlock.AccessPropertyUnsafe<float3x4>(WORLD_TO_OBJECT_ID),
                }
                .ScheduleParallel(batch.instancesCount, 64, default);
            }

            JobHandle.ScheduleBatchedJobs();
            return JobHandleUnsafeUtility.CombineDependencies(handles, batchesIndices.length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe JobHandle UpdateDefinedProperties(JobHandle dependency)
        {
            using var handles = new NativeList<JobHandle>(Allocator.Temp);

            for (int i = 0; i < batchesIndices.length; i++)
            {
                ref var batch = ref batchMap.GetValueRefByIndex(batchesIndices.data[i]);
                var properties = propertiesPerDrawKey[batch.batchDrawKey];

                for (int j = 0; j < properties.length; j++)
                {
                    var property = properties.data[j];

                    if (property.format == BatchMaterialPropertyFormat.Float)
                    {
                        handles.Add(UpdateBatchPropertyJob<float>.ScheduleParallel(ref batch, property, World, dependency));
                    }
                    else if (property.format == BatchMaterialPropertyFormat.Float2)
                    {
                        handles.Add(UpdateBatchPropertyJob<float2>.ScheduleParallel(ref batch, property, World, dependency));
                    }
                    else if (property.format == BatchMaterialPropertyFormat.Float3)
                    {
                        handles.Add(UpdateBatchPropertyJob<float3>.ScheduleParallel(ref batch, property, World, dependency));
                    }
                    else if (property.format == BatchMaterialPropertyFormat.Float4)
                    {
                        handles.Add(UpdateBatchPropertyJob<float4>.ScheduleParallel(ref batch, property, World, dependency));
                    }
                    else if (property.format == BatchMaterialPropertyFormat.Float2x4)
                    {
                        handles.Add(UpdateBatchPropertyJob<float2x4>.ScheduleParallel(ref batch, property, World, dependency));
                    }
                    else if (property.format == BatchMaterialPropertyFormat.Float3x4)
                    {
                        handles.Add(UpdateBatchPropertyJob<float3x4>.ScheduleParallel(ref batch, property, World, dependency));
                    }
                    else
                    {
                        handles.Add(UpdateBatchPropertyJob<float4x4>.ScheduleParallel(ref batch, property, World, dependency));
                    }
                }
            }

            if (handles.Length > 0)
            {
                dependency = JobHandle.CombineDependencies(handles.AsArray());
            }

            return dependency;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void WriteInstanceData(JobHandle dependency)
        {
            JobHandle* handles = stackalloc JobHandle[batchesIndices.length];

            for (int i = 0; i < batchesIndices.length; i++)
            {
                var key = batchesIndices.data[i];

                ref var batch = ref batchMap.GetValueRefByIndex(key);
                var instanceData = instanceDataMap.GetValueByIndex(key);

                var propertyBlockData = batch.batchPropertyBlock.GetRawData();
                var bufferRef = instanceData.LockBufferForWrite<uint>(0, instanceData.count);

                handles[i] = new WriteToInstanceBufferJob()
                {
                    instanceData = (uint*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(propertyBlockData),
                    output = (uint*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(bufferRef)
                }
                .ScheduleParallel(bufferRef.Length, 128, dependency);
            }

            JobHandleUnsafeUtility.CombineDependencies(handles, batchesIndices.length).Complete();

            for (int i = 0; i < batchesIndices.length; i++)
            {
                var instanceData = instanceDataMap.GetValueByIndex(batchesIndices.data[i]);
                instanceData.UnlockBufferAfterWrite<uint>(instanceData.count);
            }
        }

        private unsafe JobHandle OnPerformCulling(
        BatchRendererGroup rendererGroup,
        BatchCullingContext cullingContext,
        BatchCullingOutput cullingOutput,
        IntPtr userContext)
        {
            BatchCullingOutputDrawCommands drawCommands = new BatchCullingOutputDrawCommands();

            int batchesCount = batchesIndices.length;
            int instancesCount = batchesCount * batchCapacity;

            drawCommands.visibleInstances = Malloc<int>(instancesCount);

            using var drawCommandsFrameBuffer = new NativeQueue<BatchDrawCommand>(Allocator.TempJob);

            fixed (int* indicesPtr = &batchesIndices.data[0])
            fixed (SrpBatch* batchesPtr = &batchMap.data[0])
            {
                new CullingJob()
                {
                    cullingPlanes = cullingContext.cullingPlanes,
                    visibleInstances = drawCommands.visibleInstances,
                    batches = batchesPtr,
                    batchesIndices = indicesPtr,
                    drawCommands = drawCommandsFrameBuffer.AsParallelWriter(),
                    objectToWorldId = OBJECT_TO_WORLD_ID,
                }
                .ScheduleParallel(batchesCount, 16, default)
                .Complete();
            }

            drawCommands.drawRangeCount = 1;
            drawCommands.drawRanges = Malloc<BatchDrawRange>(1);
            drawCommands.drawRanges[0] = new BatchDrawRange()
            {
                drawCommandsBegin = 0,
                drawCommandsCount = (uint)drawCommandsFrameBuffer.Count,
                filterSettings = new BatchFilterSettings()
                {
                    renderingLayerMask = 1,
                    layer = 0,
                    motionMode = MotionVectorGenerationMode.Camera,
                    shadowCastingMode = ShadowCastingMode.Off,
                    receiveShadows = false,
                    staticShadowCaster = false,
                    allDepthSorted = false
                }
            };

            drawCommands.instanceSortingPositions = null;
            drawCommands.instanceSortingPositionFloatCount = 0;

            drawCommands.drawCommandCount = drawCommandsFrameBuffer.Count;
            drawCommands.drawCommands = Malloc<BatchDrawCommand>(drawCommands.drawCommandCount);

            int index = 0;

            while (drawCommandsFrameBuffer.TryDequeue(out var drawCommand))
            {
                drawCommands.drawCommands[index++] = drawCommand;
            }

            cullingOutput.drawCommands[0] = drawCommands;

            return default;
        }
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public unsafe struct UpdateBatchMatricesJob : IJobFor
    {
        [ReadOnly] public NativeArray<EntityId> entities;
        [ReadOnly] public NativeStash<TransformComponent> stash;

        [WriteOnly, NativeDisableUnsafePtrRestriction] public float3x4* objectToWorld;
        [WriteOnly, NativeDisableUnsafePtrRestriction] public float3x4* worldToObject;

        public void Execute(int index)
        {
            ref var trs = ref stash.Get(entities[index]);

            float4x4 matrix = trs;
            float4x4 inverse = math.inverse(matrix);

            float3x4 objectToWorldPacked = new float3x4(matrix.c0.xyz, matrix.c1.xyz, matrix.c2.xyz, matrix.c3.xyz);
            float3x4 worldToObjectPacked = new float3x4(inverse.c0.xyz, inverse.c1.xyz, inverse.c2.xyz, inverse.c3.xyz);

            objectToWorld[index] = objectToWorldPacked;
            worldToObject[index] = worldToObjectPacked;
        }
    }

    [BurstCompile]
    public unsafe struct UpdateBatchPropertyJob<T> : IJobFor where T : unmanaged
    {
        [ReadOnly] public NativeArray<EntityId> entities;
        [ReadOnly] public NativeUnmanagedStash<T> stash;

        [WriteOnly, NativeDisableUnsafePtrRestriction] public T* propertyAccess;

        public void Execute(int index)
        {
            ref var property = ref stash.Get(entities[index]);
            propertyAccess[index] = property;
        }

        public static JobHandle Schedule(ref SrpBatch batch, MaterialPropertyData propertyData, World world, JobHandle dependency = default)
        {
            return new UpdateBatchPropertyJob<T>()
            {
                entities = batch.entities,
                stash = world.CreateUnmanagedStashDangerous<T>(propertyData.associatedComponentType),
                propertyAccess = batch.batchPropertyBlock.AccessPropertyUnsafe<T>(propertyData.propertyId)
            }
            .Schedule(batch.instancesCount, dependency);
        }

        public static JobHandle ScheduleParallel(ref SrpBatch batch, MaterialPropertyData propertyData, World world, JobHandle dependency = default, int innerloopBatchCount = 64)
        {
            return new UpdateBatchPropertyJob<T>()
            {
                entities = batch.entities,
                stash = world.CreateUnmanagedStashDangerous<T>(propertyData.associatedComponentType),
                propertyAccess = batch.batchPropertyBlock.AccessPropertyUnsafe<T>(propertyData.propertyId)
            }
            .ScheduleParallel(batch.instancesCount, innerloopBatchCount, dependency);
        }
    }

    [BurstCompile]
    public unsafe struct WriteToInstanceBufferJob : IJobFor
    {
        [ReadOnly, NativeDisableUnsafePtrRestriction] public uint* instanceData;
        [WriteOnly, NativeDisableUnsafePtrRestriction] public uint* output;

        public void Execute(int index)
        {
            output[index] = instanceData[index];
        }
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public unsafe struct CullingJob : IJobFor
    {
        [WriteOnly, NativeDisableUnsafePtrRestriction] public int* visibleInstances;
        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeQueue<BatchDrawCommand>.ParallelWriter drawCommands;

        [ReadOnly, NativeDisableUnsafePtrRestriction] public SrpBatch* batches;
        [ReadOnly, NativeDisableUnsafePtrRestriction] public int* batchesIndices;
        [ReadOnly] public NativeArray<Plane> cullingPlanes;
        [ReadOnly] public int objectToWorldId;

        public void Execute(int index)
        {
            ref var batch = ref batches[batchesIndices[index]];
            int batchOffset = index * batch.capacity;
            int visibleCount = 0;

            var objectToWorld = batch.batchPropertyBlock.AccessPropertyUnsafe<float3x4>(objectToWorldId);

            for (int i = 0; i < batch.instancesCount; i++)
            {
                var matrix = objectToWorld[i];
                var localBounds = batch.localBounds;

                AABB worldBounds;

                worldBounds.Extents =
                    math.abs(matrix.c0 * localBounds.Extents.x) +
                    math.abs(matrix.c1 * localBounds.Extents.y) +
                    math.abs(matrix.c2 * localBounds.Extents.z);

                float3 b = localBounds.Center;

                worldBounds.Center = matrix.c0 * b.x + matrix.c1 * b.y + matrix.c2 * b.z + matrix.c3;

                float3 m = worldBounds.Center;
                float3 extent = worldBounds.Extents;

                var inCount = 0;

                FrustumPlanes.IntersectResult result = FrustumPlanes.IntersectResult.Partial;

                for (int j = 0; j < cullingPlanes.Length; j++)
                {
                    float3 normal = cullingPlanes[j].normal;
                    float dist = math.dot(normal, m) + cullingPlanes[j].distance;
                    float radius = math.dot(extent, math.abs(normal));

                    if (dist + radius <= 0)
                    {
                        result = FrustumPlanes.IntersectResult.Out;
                        break;
                    }

                    if (dist > radius)
                    {
                        inCount++;
                    }
                }

                if (inCount == cullingPlanes.Length)
                {
                    result = FrustumPlanes.IntersectResult.In;
                }

                if (result != FrustumPlanes.IntersectResult.Out)
                {
                    visibleInstances[batchOffset + visibleCount] = i;
                    visibleCount++;
                }
            }

            if (visibleCount > 0)
            {
                drawCommands.Enqueue(new BatchDrawCommand()
                {
                    visibleOffset = (uint)batchOffset,
                    visibleCount = (uint)visibleCount,
                    batchID = batch.batchId,
                    meshID = batch.batchDrawKey.meshID,
                    materialID = batch.batchDrawKey.materialID,
                    submeshIndex = (ushort)batch.batchDrawKey.submeshIndex,
                    splitVisibilityMask = 0xff,
                    flags = BatchDrawCommandFlags.None,
                    sortingPosition = 0
                });
            }
        }
    }
}
