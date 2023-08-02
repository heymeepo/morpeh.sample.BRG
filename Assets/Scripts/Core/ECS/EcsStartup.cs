using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
#if VCONTAINER
using VContainer;
using VContainer.Unity;
#endif

namespace Prototypes.Core.ECS //TODO: Multiple Worlds, AddEcsFeature integration?
{
    public sealed class EcsStartup : IDisposable
    {
        private readonly Dictionary<int, SystemsGroup> systemsGroups;
#if VCONTAINER
        private readonly LifetimeScope currentScope;
        private LifetimeScope systemsScope;
        private Action<IContainerBuilder> registerSystems;
#endif
        private Action setupSystemsGroups;
        private World world;

        private bool initialized;
        private bool disposed;

#if VCONTAINER
        public EcsStartup(LifetimeScope currentScope)
        {
            this.currentScope = currentScope;
            systemsGroups = new Dictionary<int, SystemsGroup>();
            world = World.Default;
            initialized = false;
            disposed = false;
        }
#else
		public EcsStartup() 
		{ 
			systemsGroups = new Dictionary<int, SystemsGroup>();
			world = World.Default;
			initialized = false;
			disposed = false;
		}
#endif
        public SystemsGroupBuilder SystemsGroupOrder(int order) => new SystemsGroupBuilder(this, order);

        public void Initialize(bool updateByUnity)
        {
            if (initialized)
            {
                if (disposed)
                {
                    Debug.LogError("The EcsStartup has already been disposed. Create a new one to use it.");
                }
                else
                {
                    Debug.LogWarning($"EcsStartup with {world.GetFriendlyName()} already initialized.");
                }

                return;
            }

            world ??= World.Create();
            world.UpdateByUnity = updateByUnity;
#if VCONTAINER
            systemsScope = currentScope.CreateChild(builder => registerSystems?.Invoke(builder));
#endif
            setupSystemsGroups?.Invoke();

            foreach (var group in systemsGroups)
            {
                world.AddSystemsGroup(group.Key, group.Value);
            }

            initialized = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(float deltaTime) => world.Update(deltaTime);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FixedUpdate(float fixedDeltaTime) => world.FixedUpdate(fixedDeltaTime);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LateUpdate(float deltaTime)
        {
            world.LateUpdate(deltaTime);
            world.CleanupUpdate(deltaTime);
        }

        public void Dispose()
        {
            systemsGroups.Clear();
            world.Dispose();
            setupSystemsGroups = null;
#if VCONTAINER
            systemsScope.Dispose();
            registerSystems = null;
#endif
            world = null;
            disposed = true;
        }
#if VCONTAINER
        private void AddSystemInjected<T>(int order) where T : class, ISystem
        {
            registerSystems += (builder) => builder.Register<T>(Lifetime.Transient);

            setupSystemsGroups += () =>
            {
                var system = systemsScope.Container.Resolve<T>();
                var systemsGroup = GetOrCreateSystemsGroup(order);
                systemsGroup.AddSystem(system);
            };
        }

        private void AddInitializerInjected<T>(int order) where T : class, IInitializer
        {
            registerSystems += (builder) => builder.Register<T>(Lifetime.Transient);

            setupSystemsGroups += () =>
            {
                var intitializer = systemsScope.Container.Resolve<T>();
                var systemsGroup = GetOrCreateSystemsGroup(order);
                systemsGroup.AddInitializer(intitializer);
            };
        }
#endif
        private void AddSystem<T>(int order, T system) where T : class, ISystem
        {
            setupSystemsGroups += () =>
            {
                var systemsGroup = GetOrCreateSystemsGroup(order);
                systemsGroup.AddSystem(system);
            };
        }

        private void AddInitializer<T>(int order, T initializer) where T : class, IInitializer
        {
            setupSystemsGroups += () =>
            {
                var systemsGroup = GetOrCreateSystemsGroup(order);
                systemsGroup.AddInitializer(initializer);
            };
        }

        private SystemsGroup GetOrCreateSystemsGroup(int order)
        {
            if (systemsGroups.TryGetValue(order, out SystemsGroup systemsGroup) == false)
            {
                systemsGroup = systemsGroups[order] = world.CreateSystemsGroup();
            }

            return systemsGroup;
        }

        public readonly struct SystemsGroupBuilder
        {
            private readonly EcsStartup ecsStartup;
            private readonly int order;

            public SystemsGroupBuilder(EcsStartup ecsStartup, int order)
            {
                this.ecsStartup = ecsStartup;
                this.order = order;
            }
#if VCONTAINER
            public SystemsGroupBuilder AddInitializerInjected<T>() where T : class, IInitializer
            {
                ecsStartup.AddInitializerInjected<T>(order);
                return this;
            }

            public SystemsGroupBuilder AddUpdateSystemInjected<T>() where T : class, IUpdateSystem
            {
                ecsStartup.AddSystemInjected<T>(order);
                return this;
            }

            public SystemsGroupBuilder AddFixedSystemInjected<T>() where T : class, IFixedSystem
            {
                ecsStartup.AddSystemInjected<T>(order);
                return this;
            }

            public SystemsGroupBuilder AddLateSystemInjected<T>() where T : class, ILateSystem
            {
                ecsStartup.AddSystemInjected<T>(order);
                return this;
            }
#endif
            public SystemsGroupBuilder AddInitializer<T>(T initializer) where T : class, IInitializer
            {
                ecsStartup.AddInitializer(order, initializer);
                return this;
            }

            public SystemsGroupBuilder AddUpdateSystem<T>(T system) where T : class, IUpdateSystem
            {
                ecsStartup.AddSystem(order, system);
                return this;
            }

            public SystemsGroupBuilder AddFixedSystem<T>(T system) where T : class, IFixedSystem
            {
                ecsStartup.AddSystem(order, system);
                return this;
            }

            public SystemsGroupBuilder AddLateSystem<T>(T system) where T : class, ILateSystem
            {
                ecsStartup.AddSystem(order, system);
                return this;
            }
        }
    }
}
