// <copyright file="WaterToolUISystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Water_Features.Tools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using cohtml.Net;
    using Colossal.Logging;
    using Game.Prefabs;
    using Game.SceneFlow;
    using Game.Tools;
    using Game.UI;
    using Unity.Entities;
    using Water_Features;
    using Water_Features.Prefabs;
    using Water_Features.Utils;

    /// <summary>
    /// UI system for Object Tool while using tree prefabs.
    /// </summary>
    public partial class WaterToolUISystem : UISystemBase
    {
        private readonly Dictionary<SourceType, string> m_SourceTypeToButtonIDs = new Dictionary<SourceType, string>() 
        {
            { SourceType.Creek, "YYWT-creek" },
            { SourceType.Lake, "YYWT-lake" },
            { SourceType.River, "YYWT-river" },
            { SourceType.Sea, "YYWT-sea" },
            { SourceType.AutofillingLake, "YYWT-autofilling-lake" },
            { SourceType.DetentionBasin, "YYWT-detention-basin" },
            { SourceType.RetentionBasin, "YYWT-retention-basin" },
        };

        private readonly Dictionary<string, SourceType> m_ButtonIDsToSourceType = new Dictionary<string, SourceType>()
        {
            { "YYWT-creek", SourceType.Creek },
            { "YYWT-lake", SourceType.Lake },
            { "YYWT-river", SourceType.River },
            { "YYWT-sea", SourceType.Sea },
            { "YYWT-autofilling-lake", SourceType.AutofillingLake },
            { "YYWT-detention-basin", SourceType.DetentionBasin },
            { "YYWT-retention-basin", SourceType.RetentionBasin },
        };

        private View m_UiView;
        private ToolSystem m_ToolSystem;
        private string m_InjectedJS = string.Empty;
        private string m_WaterToolPanelScript = string.Empty;
        private CustomWaterToolSystem m_ExtendedWaterToolSystem;
        private ILog m_Log;
        private bool m_WaterToolPanelShown;
        private List<BoundEventHandle> m_BoundEventHandles;
        private float m_Radius = 10f;
        private float m_Amount = 5f;
        private Dictionary<string, Action> m_ChangeValueActions;
        private bool m_ButtonPressed = false;

        /// <summary>
        /// Types of water sources.
        /// </summary>
        public enum SourceType
        {
            /// <summary>
            /// Constant Rate Water Sources that may vary with season and precipitation.
            /// </summary>
            Creek,

            /// <summary>
            /// Constant level water sources.
            /// </summary>
            Lake,

            /// <summary>
            /// Border River water sources.
            /// </summary>
            River,

            /// <summary>
            /// Border Sea water sources. Level may change with waves and tides.
            /// </summary>
            Sea,

            /// <summary>
            /// Starts as a creek and settles into a lake.
            /// </summary>
            AutofillingLake,

            /// <summary>
            /// Pond that fills when its rainy but will empty completely eventually.
            /// </summary>
            DetentionBasin,

            /// <summary>
            /// Pond that fills when its raining and has a minimum water level.
            /// </summary>
            RetentionBasin,
        }

        /// <summary>
        /// Gets the radius.
        /// </summary>
        public float Radius
        {
            get { return m_Radius; }
        }

        /// <summary>
        /// Gets the amount.
        /// </summary>
        public float Amount
        {
            get { return m_Amount; }
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = WaterFeaturesMod.Instance.Log;
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            m_ExtendedWaterToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<CustomWaterToolSystem>();
            m_UiView = GameManager.instance.userInterface.view.View;
            ToolSystem toolSystem = m_ToolSystem; // I don't know why vanilla game did this.
            m_ToolSystem.EventToolChanged = (Action<ToolBaseSystem>)Delegate.Combine(toolSystem.EventToolChanged, new Action<ToolBaseSystem>(OnToolChanged));
            ToolSystem toolSystem2 = m_ToolSystem; // I don't know why vanilla game did this.
            m_ToolSystem.EventPrefabChanged = (Action<PrefabBase>)Delegate.Combine(toolSystem.EventPrefabChanged, new Action<PrefabBase>(OnPrefabChanged));

            m_BoundEventHandles = new ();

            if (m_UiView != null)
            {
                m_InjectedJS = UIFileUtils.ReadJS(Path.Combine(UIFileUtils.AssemblyPath, "ui.js"));
                m_WaterToolPanelScript = UIFileUtils.ReadHTML(Path.Combine(UIFileUtils.AssemblyPath, "YYWT-Water-Tool-Panel.html"), "if (document.getElementById(\"yy-water-tool-panel\") == null) { yyWaterTool.div.className = \"tool-options-panel_Se6\"; yyWaterTool.div.id = \"yy-water-tool-panel\"; yyWaterTool.ToolColumns = document.getElementsByClassName(\"tool-side-column_l9i\"); if (yyWaterTool.ToolColumns[0] != null) yyWaterTool.ToolColumns[0].appendChild(yyWaterTool.div);}");
            }
            else
            {
                m_Log.Info($"{nameof(WaterToolUISystem)}.{nameof(OnCreate)} m_UiView == null");
            }

            m_ChangeValueActions = new Dictionary<string, Action>()
            {
                { "YYWT-amount-down-arrow", (Action)DecreaseAmount },
                { "YYWT-amount-up-arrow", (Action)IncreaseAmount },
                { "YYWT-radius-down-arrow", (Action)DecreaseRadius },
                { "YYWT-radius-up-arrow", (Action)IncreaseRadius },
            };

            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_UiView == null)
            {
                return;
            }

            if (m_ToolSystem.activeTool != m_ExtendedWaterToolSystem)
            {
                if (m_WaterToolPanelShown)
                {
                    UnshowWaterToolPanel();
                    Enabled = false;
                }

                return;
            }

            m_ButtonPressed = false;

            if (!m_WaterToolPanelShown)
            {
                UIFileUtils.ExecuteScript(m_UiView, "if (typeof yyWaterTool != 'object') var yyWaterTool = {};");

                UIFileUtils.ExecuteScript(m_UiView, m_WaterToolPanelScript);

                // This script defines the JS functions and setups up typical buttons.
                UIFileUtils.ExecuteScript(m_UiView, m_InjectedJS);

                // This script sets the radius field to the desired radius;
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.radiusField = document.getElementById(\"YYWT-radius-field\"); if (yyWaterTool.radiusField) yyWaterTool.radiusField.innerHTML = \"{m_Radius} m\";");

                // This script sets the amount field to the desired amount;
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.amountField = document.getElementById(\"YYWT-amount-field\"); if (yyWaterTool.amountField) yyWaterTool.amountField.innerHTML = \"{m_Amount}\";");

                m_BoundEventHandles.Add(m_UiView.RegisterForEvent("YYWT-log", (Action<string>)LogFromJS));
                m_BoundEventHandles.Add(m_UiView.RegisterForEvent("CheckForElement-yy-water-tool-panel", (Action<bool>)ElementCheck));

                foreach (KeyValuePair<string, Action> kvp in m_ChangeValueActions)
                {
                    m_BoundEventHandles.Add(m_UiView.RegisterForEvent("Change-Value", (Action<string>)ChangeValue));
                }

                m_WaterToolPanelShown = true;
            }
            else
            {
                // This script checks if water tool panel exists. If it doesn't it triggers water tool panel item being recreated.
                UIFileUtils.ExecuteScript(m_UiView, $"if (document.getElementById(\"yy-water-tool-panel\") == null) engine.trigger('CheckForElement-yy-water-tool-panel', false);");
            }

            base.OnUpdate();
        }

        /// <summary>
        /// Get a script for Destroing element by id if that element exists.
        /// </summary>
        /// <param name="id">The id from HTML or JS.</param>
        /// <returns>a script for Destroing element by id if that element exists.</returns>
        private string DestroyElementByID(string id)
        {
            return $"yyWaterTool.itemElement = document.getElementById(\"{id}\"); if (yyWaterTool.itemElement) yyWaterTool.itemElement.parentElement.removeChild(yyWaterTool.itemElement);";
        }

        /// <summary>
        /// Logs a string from JS.
        /// </summary>
        /// <param name="log">A string from JS to log.</param>
        private void LogFromJS(string log) => m_Log.Debug($"{nameof(WaterToolUISystem)}.{nameof(LogFromJS)} {log}");

        /// <summary>
        /// Converts a C# bool to JS string.
        /// </summary>
        /// <param name="flag">a bool.</param>
        /// <returns>"true" or "false".</returns>
        private string BoolToString(bool flag)
        {
            if (flag)
            {
                return "true";
            }

            return "false";
        }

        /// <summary>
        /// C# Event handler for event callback form UI Javascript. Exectutes an action depending on button pressed.
        /// </summary>
        /// <param name="buttonID">The id of the button pressed.</param>
        private void ChangeValue(string buttonID)
        {
            if (buttonID == null)
            {
                m_Log.Warn($"{nameof(WaterToolUISystem)}.{nameof(ChangeValue)} buttonID was null.");
                return;
            }

            if (m_ButtonPressed)
            {
                return;
            }

            m_Log.Debug($"{nameof(WaterToolUISystem)}.{nameof(ChangeValue)} buttonID = {buttonID}");
            if (m_ChangeValueActions.ContainsKey(buttonID))
            {
                m_ChangeValueActions[buttonID].Invoke();
            }

            m_ButtonPressed = true;
        }

        private void IncreaseRadius()
        {
            if (m_Radius >= 500 && m_Radius < 1000)
            {
                m_Radius += 100;
            }
            else if (m_Radius >= 100 && m_Radius < 500)
            {
                m_Radius += 50;
            }
            else if (m_Radius < 1000)
            {
                m_Radius += 10;
            }

            // This script sets the radius field to the desired radius;
            UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.radiusField = document.getElementById(\"YYWT-radius-field\"); if (yyWaterTool.radiusField) yyWaterTool.radiusField.innerHTML = \"{m_Radius} m\";");
        }

        private void DecreaseRadius()
        {
            if (m_Radius <= 100 && m_Radius > 10)
            {
                m_Radius -= 10;
            }
            else if (m_Radius <= 500 && m_Radius > 100)
            {
                m_Radius -= 50;
            }
            else if (m_Radius > 500)
            {
                m_Radius -= 100;
            }

            // This script sets the radius field to the desired radius;
            UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.radiusField = document.getElementById(\"YYWT-radius-field\"); if (yyWaterTool.radiusField) yyWaterTool.radiusField.innerHTML = \"{m_Radius} m\";");
        }

        private void IncreaseAmount()
        {
            if (m_Amount >= 500 && m_Amount < 1000)
            {
                m_Amount += 100;
            }
            else if (m_Amount >= 100 && m_Amount < 500)
            {
                m_Amount += 50;
            }
            else if (m_Amount < 1000)
            {
                m_Amount += 10;
            }

            // This script sets the amount field to the desired amount;
            UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.amountField = document.getElementById(\"YYWT-amount-field\"); if (yyWaterTool.amountField) yyWaterTool.amountField.innerHTML = \"{m_Amount}\";");
        }

        private void DecreaseAmount()
        {
            if (m_Amount <= 100 && m_Amount > 10)
            {
                m_Amount -= 10;
            }
            else if (m_Amount <= 500 && m_Amount > 100)
            {
                m_Amount -= 50;
            }
            else if (m_Amount > 500)
            {
                m_Amount -= 100;
            }

            // This script sets the amount field to the desired amount;
            UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.amountField = document.getElementById(\"YYWT-amount-field\"); if (yyWaterTool.amountField) yyWaterTool.amountField.innerHTML = \"{m_Amount}\";");
        }

        /// <summary>
        /// C# event handler for event callback from UI JavaScript. If element YYA-anarchy-item is found then set value to true.
        /// </summary>
        /// <param name="flag">A bool for whether to element was found.</param>
        private void ElementCheck(bool flag) => m_WaterToolPanelShown = flag;

        /// <summary>
        /// Handles cleaning up after the icons are no longer needed.
        /// </summary>
        private void UnshowWaterToolPanel()
        {
            if (m_UiView == null)
            {
                return;
            }

            // This script destroys the anarchy item if it exists.
            UIFileUtils.ExecuteScript(m_UiView, DestroyElementByID("yy-water-tool-panel"));

            // This unregisters the events.
            foreach (BoundEventHandle eventHandle in m_BoundEventHandles)
            {
                m_UiView.UnregisterFromEvent(eventHandle);
            }

            m_BoundEventHandles.Clear();

            // This records that everything is cleaned up.
            m_WaterToolPanelShown = false;
        }

        private void OnToolChanged(ToolBaseSystem tool)
        {
            if (tool != m_ExtendedWaterToolSystem)
            {
                if (m_WaterToolPanelShown)
                {
                    UnshowWaterToolPanel();
                }

                Enabled = false;
                return;
            }

            Enabled = true;
        }

        private void OnPrefabChanged(PrefabBase prefabBase)
        {
            if (prefabBase is WaterSourcePrefab && m_UiView != null)
            {
                WaterSourcePrefab waterSourcePrefab = prefabBase as WaterSourcePrefab;
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.amount = document.getElementById(\"YYWT-amount-label\"); yyWaterTool.amount.localeKey = \"{waterSourcePrefab.m_AmountLocaleKey}\"; if (typeof yyWaterTool.applyLocalization == 'function') yyWaterTool.applyLocalization(\"yyWaterTool.amount\");");
                return;
            }
        }
    }
}
