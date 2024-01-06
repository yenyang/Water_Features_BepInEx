﻿// <copyright file="WaterSourcePrefab.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Prefabs
{
    using System;
    using System.Collections.Generic;
    using Game.Prefabs;
    using Game.Simulation;
    using Unity.Entities;
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

        /// <inheritdoc/>
        public override void GetPrefabComponents(HashSet<ComponentType> components)
        {
            WaterFeaturesMod.Instance.Log.Info($"{nameof(WaterSourcePrefab)}.{nameof(GetPrefabComponents)}");
            base.GetPrefabComponents(components);
            components.Add(ComponentType.ReadWrite<Game.Simulation.WaterSourceData>());
            components.Add(ComponentType.ReadWrite<Game.Objects.Transform>());
        }

        /// <inheritdoc/>
        public override void GetArchetypeComponents(HashSet<ComponentType> components)
        {
            WaterFeaturesMod.Instance.Log.Info($"{nameof(WaterSourcePrefab)}.{nameof(GetArchetypeComponents)}");
            base.GetArchetypeComponents(components);
        }
    }
}
