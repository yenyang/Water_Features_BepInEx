// <copyright file="WaterFeaturesSettings.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Settings
{
    using Colossal.IO.AssetDatabase;
    using Game.Modding;
    using Game.Settings;
    using Unity.Entities;
    using Water_Features.Systems;

    /// <summary>
    /// The mod settings for the Water Features Mod.
    /// </summary>
    [FileLocation("Mods_Yenyang_Water_Features")]
    [SettingsUISection(General, Experimental)]
    [SettingsUIShowGroupName(SeasonalStreams, EvaporationRateGroup, WavesAndTides)]
    public class WaterFeaturesSettings : ModSetting
    {
        /// <summary>
        /// This is for settings not flagged as experimental.
        /// </summary>
        public const string General = "General";

        /// <summary>
        /// This is for experimental settings that should be used with caution.
        /// </summary>
        public const string Experimental = "Experimental";

        /// <summary>
        /// This is for settings that affect the UI for the mod.
        /// </summary>
        public const string SeasonalStreams = "Seasonal Streams";

        /// <summary>
        /// This is for options related to unique buildings.
        /// </summary>
        public const string EvaporationRateGroup = "Evaporation Rate";

        /// <summary>
        /// This is for options related to prop culling.
        /// </summary>
        public const string WavesAndTides = "Waves and Tides";

        /// <summary>
        /// This is for the reset button(s).
        /// </summary>
        public const string Reset = "Reset";

        private ChangeWaterSystemValues m_ChangeWaterSystemValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaterFeaturesSettings"/> class.
        /// </summary>
        /// <param name="mod">Water Features mod.</param>
        public WaterFeaturesSettings(IMod mod)
            : base(mod)
        {
            SetDefaults();
            Contra = false;
            m_ChangeWaterSystemValues = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ChangeWaterSystemValues>();
        }

        /// <summary>
        /// An enum with the types of tides that can be simulated.
        /// </summary>
        public enum TideClassificationYYTAW
        {
            /// <summary>
            /// Diurnal tides have one high and one low tide per day.
            /// </summary>
            Diurnal = 12,

            /// <summary>
            /// Semidirurnal tides have two high and two low tides per day.
            /// </summary>
            Semidiurnal = 24,
        }

        /// <summary>
        /// Gets or sets the evaporatin rate for the whole map.
        /// </summary>
        [SettingsUISection(General, EvaporationRateGroup)]
        [SettingsUISlider(min = 0.1f, max = 1f, step = 0.1f, unit = "percentageSingleFraction", scalarMultiplier = 1000f)]
        public float EvaporationRate { get; set; }

        /// <summary>
        /// Sets a value indicating whether the toggle for applying a new evaporation rate is on.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(General, EvaporationRateGroup)]
        public bool WaterCleanUpCycleButton
        {
            set { m_ChangeWaterSystemValues.ApplyNewEvaporationRate = true; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether: Used to force saving of Modsettings if settings would result in empty Json.
        /// </summary>
        [SettingsUIHidden]
        public bool Contra { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to simulate snow melt with creeks.
        /// </summary>
        [SettingsUISection(General, SeasonalStreams)]
        public bool SimulateSnowMelt { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the multiplier for water always emitted from a creek.
        /// </summary>
        [SettingsUISlider(min = 0f, max = 100f, step = 5f, unit = "percentageSingleFraction", scalarMultiplier = 100f)]
        [SettingsUISection(General, SeasonalStreams)]
        public float CreekSpringWater { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the multiplier for water seaonally emitted from a creek.
        /// </summary>
        [SettingsUISlider(min = 0f, max = 100f, step = 5f, unit = "percentageSingleFraction", scalarMultiplier = 100f)]
        [SettingsUISection(General, SeasonalStreams)]
        public float CreekMeanPrecipitationWeight { get; set; }


        /// <summary>
        /// Gets or sets a value with a slider indicating the multiplier for water emitted from a creek due to rain.
        /// </summary>
        [SettingsUISlider(min = 0f, max = 100f, step = 5f, unit = "percentageSingleFraction", scalarMultiplier = 100f)]
        [SettingsUISection(General, SeasonalStreams)]
        public float CreekCurrentPrecipitationWeight { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the minimum multiplier to apply to creeks.
        /// </summary>
        [SettingsUISection(General, SeasonalStreams)]
        [SettingsUISlider(min = 0f, max = 1f, step = 0.1f, unit = "floatSingleFraction")]
        public float MinimumMultiplier { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the maximum multiplier to apply to creeks.
        /// </summary>
        [SettingsUISlider(min = 1f, max = 10f, step = 0.1f, unit = "floatSingleFraction")]
        [SettingsUISection(General, SeasonalStreams)]
        public float MaximumMultiplier { get; set; }

        /// <summary>
        /// Sets a value indicating whether: a button for Resetting the settings for the Mod.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(General, Reset)]
        public bool ResetGeneralSettingsButton
        {
            set
            {
                ResetGeneralSettings();
            }
        }

        /// <summary>
        /// Gets or sets a value with a slider indicating the height of waves generated.
        /// </summary>
        [SettingsUISection(Experimental, WavesAndTides)]
        [SettingsUISlider(min = 0f, max = 50f, step = 1f, unit = "integer")]
        public float WaveHeight { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the frequency of waves generated.
        /// </summary>
        [SettingsUISection(Experimental, WavesAndTides)]
        [SettingsUISlider(min = 10f, max = 250f, step = 10f)]
        public float WaveFrequency { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the height of tides generated.
        /// </summary>
        [SettingsUISection(Experimental, WavesAndTides)]
        [SettingsUISlider(min = 0f, max = 50f, step = 1f, unit = "integer")]
        public float TideHeight { get; set; }

        /// <summary>
        /// Gets or sets an enum value indicating the tide classification.
        /// </summary>
        [SettingsUISection(Experimental, WavesAndTides)]
        public TideClassificationYYTAW TideClassification { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the damping factor of the water system.
        /// </summary>
        [SettingsUISection(Experimental, WavesAndTides)]
        [SettingsUISlider(min = 99f, max = 100f, step = 0.1f, unit = "percentageSingleFraction", scalarMultiplier = 100f)]
        public float Damping { get; set; }

        /// <summary>
        /// Sets a value indicating whether: a button for Resetting the settings for the Mod.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(Experimental, WavesAndTides)]
        public bool ResetWavesAndTidesSettingsButton
        {
            set
            {
                ResetWavesAndTidesSettings();
            }
        }

        /// <summary>
        /// Resets only the general settings tab.
        /// </summary>
        public void ResetGeneralSettings()
        {
            EvaporationRate = 0.0001f;
            CreekSpringWater = 0f;
            CreekMeanPrecipitationWeight = 0.75f;
            CreekCurrentPrecipitationWeight = 0.75f;
            MinimumMultiplier = 0f;
            MaximumMultiplier = 1.0f;
            SimulateSnowMelt = true;
        }

        /// <summary>
        /// Resets only the waves and tides settings tab.
        /// </summary>
        public void ResetWavesAndTidesSettings()
        {
            WaveHeight = 0f;
            TideHeight = 0f;
            WaveFrequency = 130f;
            TideClassification = TideClassificationYYTAW.Semidiurnal;
            Damping = 0.995f;
        }

        /// <inheritdoc/>
        public override void SetDefaults()
        {
            Contra = true;
            EvaporationRate = 0.0001f;
            CreekSpringWater = 0f;
            CreekMeanPrecipitationWeight = 0.75f;
            CreekCurrentPrecipitationWeight = 0.75f;
            MinimumMultiplier = 0f;
            MaximumMultiplier = 1.0f;
            SimulateSnowMelt = true;
            WaveHeight = 0f;
            TideHeight = 0f;
            WaveFrequency = 130f;
            TideClassification = TideClassificationYYTAW.Semidiurnal;
            Damping = 0.995f;
        }
    }
}
