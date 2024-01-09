// <copyright file="AddPrefabsSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Systems
{
    using System.Collections.Generic;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Prefabs;
    using Unity.Entities;
    using UnityEngine;
    using Water_Features.Prefabs;
    using static Water_Features.Tools.WaterToolUISystem;

    /// <summary>
    /// System for adding new prefabs for custom water sources.
    /// </summary>
    public class AddPrefabsSystem : GameSystemBase
    {
        private const string PrefabPrefix = "WaterSource ";
        private const string TabName = "Water Tool";
        private readonly List<WaterSourcePrefabData> m_SourcePrefabDataList = new List<WaterSourcePrefabData>()
        {
            { new WaterSourcePrefabData { m_SourceType = SourceType.Creek, m_Icon = "coui://yy-water-tool/WaterSourceCreek.svg", m_AmountLocalKey = "yyWaterTool.Flow", m_Priority = 10, m_DefaultRadius = 30f, m_DefaultAmount = 20f, } },
            { new WaterSourcePrefabData { m_SourceType = SourceType.Lake, m_Icon = "coui://yy-water-tool/WaterSourceLake.svg", m_AmountLocalKey = "yyWaterTool.Depth", m_Priority = 60, m_DefaultRadius = 40f, m_DefaultAmount = 30f, } },
            { new WaterSourcePrefabData { m_SourceType = SourceType.River, m_Icon = "coui://yy-water-tool/WaterSourceRiver.svg", m_AmountLocalKey = "yyWaterTool.Depth", m_Priority = 20, m_DefaultRadius = 50f, m_DefaultAmount = 40f, } },
            { new WaterSourcePrefabData { m_SourceType = SourceType.Sea, m_Icon = "coui://yy-water-tool/WaterSourceSea.svg", m_AmountLocalKey = "yyWaterTool.Depth", m_Priority = 70, m_DefaultRadius = 5000f, m_DefaultAmount = 50f, } },
            { new WaterSourcePrefabData { m_SourceType = SourceType.DetentionBasin, m_Icon = "coui://yy-water-tool/WaterSourceDetentionBasin.svg", m_AmountLocalKey = "yyWaterTool.MaxDepth", m_Priority = 30, m_DefaultRadius = 40f, m_DefaultAmount = 30f, } },
            { new WaterSourcePrefabData { m_SourceType = SourceType.RetentionBasin, m_Icon = "coui://yy-water-tool/WaterSourceRetentionBasin.svg", m_AmountLocalKey = "yyWaterTool.MaxDepth", m_Priority = 40, m_DefaultRadius = 50f, m_DefaultAmount = 10f, } },
            { new WaterSourcePrefabData { m_SourceType = SourceType.AutofillingLake, m_Icon = "coui://yy-water-tool/WaterSourceAutomaticFill.svg", m_AmountLocalKey = "yyWaterTool.Depth", m_Priority = 50, m_DefaultRadius = 40f, m_DefaultAmount = 30f, } },
        };

        private PrefabSystem m_PrefabSystem;
        private ILog m_Log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddPrefabsSystem"/> class.
        /// </summary>
        public AddPrefabsSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = WaterFeaturesMod.Instance.Log;
            m_PrefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            foreach (WaterSourcePrefabData source in m_SourcePrefabDataList)
            {
                WaterSourcePrefab sourcePrefabBase = ScriptableObject.CreateInstance<WaterSourcePrefab>();
                sourcePrefabBase.active = true;
                sourcePrefabBase.m_SourceType = source.m_SourceType;
                sourcePrefabBase.m_DefaultRadius = source.m_DefaultRadius;
                sourcePrefabBase.m_DefaultAmount = source.m_DefaultAmount;
                sourcePrefabBase.name = $"{PrefabPrefix}{source.m_SourceType}";
                UIObject uiObject = ScriptableObject.CreateInstance<UIObject>();
                uiObject.m_Group = GetOrCreateNewToolCategory(TabName, "Landscaping", "coui://yy-water-tool/water_features_icon.svg") ?? uiObject.m_Group;
                uiObject.m_Priority = source.m_Priority;
                uiObject.m_Icon = source.m_Icon;
                uiObject.active = true;
                uiObject.m_IsDebugObject = false;
                sourcePrefabBase.AddComponentFrom(uiObject);
                if (m_PrefabSystem.AddPrefab(sourcePrefabBase))
                {
                    m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnUpdate)} Added prefab for Water Source {source.m_SourceType}");
                }
            }

            Enabled = false;
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            // For some reason these components are not assigned automatically from the ones in the prefab so I am manually assigning them to the prefab entities.

            if (m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetCategoryPrefab), TabName), out var waterToolTabPrefab) || waterToolTabPrefab is UIAssetCategoryPrefab)
            {
                if (!m_PrefabSystem.TryGetEntity(waterToolTabPrefab, out Entity waterToolTabPrefabEntity))
                {
                    return;
                }

                if (!EntityManager.TryGetComponent(waterToolTabPrefabEntity, out UIAssetCategoryData currentMenu))
                {
                    return;
                }

                if (!EntityManager.TryGetComponent(waterToolTabPrefabEntity, out UIObjectData objectData))
                {
                    return;
                }

                if (currentMenu.m_Menu == Entity.Null && m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetMenuPrefab), "Landscaping"), out PrefabBase landscapeTabPrefab))
                {
                    Entity landscapeTabEntity = m_PrefabSystem.GetEntity(landscapeTabPrefab);
                    m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnGameLoadingComplete)} currentMenu = Entity.Null so set to {landscapeTabEntity.Index}.{landscapeTabEntity.Version}");
                    currentMenu.m_Menu = landscapeTabEntity;
                    objectData.m_Priority = 21;
                    EntityManager.SetComponentData(waterToolTabPrefabEntity, currentMenu);
                    EntityManager.SetComponentData(waterToolTabPrefabEntity, objectData);
                    if (!EntityManager.TryGetBuffer(landscapeTabEntity, false, out DynamicBuffer<UIGroupElement> uiGroupBuffer))
                    {
                        m_Log.Warn("Couldn't find UIGroupElement buffer in landscapeTabEntity.");
                        return;
                    }

                    UIGroupElement groupElement = new UIGroupElement()
                    {
                        m_Prefab = waterToolTabPrefabEntity,
                    };
                    uiGroupBuffer.Add(groupElement);
                }
            }

            foreach (WaterSourcePrefabData source in m_SourcePrefabDataList)
            {
                if (m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(WaterSourcePrefab), $"{PrefabPrefix}{source.m_SourceType}"), out var waterSourcePrefab) || waterSourcePrefab is WaterSourcePrefab)
                {
                    if (!m_PrefabSystem.TryGetEntity(waterSourcePrefab, out Entity waterSourcePrefabEntity))
                    {
                        continue;
                    }

                    if (!EntityManager.TryGetComponent(waterSourcePrefabEntity, out UIObjectData uIObjectData))
                    {
                        continue;
                    }

                    if (uIObjectData.m_Group == Entity.Null && m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetCategoryPrefab), TabName), out PrefabBase prefab1))
                    {
                        Entity waterToolTabPrefabEntity = m_PrefabSystem.GetEntity(prefab1);
                        if (!EntityManager.TryGetBuffer(waterToolTabPrefabEntity, false, out DynamicBuffer<UIGroupElement> uiGroupBuffer))
                        {
                            m_Log.Warn($"Couldn't find UIGroupElement buffer in waterToolTabPrefabEntity for {source.m_SourceType}");
                            continue;
                        }

                        m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnGameLoadingComplete)} uIObjectData.m_Group = Entity.Null so set to {waterToolTabPrefabEntity.Index}.{waterToolTabPrefabEntity.Version}");
                        uIObjectData.m_Group = waterToolTabPrefabEntity;
                        uIObjectData.m_Priority = source.m_Priority;
                        EntityManager.SetComponentData(waterSourcePrefabEntity, uIObjectData);
                        UIGroupElement groupElement = new UIGroupElement()
                        {
                            m_Prefab = waterSourcePrefabEntity,
                        };
                        uiGroupBuffer.Insert(uiGroupBuffer.Length, groupElement);
                    }
                }
            }
        }

        private UIAssetCategoryPrefab GetOrCreateNewToolCategory(string name, string menuName, string path)
        {
            if (m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetCategoryPrefab), name), out var prefab) || prefab is UIAssetCategoryPrefab)
            {
                return (UIAssetCategoryPrefab)prefab;
            }

            if (!m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetMenuPrefab), menuName), out var prefabBase)
            || prefabBase is not UIAssetMenuPrefab menu)
            {
                // it can happen that the menu isn't added yet, as is the case on the first run through
                return null;
            }

            UIAssetCategoryPrefab newCategory = ScriptableObject.CreateInstance<UIAssetCategoryPrefab>();
            newCategory.name = name;
            newCategory.m_Menu = menu;
            UIObject uiObjectComponent = ScriptableObject.CreateInstance<UIObject>();
            uiObjectComponent.m_Icon = path;
            uiObjectComponent.m_Priority = 21;
            uiObjectComponent.active = true;
            uiObjectComponent.m_IsDebugObject = false;
            newCategory.AddComponentFrom(uiObjectComponent);

            if (m_PrefabSystem.AddPrefab(newCategory))
            {
                m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnUpdate)} Added prefab for Category {name}");
            }

            return newCategory;
        }

        private struct WaterSourcePrefabData
        {
            public SourceType m_SourceType;
            public string m_Icon;
            public string m_AmountLocalKey;
            public int m_Priority;
            public float m_DefaultRadius;
            public float m_DefaultAmount;
        }
    }
}
