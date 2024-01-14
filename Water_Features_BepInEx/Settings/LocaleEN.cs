// <copyright file="LocaleEN.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Settings
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Localization for <see cref="WaterFeaturesMod"/> in English.
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
                { m_Setting.GetOptionTabLocaleID(WaterFeaturesSettings.SeasonalStreams), "Seasonal Streams" },
                { m_Setting.GetOptionTabLocaleID(WaterFeaturesSettings.WaterToolGroup), "Water Tool" },
                { m_Setting.GetOptionTabLocaleID(WaterFeaturesSettings.WavesAndTides), "Waves and Tides" },
                { m_Setting.GetOptionGroupLocaleID(WaterFeaturesSettings.SeasonalStreams), "Seasonal Streams" },
                { m_Setting.GetOptionGroupLocaleID(WaterFeaturesSettings.WaterToolGroup), "Water Tool" },
                { m_Setting.GetOptionGroupLocaleID(WaterFeaturesSettings.WavesAndTides), "Waves and Tides" },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.IncludeRetentionBasins)), "Add Retention Basins (Restart Required)" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.IncludeRetentionBasins)), "Custom modded water source that rises with precipitation and snowmelt and slowly drains when the weather is dry. They have a maximum water surface elevation and a minimum water surface elevation. You may need to adjust the global evaporation rate in the settings for desirable infiltration of the pond water." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.IncludeDetentionBasins)), "Add Detention Basins (Restart Required)" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.IncludeDetentionBasins)), "Custom modded water source that rises with precipitation and snowmelt and slowly drains when the weather is dry. They have a maximum water surface elevation but no minimum water surface elevation. You may need to adjust the global evaporation rate in the settings for desirable infiltration of the pond water." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.EvaporationRate)), "Evaporation Rate" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.EvaporationRate)), "Actual evaporation rate is 1000x smaller but this gives you control over the rate. This is global and you may want to rebalance and increase flows of water sources after increasing it." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.EnableSeasonalStreams)), "Enable Seasonal Streams" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.EnableSeasonalStreams)), "Seasonal streams takes Creeks (a.k.a. Constant Rate Water Source) and ties them to the climate and weather for the map. For example, if your map features a dry summer, then these water sources will decrease during the summer. Seasonal streams by it-self should not cause flooding since it treats the map's default water source amount as a maximum unless you change it. All aspects are optional and adjustable in the mod's settings." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.SimulateSnowMelt)), "Simulate Snow Melt" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.SimulateSnowMelt)), "Simulate snow accumulation and snow melt. Snow melt is not currently tied to snow visuals. This affects Constant Rate Water Sources, Detention and Retention basins." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.CreekSpringWater)), "Constant Flow Rate" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.CreekSpringWater)), "Constant flow rate from Creeks (Constant Rate Water Source) unaffected by season or weather." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.CreekMeanPrecipitationWeight)), "Seasonality" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.CreekMeanPrecipitationWeight)), "Creeks (Constant Rate Water Source) flowrates will increase by this percentage during the season with the highest mean seasonal precipitation. Other seasons will be proportional." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.CreekCurrentPrecipitationWeight)), "Stormwater Effects" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.CreekCurrentPrecipitationWeight)), "Creeks (Constant Rate Water Source) flowrates will increase by this percentage when current precipitation (rain) is at a maximum. Less precipitation is proportional." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.MinimumMultiplier)), "Minimum Multiplier" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.MinimumMultiplier)), "Minimum multiplier applied to stream flowrates." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.MaximumMultiplier)), "Maximum Multiplier" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.MaximumMultiplier)), "Maximum multiplier applied to stream flowrates." },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.EnableWavesAndTides)), "Enable Waves and Tides" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.EnableWavesAndTides)), "This feature is dependent on map design. Maps with a sea water source and a single shoreline work best. \r\nThe point of the waves feature is to make the shore move in and out and make sand along the shoreline. A better way to make beaches is to just paint them with surface painter instead. \r\nWaves exacerbate the magnitude of the water surface. Tides are similar but happen once or twice a day." },
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
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.ResetWaterToolGroupButton)), "Reset Water Tool Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.ResetWaterToolGroupButton)), "On confirmation, resets Water Tool Settings." },
                { m_Setting.GetOptionWarningLocaleID(nameof(WaterFeaturesSettings.ResetWaterToolGroupButton)), "Reset Water Tool Settings?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.ResetSeasonalStreamsSettingsButton)), "Reset Seasonal Streams Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.ResetSeasonalStreamsSettingsButton)), "On confirmation, resets Seasonal Streams Settings." },
                { m_Setting.GetOptionWarningLocaleID(nameof(WaterFeaturesSettings.ResetSeasonalStreamsSettingsButton)), "Reset Seasonal Streams Settings?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(WaterFeaturesSettings.ResetWavesAndTidesSettingsButton)), "Reset Waves and Tides Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(WaterFeaturesSettings.ResetWavesAndTidesSettingsButton)), "On confirmation, resets Waves and Tides Settings." },
                { m_Setting.GetOptionWarningLocaleID(nameof(WaterFeaturesSettings.ResetWavesAndTidesSettingsButton)), "Reset Waves and Tides Settings?" },
                { m_Setting.GetEnumValueLocaleID(WaterFeaturesSettings.TideClassificationYYTAW.Diurnal), "Diurnal" },
                { m_Setting.GetEnumValueLocaleID(WaterFeaturesSettings.TideClassificationYYTAW.Semidiurnal), "Semidiurnal" },
                { "YY_WATER_FEATURES.Flow", "Flow" },
                { "YY_WATER_FEATURES.Depth", "Depth" },
                { "YY_WATER_FEATURES.MaxDepth", "Max Depth" },
                { "YY_WATER_FEATURES.Elevation", "Elevation" },
                { "YY_WATER_FEATURES.MinDepth", "Min Depth" },
                { "YY_WATER_FEATURES.Radius", "Radius" },
                { "YY_WATER_FEATURES.amount-down-arrow", "Reduce Flow/Depth/Max Depth" },
                { "YY_WATER_FEATURES_DESCRIPTION.amount-down-arrow", "Reduces the flow for constant rate water sources. Decreases the depth for rivers, seas, constant level water sources, automatic filling lakes. Reduces the max depth for retention and detention basins." },
                { "YY_WATER_FEATURES.amount-up-arrow", "Increase Flow/Depth/Max Depth" },
                { "YY_WATER_FEATURES_DESCRIPTION.amount-up-arrow", "Increases the flow for constant rate water sources. Increases the depth for rivers, seas, constant level water sources, automatic filling lakes. Increases the max depth for retention and detention basins." },
                { "YY_WATER_FEATURES.radius-down-arrow", "Reduce Radius" },
                { "YY_WATER_FEATURES_DESCRIPTION.radius-down-arrow", "Reduces the radius." },
                { "YY_WATER_FEATURES.radius-up-arrow", "Increase Radius" },
                { "YY_WATER_FEATURES_DESCRIPTION.radius-up-arrow", "Increases the radius.." },
                { "YY_WATER_FEATURES.min-depth-down-arrow", "Reduce Min Depth" },
                { "YY_WATER_FEATURES_DESCRIPTION.min-depth-down-arrow", "Reduces the minimum depth." },
                { "YY_WATER_FEATURES.min-depth-up-arrow", "Increase Min Depth" },
                { "YY_WATER_FEATURES_DESCRIPTION.min-depth-up-arrow", "Increases the minimum depth." },
                { "YY_WATER_FEATURES.amount-rate-of-change", "Flow/Depth/Max Depth Rate of Change" },
                { "YY_WATER_FEATURES_DESCRIPTION.amount-rate-of-change", "Changes the rate in which the increase and decrease buttons work for Flow, Depth and Max Depth." },
                { "YY_WATER_FEATURES.radius-rate-of-change", "Radius Rate of Change" },
                { "YY_WATER_FEATURES_DESCRIPTION.radius-rate-of-change", "Changes the rate in which the increase and decrease buttons work for Radius." },
                { "YY_WATER_FEATURES.min-depth-rate-of-change", "Minimum Depth Rate of Change" },
                { "YY_WATER_FEATURES_DESCRIPTION.min-depth-rate-of-change", "Changes the rate in which the increase and decrease buttons work for minimum depth." },
                { "SubServices.NAME[WaterTool]", "Water Tool" },
                { "Assets.SUB_SERVICE_DESCRIPTION[WaterTool]", "Water tool allows you to add and remove water sources from your map." },
                { "Assets.NAME[WaterSource Creek]", "Creek" },
                { "Assets.DESCRIPTION[WaterSource Creek]", "Map Makers and CO call this Constant Rate Water Source. Vanilla versions emit a constant rate of water. Typically has small radius & small amount. With this mod the amount varies with season, precipitation, and snowmelt depending on your settings for the mod." },
                /* { "Assets.NAME[WaterSource Lake]", "Constant Level Water Source" },
                { "Assets.DESCRIPTION[WaterSource Lake]", "(i.e. lake, pond) Will maintain the water level at this location." },*/
                { "Assets.NAME[WaterSource River]", "River" },
                { "Assets.DESCRIPTION[WaterSource River]", "Map Makers and CO call this Border River Water Source. These sources have a constant level and must be placed at the border of the map. Typically has medium radius. They will affect the non-playable area beyond the borders." },
                { "Assets.NAME[WaterSource Sea]", "Sea" },
                { "Assets.DESCRIPTION[WaterSource Sea]", "Map Makers and CO call this Border Sea Water Source. Vanilla sea sources have a constant level and must touch a border. Typically has very large radius. They will affect the non-playable area beyond the borders. This mod has experimental options to add waves and tides to sea water sources. It works best with a single shore line parralel with the border." },
                { "Assets.NAME[WaterSource AutofillingLake]", "Lake" },
                { "Assets.DESCRIPTION[WaterSource AutofillingLake]", "Custom modded water source that starts as a Constant Rate Water Source (so it fills up quickly) until it gets to the desired level and then turns into a Constant Level Water Source (The Vanilla equivilant to a lake)." },
                { "Assets.NAME[WaterSource DetentionBasin]", "Detention Basin" },
                { "Assets.DESCRIPTION[WaterSource DetentionBasin]", "Custom modded water source that rises with precipitation and snowmelt and slowly drains when the weather is dry. They have a maximum water surface elevation but no minimum water surface elevation. You may need to adjust the global evaporation rate in the settings for desirable infiltration of the pond water." },
                { "Assets.NAME[WaterSource RetentionBasin]", "Retention Basin" },
                { "Assets.DESCRIPTION[WaterSource RetentionBasin]", "Custom modded water source that rises with precipitation and snowmelt and slowly drains when the weather is dry. They have a maximum water surface elevation and a minimum water surface elevation. You may need to adjust the global evaporation rate in the settings for desirable infiltration of the pond water." },
                { "Tooltip.LABEL[YY.WT.PlaceNearBorder]", "Rivers must be placed near map border." },
                { "Tooltip.LABEL[YY.WT.RadiusTooSmall]", "The radius is too small and has been automically increased." },
                { "Tooltip.LABEL[YY.WT.RemoveWaterSource]", "Right click to remove water source." },
                { "Tooltip.LABEL[YY.WT.PlaceInsideBorder]", "This water source must be placed inside the playable map." },
                { "Tooltip.LABEL[YY.WT.MustTouchBorder]", "Sea water sources must touch the map border." },
            };
        }

        /// <inheritdoc/>
        public void Unload()
        {
        }
    }
}
