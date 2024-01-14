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
    [SettingsUIShowGroupName(SeasonalStreams, WaterToolGroup, WavesAndTides)]
    [SettingsUIGroupOrder(SeasonalStreams, WaterToolGroup, WavesAndTides)]
    public class WaterFeaturesSettings : ModSetting
    {
        /// <summary>
        /// This is for settings that affect the UI for the mod.
        /// </summary>
        public const string SeasonalStreams = "Seasonal Streams";

        /// <summary>
        /// This is for options related to unique buildings.
        /// </summary>
        public const string WaterToolGroup = "Water Tool";

        /// <summary>
        /// This is for options related to prop culling.
        /// </summary>
        public const string WavesAndTides = "Waves and Tides";

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
        /// Gets or sets a value indicating whether to Include Detention Basins.
        /// </summary>
        [SettingsUISection(WaterToolGroup)]
        public bool IncludeDetentionBasins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to Include Retention Basins.
        /// </summary>
        [SettingsUISection(WaterToolGroup)]
        public bool IncludeRetentionBasins { get; set; }

        /// <summary>
        /// Gets or sets the evaporatin rate for the whole map.
        /// </summary>
        [SettingsUISection(WaterToolGroup)]
        [SettingsUISlider(min = 0.1f, max = 1f, step = 0.1f, unit = "percentageSingleFraction", scalarMultiplier = 1000f)]
        public float EvaporationRate { get; set; }

        /// <summary>
        /// Sets a value indicating whether the toggle for applying a new evaporation rate is on.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(WaterToolGroup)]
        public bool WaterCleanUpCycleButton
        {
            set { m_ChangeWaterSystemValues.ApplyNewEvaporationRate = true; }
        }

        /// <summary>
        /// Sets a value indicating whether: a button for Resetting the settings for the general tab.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(WaterToolGroup)]
        public bool ResetWaterToolGroupButton
        {
            set
            {
                ResetWaterToolSettings();
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether: Used to force saving of Modsettings if settings would result in empty Json.
        /// </summary>
        [SettingsUIHidden]
        public bool Contra { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to have Seasonal Streams.
        /// </summary>
        [SettingsUISection(SeasonalStreams)]
        public bool EnableSeasonalStreams { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to simulate snow melt with creeks.
        /// </summary>
        [SettingsUISection(SeasonalStreams)]
        [SettingsUIHideByCondition(typeof(WaterFeaturesSettings), nameof(IsSeasonalStreamsDisabled))]
        public bool SimulateSnowMelt { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the multiplier for water always emitted from a creek.
        /// </summary>
        [SettingsUISlider(min = 0f, max = 100f, step = 5f, unit = "percentageSingleFraction", scalarMultiplier = 100f)]
        [SettingsUISection(SeasonalStreams)]
        [SettingsUIHideByCondition(typeof(WaterFeaturesSettings), nameof(IsSeasonalStreamsDisabled))]
        public float CreekSpringWater { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the multiplier for water seaonally emitted from a creek.
        /// </summary>
        [SettingsUISlider(min = 0f, max = 100f, step = 5f, unit = "percentageSingleFraction", scalarMultiplier = 100f)]
        [SettingsUISection(SeasonalStreams)]
        [SettingsUIHideByCondition(typeof(WaterFeaturesSettings), nameof(IsSeasonalStreamsDisabled))]
        public float CreekMeanPrecipitationWeight { get; set; }


        /// <summary>
        /// Gets or sets a value with a slider indicating the multiplier for water emitted from a creek due to rain.
        /// </summary>
        [SettingsUISlider(min = 0f, max = 100f, step = 5f, unit = "percentageSingleFraction", scalarMultiplier = 100f)]
        [SettingsUISection(SeasonalStreams)]
        [SettingsUIHideByCondition(typeof(WaterFeaturesSettings), nameof(IsSeasonalStreamsDisabled))]
        public float CreekCurrentPrecipitationWeight { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the minimum multiplier to apply to creeks.
        /// </summary>
        [SettingsUISection(SeasonalStreams)]
        [SettingsUIHideByCondition(typeof(WaterFeaturesSettings), nameof(IsSeasonalStreamsDisabled))]
        [SettingsUISlider(min = 0f, max = 1f, step = 0.1f, unit = "floatSingleFraction")]
        public float MinimumMultiplier { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the maximum multiplier to apply to creeks.
        /// </summary>
        [SettingsUISlider(min = 1f, max = 10f, step = 0.1f, unit = "floatSingleFraction")]
        [SettingsUISection(SeasonalStreams)]
        [SettingsUIHideByCondition(typeof(WaterFeaturesSettings), nameof(IsSeasonalStreamsDisabled))]
        public float MaximumMultiplier { get; set; }

        /// <summary>
        /// Sets a value indicating whether: a button for Resetting the settings for the general tab.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(SeasonalStreams)]
        [SettingsUIHideByCondition(typeof(WaterFeaturesSettings), nameof(IsSeasonalStreamsDisabled))]
        public bool ResetSeasonalStreamsSettingsButton
        {
            set
            {
                ResetSeasonalStreamsSettings();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to have Waves and Tides.
        /// </summary>
        [SettingsUISection(WavesAndTides)]
        public bool EnableWavesAndTides { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the height of waves generated.
        /// </summary>
        [SettingsUISection(WavesAndTides)]
        [SettingsUISlider(min = 0f, max = 50f, step = 1f, unit = "integer")]
        [SettingsUIHideByCondition(typeof(WaterFeaturesSettings), nameof(IsWavesAndTidesDisabled))]
        public float WaveHeight { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the frequency of waves generated.
        /// </summary>
        [SettingsUISection(WavesAndTides)]
        [SettingsUISlider(min = 10f, max = 250f, step = 10f)]
        [SettingsUIHideByCondition(typeof(WaterFeaturesSettings), nameof(IsWavesAndTidesDisabled))]
        public float WaveFrequency { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the height of tides generated.
        /// </summary>
        [SettingsUISection(WavesAndTides)]
        [SettingsUISlider(min = 0f, max = 50f, step = 1f, unit = "integer")]
        [SettingsUIHideByCondition(typeof(WaterFeaturesSettings), nameof(IsWavesAndTidesDisabled))]
        public float TideHeight { get; set; }

        /// <summary>
        /// Gets or sets an enum value indicating the tide classification.
        /// </summary>
        [SettingsUISection(WavesAndTides)]
        [SettingsUIHideByCondition(typeof(WaterFeaturesSettings), nameof(IsWavesAndTidesDisabled))]
        public TideClassificationYYTAW TideClassification { get; set; }

        /// <summary>
        /// Gets or sets a value with a slider indicating the damping factor of the water system.
        /// </summary>
        [SettingsUISection(WavesAndTides)]
        [SettingsUISlider(min = 99f, max = 99.9f, step = 0.1f, unit = "percentageSingleFraction", scalarMultiplier = 100f)]
        [SettingsUIHideByCondition(typeof(WaterFeaturesSettings), nameof(IsWavesAndTidesDisabled))]
        public float Damping { get; set; }

        /// <summary>
        /// Sets a value indicating whether: a button for Resetting the settings for the Waves and tides.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(WavesAndTides)]
        [SettingsUIHideByCondition(typeof(WaterFeaturesSettings), nameof(IsWavesAndTidesDisabled))]
        public bool ResetWavesAndTidesSettingsButton
        {
            set
            {
                ResetWavesAndTidesSettings();
            }
        }

        /// <summary>
        /// Resets only the water tool settings.
        /// </summary>
        public void ResetWaterToolSettings()
        {
            EvaporationRate = 0.0001f;
            IncludeDetentionBasins = false;
            IncludeRetentionBasins = false;
        }

        /// <summary>
        /// Resets only the Seasonal streams settings.
        /// </summary>
        public void ResetSeasonalStreamsSettings()
        {
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
            WaveHeight = 5f;
            TideHeight = 5f;
            WaveFrequency = 130f;
            TideClassification = TideClassificationYYTAW.Semidiurnal;
            Damping = 0.998f;
        }

        /// <summary>
        /// Checks if seasonal streams feature is off or on.
        /// </summary>
        /// <returns>Opposite of Enable Seasonal Streams.</returns>
        public bool IsSeasonalStreamsDisabled() => !EnableSeasonalStreams;

        /// <summary>
        /// Checks if waves and tides feature is off or on.
        /// </summary>
        /// <returns>Opposite of Enable Waves and Tides.</returns>
        public bool IsWavesAndTidesDisabled() => !EnableWavesAndTides;

        /// <inheritdoc/>
        public override void SetDefaults()
        {
            Contra = true;
            EvaporationRate = 0.0001f;
            IncludeDetentionBasins = false;
            IncludeRetentionBasins = false;
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
            EnableSeasonalStreams = true;
            EnableWavesAndTides = false;
        }
    }
}
