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

        /// <summary>
        /// Initializes a new instance of the <see cref="TidesAndWavesSystem"/> class.
        /// </summary>
        public TidesAndWavesSystem()
        {
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
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            __TypeHandle.__Game_Simulation_WaterSourceData_RW_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__TidesAndWavesData_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
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
                        currentWaterSourceData.m_Amount = currentTidesAndWavesData.m_OriginalAmount + m_WaveHeight;
                        buffer.SetComponent(currentEntity, currentWaterSourceData);
                    }
                }
            }
        }
    }
}