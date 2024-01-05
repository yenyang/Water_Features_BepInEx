// <copyright file="WaterSourcePrefab.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Prefabs
{
    using System;
    using Game.Prefabs;
    using UnityEngine;
    using Water_Features.Tools;

    /// <summary>
    /// A custom prefab for water sources to put in the menu.
    /// </summary>
    [ComponentMenu("Tools/", new Type[] { })]
    public class WaterSourcePrefab : PrefabBase
    {
        /// <summary>
        /// Type of water source.
        /// </summary>
        public WaterToolUISystem.SourceType m_SourceType;

        /// <summary>
        /// Color for overlay rendering.
        /// </summary>
        public Color m_Color;
    }
}
