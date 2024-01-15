// <copyright file="TidesAndWavesSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Systems
{
    using System.Runtime.CompilerServices;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Simulation;
    using Game.Tools;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using UnityEngine;
    using Water_Features.Components;

    /// <summary>
    /// A system for handing waves and tides.
    /// </summary>
    public partial class TidesAndWavesSystem : GameSystemBase
    {
        private EndFrameBarrier m_EndFrameBarrier;
        private TypeHandle __TypeHandle;
        private TimeSystem m_TimeSystem;
        private EntityQuery m_WaterSourceQuery;
        private ILog m_Log;
        private Entity m_DummySeaWaterSource = Entity.Null;
        private float m_PreviousWaveAndTideHeight = 0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="TidesAndWavesSystem"/> class.
        /// </summary>
        public TidesAndWavesSystem()
        {
        }

        /// <summary>
        /// Gets the previous wave and tide height that was used to determine the dummy sea water source.
        /// </summary>
        public float PreviousWaveAndTideHeight { get => m_PreviousWaveAndTideHeight; }

        /// <summary>
        /// The dummy sea water source should not be saved so this allows it to be removed before saving.
        /// </summary>
        public void ResetDummySeaWaterSource()
        {
            if (m_DummySeaWaterSource != Entity.Null)
            {
                EntityManager.DestroyEntity(m_DummySeaWaterSource);
                m_DummySeaWaterSource = Entity.Null;
            }
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = WaterFeaturesMod.Instance.Log;
            m_TimeSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TimeSystem>();
            m_EndFrameBarrier = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<EndFrameBarrier>();
            m_WaterSourceQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadWrite<Game.Simulation.WaterSourceData>(),
                        ComponentType.ReadOnly<TidesAndWavesData>(),
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Deleted>(),
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Owner>(),
                    },
                },
            });
            RequireForUpdate(m_WaterSourceQuery);
            m_Log.Info($"[{nameof(TidesAndWavesSystem)}] {nameof(OnCreate)}");
            if (!WaterFeaturesMod.Settings.EnableWavesAndTides)
            {
                m_Log.Info($"[{nameof(TidesAndWavesSystem)}] {nameof(OnCreate)} Waves and Tides disabled.");
                Enabled = false;
                DisableWavesAndTidesSystem disableWavesAndTidesSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<DisableWavesAndTidesSystem>();
                disableWavesAndTidesSystem.Enabled = true;
            }
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (WaterFeaturesMod.Settings.WaveHeight == 0f && WaterFeaturesMod.Settings.TideHeight == 0f)
            {
                return;
            }

            __TypeHandle.__Game_Simulation_WaterSourceData_RW_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__TidesAndWavesData_RO_ComponentTypeHandle.Update(ref CheckedStateRef);

            if (m_DummySeaWaterSource == Entity.Null)
            {
                float seaLevel = float.MaxValue;
                NativeArray<TidesAndWavesData> seaWaterSources = m_WaterSourceQuery.ToComponentDataArray<TidesAndWavesData>(Allocator.Temp);
                foreach (TidesAndWavesData seaData in seaWaterSources)
                {
                    if (seaLevel > seaData.m_OriginalAmount)
                    {
                        seaLevel = seaData.m_OriginalAmount;
                    }
                }

                m_PreviousWaveAndTideHeight = WaterFeaturesMod.Settings.WaveHeight + WaterFeaturesMod.Settings.TideHeight;
                seaLevel -= WaterFeaturesMod.Settings.WaveHeight + WaterFeaturesMod.Settings.TideHeight;
                WaterSourceData waterSourceData = new WaterSourceData()
                {
                    m_Amount = seaLevel,
                    m_ConstantDepth = 3,
                    m_Multiplier = 30f,
                    m_Polluted = 0f,
                    m_Radius = 0f,
                };

                Game.Objects.Transform transform = new Game.Objects.Transform()
                {
                    m_Position = default,
                    m_Rotation = default,
                };

                m_DummySeaWaterSource = EntityManager.CreateEntity();
                EntityManager.AddComponent(m_DummySeaWaterSource, ComponentType.ReadWrite<WaterSourceData>());
                EntityManager.SetComponentData(m_DummySeaWaterSource, waterSourceData);
                EntityManager.AddComponent(m_DummySeaWaterSource, ComponentType.ReadWrite<Game.Objects.Transform>());
                EntityManager.SetComponentData(m_DummySeaWaterSource, transform);
            }

            AlterSeaWaterSourcesJob alterSeaWaterSourcesJob = new ()
            {
                m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle,
                m_SourceType = __TypeHandle.__Game_Simulation_WaterSourceData_RW_ComponentTypeHandle,
                m_TidesAndWavesDataType = __TypeHandle.__TidesAndWavesData_RO_ComponentTypeHandle,
                buffer = m_EndFrameBarrier.CreateCommandBuffer(),
                m_WaveHeight = (WaterFeaturesMod.Settings.WaveHeight / 2f * Mathf.Sin(2f * Mathf.PI * WaterFeaturesMod.Settings.WaveFrequency * m_TimeSystem.normalizedTime)) + (WaterFeaturesMod.Settings.TideHeight / 2f * Mathf.Cos(2f * Mathf.PI * (float)WaterFeaturesMod.Settings.TideClassification * m_TimeSystem.normalizedDate)) + (WaterFeaturesMod.Settings.WaveHeight / 2) + (WaterFeaturesMod.Settings.TideHeight / 2),
            };
            JobHandle jobHandle = JobChunkExtensions.Schedule(alterSeaWaterSourcesJob, m_WaterSourceQuery, Dependency);
            m_EndFrameBarrier.AddJobHandleForProducer(jobHandle);
            Dependency = jobHandle;
        }

        /// <inheritdoc/>
        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            __TypeHandle.AssignHandles(ref CheckedStateRef);
        }

        private struct TypeHandle
        {
            public ComponentTypeHandle<Game.Simulation.WaterSourceData> __Game_Simulation_WaterSourceData_RW_ComponentTypeHandle;
            [ReadOnly]
            public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<TidesAndWavesData> __TidesAndWavesData_RO_ComponentTypeHandle;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AssignHandles(ref SystemState state)
            {
                __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                __Game_Simulation_WaterSourceData_RW_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Simulation.WaterSourceData>();
                __TidesAndWavesData_RO_ComponentTypeHandle = state.GetComponentTypeHandle<TidesAndWavesData>();
            }
        }

        private struct AlterSeaWaterSourcesJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            public ComponentTypeHandle<Game.Simulation.WaterSourceData> m_SourceType;
            public EntityCommandBuffer buffer;
            public ComponentTypeHandle<TidesAndWavesData> m_TidesAndWavesDataType;
            public float m_WaveHeight;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<Game.Simulation.WaterSourceData> waterSourceDataNativeArray = chunk.GetNativeArray(ref m_SourceType);
                NativeArray<TidesAndWavesData> wavesAndTidesDataNativeArray = chunk.GetNativeArray(ref m_TidesAndWavesDataType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];
                    Game.Simulation.WaterSourceData currentWaterSourceData = waterSourceDataNativeArray[i];
                    TidesAndWavesData currentTidesAndWavesData = wavesAndTidesDataNativeArray[i];
                    if (currentWaterSourceData.m_ConstantDepth == 3 && currentWaterSourceData.m_Amount > 0f)
                    {
                        currentWaterSourceData.m_Amount = currentTidesAndWavesData.m_OriginalAmount - m_WaveHeight;
                        buffer.SetComponent(currentEntity, currentWaterSourceData);
                    }
                }
            }
        }
    }
}