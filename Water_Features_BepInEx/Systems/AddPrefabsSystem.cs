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
        private readonly Dictionary<SourceType, string> m_SourceTypeIcons = new Dictionary<SourceType, string>()
        {
            { SourceType.Creek, "coui://yy-water-tool/WaterSourceCreek.svg" },
            { SourceType.Lake, "coui://yy-water-tool/WaterSourceLake.svg" },
            { SourceType.River, "coui://yy-water-tool/WaterSourceRiver.svg" },
            { SourceType.Sea, "coui://yy-water-tool/WaterSourceSea.svg" },
            { SourceType.AutofillingLake, "coui://yy-water-tool/Colored/AutomaticFill.svg" },
            { SourceType.DetentionBasin, "coui://yy-water-tool/WaterSourceDetentionBasin.svg" },
            { SourceType.RetentionBasin, "coui://yy-water-tool/WaterSourceRetentionBasin.svg" },
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
            foreach (KeyValuePair<SourceType, string> sources in m_SourceTypeIcons)
            {
                WaterSourcePrefab sourcePrefabBase = ScriptableObject.CreateInstance<WaterSourcePrefab>();
                sourcePrefabBase.active = true;
                sourcePrefabBase.m_SourceType = sources.Key;
                sourcePrefabBase.m_Color = Color.red;
                sourcePrefabBase.name = $"{PrefabPrefix}{sources.Key}";
                UIObject uiObject = ScriptableObject.CreateInstance<UIObject>();
                uiObject.m_Group = GetOrCreateNewToolCategory(TabName, "Landscaping", "coui://yy-water-tool/water_features_icon.svg") ?? uiObject.m_Group;
                uiObject.m_Priority = (int)sources.Key * 10;
                uiObject.m_Icon = sources.Value;
                uiObject.active = true;
                uiObject.m_IsDebugObject = false;
                sourcePrefabBase.AddComponentFrom(uiObject);
                if (m_PrefabSystem.AddPrefab(sourcePrefabBase))
                {
                    m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnUpdate)} Added prefab for Water Source {sources.Key}");
                }
            }

            Enabled = false;
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

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
                    objectData.m_Priority = 12;
                    EntityManager.SetComponentData(waterToolTabPrefabEntity, currentMenu);
                    EntityManager.SetComponentData(waterToolTabPrefabEntity, objectData);
                    if (!EntityManager.TryGetBuffer(landscapeTabEntity, false, out DynamicBuffer<UIGroupElement> uiGroupBuffer))
                    {
                        m_Log.Info("Couldn't find buffer");
                    }

                    UIGroupElement groupElement = new UIGroupElement()
                    {
                        m_Prefab = waterToolTabPrefabEntity,
                    };
                    uiGroupBuffer.Add(groupElement);
                }
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

                    if (uIObjectData.m_Group == Entity.Null && m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetCategoryPrefab), TabName), out PrefabBase prefab1))
                    {
                        Entity waterToolTabPrefabEntity = m_PrefabSystem.GetEntity(prefab1);
                        if (!EntityManager.TryGetBuffer(waterToolTabPrefabEntity, false, out DynamicBuffer<UIGroupElement> uiGroupBuffer))
                        {
                            m_Log.Info("Couldn't find buffer");
                        }

                        m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnGameLoadingComplete)} uIObjectData.m_Group = Entity.Null so set to {waterToolTabPrefabEntity.Index}.{waterToolTabPrefabEntity.Version}");
                        uIObjectData.m_Group = waterToolTabPrefabEntity;
                        uIObjectData.m_Priority = (int)sources.Key * 10;
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
            uiObjectComponent.m_Priority = 12;
            uiObjectComponent.active = true;
            uiObjectComponent.m_IsDebugObject = false;
            newCategory.AddComponentFrom(uiObjectComponent);

            if (m_PrefabSystem.AddPrefab(newCategory))
            {
                m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnUpdate)} Added prefab for Category {name}");
            }

            return newCategory;
        }
    }
}
