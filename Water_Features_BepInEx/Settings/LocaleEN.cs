// <copyright file="LocaleEN.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Settings
{
    using System.Collections.Generic;
    using Colossal;
    using Colossal.IO.AssetDatabase.Internal;

    /// <summary>
    /// Localization for <see cref="DisasterControllerMod"/> in English.
    /// </summary>
    public class LocaleEN : IDictionarySource
    {
        private readonly WaterFeaturesSettings m_Setting;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocaleEN"/> class.
        /// </summary>
        /// <param name="setting">Settings class.</param>
        public LocaleEN(WaterFeaturesSettings setting)
        {
            m_Setting = setting;
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Water Features" },
                { m_Setting.GetOptionTabLocaleID(WaterFeaturesSettings.KWaterToolTab), "Water Tool" },
                { m_Setting.GetOptionTabLocaleID(WaterFeaturesSettings.KSeasonalStreamTab), "Seasonal Streams" },
                { m_Setting.GetOptionTabLocaleID(WaterFeaturesSettings.KWavesAndTidesTab), "Waves and Tides" },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.ShowPollution)), "Show Pollution" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.ShowPollution)), "This will allow you to add water sources with Pollution." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.ShowEntityInfo)), "Show Enity Info" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.ShowEntityInfo)), "For using Entity lookup with Scene Explorer mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.EvaporationRate)), "Evaporation Rate" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.EvaporationRate)), "Actual evaporation rate is 1000x smaller but this gives you control over the rate. This is global and you may want to rebalance and increase flows of water sources after increasing it." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.SimulateSnowMelt)), "Simulate Snow Melt" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.SimulateSnowMelt)), "Simulate snow accumulation and snow melt. Snow melt is not currently tied to snow visuals." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.CreekSpringWater)), "Creek Spring Water" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.CreekSpringWater)), "Constant flow rate from Creeks unaffected by season or weather." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.CreekMeanPrecipitationWeight)), "Creek Seasonality" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.CreekMeanPrecipitationWeight)), "Creek flowrates will increase by this percentage during the season with the highest mean seasonal precipitation. Other seasons will be proportional." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.CreekCurrentPrecipitationWeight)), "Creek Stormwater Effects" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.CreekCurrentPrecipitationWeight)), "Creek flowrates will increase by this percentage when current precipitation (rain) is at a maximum. Less precipitation is proportional." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.MinimumMultiplier)), "Minimum Multiplier" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.MinimumMultiplier)), "Minimum multiplier applied to stream flowrates." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.MaximumMultiplier)), "Maximum Multiplier" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.MaximumMultiplier)), "Maximum multiplier applied to stream flowrates." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.WaveHeight)), "Wave Height" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.WaveHeight)), "EXPERIMENTAL! Waves are generated at the map boundary where there is a Sea water source. Once generated they head towards shore. Maps were not designed for these waves. Be prepared to need to change the map and design your build to accommodate the waves. You may even want to remove some or all sea water sources and make new ones in better locations." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.WaveFrequency)), "Wave Frequency" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.WaveFrequency)), "Frequency for waves per day." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.TideHeight)), "Tide Height" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.TideHeight)), "EXPERIMENTAL! Tides are the biggest waves and they cause the sea to rise and fall along the shore. Tides can add sandy graphics along shorelines but the sand may not persist the entire time between low tide and high tide. " },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.TideClassification)), "Tide Classification" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.TideClassification)), "Diurnal tides have one high and one low tide per day. Semidiurnal has two high tides and two low tides per day." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.Damping)), "Damping" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.Damping)), "A higher value makes waves stronger while a lower value makes waves weaker. Stronger waves reach farther into the map. Weak waves may disperse before reaching shore. Vanilla is 99.5%. Recommended 99.7%-99.8%." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.WaterCleanUpCycleButton)), "Water Clean Up Cycle" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.WaterCleanUpCycleButton)), "This will increase the rate of evaporation by 1000x FOR THE WHOLE MAP for a short amount of time before returning to normal. For cleaning up water messes but USE AT YOUR OWN RISK!" },
                { m_Setting.GetOptionWarningLocaleID(nameof(WaterFeaturesSettings.WaterCleanUpCycleButton)), "Start Water Clean Up Cycle?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.ResetWaterToolSettingsButton)), "Reset Water Tool Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.ResetWaterToolSettingsButton)), "On confirmation, resets Water Tool Settings." },
                { m_Setting.GetOptionWarningLocaleID(nameof(WaterFeaturesSettings.ResetWaterToolSettingsButton)), "Reset Water Tool Settings?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.ResetSeasonalStreamsSettingsButton)), "Reset Seasonal Streams Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.ResetSeasonalStreamsSettingsButton)), "On confirmation, resets Seasonal Streams Settings." },
                { m_Setting.GetOptionWarningLocaleID(nameof(WaterFeaturesSettings.ResetSeasonalStreamsSettingsButton)), "Reset Seasonal Streams Settings?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.ResetWavesAndTidesSettingsButton)), "Reset Waves and Tides Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.ResetWavesAndTidesSettingsButton)), "On confirmation, resets Waves and Tides Settings." },
                { m_Setting.GetOptionWarningLocaleID(nameof(WaterFeaturesSettings.ResetWavesAndTidesSettingsButton)), "Reset Waves and Tides Settings?" },
                { m_Setting.GetEnumValueLocaleID(WaterFeaturesSettings.TideClassificationYYTAW.Diurnal), "Diurnal" },
                { m_Setting.GetEnumValueLocaleID(WaterFeaturesSettings.TideClassificationYYTAW.Semidiurnal), "Semidiurnal" },
                { "YY_WATER_FEATURES.Type", "Type" },
                { "YY_WATER_FEATURES.Amount", "Amount" },
                { "YY_WATER_FEATURES.Radius", "Radius" },
            };
        }

        /// <inheritdoc/>
        public void Unload()
        {
        }
    }
}
