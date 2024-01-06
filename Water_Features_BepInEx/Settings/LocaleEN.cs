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
                { "YY_WATER_FEATURES.Amount", "Amount/Height" },
                { "YY_WATER_FEATURES.Radius", "Radius" },
                { "YY_WATER_FEATURES.amount-down-arrow", "Reduce Amount/Height" },
                { "YY_WATER_FEATURES_DESCRIPTION.amount-down-arrow", "Reduces the amount/height." },
                { "YY_WATER_FEATURES.amount-up-arrow", "Increase Amount/Height" },
                { "YY_WATER_FEATURES_DESCRIPTION.amount-up-arrow", "Reduces the amount/height." },
                { "YY_WATER_FEATURES.radius-down-arrow", "Reduce Radius" },
                { "YY_WATER_FEATURES_DESCRIPTION.radius-down-arrow", "Reduces the radius." },
                { "YY_WATER_FEATURES.radius-up-arrow", "Increase Radius" },
                { "YY_WATER_FEATURES_DESCRIPTION.radius-up-arrow", "Increases the radius.." },
                { "SUBSERVICES.NAME[WATER TOOL]", "Water Tool" },
                { "Assets.SUB_SERVICE_DESCRIPTION[Water Tool]", "Water tool allows you to add and remove water sources from your map." },
                { "Assets.NAME[WaterSource Creek]", "Constant Rate Water Source" },
                { "Assets.DESCRIPTION[WaterSource Creek]", "(i.e. creek, brook, stream, spring) Vanilla versions emit a constant rate of water. Typically has small radius & small amount. With this mod the amount varies with season, precipitation, and snowmelt depending on your settings for the mod." },
                { "Assets.NAME[WaterSource Lake]", "Constant Level Water Source" },
                { "Assets.DESCRIPTION[WaterSource Lake]", "(i.e. lake, pond) Will maintain the water level at this location." },
                { "Assets.NAME[WaterSource River]", "Border River Water Source" },
                { "Assets.DESCRIPTION[WaterSource River]", "These sources have a constant rate and must be placed at the border of the map. Typically has medium radius & large amount. They will affect the non-playable area beyond the borders." },
                { "Assets.NAME[WaterSource Sea]", "Border Sea Water Source" },
                { "Assets.DESCRIPTION[WaterSource Sea]", "Vanilla sea sources have a constant level and must touch a border. Typically has very large radius. They will affect the non-playable area beyond the borders. This mod has experimental options to add waves and tides to sea water sources. It works best with a single shore line parralel with the border sea water source." },
                { "Assets.NAME[WaterSource AutofillingLake]", "Autofilling Lake" },
                { "Assets.DESCRIPTION[WaterSource AutofillingLake]", "Custom modded water source that starts as a Constant Rate Water Source until it gets to the desired level and then turns into a Constant Level Water Source." },
                { "Assets.NAME[WaterSource DetentionBasin]", "Detention Basin" },
                { "Assets.DESCRIPTION[WaterSource DetentionBasin]", "Custom modded water source that rises with precipitation and snowmelt and slowly drains when the weather is dry. They have a maximum water surface elevation but no minimum water surface elevation. You may need to adjust the global evaporation rate in the settings for desirable infiltration of the pond water." },
                { "Assets.NAME[WaterSource RetentionBasin]", "Retention Basin" },
                { "Assets.DESCRIPTION[WaterSource RetentionBasin]", "Custom modded water source that rises with precipitation and snowmelt and slowly drains when the weather is dry. They have a maximum water surface elevation and a minimum water surface elevation. You may need to adjust the global evaporation rate in the settings for desirable infiltration of the pond water." },
            };
        }

        /// <inheritdoc/>
        public void Unload()
        {
        }
    }
}
