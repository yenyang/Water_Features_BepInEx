// <copyright file="SeasonalStreamsSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Prefabs.Climate;
    using Game.Simulation;
    using Game.Tools;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine;
    using Water_Features.Components;
    using Water_Features.Prefabs;
    using Water_Features.Settings;
    using Water_Features.Utils;
    using static Game.Simulation.ClimateSystem;
    using static Water_Features.Tools.WaterToolUISystem;

    /// <summary>
    /// A system for handing creek seasonality and runoff.
    /// </summary>
    public partial class SeasonalStreamsSystem : GameSystemBase
    {
        public static readonly int kUpdatesPerDay = 128;

        private const string PrefabPrefix = "WaterSource ";
        private const string TabName = "Water Tool";
        private readonly Dictionary<SourceType, string> m_SourceTypeIcons = new Dictionary<SourceType, string>()
        {
            { SourceType.Creek, "coui://uil/Standard/Creek.svg" },
            { SourceType.Lake, "coui://uil/Standard/Lake.svg" },
            { SourceType.River, "coui://uil/Standard/River.svg" },
            { SourceType.Sea, "coui://uil/Standard/Sea.svg" },
            { SourceType.AutofillingLake, "coui://uil/Standard/AutomaticFill.svg" },
            { SourceType.DetentionBasin, "coui://uil/Standard/DetentionBasin.svg" },
            { SourceType.RetentionBasin, "coui://uil/Standard/RetentionBasin.svg" },
        };

        private TypeHandle __TypeHandle;
        private ClimateSystem m_ClimateSystem;
        private string m_CurrentSeason;
        private EntityQuery m_ClimateQuery;
        private EntityQuery m_OriginalAmountsQuery;
        private PrefabSystem m_PrefabSystem;
        private ClimatePrefab m_ClimatePrefab;
        private float m_CurrentSeasonMeanPrecipitation = 0f;
        private TerrainSystem m_TerrainSystem;
        private float m_MaxSeasonMeanPrecipitation = 0f;
        private Entity m_ClimateEntity;
        private ILog m_Log;
        private bool ClimateInteractionInitialized = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonalStreamsSystem"/> class.
        /// </summary>
        public SeasonalStreamsSystem()
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
            m_CurrentSeason = m_ClimateSystem.currentSeasonNameID;
            m_PrefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            m_ClimateQuery = GetEntityQuery(ComponentType.ReadOnly<ClimateData>());
            m_TerrainSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TerrainSystem>();
            m_OriginalAmountsQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new ()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadWrite<Game.Simulation.WaterSourceData>(),
                        ComponentType.ReadWrite<SeasonalStreamsData>(),
                        ComponentType.ReadOnly<Game.Objects.Transform>(),
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Deleted>(),
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Owner>(),
                    },
                },
            });
            RequireForUpdate(m_ClimateQuery);
            RequireForUpdate(m_OriginalAmountsQuery);
            m_Log.Info($"[{nameof(SeasonalStreamsSystem)}] {nameof(OnCreate)}");
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (ClimateInteractionInitialized == false)
            {
                InitializeClimateInteraction();
                ClimateInteractionInitialized = true;
            }

            if (m_ClimateSystem.currentSeasonNameID != m_CurrentSeason)
            {
                m_CurrentSeason = m_ClimateSystem.currentSeasonNameID;
                float normalizedClimateDate = GetClimateDate();
                m_CurrentSeasonMeanPrecipitation = GetMeanPrecipitation(m_ClimatePrefab, normalizedClimateDate);
            }

            __TypeHandle.__Game_Simulation_WaterSourceData_RW_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Seasonal_Streams_OriginalAmountComponent_RW_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Objects_Transform_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            ReviseWaterSourcesJob reviseWaterSourcesJob = new ()
            {
                m_SourceType = __TypeHandle.__Game_Simulation_WaterSourceData_RW_ComponentTypeHandle,
                m_SeasonalStreamDataType = __TypeHandle.__Seasonal_Streams_OriginalAmountComponent_RW_ComponentTypeHandle,
                m_TerrainHeightData = m_TerrainSystem.GetHeightData(false),
                m_TransformType = __TypeHandle.__Game_Objects_Transform_RO_ComponentTypeHandle,
            };
            if (m_ClimateSystem.isSnowing == false)
            {
                reviseWaterSourcesJob.m_WaterSourceMultiplier = Mathf.Clamp((m_CurrentSeasonMeanPrecipitation / m_MaxSeasonMeanPrecipitation * WaterFeaturesMod.Settings.CreekMeanPrecipitationWeight) + (m_ClimateSystem.precipitation * WaterFeaturesMod.Settings.CreekCurrentPrecipitationWeight) + WaterFeaturesMod.Settings.CreekSpringWater, WaterFeaturesMod.Settings.MinimumMultiplier, WaterFeaturesMod.Settings.MaximumMultiplier);
                reviseWaterSourcesJob.m_SnowAccumulationMultiplier = 0f;
                if (WaterFeaturesMod.Settings.SimulateSnowMelt == true && m_ClimateSystem.temperature.value > m_ClimateSystem.freezingTemperature && reviseWaterSourcesJob.m_WaterSourceMultiplier < WaterFeaturesMod.Settings.MaximumMultiplier)
                {
                    reviseWaterSourcesJob.m_PotentialSnowMeltMultiplier = WaterFeaturesMod.Settings.MaximumMultiplier - reviseWaterSourcesJob.m_WaterSourceMultiplier;
                    reviseWaterSourcesJob.m_TemperatureDifferential = m_ClimateSystem.temperature.value - m_ClimateSystem.freezingTemperature;
                }
                else
                {
                    reviseWaterSourcesJob.m_PotentialSnowMeltMultiplier = 0f;
                    reviseWaterSourcesJob.m_TemperatureDifferential = 0f;
                }
            }
            else if (WaterFeaturesMod.Settings.SimulateSnowMelt == true)
            {
                reviseWaterSourcesJob.m_WaterSourceMultiplier = Mathf.Clamp((m_CurrentSeasonMeanPrecipitation / m_MaxSeasonMeanPrecipitation * WaterFeaturesMod.Settings.CreekMeanPrecipitationWeight) + WaterFeaturesMod.Settings.CreekSpringWater, WaterFeaturesMod.Settings.MinimumMultiplier, WaterFeaturesMod.Settings.MaximumMultiplier);
                reviseWaterSourcesJob.m_SnowAccumulationMultiplier = m_ClimateSystem.precipitation * WaterFeaturesMod.Settings.CreekCurrentPrecipitationWeight;
                reviseWaterSourcesJob.m_PotentialSnowMeltMultiplier = 0f;
                reviseWaterSourcesJob.m_TemperatureDifferential = 0f;
            }

            ReviseWaterSourcesJob jobData = reviseWaterSourcesJob;
            JobHandle jobHandle = JobChunkExtensions.Schedule(jobData, m_OriginalAmountsQuery, Dependency);
            m_TerrainSystem.AddCPUHeightReader(jobHandle);
            Dependency = jobHandle;

            if (m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetCategoryData), TabName), out var waterToolTabPrefab) || waterToolTabPrefab is UIAssetCategoryPrefab)
            {
                if (!m_PrefabSystem.TryGetEntity(waterToolTabPrefab, out Entity waterToolTabPrefabEntity))
                {
                    m_Log.Info("Couldn't find prefab entitty");
                    return;
                }

                if (!EntityManager.TryGetComponent(waterToolTabPrefabEntity, out UIAssetCategoryData currentMenu))
                {
                    m_Log.Info("Couldn't find menu component");
                    return;
                }

                if (currentMenu.m_Menu == Entity.Null && m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetMenuPrefab), "Landscaping"), out PrefabBase prefab1))
                {
                    Entity entity = m_PrefabSystem.GetEntity(prefab1);
                    m_Log.Info($"{nameof(SeasonalStreamsSystem)}.{nameof(OnUpdate)} currentMenu = Entity.Null so set to {entity.Index}.{entity.Version}");
                    currentMenu.m_Menu = entity;
                    EntityManager.SetComponentData(waterToolTabPrefabEntity, currentMenu);
                    m_PrefabSystem.UpdatePrefab(prefab1);
                }
                else
                {
                    m_Log.Info("Couldn't find menu or menu is not null");
                }
            }
            else
            {
                m_Log.Info("Couldn't find prefab anymore.");
            }

            foreach (KeyValuePair<SourceType, string> sources in m_SourceTypeIcons)
            {
                if (m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(WaterSourcePrefab), $"{PrefabPrefix}{sources.Key}"), out var waterSourcePrefab) || waterSourcePrefab is WaterSourcePrefab)
                {
                    if (!m_PrefabSystem.TryGetEntity(waterSourcePrefab, out Entity waterSourcePrefabEntity))
                    {
                        continue;
                    }

                    if (!EntityManager.TryGetComponent(waterSourcePrefabEntity, out UIObjectData uIObjectData))
                    {
                        continue;
                    }

                    if (uIObjectData.m_Group == Entity.Null && m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetCategoryData), TabName), out PrefabBase prefab1))
                    {
                        Entity entity = m_PrefabSystem.GetEntity(prefab1);
                        m_Log.Info($"{nameof(SeasonalStreamsSystem)}.{nameof(OnUpdate)} uIObjectData.m_Group = Entity.Null so set to {entity.Index}.{entity.Version}");
                        uIObjectData.m_Group = entity;
                        EntityManager.SetComponentData(waterSourcePrefabEntity, uIObjectData);
                    }
                }
                else
                {
                    m_Log.Info("Couldn't find tab name or entity is not null.");
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            ClimateInteractionInitialized = false;
            base.OnGamePreload(purpose, mode);
        }

        /// <inheritdoc/>
        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            __TypeHandle.AssignHandles(ref CheckedStateRef);
        }

        private void InitializeClimateInteraction()
        {
            var m_ClimateEntityVar = m_ClimateSystem.GetMemberValue("m_CurrentClimate");
            m_ClimateEntity = (Entity)m_ClimateEntityVar;
            m_ClimatePrefab = m_PrefabSystem.GetPrefab<ClimatePrefab>(m_ClimateEntity);
            float normalizedClimateDate = GetClimateDate();
            m_CurrentSeasonMeanPrecipitation = GetMeanPrecipitation(m_ClimatePrefab, normalizedClimateDate);
            float seasonLength = GetSeasonLength(m_ClimatePrefab, normalizedClimateDate);
            for (int i = 0; i < 4; i++)
            {
                float testDate = i*seasonLength;
                float testSeasonMeanPrecipitation = GetMeanPrecipitation(m_ClimatePrefab, testDate);
                if (testSeasonMeanPrecipitation > m_MaxSeasonMeanPrecipitation)
                {
                    m_MaxSeasonMeanPrecipitation= testSeasonMeanPrecipitation;
                }
            }
        }

        private float GetMeanPrecipitation(ClimatePrefab climatePrefab, float normalizedDate)
        {
            (SeasonInfo, float, float) valueTuple = climatePrefab.FindSeasonByTime(normalizedDate);
            System.Reflection.MethodInfo calculateMeanPrecipitationMethod = m_ClimateSystem.GetType().GetMethod("CalculateMeanPrecipitation", BindingFlags.NonPublic | BindingFlags.Instance);
            object meanPrecipitationObject = calculateMeanPrecipitationMethod.Invoke(m_ClimateSystem, new object[] { climatePrefab, 48, valueTuple.Item2, valueTuple.Item3 });
            float meanPrecipitation = (float)meanPrecipitationObject;
            return meanPrecipitation;
        }

        private float GetSeasonLength(ClimatePrefab climatePrefab, float normalizedDate)
        {
            (SeasonInfo, float, float) valueTuple = climatePrefab.FindSeasonByTime(normalizedDate);
            return Math.Abs(valueTuple.Item3 - valueTuple.Item2);
        }

        private float GetClimateDate()
        {
            var climateDateVar = m_ClimateSystem.GetMemberValue("m_Date");
            OverridableProperty<float> climateDate = (OverridableProperty<float>)climateDateVar;
            float normalizedClimateDate = float.Parse(climateDate.ToString(), CultureInfo.InvariantCulture.NumberFormat); // This is a dumb solution but climateDate.value is coming up as 0.
            return normalizedClimateDate;
        }

        private struct TypeHandle
        {
            public ComponentTypeHandle<Game.Simulation.WaterSourceData> __Game_Simulation_WaterSourceData_RW_ComponentTypeHandle;
            public ComponentTypeHandle<SeasonalStreamsData> __Seasonal_Streams_OriginalAmountComponent_RW_ComponentTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<Game.Objects.Transform> __Game_Objects_Transform_RO_ComponentTypeHandle;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AssignHandles(ref SystemState state)
            {
                __Game_Simulation_WaterSourceData_RW_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Simulation.WaterSourceData>();
                __Seasonal_Streams_OriginalAmountComponent_RW_ComponentTypeHandle = state.GetComponentTypeHandle<SeasonalStreamsData>();
                __Game_Objects_Transform_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Objects.Transform>();
            }
        }

        private struct ReviseWaterSourcesJob : IJobChunk
        {
            public ComponentTypeHandle<Game.Simulation.WaterSourceData> m_SourceType;
            public ComponentTypeHandle<SeasonalStreamsData> m_SeasonalStreamDataType;
            public ComponentTypeHandle<Game.Objects.Transform> m_TransformType;
            public float m_WaterSourceMultiplier;
            public float m_SnowAccumulationMultiplier;
            public float m_PotentialSnowMeltMultiplier;
            public float m_TemperatureDifferential;
            public TerrainHeightData m_TerrainHeightData;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Game.Objects.Transform> transformNativeArray = chunk.GetNativeArray(ref m_TransformType);
                NativeArray<Game.Simulation.WaterSourceData> waterSourceDataNativeArray = chunk.GetNativeArray(ref m_SourceType);
                NativeArray<SeasonalStreamsData> seasonalStreamDataNativeArray = chunk.GetNativeArray(ref m_SeasonalStreamDataType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    float snowMelt = 0f;
                    Game.Simulation.WaterSourceData currentWaterSourceData = waterSourceDataNativeArray[i];
                    Game.Objects.Transform currentTransform = transformNativeArray[i];
                    float3 terrainPosition = new(currentTransform.m_Position.x, TerrainUtils.SampleHeight(ref m_TerrainHeightData, currentTransform.m_Position), currentTransform.m_Position.z);
                    float temperatureDifferentialAtWaterSource = m_TemperatureDifferential - (terrainPosition.y / 500f);
                    SeasonalStreamsData currentSeasonalStreamData = seasonalStreamDataNativeArray[i];
                    if (m_SnowAccumulationMultiplier > 0f)
                    {
                        currentSeasonalStreamData.m_SnowAccumulation += seasonalStreamDataNativeArray[i].m_OriginalAmount * m_SnowAccumulationMultiplier;
                    }
                    if (m_PotentialSnowMeltMultiplier > 0f && currentSeasonalStreamData.m_SnowAccumulation > 0f && temperatureDifferentialAtWaterSource > 0f)
                    {
                        snowMelt = TryMeltSnow(m_PotentialSnowMeltMultiplier, ref currentSeasonalStreamData, temperatureDifferentialAtWaterSource);
                    }
                    currentWaterSourceData.m_Amount = (seasonalStreamDataNativeArray[i].m_OriginalAmount * m_WaterSourceMultiplier) + snowMelt;
                    waterSourceDataNativeArray[i] = currentWaterSourceData;
                    seasonalStreamDataNativeArray[i] = currentSeasonalStreamData;
                }
            }

            private static float TryMeltSnow(float maxMultiplier, ref SeasonalStreamsData data, float temperatureDifferential)
            {
                const float meltingRate = 1f / 30f;
                float maxSnowMelt = Mathf.Min(data.m_OriginalAmount * maxMultiplier, data.m_SnowAccumulation, temperatureDifferential * meltingRate * data.m_OriginalAmount);
                data.m_SnowAccumulation -= maxSnowMelt;
                return maxSnowMelt;
            }
        }
    }
}
