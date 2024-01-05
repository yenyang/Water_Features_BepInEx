// <copyright file="BeforeSerializeSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Systems
{
    using System.Runtime.CompilerServices;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Tools;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Water_Features.Components;

    /// <summary>
    /// A system that runes before serailaztion so that all water sources are reset and/or saved in a manner that can be reloaded safely without the mod.
    /// </summary>
    public partial class BeforeSerializeSystem : GameSystemBase
    {
        private TypeHandle __TypeHandle;
        private EntityQuery m_SeasonalStreamsDataQuery;
        private EntityQuery m_TidesAndWavesDataQuery;
        private EntityQuery m_LakeLikeWaterSourcesQuery;
        private ILog m_Log;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeforeSerializeSystem"/> class.
        /// </summary>
        public BeforeSerializeSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate ()
        {
            base.OnCreate();
            m_Log = WaterFeaturesMod.Instance.Log;
            m_SeasonalStreamsDataQuery = GetEntityQuery(new EntityQueryDesc[] {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadWrite<Game.Simulation.WaterSourceData>(),
                        ComponentType.ReadOnly<SeasonalStreamsData>(),
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Deleted>(),
                        ComponentType.ReadOnly<Temp>(),
                    },
                },
            });
            m_TidesAndWavesDataQuery = GetEntityQuery(new EntityQueryDesc[] {
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
                    },
                },
            });
            m_LakeLikeWaterSourcesQuery = GetEntityQuery(new EntityQueryDesc[] {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadWrite<Game.Simulation.WaterSourceData>(),
                    },
                    Any = new ComponentType[]
                    {
                        ComponentType.ReadOnly<AutofillingLake>(),
                        ComponentType.ReadOnly<DetentionBasin>(),
                        ComponentType.ReadOnly<RetentionBasin>(),
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Deleted>(),
                        ComponentType.ReadOnly<Temp>(),
                    },
                },
            });
            RequireAnyForUpdate(new EntityQuery[] { m_LakeLikeWaterSourcesQuery, m_SeasonalStreamsDataQuery, m_TidesAndWavesDataQuery});
            m_Log.Info($"[{nameof(BeforeSerializeSystem)}] {nameof(OnCreate)}");
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            __TypeHandle.__Game_Simulation_WaterSourceData_RW_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Seasonal_Streams_OriginalAmountComponent_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__TidesAndWavesData_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            BeforeSerializeSeasonalStreamsJob beforeSerializeSeasonalStreamsJob = new ()
            {
                m_OriginalAmountType = __TypeHandle.__Seasonal_Streams_OriginalAmountComponent_RO_ComponentTypeHandle,
                m_SourceType = __TypeHandle.__Game_Simulation_WaterSourceData_RW_ComponentTypeHandle,
            };
            Dependency = JobChunkExtensions.Schedule(beforeSerializeSeasonalStreamsJob, m_SeasonalStreamsDataQuery, Dependency);
            BeforeSerializeTidesAndWavesJob beforeSerializeTidesAndWavesJob = new ()
            {
                m_OriginalAmountType = __TypeHandle.__TidesAndWavesData_RO_ComponentTypeHandle,
                m_SourceType = __TypeHandle.__Game_Simulation_WaterSourceData_RW_ComponentTypeHandle,
            };
            Dependency = JobChunkExtensions.Schedule(beforeSerializeTidesAndWavesJob, m_TidesAndWavesDataQuery, Dependency);
            BeforeSerializeLakeLikeWaterSourcesJob beforeSerializeLakeLikeWaterSourcesJob = new ()
            {
                m_SourceType = __TypeHandle.__Game_Simulation_WaterSourceData_RW_ComponentTypeHandle,
            };
            Dependency = JobChunkExtensions.Schedule(beforeSerializeLakeLikeWaterSourcesJob, m_LakeLikeWaterSourcesQuery, Dependency);
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
            public ComponentTypeHandle<SeasonalStreamsData> __Seasonal_Streams_OriginalAmountComponent_RO_ComponentTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<TidesAndWavesData> __TidesAndWavesData_RO_ComponentTypeHandle;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AssignHandles(ref SystemState state)
            {
                __Game_Simulation_WaterSourceData_RW_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Simulation.WaterSourceData>();
                __Seasonal_Streams_OriginalAmountComponent_RO_ComponentTypeHandle = state.GetComponentTypeHandle<SeasonalStreamsData>();
                __TidesAndWavesData_RO_ComponentTypeHandle = state.GetComponentTypeHandle<TidesAndWavesData>();
            }
        }

        private struct BeforeSerializeSeasonalStreamsJob : IJobChunk
        {
            public ComponentTypeHandle<Game.Simulation.WaterSourceData> m_SourceType;
            [ReadOnly]
            public ComponentTypeHandle<SeasonalStreamsData> m_OriginalAmountType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Game.Simulation.WaterSourceData> waterSourceDataNativeArray = chunk.GetNativeArray(ref m_SourceType);
                NativeArray<SeasonalStreamsData> originalAmountNativeArray = chunk.GetNativeArray(ref m_OriginalAmountType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Game.Simulation.WaterSourceData currentWaterSourceData = waterSourceDataNativeArray[i];
                    currentWaterSourceData.m_Amount = originalAmountNativeArray[i].m_OriginalAmount;
                    waterSourceDataNativeArray[i] = currentWaterSourceData;
                }
            }
        }

        private struct BeforeSerializeTidesAndWavesJob : IJobChunk
        {
            public ComponentTypeHandle<Game.Simulation.WaterSourceData> m_SourceType;
            [ReadOnly]
            public ComponentTypeHandle<TidesAndWavesData> m_OriginalAmountType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Game.Simulation.WaterSourceData> waterSourceDataNativeArray = chunk.GetNativeArray(ref m_SourceType);
                NativeArray<TidesAndWavesData> originalAmountNativeArray = chunk.GetNativeArray(ref m_OriginalAmountType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Game.Simulation.WaterSourceData currentWaterSourceData = waterSourceDataNativeArray[i];
                    currentWaterSourceData.m_Amount = originalAmountNativeArray[i].m_OriginalAmount;
                    waterSourceDataNativeArray[i] = currentWaterSourceData;
                }
            }
        }

        private struct BeforeSerializeLakeLikeWaterSourcesJob : IJobChunk
        {
            public ComponentTypeHandle<Game.Simulation.WaterSourceData> m_SourceType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Game.Simulation.WaterSourceData> waterSourceDataNativeArray = chunk.GetNativeArray(ref m_SourceType);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Game.Simulation.WaterSourceData currentWaterSourceData = waterSourceDataNativeArray[i];
                    currentWaterSourceData.m_ConstantDepth = 1; // Lake
                    waterSourceDataNativeArray[i] = currentWaterSourceData;
                }
            }
        }
    }
}
