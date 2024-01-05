// <copyright file="AddPrefabsSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Systems
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Citizens;
    using Game.Net;
    using Game.Prefabs;
    using System.Collections.Generic;
    using Unity.Entities;
    using UnityEngine;
    using Water_Features.Prefabs;
    using static Game.Objects.SubObjectSystem;
    using static Water_Features.Tools.WaterToolUISystem;

    /// <summary>
    /// System for adding new prefabs for custom water sources.
    /// </summary>
    public class AddPrefabsSystem : GameSystemBase
    {
        private const string PrefabPrefix = "WaterSource ";
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
                sourcePrefabBase.m_SourceType = sources.Key;
                sourcePrefabBase.m_Color = Color.red;
                sourcePrefabBase.name = $"{PrefabPrefix}{sources.Key}";
                UIObject uiObject = sourcePrefabBase.AddComponent<UIObject>();
                uiObject.m_Group = GetOrCreateNewToolCategory("Water Tool", "Landscaping", "coui://yy-water-tool/water_features_icon.svg") ?? uiObject.m_Group;
                uiObject.m_Priority = 1;
                uiObject.m_Icon = sources.Value;
                uiObject.active = true;
                uiObject.m_IsDebugObject = false;
                if (m_PrefabSystem.AddPrefab(sourcePrefabBase))
                {
                    m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnUpdate)} Added prefab for Water Source {sources.Key}");
                }

                if (m_PrefabSystem.TryGetEntity(sourcePrefabBase, out Entity e))
                {
                    m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnUpdate)} prefabEntity = {e.Index}.{e.Version}");

                    if (EntityManager.TryGetComponent(e, out UIObjectData uIObjectData))
                    {
                        if (uIObjectData.m_Group == Entity.Null && m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetMenuPrefab), "Landscaping"), out PrefabBase prefab1))
                        {
                            Entity entity = m_PrefabSystem.GetEntity(prefab1);
                            m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnUpdate)} uIObjectData.m_Group = Entity.Null so set to {entity.Index}.{entity.Version}");
                            uIObjectData.m_Group = entity;
                        }
                    }
                }
            }

            Enabled = false;
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetCategoryData), "Water Tool"), out var waterToolTabPrefab) || waterToolTabPrefab is UIAssetCategoryPrefab)
            {
                if (!m_PrefabSystem.TryGetEntity(waterToolTabPrefab, out Entity waterToolTabPrefabEntity))
                {
                    return;
                }

                if (!EntityManager.TryGetComponent(waterToolTabPrefabEntity, out UIAssetCategoryData currentMenu))
                {
                    return;
                }

                if (currentMenu.m_Menu == Entity.Null && m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetMenuPrefab), "Landscaping"), out PrefabBase prefab1))
                {
                    Entity entity = m_PrefabSystem.GetEntity(prefab1);
                    m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnGameLoadingComplete)} currentMenu = Entity.Null so set to {entity.Index}.{entity.Version}");
                    currentMenu.m_Menu = entity;
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

                    if (uIObjectData.m_Group == Entity.Null && m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetMenuPrefab), "Landscaping"), out PrefabBase prefab1))
                    {
                        Entity entity = m_PrefabSystem.GetEntity(prefab1);
                        m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnGameLoadingComplete)} uIObjectData.m_Group = Entity.Null so set to {entity.Index}.{entity.Version}");
                        uIObjectData.m_Group = entity;
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
            UIObject uiObjectComponent = newCategory.AddComponent<UIObject>();
            uiObjectComponent.m_Icon = path;
            uiObjectComponent.m_Priority = 10;
            uiObjectComponent.active = true;
            uiObjectComponent.m_IsDebugObject = false;

            if (m_PrefabSystem.AddPrefab(newCategory))
            {
                m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnUpdate)} Added prefab for Category {name}");
            }

            if (m_PrefabSystem.TryGetEntity(newCategory, out Entity e))
            {
                m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnUpdate)} prefabEntity = {e.Index}.{e.Version}");

                if (EntityManager.TryGetComponent(e, out UIAssetCategoryData currentMenu))
                {
                    if (currentMenu.m_Menu == Entity.Null && m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetMenuPrefab), menuName), out PrefabBase prefab1))
                    {
                        Entity entity = m_PrefabSystem.GetEntity(prefab1);
                        m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnUpdate)} currentMenu = Entity.Null so set to {entity.Index}.{entity.Version}");
                        currentMenu.m_Menu = entity;
                    }
                }
            }

            return newCategory;
        }
    }
}
