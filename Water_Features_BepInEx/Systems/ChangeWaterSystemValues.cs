// <copyright file="ChangeWaterSystemValues.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Systems
{
    using Colossal.Logging;
    using Game;
    using Game.Simulation;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// Changes the various rates of the vanilla water system.
    /// </summary>
    public partial class ChangeWaterSystemValues : GameSystemBase
    {
        private readonly float m_ResetTimeLimit = 0.005f;
        private readonly float m_TemporaryEvaporation = 0.1f;
        private bool applyNewEvaporationRate = false;
        private TimeSystem m_TimeSystem;
        private WaterSystem m_WaterSystem;
        private float m_TimeLastChanged = 0f;
        private float m_DateLastChange = 0f;
        private ILog m_Log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeWaterSystemValues"/> class.
        /// </summary>
        public ChangeWaterSystemValues()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether to apply a new evaporation rate. Toggled by WaterFeaturesSetting Button.
        /// </summary>
        public bool ApplyNewEvaporationRate { get => applyNewEvaporationRate; set => applyNewEvaporationRate = value; }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = WaterFeaturesMod.Instance.Log;
            m_WaterSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<WaterSystem>();
            m_TimeSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TimeSystem>();
            m_Log.Info($"[{nameof(ChangeWaterSystemValues)}] {nameof(OnCreate)}");
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (ApplyNewEvaporationRate)
            {
                m_WaterSystem.m_Evaporation = m_TemporaryEvaporation;
                m_Log.Info($"[{nameof(ChangeWaterSystemValues)}] {nameof(OnCreate)} changed evaporation rate to {m_TemporaryEvaporation}");
                m_TimeLastChanged = m_TimeSystem.normalizedTime;
                m_DateLastChange = m_TimeSystem.normalizedDate;
                ApplyNewEvaporationRate = false;
                WaterFeaturesMod.Settings.ApplyAndSave();
            }

            if (!Mathf.Approximately(WaterFeaturesMod.Settings.EvaporationRate, m_WaterSystem.m_Evaporation))
            {
                if (m_TimeSystem.normalizedTime > m_TimeLastChanged + m_ResetTimeLimit || m_DateLastChange > m_DateLastChange + m_ResetTimeLimit)
                {
                    m_WaterSystem.m_Evaporation = WaterFeaturesMod.Settings.EvaporationRate;
                    m_Log.Info($"[{nameof(ChangeWaterSystemValues)}] {nameof(OnCreate)} changed evaporation rate back to {WaterFeaturesMod.Settings.EvaporationRate}");
                }
            }

            if (!Mathf.Approximately(m_WaterSystem.m_Damping, WaterFeaturesMod.Settings.Damping))
            {
                m_WaterSystem.m_Damping = WaterFeaturesMod.Settings.Damping;
            }
        }
    }
}