// <copyright file="WaterFeaturesPlugin.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features
{
    using BepInEx;
    using Game;
    using Game.Common;
    using HarmonyLib;

    /// <summary>
    /// Mod entry point for BepInEx configuaration.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, "Water Features", "1.0.1")]
    [HarmonyPatch]
    public class WaterFeaturesPlugin : BaseUnityPlugin
    {
        /// <summary>
        /// A static instance of the IMod for mod entry point.
        /// </summary>
        internal static WaterFeaturesMod _mod;

        /// <summary>
        /// Patches and Injects mod into game via Harmony.
        /// </summary>
        public void Awake()
        {
            _mod = new ();
            _mod.OnLoad();
            _mod.Log.Info($"{nameof(WaterFeaturesPlugin)}.{nameof(Awake)}");
            Harmony.CreateAndPatchAll(typeof(WaterFeaturesPlugin).Assembly, MyPluginInfo.PLUGIN_GUID);
        }

        [HarmonyPatch(typeof(SystemOrder), nameof(SystemOrder.Initialize), new[] { typeof(UpdateSystem) })]
        [HarmonyPostfix]
        private static void InjectSystems(UpdateSystem updateSystem)
        {
            _mod.Log.Info($"{nameof(WaterFeaturesPlugin)}.{nameof(InjectSystems)}");
            _mod.OnCreateWorld(updateSystem);
        }
    }
}