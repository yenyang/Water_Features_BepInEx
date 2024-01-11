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
    using UnityEngine;
    using Water_Features;
    using Water_Features.Prefabs;
    using Water_Features.Utils;

    /// <summary>
    /// UI system for Custom Water Tool
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
        private string m_AmountItemScript = string.Empty;
        private string m_RadiusItemScript = string.Empty;
        private string m_MinDepthItemScript = string.Empty;
        private CustomWaterToolSystem m_CustomWaterToolSystem;
        private ILog m_Log;
        private bool m_WaterToolPanelShown;
        private List<BoundEventHandle> m_BoundEventHandles;
        private float m_Radius = 10f;
        private float m_Amount = 5f;
        private float m_MinDepth = 10f;
        private Dictionary<string, Action> m_ChangeValueActions;
        private bool m_ButtonPressed = false;
        private float m_AmountRateOfChange = 1f;
        private float m_RadiusRateOfChange = 1f;
        private float m_MinDepthRateOfChange = 1f;
        private bool m_ResetValues = true;

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

        /// <summary>
        /// Gets the min depth.
        /// </summary>
        public float MinDepth
        {
            get { return m_MinDepth; }
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = WaterFeaturesMod.Instance.Log;
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            m_CustomWaterToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<CustomWaterToolSystem>();
            m_UiView = GameManager.instance.userInterface.view.View;
            ToolSystem toolSystem = m_ToolSystem; // I don't know why vanilla game did this.
            m_ToolSystem.EventToolChanged = (Action<ToolBaseSystem>)Delegate.Combine(toolSystem.EventToolChanged, new Action<ToolBaseSystem>(OnToolChanged));
            ToolSystem toolSystem2 = m_ToolSystem; // I don't know why vanilla game did this.
            m_ToolSystem.EventPrefabChanged = (Action<PrefabBase>)Delegate.Combine(toolSystem2.EventPrefabChanged, new Action<PrefabBase>(OnPrefabChanged));

            m_BoundEventHandles = new ();

            if (m_UiView != null)
            {
                m_InjectedJS = UIFileUtils.ReadJS(Path.Combine(UIFileUtils.AssemblyPath, "ui.js"));
                m_AmountItemScript = UIFileUtils.ReadHTML(Path.Combine(UIFileUtils.AssemblyPath, "YYWT-Amount-Item.html"), "if (document.getElementById(\"YYWT-amount-item\") == null) { yyWaterTool.div.className = \"item_bZY\"; yyWaterTool.div.id = \"YYWT-amount-item\"; yyWaterTool.entities = document.getElementsByClassName(\"tool-options-panel_Se6\"); if (yyWaterTool.entities[0] != null) { yyWaterTool.entities[0].insertAdjacentElement('afterbegin', yyWaterTool.div); } }");
                m_RadiusItemScript = UIFileUtils.ReadHTML(Path.Combine(UIFileUtils.AssemblyPath, "YYWT-Radius-Item.html"), "if (document.getElementById(\"YYWT-radius-item\") == null) { yyWaterTool.div.className = \"item_bZY\"; yyWaterTool.div.id = \"YYWT-radius-item\"; yyWaterTool.amountItem = document.getElementById(\"YYWT-amount-item\"); if (yyWaterTool.amountItem != null) { yyWaterTool.amountItem.insertAdjacentElement('afterend', yyWaterTool.div); } }");
                m_MinDepthItemScript = UIFileUtils.ReadHTML(Path.Combine(UIFileUtils.AssemblyPath, "YYWT-Min-Depth-Item.html"), "if (document.getElementById(\"YYWT-min-depth-item\") == null) { yyWaterTool.div.className = \"item_bZY\"; yyWaterTool.div.id = \"YYWT-min-depth-item\"; yyWaterTool.amountItem = document.getElementById(\"YYWT-amount-item\"); if (yyWaterTool.amountItem != null) { yyWaterTool.amountItem.insertAdjacentElement('afterend', yyWaterTool.div); } }");
            }
            else
            {
                m_Log.Warn($"{nameof(WaterToolUISystem)}.{nameof(OnCreate)} m_UiView == null");
            }

            m_ChangeValueActions = new Dictionary<string, Action>()
            {
                { "YYWT-amount-down-arrow", (Action)DecreaseAmount },
                { "YYWT-amount-up-arrow", (Action)IncreaseAmount },
                { "YYWT-radius-down-arrow", (Action)DecreaseRadius },
                { "YYWT-radius-up-arrow", (Action)IncreaseRadius },
                { "YYWT-min-depth-down-arrow", (Action)DecreaseMinDepth },
                { "YYWT-min-depth-up-arrow", (Action)IncreaseMinDepth },
                { "YYWT-amount-rate-of-change", (Action)AmountRateOfChangePressed },
                { "YYWT-radius-rate-of-change", (Action)RadiusRateOfChangePressed },
                { "YYWT-min-depth-rate-of-change", (Action)MinDepthRateOfChangePressed },
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

            if (m_ToolSystem.activeTool != m_CustomWaterToolSystem)
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

                UIFileUtils.ExecuteScript(m_UiView, m_AmountItemScript);

                UIFileUtils.ExecuteScript(m_UiView, m_RadiusItemScript);

                // This script defines the JS functions and sets up typical buttons.
                UIFileUtils.ExecuteScript(m_UiView, m_InjectedJS);

                string unit = " m";

                if (m_CustomWaterToolSystem.GetPrefab() != null)
                {
                    WaterSourcePrefab waterSourcePrefab = m_CustomWaterToolSystem.GetPrefab() as WaterSourcePrefab;

                    // This script changes and translates the Amount label according to the active prefab.
                    UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.amount = document.getElementById(\"YYWT-amount-label\"); if (yyWaterTool.amount) {{ yyWaterTool.amount.localeKey = \"{waterSourcePrefab.m_AmountLocaleKey}\"; yyWaterTool.amount.innerHTML = engine.translate(yyWaterTool.amount.localeKey); }}");

                    if (m_ResetValues)
                    {
                        m_Radius = waterSourcePrefab.m_DefaultRadius;
                        m_Amount = waterSourcePrefab.m_DefaultAmount;
                    }

                    if (waterSourcePrefab.m_SourceType == SourceType.Creek)
                    {
                        unit = string.Empty;
                    }

                    if (waterSourcePrefab.m_SourceType == SourceType.RetentionBasin)
                    {
                        UIFileUtils.ExecuteScript(m_UiView, m_MinDepthItemScript);

                        if (m_ResetValues)
                        {
                            m_MinDepth = 10f;
                        }

                        // This script sets the min depth field to the desired min depth;
                        UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.minDepthField = document.getElementById(\"YYWT-min-depth-field\"); if (yyWaterTool.minDepthField) yyWaterTool.minDepthField.innerHTML = \"{m_MinDepth} m\";");

                        // This script sets up the up and down buttons for min depth and applies localization to the row.
                        UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.applyLocalization(document.getElementById(\"YYWT-min-depth-item\")); yyWaterTool.setupButton(\"YYWT-min-depth-down-arrow\", \"min-depth-down-arrow\"); yyWaterTool.setupButton(\"YYWT-min-depth-up-arrow\", \"min-depth-up-arrow\");  yyWaterTool.setupButton(\"YYWT-min-depth-rate-of-change\", \"min-depth-rate-of-change\");");

                        SetRateIcon(m_MinDepthRateOfChange, "min-depth");
                    }

                    m_ResetValues = false;
                }

                // This script sets the radius field to the desired radius;
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.radiusField = document.getElementById(\"YYWT-radius-field\"); if (yyWaterTool.radiusField) yyWaterTool.radiusField.innerHTML = \"{m_Radius} m\";");

                // This script sets the amount field to the desired amount;
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.amountField = document.getElementById(\"YYWT-amount-field\"); if (yyWaterTool.amountField) yyWaterTool.amountField.innerHTML = \"{m_Amount}{unit}\";");

                SetRateIcon(m_RadiusRateOfChange, "radius");
                SetRateIcon(m_AmountRateOfChange, "amount");

                m_BoundEventHandles.Add(m_UiView.RegisterForEvent("YYWT-log", (Action<string>)LogFromJS));
                m_BoundEventHandles.Add(m_UiView.RegisterForEvent("CheckForElement-YYWT-amount-item", (Action<bool>)ElementCheck));

                foreach (KeyValuePair<string, Action> kvp in m_ChangeValueActions)
                {
                    m_BoundEventHandles.Add(m_UiView.RegisterForEvent("Change-Value", (Action<string>)ChangeValue));
                }

                m_WaterToolPanelShown = true;
            }
            else
            {
                // This script checks if water tool panel exists. If it doesn't it triggers water tool panel item being recreated.
                UIFileUtils.ExecuteScript(m_UiView, $"if (document.getElementById(\"YYWT-amount-item\") == null) engine.trigger('CheckForElement-YYWT-amount-item', false);");
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
            if (m_Radius >= 1000 && m_Radius < 10000)
            {
                m_Radius += 500 * m_RadiusRateOfChange;
            }
            else if (m_Radius >= 500 && m_Radius < 1000)
            {
                m_Radius += 100 * m_RadiusRateOfChange;
            }
            else if (m_Radius >= 100 && m_Radius < 500)
            {
                m_Radius += 50 * m_RadiusRateOfChange;
            }
            else if (m_Radius >= 10 && m_Radius < 100)
            {
                m_Radius += 10 * m_RadiusRateOfChange;
            }
            else if (m_Radius < 10000)
            {
                m_Radius += 1 * m_RadiusRateOfChange;
            }

            m_Radius = Mathf.Clamp(m_Radius, 5f, 10000f);

            // This script sets the radius field to the desired radius;
            UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.radiusField = document.getElementById(\"YYWT-radius-field\"); if (yyWaterTool.radiusField) yyWaterTool.radiusField.innerHTML = \"{m_Radius} m\";");
        }

        private void DecreaseRadius()
        {
            if (m_Radius <= 10 && m_Radius > 5)
            {
                m_Radius -= 1 * m_RadiusRateOfChange;
            }
            else if (m_Radius <= 100 && m_Radius > 10)
            {
                m_Radius -= 10 * m_RadiusRateOfChange;
            }
            else if (m_Radius <= 500 && m_Radius > 100)
            {
                m_Radius -= 50 * m_RadiusRateOfChange;
            }
            else if (m_Radius <= 1000 && m_Radius > 500)
            {
                m_Radius -= 100 * m_RadiusRateOfChange;
            }
            else if (m_Radius > 1000)
            {
                m_Radius -= 500 * m_RadiusRateOfChange;
            }

            m_Radius = Mathf.Clamp(m_Radius, 5f, 10000f);

            // This script sets the radius field to the desired radius;
            UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.radiusField = document.getElementById(\"YYWT-radius-field\"); if (yyWaterTool.radiusField) yyWaterTool.radiusField.innerHTML = \"{m_Radius} m\";");
        }

        private void IncreaseMinDepth()
        {
            if (m_MinDepth >= 500 && m_MinDepth < 1000)
            {
                m_MinDepth += 100 * m_MinDepthRateOfChange;
            }
            else if (m_MinDepth >= 100 && m_MinDepth < 500)
            {
                m_MinDepth += 50 * m_MinDepthRateOfChange;
            }
            else if (m_MinDepth < 100 && m_MinDepth >= 10)
            {
                m_MinDepth += 10 * m_MinDepthRateOfChange;
            }
            else if (m_MinDepth < 1000)
            {
                m_MinDepth += 1 * m_MinDepthRateOfChange;
            }

            m_MinDepth = Mathf.Clamp(m_MinDepth, 0.125f, 1000f);

            if (m_MinDepth > m_Amount)
            {
                m_Amount = m_MinDepth;

                string unit = " m";

                if (m_CustomWaterToolSystem.GetPrefab() != null)
                {
                    WaterSourcePrefab waterSourcePrefab = m_CustomWaterToolSystem.GetPrefab() as WaterSourcePrefab;
                    if (waterSourcePrefab.m_SourceType == SourceType.Creek)
                    {
                        unit = string.Empty;
                    }
                }

                // This script sets the amount field to the desired amount.
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.amountField = document.getElementById(\"YYWT-amount-field\"); if (yyWaterTool.amountField) yyWaterTool.amountField.innerHTML = \"{m_Amount}{unit}\";");
            }

            // This script sets the min depth field to the desired min depth;
            UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.minDepthField = document.getElementById(\"YYWT-min-depth-field\"); if (yyWaterTool.minDepthField) yyWaterTool.minDepthField.innerHTML = \"{m_MinDepth} m\";");
        }

        private void DecreaseMinDepth()
        {
            if (m_MinDepth <= 10 && m_MinDepth > 0.125f)
            {
                m_MinDepth -= 1 * m_MinDepthRateOfChange;
            }
            else if (m_MinDepth <= 100 && m_MinDepth > 10)
            {
                m_MinDepth -= 10 * m_MinDepthRateOfChange;
            }
            else if (m_MinDepth <= 500 && m_MinDepth > 100)
            {
                m_MinDepth -= 50 * m_MinDepthRateOfChange;
            }
            else if (m_MinDepth > 500)
            {
                m_MinDepth -= 100 * m_MinDepthRateOfChange;
            }

            m_MinDepth = Mathf.Clamp(m_MinDepth, 0.125f, 1000f);

            // This script sets the radius field to the desired radius;
            UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.minDepthField = document.getElementById(\"YYWT-min-depth-field\"); if (yyWaterTool.minDepthField) yyWaterTool.minDepthField.innerHTML = \"{m_MinDepth} m\";");
        }

        private void IncreaseAmount()
        {
            if (m_Amount >= 500 && m_Amount < 1000)
            {
                m_Amount += 100 * m_AmountRateOfChange;
            }
            else if (m_Amount >= 100 && m_Amount < 500)
            {
                m_Amount += 50 * m_AmountRateOfChange;
            }
            else if (m_Amount < 100 && m_Amount >= 10)
            {
                m_Amount += 10 * m_AmountRateOfChange;
            }
            else if (m_Amount < 1000)
            {
                m_Amount += 1 * m_AmountRateOfChange;
            }

            m_Amount = Mathf.Clamp(m_Amount, 0.125f, 1000f);

            string unit = " m";

            if (m_CustomWaterToolSystem.GetPrefab() != null)
            {
                WaterSourcePrefab waterSourcePrefab = m_CustomWaterToolSystem.GetPrefab() as WaterSourcePrefab;
                if (waterSourcePrefab.m_SourceType == SourceType.Creek)
                {
                    unit = string.Empty;
                }
            }

            // This script sets the amount field to the desired amount;
            UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.amountField = document.getElementById(\"YYWT-amount-field\"); if (yyWaterTool.amountField) yyWaterTool.amountField.innerHTML = \"{m_Amount}{unit}\";");
        }

        private void DecreaseAmount()
        {
            if (m_Amount <= 10 && m_Amount > 0.125f)
            {
                m_Amount -= 1 * m_AmountRateOfChange;
            }
            else if (m_Amount <= 100 && m_Amount > 10)
            {
                m_Amount -= 10 * m_AmountRateOfChange;
            }
            else if (m_Amount <= 500 && m_Amount > 100)
            {
                m_Amount -= 50 * m_AmountRateOfChange;
            }
            else if (m_Amount > 500)
            {
                m_Amount -= 100 * m_AmountRateOfChange;
            }

            m_Amount = Mathf.Clamp(m_Amount, 0.125f, 1000f);

            if (m_Amount < m_MinDepth)
            {
                m_MinDepth = m_Amount;

                // This script sets the min depth field to the desired min depth;
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.minDepthField = document.getElementById(\"YYWT-min-depth-field\"); if (yyWaterTool.minDepthField) yyWaterTool.minDepthField.innerHTML = \"{m_MinDepth} m\";");
            }

            string unit = " m";

            if (m_CustomWaterToolSystem.GetPrefab() != null)
            {
                WaterSourcePrefab waterSourcePrefab = m_CustomWaterToolSystem.GetPrefab() as WaterSourcePrefab;
                if (waterSourcePrefab.m_SourceType == SourceType.Creek)
                {
                    unit = string.Empty;
                }
            }

            // This script sets the amount field to the desired amount;
            UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.amountField = document.getElementById(\"YYWT-amount-field\"); if (yyWaterTool.amountField) yyWaterTool.amountField.innerHTML = \"{m_Amount}{unit}\";");
        }

        private void RadiusRateOfChangePressed()
        {
            m_RadiusRateOfChange /= 2f;
            if (m_RadiusRateOfChange < 0.125f)
            {
                m_RadiusRateOfChange = 1.0f;
            }

            SetRateIcon(m_RadiusRateOfChange, "radius");
        }

        private void AmountRateOfChangePressed()
        {
            m_AmountRateOfChange /= 2f;
            if (m_AmountRateOfChange < 0.125f)
            {
                m_AmountRateOfChange = 1.0f;
            }

            SetRateIcon(m_AmountRateOfChange, "amount");
        }

        private void MinDepthRateOfChangePressed()
        {
            m_MinDepthRateOfChange /= 2f;
            if (m_MinDepthRateOfChange < 0.125f)
            {
                m_MinDepthRateOfChange = 1.0f;
            }

            SetRateIcon(m_MinDepthRateOfChange, "min-depth");
        }

        private void SetRateIcon(float field, string id)
        {
            if (field == 1f)
            {
                // This script changes the fill color of one of the rate of change indicators.
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.rateOfChange = document.getElementById(\"YYWT-{id}-roc-1\"); if (yyWaterTool.rateOfChange) yyWaterTool.rateOfChange.setAttribute(\"fill\",\"#1e83aa\");");

                // This script changes the fill color of one of the rate of change indicators.
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.rateOfChange = document.getElementById(\"YYWT-{id}-roc-0pt5\"); if (yyWaterTool.rateOfChange) yyWaterTool.rateOfChange.setAttribute(\"fill\",\"#1e83aa\");");

                // This script changes the fill color of one of the rate of change indicators.
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.rateOfChange = document.getElementById(\"YYWT-{id}-roc-0pt25\"); if (yyWaterTool.rateOfChange) yyWaterTool.rateOfChange.setAttribute(\"fill\",\"#1e83aa\");");
            }
            else if (field == 0.5f)
            {
                // This script changes the fill color of one of the rate of change indicators.
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.rateOfChange = document.getElementById(\"YYWT-{id}-roc-1\"); if (yyWaterTool.rateOfChange) yyWaterTool.rateOfChange.setAttribute(\"fill\",\"#424242\");");
            }
            else if (field == 0.25f)
            {
                // This script changes the fill color of one of the rate of change indicators.
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.rateOfChange = document.getElementById(\"YYWT-{id}-roc-1\"); if (yyWaterTool.rateOfChange) yyWaterTool.rateOfChange.setAttribute(\"fill\",\"#424242\");");

                // This script changes the fill color of one of the rate of change indicators.
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.rateOfChange = document.getElementById(\"YYWT-{id}-roc-0pt5\"); if (yyWaterTool.rateOfChange) yyWaterTool.rateOfChange.setAttribute(\"fill\",\"#424242\");");
            }
            else
            {
                // This script changes the fill color of one of the rate of change indicators.
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.rateOfChange = document.getElementById(\"YYWT-{id}-roc-1\"); if (yyWaterTool.rateOfChange) yyWaterTool.rateOfChange.setAttribute(\"fill\",\"#424242\");");

                // This script changes the fill color of one of the rate of change indicators.
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.rateOfChange = document.getElementById(\"YYWT-{id}-roc-0pt5\"); if (yyWaterTool.rateOfChange) yyWaterTool.rateOfChange.setAttribute(\"fill\",\"#424242\");");

                // This script changes the fill color of one of the rate of change indicators.
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.rateOfChange = document.getElementById(\"YYWT-{id}-roc-0pt25\"); if (yyWaterTool.rateOfChange) yyWaterTool.rateOfChange.setAttribute(\"fill\",\"#424242\");");
            }
        }



        /// <summary>
        /// C# event handler for event callback from UI JavaScript. If element YYWT-amount-item is found then set value to flag.
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

            // This script destroys the amount item if it exists.
            UIFileUtils.ExecuteScript(m_UiView, DestroyElementByID("YYWT-amount-item"));

            // This script destroys the radius item if it exists.
            UIFileUtils.ExecuteScript(m_UiView, DestroyElementByID("YYWT-radius-item"));

            // This script destroys the min depth item if it exists.
            UIFileUtils.ExecuteScript(m_UiView, DestroyElementByID("YYWT-min-depth-item"));

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
            if (tool != m_CustomWaterToolSystem)
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
            m_Log.Debug($"{nameof(WaterToolUISystem)}.{nameof(OnPrefabChanged)}");
            if (prefabBase is WaterSourcePrefab && m_UiView != null)
            {
                m_Log.Debug($"{nameof(WaterToolUISystem)}.{nameof(OnPrefabChanged)} prefab is water source.");
                WaterSourcePrefab waterSourcePrefab = prefabBase as WaterSourcePrefab;

                // This script sets up the yyWaterTool object if it is not defined.
                UIFileUtils.ExecuteScript(m_UiView, "if (typeof yyWaterTool != 'object') var yyWaterTool = {};");

                // This script changes and translates the Amount label according to the active prefab.
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.amount = document.getElementById(\"YYWT-amount-label\"); if (yyWaterTool.amount) {{ yyWaterTool.amount.localeKey = \"{waterSourcePrefab.m_AmountLocaleKey}\"; yyWaterTool.amount.innerHTML = engine.translate(yyWaterTool.amount.localeKey); }}");

                m_Radius = waterSourcePrefab.m_DefaultRadius;
                m_Amount = waterSourcePrefab.m_DefaultAmount;

                // This script sets the radius field to the desired radius;
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.radiusField = document.getElementById(\"YYWT-radius-field\"); if (yyWaterTool.radiusField) yyWaterTool.radiusField.innerHTML = \"{m_Radius} m\";");

                string unit = " m";
                if (waterSourcePrefab.m_SourceType == SourceType.Creek)
                {
                    unit = string.Empty;
                }

                // This script sets the amount field to the desired amount;
                UIFileUtils.ExecuteScript(m_UiView, $"yyWaterTool.amountField = document.getElementById(\"YYWT-amount-field\"); if (yyWaterTool.amountField) yyWaterTool.amountField.innerHTML = \"{m_Amount}{unit}\";");

                if (waterSourcePrefab.m_SourceType == SourceType.RetentionBasin)
                {
                    m_WaterToolPanelShown = false;
                }
                else
                {
                    // This script destroys the min depth item if it exists.
                    UIFileUtils.ExecuteScript(m_UiView, DestroyElementByID("YYWT-min-depth-item"));
                }

                m_ResetValues = true;

                return;
            }
        }
    }
}
