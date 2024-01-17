// <copyright file="WaterTooltipSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Tools
{
    using Colossal.Logging;
    using Game.Tools;
    using Game.UI.Localization;
    using Game.UI.Tooltip;
    using Unity.Entities;
    using UnityEngine;
    using Water_Features.Prefabs;

    /// <summary>
    /// A system for handing the tooltip for custom water tool.
    /// </summary>
    public partial class WaterTooltipSystem : TooltipSystemBase
    {
        private Vector3 m_HitPosition = new ();
        private bool m_RadiusTooSmall = false;
        private ToolSystem m_ToolSystem;
        private CustomWaterToolSystem m_CustomWaterTool;
        private float timeLastWarned;
        private ILog m_Log;
        private WaterToolUISystem m_WaterToolUISystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaterTooltipSystem"/> class.
        /// </summary>
        public WaterTooltipSystem()
        {
        }

        /// <summary>
        /// Sets a value indicating the hit position.
        /// </summary>
        public Vector3 HitPosition
        {
            set { m_HitPosition = value; }
        }

        /// <summary>
        /// Sets a value indicating whether the radius is too small.
        /// </summary>
        public bool RadiusTooSmall
        {
            set { m_RadiusTooSmall = value; }
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = WaterFeaturesMod.Instance.Log;
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            m_CustomWaterTool = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<CustomWaterToolSystem>();
            m_WaterToolUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<WaterToolUISystem>();
            m_Log.Info($"[{nameof(WaterTooltipSystem)}] {nameof(OnCreate)}");
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.activeTool != m_CustomWaterTool)
            {
                return;
            }

            var prefab = m_CustomWaterTool.GetPrefab();
            if (prefab == null || prefab is not WaterSourcePrefab)
            {
                return;
            }

            WaterSourcePrefab waterSourcePrefab = prefab as WaterSourcePrefab;

            // Checks position of river and displays tooltip if needed.
            if (waterSourcePrefab.m_SourceType == WaterToolUISystem.SourceType.River)
            {
                if (!m_CustomWaterTool.IsPositionNearBorder(m_HitPosition, m_WaterToolUISystem.Radius, true))
                {
                    StringTooltip mustBePlacedNearMapBorderTooltip = new ()
                    {
                        path = "Tooltip.LABEL[YY.WT.PlaceNearBorder]",
                        value = LocalizedString.IdWithFallback("Tooltip.LABEL[YY.WT.PlaceNearBorder]", "Rivers must be placed near map border."),
                    };
                    AddMouseTooltip(mustBePlacedNearMapBorderTooltip);
                }
            }

            // Checks position of sea and displays tooltip if needed.
            else if (waterSourcePrefab.m_SourceType == WaterToolUISystem.SourceType.Sea)
            {
                if (!m_CustomWaterTool.IsPositionNearBorder(m_HitPosition, m_WaterToolUISystem.Radius, false))
                {
                    StringTooltip mustTouchBorderTooltip = new ()
                    {
                        path = "Tooltip.LABEL[YY.WT.MustTouchBorder]",
                        value = LocalizedString.IdWithFallback("Tooltip.LABEL[YY.WT.MustTouchBorder]", "Sea water sources must touch the map border."),
                    };
                    AddMouseTooltip(mustTouchBorderTooltip);
                }
            }

            // Checks position of water sources placed in playable area and displays tooltip if needed.
            else
            {
                if (!m_CustomWaterTool.IsPositionWithinBorder(m_HitPosition))
                {
                    StringTooltip mustBePlacedInsideBorderTooltip = new ()
                    {
                        path = "Tooltip.LABEL[YY.WT.PlaceInsideBorder]",
                        value = LocalizedString.IdWithFallback("Tooltip.LABEL[YY.WT.PlaceInsideBorder]", "This water source must be placed inside the playable map."),
                    };
                    AddMouseTooltip(mustBePlacedInsideBorderTooltip);
                }
            }

            // If Radius is too small displays a tooltip. UI should prevent this from happening.
            if (m_RadiusTooSmall || UnityEngine.Time.time < timeLastWarned + 1f)
            {
                StringTooltip radiusTooSmallTooltip = new ()
                {
                    path = "Tooltip.LABEL[YY.WT.RadiusTooSmall]",
                    value = LocalizedString.IdWithFallback("Tooltip.LABEL[YY.WT.RadiusTooSmall]", "The radius is too small and has been automically increased."),
                };
                AddMouseTooltip(radiusTooSmallTooltip);
                m_RadiusTooSmall = false;
                timeLastWarned = UnityEngine.Time.time;
            }

            // Displays a tooltip while hovering over a water source.
            if (m_CustomWaterTool.CanDeleteWaterSource())
            {
                StringTooltip removeWaterSourceTooltip = new ()
                {
                    path = "Tooltip.LABEL[YY.WT.RemoveWaterSource]",
                    value = LocalizedString.IdWithFallback("Tooltip.LABEL[YY.WT.RemoveWaterSource]", "Right click to remove water source."),
                };
                AddMouseTooltip(removeWaterSourceTooltip);
            }

            // Informs the player if they can set the elevation by right clicking.
            else if (waterSourcePrefab.m_SourceType != WaterToolUISystem.SourceType.Stream && !m_WaterToolUISystem.AmountIsAnElevation)
            {
                StringTooltip lockElevationTooltip = new ()
                {
                    path = "Tooltip.LABEL[YY.WT.LockElevation]",
                    value = LocalizedString.IdWithFallback("Tooltip.LABEL[YY.WT.LockElevation]", "Right click to designate the water surface elevation."),
                };
                AddMouseTooltip(lockElevationTooltip);
            }
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
