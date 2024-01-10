// <copyright file="RetentionBasinSystem.cs" company="Yenyang's Mods. MIT License">
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
    using Unity.Mathematics;
    using UnityEngine;
    using Water_Features.Components;
    using Water_Features.Settings;

    /// <summary>
    /// A system for handling retention basin custom water sources.
    /// </summary>
    public partial class RetentionBasinSystem : GameSystemBase
    {
        public static readonly int kUpdatesPerDay = 128;

        private TypeHandle __TypeHandle;
        private EndFrameBarrier m_EndFrameBarrier;
        private ClimateSystem m_ClimateSystem;
        private WaterSystem m_WaterSystem;
        private TerrainSystem m_TerrainSystem;
        private EntityQuery m_RetentionBasinQuery;
        private ILog m_Log;


        /// <summary>
        /// Initializes a new instance of the <see cref="RetentionBasinSystem"/> class.
        /// </summary>
        public RetentionBasinSystem()
        {
        }

        /// <inheritdoc/>
        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 262144 / kUpdatesPerDay;
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = WaterFeaturesMod.Instance.Log;
            m_ClimateSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ClimateSystem>();
            m_EndFrameBarrier = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<EndFrameBarrier>();
            m_WaterSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<WaterSystem>();
            m_TerrainSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TerrainSystem>();
            m_RetentionBasinQuery = GetEntityQuery(new EntityQueryDesc[] 
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadWrite<Game.Simulation.WaterSourceData>(),
                        ComponentType.ReadOnly<Game.Objects.Transform>(),
                        ComponentType.ReadWrite<RetentionBasin>(),
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Deleted>(),
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Owner>(),
                    },
                },
            });
            RequireForUpdate(m_RetentionBasinQuery);
            m_Log.Info($"[{nameof(RetentionBasinSystem)}] {nameof(OnCreate)}");
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            __TypeHandle.__RetentionBasin_RW_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Simulation_WaterSourceData_RW_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Objects_Transform_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Simulation_WaterSourceData_RW_ComponentTypeHandle.Update(ref CheckedStateRef);
            RetentionBasinJob retentionBasinJob = new ()
            {
                m_RetentionBasinType = __TypeHandle.__RetentionBasin_RW_ComponentTypeHandle,
                m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle,
                m_SourceType = __TypeHandle.__Game_Simulation_WaterSourceData_RW_ComponentTypeHandle,
                m_TerrainHeightData = m_TerrainSystem.GetHeightData(false),
                m_WaterSurfaceData = m_WaterSystem.GetSurfaceData(out JobHandle waterSurfaceDataJob),
                m_TransformType = __TypeHandle.__Game_Objects_Transform_RO_ComponentTypeHandle,
                buffer = m_EndFrameBarrier.CreateCommandBuffer(),
                m_Precipiation = m_ClimateSystem.precipitation,
                m_Snowing = m_ClimateSystem.isSnowing,
                m_TemperatureDifferential = m_ClimateSystem.temperature.value - m_ClimateSystem.freezingTemperature,
            };
            JobHandle jobHandle = JobChunkExtensions.Schedule(retentionBasinJob, m_RetentionBasinQuery, JobHandle.CombineDependencies(waterSurfaceDataJob, Dependency));
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
            public ComponentTypeHandle<RetentionBasin> __RetentionBasin_RW_ComponentTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<Game.Objects.Transform> __Game_Objects_Transform_RO_ComponentTypeHandle;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AssignHandles(ref SystemState state)
            {
                __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                __Game_Simulation_WaterSourceData_RW_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Simulation.WaterSourceData>();
                __RetentionBasin_RW_ComponentTypeHandle = state.GetComponentTypeHandle<RetentionBasin>();
                __Game_Objects_Transform_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Objects.Transform>();
            }
        }

        /// <summary>
        /// This handles retention basins going up and staying at maximum.
        /// </summary>
        private struct RetentionBasinJob : IJobChunk
        {
            public ComponentTypeHandle<RetentionBasin> m_RetentionBasinType;
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            public ComponentTypeHandle<Game.Simulation.WaterSourceData> m_SourceType;
            public ComponentTypeHandle<Game.Objects.Transform> m_TransformType;
            public TerrainHeightData m_TerrainHeightData;
            public WaterSurfaceData m_WaterSurfaceData;
            public EntityCommandBuffer buffer;
            public float m_Precipiation;
            public bool m_Snowing;
            public float m_TemperatureDifferential;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Game.Simulation.WaterSourceData> waterSourceDataNativeArray = chunk.GetNativeArray(ref m_SourceType);
                NativeArray<Game.Objects.Transform> transformNativeArray = chunk.GetNativeArray(ref m_TransformType);
                NativeArray<RetentionBasin> retentionBasinNativeArray = chunk.GetNativeArray(ref m_RetentionBasinType);
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                float maxDepthToRunoffCoefficient = 0.1f;
                for (int i = 0; i < chunk.Count; i++)
                {
                    Game.Simulation.WaterSourceData currentWaterSourceData = waterSourceDataNativeArray[i];
                    Game.Objects.Transform currentTransform = transformNativeArray[i];
                    RetentionBasin currentRetentionBasin = retentionBasinNativeArray[i];
                    Entity currentEntity = entityNativeArray[i];
                    float3 terrainPosition = new (currentTransform.m_Position.x, TerrainUtils.SampleHeight(ref m_TerrainHeightData, currentTransform.m_Position), currentTransform.m_Position.z);
                    float3 waterPosition = new (currentTransform.m_Position.x, WaterUtils.SampleHeight(ref m_WaterSurfaceData, ref m_TerrainHeightData, currentTransform.m_Position), currentTransform.m_Position.z);
                    float waterHeight = waterPosition.y;
                    float maxDepth = currentRetentionBasin.m_MaximumWaterHeight - terrainPosition.y;
                    float temperatureDifferentialAtWaterSource = m_TemperatureDifferential - (terrainPosition.y / 500f);
                    if (currentWaterSourceData.m_ConstantDepth != 0) // Creek
                    {
                        currentWaterSourceData.m_ConstantDepth = 0; // Creek
                    }

                    if (m_Precipiation > 0f && m_Snowing)
                    {
                        currentRetentionBasin.m_SnowAccumulation += m_Precipiation * maxDepth * maxDepthToRunoffCoefficient;
                        buffer.SetComponent(currentEntity, currentRetentionBasin);
                    }

                    if (waterHeight > currentRetentionBasin.m_MaximumWaterHeight && currentWaterSourceData.m_Amount > 0f)
                    {
                        currentWaterSourceData.m_Amount = 0f;
                        buffer.SetComponent(currentEntity, currentWaterSourceData);
                    }
                    else if (waterHeight < 0.95 * currentRetentionBasin.m_MinimumWaterHeight && currentRetentionBasin.m_MinimumWaterHeight < currentRetentionBasin.m_MaximumWaterHeight)
                    {
                        currentWaterSourceData.m_Amount = (currentRetentionBasin.m_MinimumWaterHeight - terrainPosition.y) * maxDepthToRunoffCoefficient;
                        buffer.SetComponent(currentEntity, currentWaterSourceData);
                    }
                    else if (m_Precipiation > 0f && !m_Snowing)
                    {
                        if (Mathf.Approximately(currentRetentionBasin.m_SnowAccumulation, 0f) && temperatureDifferentialAtWaterSource > 0f)
                        {
                            currentWaterSourceData.m_Amount = m_Precipiation * maxDepth * maxDepthToRunoffCoefficient;
                        }
                        else
                        {
                            currentWaterSourceData.m_Amount = (m_Precipiation * maxDepth * maxDepthToRunoffCoefficient) + TryMeltSnow(ref currentRetentionBasin, temperatureDifferentialAtWaterSource);
                            buffer.SetComponent(currentEntity, currentRetentionBasin);
                        }
                        if (waterHeight > 0.95f * currentRetentionBasin.m_MaximumWaterHeight)
                        {
                            currentWaterSourceData.m_Amount = Mathf.Min(currentWaterSourceData.m_Amount, currentRetentionBasin.m_MaximumWaterHeight * .1f);
                        }
                        buffer.SetComponent(currentEntity, currentWaterSourceData);
                    }
                    else
                    {
                        currentWaterSourceData.m_Amount = 0f;
                        buffer.SetComponent(currentEntity, currentWaterSourceData);
                    }
                }
            }

            private static float TryMeltSnow(ref RetentionBasin data, float temperatureDifferential)
            {
                const float meltingRate = 1f / 30f;
                float maxHeightToRunoffCoefficient = 0.1f;
                float maxSnowMelt = Mathf.Min(data.m_SnowAccumulation, temperatureDifferential * meltingRate * data.m_MaximumWaterHeight * maxHeightToRunoffCoefficient);
                data.m_SnowAccumulation -= maxSnowMelt;
                return maxSnowMelt;
            }
        }
    }
}
