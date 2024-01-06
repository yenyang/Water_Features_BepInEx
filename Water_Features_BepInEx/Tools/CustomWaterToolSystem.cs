// <copyright file="CustomWaterToolSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Tools
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Input;
    using Game.Prefabs;
    using Game.Rendering;
    using Game.Simulation;
    using Game.Tools;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine;
    using Water_Features;
    using Water_Features.Components;
    using Water_Features.Prefabs;
    using Water_Features.Systems;
    using Water_Features.Utils;

    /// <summary>
    /// A custom water tool system for creating or modifying water sources.
    /// </summary>
    public partial class CustomWaterToolSystem : ToolBaseSystem
    {
        private const float MapExtents = 7200f;

        private readonly List<WaterToolUISystem.SourceType> LakeLikeSources = new ()
        {
            { WaterToolUISystem.SourceType.Lake },
            { WaterToolUISystem.SourceType.AutofillingLake },
            { WaterToolUISystem.SourceType.DetentionBasin },
            { WaterToolUISystem.SourceType.RetentionBasin },
        };

        private EntityArchetype m_WaterSourceArchetype;
        private EntityArchetype m_AutoFillingLakeArchetype;
        private EntityArchetype m_DetentionBasinArchetype;
        private EntityArchetype m_RetentionBasinArchetype;
        private ProxyAction m_ApplyAction;
        private ProxyAction m_SecondaryApplyAction;
        private ControlPoint m_RaycastPoint;
        private TypeHandle __TypeHandle;
        private EntityQuery m_WaterSourcesQuery;
        private ToolOutputBarrier m_ToolOutputBarrier;
        private OverlayRenderSystem m_OverlayRenderSystem;
        private WaterToolUISystem m_WaterToolUISystem;
        private FindWaterSourcesSystem m_FindWaterSourcesSystem;
        private WaterTooltipSystem m_WaterTooltipSystem;
        private WaterSystem m_WaterSystem;
        private TerrainSystem m_TerrainSystem;
        private ILog m_Log;
        private NativeList<Entity> m_HoveredWaterSources;
        private WaterSourcePrefab m_ActivePrefab;

        /// <summary>
        /// Gets a value indicating the toolid.
        /// </summary>
        public override string toolID => "Yenyang's Water Tool";

        /// <summary>
        /// A method for determining if a position is close to the border.
        /// </summary>
        /// <param name="pos">Position to be checked.</param>
        /// <returns>True if within proximity of border.</returns>
        public bool IsPositionNearBorder(Vector3 pos)
        {
            float tolerance = 100f;
            if (pos.x > MapExtents - tolerance || pos.x < (-1 * MapExtents) + tolerance || pos.z > MapExtents - tolerance || pos.z < (-1 * MapExtents) + tolerance)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// When the tool is canceled set active tool to default tool.
        /// </summary>
        public void RequestDisable()
        {
            m_ToolSystem.activeTool = m_DefaultToolSystem;
        }

        /// <inheritdoc/>
        public override PrefabBase GetPrefab()
        {
            if (m_ToolSystem.activeTool == this && m_ActivePrefab != null)
            {
                return m_ActivePrefab;
            }

            return null;
        }

        /// <inheritdoc/>
        public override bool TrySetPrefab(PrefabBase prefab)
        {
            m_Log.Debug($"{nameof(CustomWaterToolSystem)}.{nameof(TrySetPrefab)}");
            if (prefab is WaterSourcePrefab)
            {
                m_ActivePrefab = prefab as WaterSourcePrefab;
                m_Log.Debug($"{nameof(CustomWaterToolSystem)}.{nameof(TrySetPrefab)} prefab is {prefab.name}.");
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override void InitializeRaycast()
        {
            base.InitializeRaycast();
            m_ToolRaycastSystem.typeMask = TypeMask.Terrain;
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = WaterFeaturesMod.Instance.Log;
            Enabled = false;
            m_ApplyAction = InputManager.instance.FindAction("Tool", "Apply");
            m_SecondaryApplyAction = InputManager.instance.FindAction("Tool", "Secondary Apply");
            m_Log.Info($"[{nameof(CustomWaterToolSystem)}] {nameof(OnCreate)}");
            m_ToolOutputBarrier = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_WaterSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<WaterSystem>();
            m_WaterTooltipSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<WaterTooltipSystem>();
            m_TerrainSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TerrainSystem>();
            m_WaterToolUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<WaterToolUISystem>();
            m_WaterSourceArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Simulation.WaterSourceData>(), ComponentType.ReadWrite<Game.Objects.Transform>());
            m_AutoFillingLakeArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Simulation.WaterSourceData>(), ComponentType.ReadWrite<Game.Objects.Transform>(), ComponentType.ReadWrite<AutofillingLake>());
            m_DetentionBasinArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Simulation.WaterSourceData>(), ComponentType.ReadWrite<Game.Objects.Transform>(), ComponentType.ReadWrite<DetentionBasin>());
            m_RetentionBasinArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Simulation.WaterSourceData>(), ComponentType.ReadWrite<Game.Objects.Transform>(), ComponentType.ReadWrite<RetentionBasin>());
            m_OverlayRenderSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<OverlayRenderSystem>();
            m_HoveredWaterSources = new NativeList<Entity>(0, Allocator.Persistent);
            m_WaterSourcesQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new ()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Game.Simulation.WaterSourceData>(),
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
            RequireForUpdate(m_WaterSourcesQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnStartRunning()
        {
            m_Log.Debug($"{nameof(CustomWaterToolSystem)}.{nameof(OnStartRunning)}");
            m_ApplyAction.shouldBeEnabled = true;
            m_SecondaryApplyAction.shouldBeEnabled = true;
            m_RaycastPoint = default;
        }

        /// <inheritdoc/>
        protected override void OnStopRunning()
        {
            m_ApplyAction.shouldBeEnabled = false;
            m_SecondaryApplyAction.shouldBeEnabled = false;
        }

        /// <inheritdoc/>
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = Dependency;
            __TypeHandle.__Game_Simulation_WaterSourceData_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Game_Objects_Transform_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref CheckedStateRef);
            if (m_ApplyAction.WasPressedThisFrame())
            {
                GetRaycastResult(out m_RaycastPoint);
                if (m_RaycastPoint.m_HitPosition.x != 0f || m_RaycastPoint.m_HitPosition.z != 0f)
                {
                    if (m_ActivePrefab.m_SourceType != WaterToolUISystem.SourceType.River)
                    {
                        TryAddWaterSource(ref inputDeps, m_RaycastPoint.m_HitPosition);
                        return inputDeps;
                    }
                    else if (IsPositionNearBorder(m_RaycastPoint.m_HitPosition))
                    {
                        Vector3 borderPosition = m_RaycastPoint.m_HitPosition;
                        if (Mathf.Abs(m_RaycastPoint.m_HitPosition.x) >= Mathf.Abs(m_RaycastPoint.m_HitPosition.z))
                        {
                            if (m_RaycastPoint.m_HitPosition.x > 0f)
                            {
                                borderPosition.x = MapExtents;
                            }
                            else
                            {
                                borderPosition.x = MapExtents * -1f;
                            }
                        }
                        else
                        {
                            if (m_RaycastPoint.m_HitPosition.z > 0f)
                            {
                                borderPosition.z = MapExtents;
                            }
                            else
                            {
                                borderPosition.z = MapExtents * -1f;
                            }
                        }

                        TryAddWaterSource(ref inputDeps, borderPosition);
                        m_FindWaterSourcesSystem.Enabled = true;
                        return inputDeps;
                    }
                }
            }
            else if (m_SecondaryApplyAction.WasReleasedThisFrame())
            {
                GetRaycastResult(out m_RaycastPoint);
                if (m_RaycastPoint.m_HitPosition.x != 0f || m_RaycastPoint.m_HitPosition.z != 0f)
                {
                    RemoveWaterSourcesJob removeWaterSourcesJob = new ()
                    {
                        m_SourceType = __TypeHandle.__Game_Simulation_WaterSourceData_RO_ComponentTypeHandle,
                        m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle,
                        m_Position = m_RaycastPoint.m_HitPosition,
                        m_TransformType = __TypeHandle.__Game_Objects_Transform_RO_ComponentTypeHandle,
                        buffer = m_ToolOutputBarrier.CreateCommandBuffer(),
                        m_MapExtents = MapExtents,
                    };
                    JobHandle jobHandle = JobChunkExtensions.Schedule(removeWaterSourcesJob, m_WaterSourcesQuery, Dependency);
                    m_ToolOutputBarrier.AddJobHandleForProducer(jobHandle);
                    inputDeps = jobHandle;
                }
            }

            GetRaycastResult(out m_RaycastPoint);

            m_WaterTooltipSystem.HitPosition = m_RaycastPoint.m_HitPosition;
            WaterSourceCirclesRenderJob waterSourceCirclesRenderJob = new ()
            {
                m_OverlayBuffer = m_OverlayRenderSystem.GetBuffer(out JobHandle outJobHandle),
                m_SourceType = __TypeHandle.__Game_Simulation_WaterSourceData_RO_ComponentTypeHandle,
                m_TransformType = __TypeHandle.__Game_Objects_Transform_RO_ComponentTypeHandle,
                m_TerrainHeightData = m_TerrainSystem.GetHeightData(false),
                m_WaterSurfaceData = m_WaterSystem.GetSurfaceData(out JobHandle waterSurfaceDataJob),
                m_MapExtents = MapExtents,
            };
            inputDeps = JobChunkExtensions.Schedule(waterSourceCirclesRenderJob, m_WaterSourcesQuery, JobHandle.CombineDependencies(inputDeps, outJobHandle, waterSurfaceDataJob));
            m_OverlayRenderSystem.AddBufferWriter(inputDeps);
            m_TerrainSystem.AddCPUHeightReader(inputDeps);
            m_WaterSystem.AddSurfaceReader(inputDeps);

            if (LakeLikeSources.Contains(m_ActivePrefab.m_SourceType))
            {
                float amount = m_WaterToolUISystem.Amount;
                float radius = m_WaterToolUISystem.Radius;
                if (m_ActivePrefab.m_SourceType == WaterToolUISystem.SourceType.RetentionBasin)
                {
                    amount *= 3f; // Needs to be replaced by a UI chosen value;
                }

                float3 lakeProjectedWaterSurfacePosition = m_RaycastPoint.m_HitPosition + new float3(0, amount, 0);
                LakeProjectionJob lakeProjectionJob = new ()
                {
                    m_OverlayBuffer = m_OverlayRenderSystem.GetBuffer(out JobHandle outputJobHandle),
                    m_Position = lakeProjectedWaterSurfacePosition,
                    m_Radius = radius,
                };
                JobHandle jobHandle = IJobExtensions.Schedule(lakeProjectionJob, outputJobHandle);
                m_OverlayRenderSystem.AddBufferWriter(jobHandle);
                inputDeps = JobHandle.CombineDependencies(jobHandle, inputDeps);
            }

            if (m_HoveredWaterSources.IsEmpty)
            {
                float radius = m_WaterToolUISystem.Radius;
                WaterToolRadiusJob waterToolRadiusJob = new ()
                {
                    m_OverlayBuffer = m_OverlayRenderSystem.GetBuffer(out JobHandle outJobHandle2),
                    m_Position = m_RaycastPoint.m_HitPosition,
                    m_Radius = radius,
                };
                JobHandle jobHandle = IJobExtensions.Schedule(waterToolRadiusJob, outJobHandle2);
                m_OverlayRenderSystem.AddBufferWriter(jobHandle);
                inputDeps = JobHandle.CombineDependencies(jobHandle, inputDeps);
            }

            m_HoveredWaterSources.Clear();
            HoverOverWaterSourceJob hoverOverWaterSourceJob = new ()
            {
                m_SourceType = __TypeHandle.__Game_Simulation_WaterSourceData_RO_ComponentTypeHandle,
                m_TransformType = __TypeHandle.__Game_Objects_Transform_RO_ComponentTypeHandle,
                m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle,
                m_Position = m_RaycastPoint.m_HitPosition,
                m_Entities = m_HoveredWaterSources,
                m_MapExtents = MapExtents,
            };
            inputDeps = JobChunkExtensions.Schedule(hoverOverWaterSourceJob, m_WaterSourcesQuery, inputDeps);

            return inputDeps;
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            m_HoveredWaterSources.Dispose();
            base.OnDestroy();
        }

        /// <inheritdoc/>
        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            __TypeHandle.AssignHandles(ref CheckedStateRef);
        }

        private void TryAddWaterSource(ref JobHandle deps, float3 position)
        {
            float pollution = 0; // Alter later if UI for adding pollution. Also check to make sure it's smaller than amount later.
            float amount = m_WaterToolUISystem.Amount;
            float radius = m_WaterToolUISystem.Radius;
            int constantDepth = (int)m_ActivePrefab.m_SourceType;
            if (constantDepth >= 4 && constantDepth <= 6)
            {
                constantDepth = 0;
            }

            Game.Simulation.WaterSourceData waterSourceDataComponent = new ()
            {
                m_Amount = amount,
                m_ConstantDepth = constantDepth,
                m_Radius = radius,
                m_Polluted = pollution,
                m_Multiplier = 30f,
            };
            Game.Objects.Transform transformComponent = new ()
            {
                m_Position = new Unity.Mathematics.float3(position.x, position.y, position.z),
                m_Rotation = quaternion.identity,
            };
            bool acceptableMultiplier = true;
            bool unacceptableMultiplier = false;
            if (m_ActivePrefab.m_SourceType != WaterToolUISystem.SourceType.River && m_ActivePrefab.m_SourceType != WaterToolUISystem.SourceType.Sea)
            {
                int attempts = 0;
                waterSourceDataComponent.m_Multiplier = 1f;
                while (waterSourceDataComponent.m_Multiplier == 1f)
                {
                    waterSourceDataComponent.m_Multiplier = WaterSystem.CalculateSourceMultiplier(waterSourceDataComponent, transformComponent.m_Position);
                    attempts++;
                    if (attempts >= 1000f)
                    {
                        acceptableMultiplier = false;
                        break;
                    }

                    if (waterSourceDataComponent.m_Multiplier == 1f)
                    {
                        waterSourceDataComponent.m_Radius++;
                        unacceptableMultiplier = true;
                    }
                }
            }

            if (unacceptableMultiplier == true)
            {
                m_WaterTooltipSystem.RadiusTooSmall = true;
                m_Log.InfoFormat("{WaterToolUpdate.TryAddWaterSource} Radius to small. Increased radius to {0}", waterSourceDataComponent.m_Radius);
            }

            if (acceptableMultiplier)
            {
                if ((int)m_ActivePrefab.m_SourceType <= 3)
                {
                    AddWaterSourceJob addWaterSourceJob = new ()
                    {
                        waterSourceData = waterSourceDataComponent,
                        transform = transformComponent,
                        buffer = m_ToolOutputBarrier.CreateCommandBuffer(),
                        entityArchetype = m_WaterSourceArchetype,
                    };
                    JobHandle jobHandle = IJobExtensions.Schedule(addWaterSourceJob, Dependency);
                    m_ToolOutputBarrier.AddJobHandleForProducer(jobHandle);
                    deps = jobHandle;
                }
                else if (m_ActivePrefab.m_SourceType == WaterToolUISystem.SourceType.AutofillingLake)
                {
                    AddAutoFillingLakeJob addAutoFillingLakeJob = new ()
                    {
                        autoFillingLakeData = new AutofillingLake() { m_MaximumWaterHeight = amount },
                        entityArchetype = m_AutoFillingLakeArchetype,
                        buffer = m_ToolOutputBarrier.CreateCommandBuffer(),
                        transform = transformComponent,
                        waterSourceData = waterSourceDataComponent,
                    };

                    JobHandle jobHandle = IJobExtensions.Schedule(addAutoFillingLakeJob, Dependency);
                    m_ToolOutputBarrier.AddJobHandleForProducer(jobHandle);
                    deps = jobHandle;
                }
                else if (m_ActivePrefab.m_SourceType == WaterToolUISystem.SourceType.DetentionBasin)
                {
                    waterSourceDataComponent.m_Amount = 0f;
                    AddDetentionBasinJob addDetentionBasinJob = new ()
                    {
                        detentionBasinData = new DetentionBasin() { m_MaximumWaterHeight = amount },
                        entityArchetype = m_DetentionBasinArchetype,
                        buffer = m_ToolOutputBarrier.CreateCommandBuffer(),
                        transform = transformComponent,
                        waterSourceData = waterSourceDataComponent,
                    };
                    JobHandle jobHandle = IJobExtensions.Schedule(addDetentionBasinJob, Dependency);
                    m_ToolOutputBarrier.AddJobHandleForProducer(jobHandle);
                    deps = jobHandle;
                }
                else if (m_ActivePrefab.m_SourceType == WaterToolUISystem.SourceType.RetentionBasin)
                {
                    AddRetentionBasinJob addRetentionBasinJob = new ()
                    {
                        retentionBasinData = new RetentionBasin() { m_MaximumWaterHeight = amount * 3f, m_MinimumWaterHeight = amount }, // UI needed for retention basin max WSE.
                        entityArchetype = m_RetentionBasinArchetype,
                        buffer = m_ToolOutputBarrier.CreateCommandBuffer(),
                        transform = transformComponent,
                        waterSourceData = waterSourceDataComponent,
                    };
                    JobHandle jobHandle = IJobExtensions.Schedule(addRetentionBasinJob, Dependency);
                    m_ToolOutputBarrier.AddJobHandleForProducer(jobHandle);
                    deps = jobHandle;
                }
            }
            else
            {
                m_Log.Warn("{WaterToolUpdate.TryAddWaterSource} After 1000 attempts couldn't produce acceptable water source!");
            }
        }

        private struct RemoveWaterSourcesJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public ComponentTypeHandle<Game.Simulation.WaterSourceData> m_SourceType;
            [ReadOnly]
            public ComponentTypeHandle<Game.Objects.Transform> m_TransformType;
            public float3 m_Position;
            public EntityCommandBuffer buffer;
            public float m_MapExtents;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<Game.Simulation.WaterSourceData> waterSourceDataNativeArray = chunk.GetNativeArray(ref m_SourceType);
                NativeArray<Game.Objects.Transform> transformNativeArray = chunk.GetNativeArray(ref m_TransformType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];
                    Game.Simulation.WaterSourceData currentWaterSourceData = waterSourceDataNativeArray[i];
                    Game.Objects.Transform currentTransform = transformNativeArray[i];
                    m_Position.y = 0;
                    currentTransform.m_Position.y = 0;
                    if (WaterSourceUtils.CheckForHoveringOverWaterSource(m_Position, currentTransform.m_Position, currentWaterSourceData.m_Radius, m_MapExtents))
                    {
                        buffer.DestroyEntity(currentEntity);
                    }
                }
            }
        }

        private struct AddWaterSourceJob : IJob
        {
            public Game.Simulation.WaterSourceData waterSourceData;
            public Game.Objects.Transform transform;
            public EntityCommandBuffer buffer;
            public EntityArchetype entityArchetype;

            public void Execute()
            {
                Entity currentEntity = buffer.CreateEntity(entityArchetype);
                buffer.SetComponent(currentEntity, waterSourceData);
                buffer.SetComponent(currentEntity, transform);
                buffer.AddComponent<Updated>(currentEntity);
            }
        }

        private struct AddAutoFillingLakeJob : IJob
        {
            public Game.Simulation.WaterSourceData waterSourceData;
            public Game.Objects.Transform transform;
            public AutofillingLake autoFillingLakeData;
            public EntityCommandBuffer buffer;
            public EntityArchetype entityArchetype;

            public void Execute()
            {
                Entity currentEntity = buffer.CreateEntity(entityArchetype);
                buffer.SetComponent(currentEntity, waterSourceData);
                buffer.SetComponent(currentEntity, transform);
                buffer.SetComponent(currentEntity, autoFillingLakeData);
                buffer.AddComponent<Updated>(currentEntity);
            }
        }

        private struct AddDetentionBasinJob : IJob
        {
            public Game.Simulation.WaterSourceData waterSourceData;
            public Game.Objects.Transform transform;
            public DetentionBasin detentionBasinData;
            public EntityCommandBuffer buffer;
            public EntityArchetype entityArchetype;

            public void Execute()
            {
                Entity currentEntity = buffer.CreateEntity(entityArchetype);
                buffer.SetComponent(currentEntity, waterSourceData);
                buffer.SetComponent(currentEntity, transform);
                buffer.SetComponent(currentEntity, detentionBasinData);
                buffer.AddComponent<Updated>(currentEntity);
            }
        }

        private struct AddRetentionBasinJob : IJob
        {
            public Game.Simulation.WaterSourceData waterSourceData;
            public Game.Objects.Transform transform;
            public RetentionBasin retentionBasinData;
            public EntityCommandBuffer buffer;
            public EntityArchetype entityArchetype;

            public void Execute()
            {
                Entity currentEntity = buffer.CreateEntity(entityArchetype);
                buffer.SetComponent(currentEntity, waterSourceData);
                buffer.SetComponent(currentEntity, transform);
                buffer.SetComponent(currentEntity, retentionBasinData);
                buffer.AddComponent<Updated>(currentEntity);
            }
        }

        private struct WaterSourceCirclesRenderJob : IJobChunk
        {
            public OverlayRenderSystem.Buffer m_OverlayBuffer;
            [ReadOnly]
            public ComponentTypeHandle<Game.Objects.Transform> m_TransformType;
            [ReadOnly]
            public ComponentTypeHandle<Game.Simulation.WaterSourceData> m_SourceType;
            public TerrainHeightData m_TerrainHeightData;
            public WaterSurfaceData m_WaterSurfaceData;
            public float m_MapExtents;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Game.Simulation.WaterSourceData> waterSourceDataNativeArray = chunk.GetNativeArray(ref m_SourceType);
                NativeArray<Game.Objects.Transform> transformNativeArray = chunk.GetNativeArray(ref m_TransformType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Game.Simulation.WaterSourceData currentWaterSourceData = waterSourceDataNativeArray[i];
                    Game.Objects.Transform currentTransform = transformNativeArray[i];
                    float3 terrainPosition = new (currentTransform.m_Position.x, TerrainUtils.SampleHeight(ref m_TerrainHeightData, currentTransform.m_Position), currentTransform.m_Position.z);
                    float3 waterPosition = new (currentTransform.m_Position.x, WaterUtils.SampleHeight(ref m_WaterSurfaceData, ref m_TerrainHeightData, currentTransform.m_Position), currentTransform.m_Position.z);
                    float3 position = terrainPosition;
                    if (waterPosition.y > terrainPosition.y)
                    {
                        position = waterPosition;
                    }

                    float radius = WaterSourceUtils.GetRadius(currentTransform.m_Position, currentWaterSourceData.m_Radius, m_MapExtents);
                    if (radius > currentWaterSourceData.m_Radius)
                    {
                        m_OverlayBuffer.DrawCircle(new UnityEngine.Color(0.95f, 0.44f, 0.13f, 1f), default, currentWaterSourceData.m_Radius / 20f, 0, new float2(0, 1), position, currentWaterSourceData.m_Radius * 2f);
                        m_OverlayBuffer.DrawCircle(UnityEngine.Color.yellow, default, radius / 20f, 0, new float2(0, 1), position, radius * 2.05f);
                    }
                    else
                    {
                        m_OverlayBuffer.DrawCircle(UnityEngine.Color.yellow, default, radius / 20f, 0, new float2(0, 1), position, radius * 2f);
                        m_OverlayBuffer.DrawCircle(new UnityEngine.Color(0.95f, 0.44f, 0.13f, 1f), default, currentWaterSourceData.m_Radius / 20f, 0, new float2(0, 1), position, currentWaterSourceData.m_Radius * 2.05f);
                    }
                }
            }
        }

        private struct WaterToolRadiusJob : IJob
        {
            public OverlayRenderSystem.Buffer m_OverlayBuffer;
            public float3 m_Position;
            public float m_Radius;

            public void Execute()
            {
                m_OverlayBuffer.DrawCircle(new UnityEngine.Color(0.95f, 0.44f, 0.13f, 1f), default, m_Radius / 20f, 0, new float2(0, 1), m_Position, m_Radius * 2f);
            }
        }

        private struct LakeProjectionJob : IJob
        {
            public OverlayRenderSystem.Buffer m_OverlayBuffer;
            public float3 m_Position;
            public float m_Radius;

            public void Execute()
            {
                m_OverlayBuffer.DrawCircle(new UnityEngine.Color(0f, 0f, 1f, 0.375f), m_Position, m_Radius * 6f);
            }
        }

        private struct HoverOverWaterSourceJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public ComponentTypeHandle<Game.Simulation.WaterSourceData> m_SourceType;
            [ReadOnly]
            public ComponentTypeHandle<Game.Objects.Transform> m_TransformType;
            public float3 m_Position;
            public NativeList<Entity> m_Entities;
            public float m_MapExtents;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<Game.Simulation.WaterSourceData> waterSourceDataNativeArray = chunk.GetNativeArray(ref m_SourceType);
                NativeArray<Game.Objects.Transform> transformNativeArray = chunk.GetNativeArray(ref m_TransformType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];
                    Game.Simulation.WaterSourceData currentWaterSourceData = waterSourceDataNativeArray[i];
                    Game.Objects.Transform currentTransform = transformNativeArray[i];
                    m_Position.y = 0;
                    currentTransform.m_Position.y = 0;
                    if (WaterSourceUtils.CheckForHoveringOverWaterSource(m_Position, currentTransform.m_Position, currentWaterSourceData.m_Radius, m_MapExtents))
                    {
                        m_Entities.Add(in currentEntity);
                    }
                }
            }
        }

        private struct TypeHandle
        {
            [ReadOnly]
            public ComponentTypeHandle<Game.Simulation.WaterSourceData> __Game_Simulation_WaterSourceData_RO_ComponentTypeHandle;
            [ReadOnly]
            public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<Game.Objects.Transform> __Game_Objects_Transform_RO_ComponentTypeHandle;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AssignHandles(ref SystemState state)
            {
                __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                __Game_Simulation_WaterSourceData_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Simulation.WaterSourceData>();
                __Game_Objects_Transform_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Objects.Transform>();
            }
        }
    }
}
