// <copyright file="AddPrefabsSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Systems
{
    using Game;
    using Game.Prefabs;
    using Game.UI.InGame;
    using Unity.Entities;
    using UnityEngine;
    using Water_Features.Components;
    using Water_Features.Tools;

    /// <summary>
    /// System for adding new prefabs for custom water sources.
    /// </summary>
    public class AddPrefabsSystem : GameSystemBase
    {
        private PrefabSystem m_PrefabSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddPrefabsSystem"/> class.
        /// </summary>
        public AddPrefabsSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_PrefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            MarkerObjectPrefab markerObjectPrefab = new MarkerObjectPrefab();
        }

        private UIAssetCategoryPrefab GetOrCreateNewToolCategory(string name)
        {
            if (m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetCategoryPrefab), name), out var prefab))
            {
                if (prefab is UIAssetCategoryPrefab)
                {
                    return (UIAssetCategoryPrefab)prefab;
                }

                if (!m_PrefabSystem.TryGetPrefab(new PrefabID(nameof(UIAssetMenuPrefab), "Landscaping"), out var p2)
                || p2 is not UIAssetMenuPrefab landscapingMenu)
                {
                    // it can happen that the "landscaping" menu isn't added yet, as is the case on the first run through
                    return null;
                }

                UIAssetCategoryPrefab newCategory = ScriptableObject.CreateInstance<UIAssetCategoryPrefab>();
                newCategory.name = name;
                newCategory.m_Menu = landscapingMenu;
            }
        }
    }
}
