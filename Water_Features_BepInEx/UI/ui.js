if (typeof yyWaterTool != 'object') var yyWaterTool = {};

if (typeof yyWaterTool.CheckForElementByID !== 'function') {
    yyWaterTool.CheckForElementByID = function (id)
    {
        if (document.getElementById(id) != null) {
            engine.trigger('CheckForElement-'+id , true);
            return;
        }
        engine.trigger('CheckForElement-' + id, false);
    }
}

if (typeof yyWaterTool.setupButton !== 'function') {
    yyWaterTool.setupButton = function(buttonId, toolTipKey) {
        const button = document.getElementById(buttonId);
        if (button == null) {
            engine.trigger('YYWT-log', "JS Error: could not setup button " + buttonId);
            return;
        }
        button.onclick = function () {
            engine.trigger(this.id);
        }
        yyWaterTool.setTooltip(buttonId, toolTipKey);
    }
}

// Function to apply translation strings.
if (typeof yyWaterTool.applyLocalization !== 'function') {
    yyWaterTool.applyLocalization = function (target) {
        if (!target) {
            return;
        }

        let targets = target.querySelectorAll('[localeKey]');
        targets.forEach(function (currentValue) {
            currentValue.innerHTML = engine.translate(currentValue.getAttribute("localeKey"));
        });
    }
}

// Function to setup tooltip.
if (typeof yyWaterTool.setTooltip !== 'function') {
    yyWaterTool.setTooltip = function (id, toolTipKey) {
        let target = document.getElementById(id);
        target.onmouseenter = () => yyWaterTool.showTooltip(document.getElementById(id), toolTipKey);
        target.onmouseleave = yyWaterTool.hideTooltip;
    }
}

// Function to show a tooltip, creating if necessary.
if (typeof yyWaterTool.showTooltip !== 'function') {
    yyWaterTool.showTooltip = function (parent, tooltipKey) {

        if (!document.getElementById("yywt-tooltip")) {
            yyWaterTool.tooltip = document.createElement("div");
            yyWaterTool.tooltip.id = "yywt-tooltip";
            yyWaterTool.tooltip.style.visibility = "hidden";
            yyWaterTool.tooltip.classList.add("balloon_qJY", "balloon_H23", "up_ehW", "center_hug", "anchored-balloon_AYp", "up_el0");
            let boundsDiv = document.createElement("div");
            boundsDiv.classList.add("bounds__AO");
            let containerDiv = document.createElement("div");
            containerDiv.classList.add("container_zgM", "container_jfe");
            let contentDiv = document.createElement("div");
            contentDiv.classList.add("content_A82", "content_JQV");
            let arrowDiv = document.createElement("div");
            arrowDiv.classList.add("arrow_SVb", "arrow_Xfn");
            let broadDiv = document.createElement("div");
            yyWaterTool.tooltipTitle = document.createElement("div");
            yyWaterTool.tooltipTitle.classList.add("title_lCJ");
            let paraDiv = document.createElement("div");
            paraDiv.classList.add("paragraphs_nbD", "description_dNa");
            yyWaterTool.tooltipPara = document.createElement("p");
            yyWaterTool.tooltipPara.setAttribute("cohinline", "cohinline");

            paraDiv.appendChild(yyWaterTool.tooltipPara);
            broadDiv.appendChild(yyWaterTool.tooltipTitle);
            broadDiv.appendChild(paraDiv);
            containerDiv.appendChild(arrowDiv);
            contentDiv.appendChild(broadDiv);
            boundsDiv.appendChild(containerDiv);
            boundsDiv.appendChild(contentDiv);
            yyWaterTool.tooltip.appendChild(boundsDiv);

            // Append tooltip to screen element.
            let screenParent = document.getElementsByClassName("game-main-screen_TRK");
            if (screenParent.length == 0) {
                screenParent = document.getElementsByClassName("editor-main-screen_m89");
            }
            if (screenParent.length > 0) {
                screenParent[0].appendChild(yyWaterTool.tooltip);
            }
        }

        // Set text and position.
        yyWaterTool.tooltipTitle.innerHTML = engine.translate("YY_WATER_FEATURES." + tooltipKey);
        yyWaterTool.tooltipPara.innerHTML = engine.translate("YY_WATER_FEATURES_DESCRIPTION." + tooltipKey);

        // Set visibility tracking to prevent race conditions with popup delay.
        yyWaterTool.tooltipVisibility = "visible";

        // Slightly delay popup by three frames to prevent premature activation and to ensure layout is ready.
        window.requestAnimationFrame(() => {
            window.requestAnimationFrame(() => {
                window.requestAnimationFrame(() => {
                    yyWaterTool.setTooltipPos(parent);
                });

            });
        });
    }
}

// Function to adjust the position of a tooltip and make visible.
if (typeof yyWaterTool.setTooltipPos !== 'function') {
    yyWaterTool.setTooltipPos = function (parent) {
        if (!yyWaterTool.tooltip) {
            return;
        }

        let tooltipRect = yyWaterTool.tooltip.getBoundingClientRect();
        let parentRect = parent.getBoundingClientRect();
        let xPos = parentRect.left + ((parentRect.width - tooltipRect.width) / 2);
        let yPos = parentRect.top - tooltipRect.height;
        yyWaterTool.tooltip.setAttribute("style", "left:" + xPos + "px; top: " + yPos + "px; --posY: " + yPos + "px; --posX:" + xPos + "px");

        yyWaterTool.tooltip.style.visibility = yyWaterTool.tooltipVisibility;
    }
}

// Function to hide the tooltip.
if (typeof yyWaterTool.hideTooltip !== 'function') {
    yyWaterTool.hideTooltip = function () {
        if (yyWaterTool.tooltip) {
            yyWaterTool.tooltipVisibility = "hidden";
            yyWaterTool.tooltip.style.visibility = "hidden";
        }
    }
}

yyWaterTool.setupButton("YYWT-creek", "YY_WATER_FEATURES.creek");
yyWaterTool.setupButton("YYWT-lake", "YY_WATER_FEATURES.lake");
yyWaterTool.setupButton("YYWT-river", "YY_WATER_FEATURES.river");
yyWaterTool.setupButton("YYWT-sea", "YY_WATER_FEATURES.sea");
yyWaterTool.setupButton("YYWT-autofilling-lake", "YY_WATER_FEATURES.autofilling-lake");
yyWaterTool.setupButton("YYWT-detention-basin", "YY_WATER_FEATURES.detention-basin");
yyWaterTool.setupButton("YYWT-retention-basin", "YY_WATER_FEATURES.retention-basin");

yyWaterTool.setupButton("YYWT-amount-down-arrow", "YY_WATER_FEATURES.amount-down-arrow");
yyWaterTool.setupButton("YYWT-amount-up-arrow", "YY_WATER_FEATURES.amount-up-arrow");

yyWaterTool.setupButton("YYWT-radius-down-arrow", "YY_WATER_FEATURES.radius-down-arrow");
yyWaterTool.setupButton("YYWT-radius-up-arrow", "YY_WATER_FEATURES.radius-up-arrow");