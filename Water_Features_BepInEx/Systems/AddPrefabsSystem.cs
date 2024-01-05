// <copyright file="AddPrefabsSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Systems
{
    using Colossal.Logging;
    using Game;
    using Game.Prefabs;
    using System.Collections.Generic;
    using Unity.Entities;
    using UnityEngine;
    using Water_Features.Prefabs;
    using static Water_Features.Tools.WaterToolUISystem;

    /// <summary>
    /// System for adding new prefabs for custom water sources.
    /// </summary>
    public class AddPrefabsSystem : GameSystemBase
    {
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
                WaterSourcePrefab sourcePrefabBase = new WaterSourcePrefab();
                sourcePrefabBase.m_SourceType = sources.Key;
                sourcePrefabBase.m_Color = Color.red;
                sourcePrefabBase.name = $"WaterSource {sources.Key}";
                UIObject uiObject = sourcePrefabBase.AddComponent<UIObject>();
                uiObject.m_Group = GetOrCreateNewToolCategory("Water Tool", "Landscaping") ?? uiObject.m_Group;
                uiObject.m_Priority = 1;
                uiObject.active = true;
                uiObject.m_IsDebugObject = false;
                if (m_PrefabSystem.AddPrefab(sourcePrefabBase))
                {
                    m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnUpdate)} Added prefab for Water Source {sources.Key}");
                }
            }
        }

        private UIAssetCategoryPrefab GetOrCreateNewToolCategory(string name, string menuName)
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
            uiObjectComponent.m_Icon = "coui//yy-water-tool/water_features_icon.svg";
            uiObjectComponent.m_Priority = 10;
            uiObjectComponent.active = true;
            uiObjectComponent.m_IsDebugObject = false;

            if (m_PrefabSystem.AddPrefab(newCategory))
            {
                m_Log.Info($"{nameof(AddPrefabsSystem)}.{nameof(OnUpdate)} Added prefab for Category {name}");
            }

            return newCategory;

        }
    }
}
