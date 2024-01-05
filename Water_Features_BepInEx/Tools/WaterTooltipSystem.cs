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

    /// <summary>
    /// A system for handing the tooltip for custom water tool.
    /// </summary>
    public partial class WaterTooltipSystem : TooltipSystemBase
    {
        private Vector3 m_HitPosition = new ();
        private bool m_RadiusTooSmall = false;
        private bool m_HoveringOverSource = false;
        private ToolSystem m_ToolSystem;
        private StringTooltip m_Tooltip;
        private CustomWaterToolSystem m_WaterTool;
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

        /// <summary>
        /// Sets a value indicating whether the cursor is hovering over a water source.
        /// </summary>
        public bool HoveringOverSource
        {
            set { m_HoveringOverSource = value; }
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            m_WaterTool = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<CustomWaterToolSystem>();
            m_WaterToolUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<WaterToolUISystem>();
            m_Tooltip = new StringTooltip
            {
                path = "Tooltip.LABEL[YY.WaterTool]",
            };
            m_Log.Info($"[{nameof(WaterTooltipSystem)}] {nameof(OnCreate)}");
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.activeTool != m_WaterTool)
            {
                return;
            }

            m_Tooltip.value = LocalizedString.IdWithFallback("Tooltip.LABEL[YY.WaterTool]", "Water Tool");
            AddMouseTooltip(m_Tooltip);
            if (m_WaterToolUISystem.SelectedSourceType == WaterToolUISystem.SourceType.River)
            {
                if (!m_WaterTool.IsPositionNearBorder(m_HitPosition))
                {
                    StringTooltip mustBePlacedNearMapBorderTooltip = new ()
                    {
                        path = "Tooltip.LABEL[YY.WT.PlaceNearBorder]",
                        value = LocalizedString.IdWithFallback("Tooltip.LABEL[YY.WT.PlaceNearBorder]", "Rivers must be placed near map border."),
                    };
                    AddMouseTooltip(mustBePlacedNearMapBorderTooltip);
                }
            }

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

            if (m_HoveringOverSource)
            {
                StringTooltip removeWaterSourceTooltip = new ()
                {
                    path = "Tooltip.LABEL[YY.WT.RemoveWaterSource]",
                    value = LocalizedString.IdWithFallback("Tooltip.LABEL[YY.WT.RemoveWaterSource]", "Right click to remove water source."),
                };
                AddMouseTooltip(removeWaterSourceTooltip);
            }
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
