// <copyright file="WaterFeaturesMod.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

#define DEBUG // Change before release.
namespace Water_Features
{
    using System.IO;
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Game;
    using Game.Modding;
    using Game.SceneFlow;
    using Game.UI;
    using Water_Features.Settings;
    using Water_Features.Systems;
    using Water_Features.Tools;
    using Water_Features.Utils;

    /// <summary>
    ///  A mod that adds Water Tool, Seasonal Streams, and Experimetnal Waves and Tides.
    /// </summary>
    public class WaterFeaturesMod : IMod
    {
        /// <summary>
        /// Gets the install folder for the mod.
        /// </summary>
        private static string m_modInstallFolder;

        /// <summary>
        /// Gets or sets the Settings for the mod.
        /// </summary>
        public static WaterFeaturesSettings Settings { get; set; }

        /// <summary>
        /// Gets the static reference to the mod instance.
        /// </summary>
        public static WaterFeaturesMod Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Install Folder for the mod as a string.
        /// </summary>
        public static string ModInstallFolder
        {
            get
            {
                if (m_modInstallFolder is null)
                {
                    m_modInstallFolder = Path.GetDirectoryName(typeof(WaterFeaturesPlugin).Assembly.Location);
                }

                return m_modInstallFolder;
            }
        }

        /// <summary>
        /// Gets the log for the mod.
        /// </summary>
        internal ILog Log { get; private set; }

        /// <inheritdoc/>
        public void OnLoad()
        {
            Instance = this;
            Log = LogManager.GetLogger("Mods_Yenyang_Water_Features", false);
            Log.Info($"[{nameof(WaterFeaturesMod)}] {nameof(OnLoad)}");
        }

        /// <inheritdoc/>
        public void OnCreateWorld(UpdateSystem updateSystem)
        {
#if DEBUG
            Log.effectivenessLevel = Colossal.Logging.Level.Debug;
#endif
#if RELEASE
            Log.effectivenessLevel = Colossal.Logging.Level.Info;
#endif
#if VERBOSE
            Log.effectivenessLevel = Colossal.Logging.Level.Verbose;
#endif

            Log.Info("Initializing Settings.");
            Settings = new (this);
            Settings.RegisterInOptionsUI();
            AssetDatabase.global.LoadSettings("Mods_Yenyang_Water_Features", Settings, new WaterFeaturesSettings(this));
            Settings.Contra = false;
            GameUIResourceHandler uiResourceHandler = GameManager.instance.userInterface.view.uiSystem.resourceHandler as GameUIResourceHandler;
            uiResourceHandler?.HostLocationsMap.Add("yy-water-tool", new System.Collections.Generic.List<string> { UIFileUtils.AssemblyPath });
            Log.Info("Handling create world");
            Log.Info("ModInstallFolder = " + ModInstallFolder);
            LoadLocales();
            updateSystem.UpdateAt<CustomWaterToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAfter<WaterTooltipSystem>(SystemUpdatePhase.UITooltip);
            updateSystem.UpdateBefore<FindWaterSourcesSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<AutofillingLakesSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<DetentionBasinSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<RetentionBasinSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateBefore<ChangeWaterSystemValues>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<TidesAndWavesSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateBefore<BeforeSerializeSystem>(SystemUpdatePhase.Serialize);
            updateSystem.UpdateAfter<TidesAndWavesSystem>(SystemUpdatePhase.Serialize);
            updateSystem.UpdateAt<SeasonalStreamsSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<DisableSeasonalStreamSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<DisableWavesAndTidesSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAfter<SeasonalStreamsSystem>(SystemUpdatePhase.Serialize);
            updateSystem.UpdateAfter<AutofillingLakesSystem>(SystemUpdatePhase.Serialize);
            updateSystem.UpdateAfter<DetentionBasinSystem>(SystemUpdatePhase.Serialize);
            updateSystem.UpdateAfter<RetentionBasinSystem>(SystemUpdatePhase.Serialize);
            updateSystem.UpdateAt<AddPrefabsSystem>(SystemUpdatePhase.PrefabUpdate);
            updateSystem.UpdateAt<WaterToolUISystem>(SystemUpdatePhase.UIUpdate);
        }

        /// <inheritdoc/>
        public void OnDispose()
        {
            Log.Info($"[{nameof(WaterFeaturesMod)}] {nameof(OnDispose)}");
        }

        private void LoadLocales()
        {
            foreach (var lang in GameManager.instance.localizationManager.GetSupportedLocales())
            {
                GameManager.instance.localizationManager.AddSource(lang, new LocaleEN(Settings));
            }
        }
    }
}