import { ViewportParameters } from '../Model/ViewportParameters.js';
import { AxisMode } from "../Model/Enums.js";
export class CanvasChart extends EventTarget {
    constructor(dotNetReference) {
        super();
        this.debugMode = false;
        this.mainChart = undefined;
        this.dotNetReference = undefined;
        this.isInitialized = false;
        this.disableEvents = false;
        this.chartGuid = "";
        this.axisMode = AxisMode.MJD;
        this.toLogString = (e) => {
            // 1. Safety Check: No logs for <= 0
            if (e.value <= 0)
                return "";
            // 2. Calculate the Logarithm
            const logValue = Math.log10(e.value);
            // 3. Integer Check: Only allow if it's close to an integer
            // We use a small epsilon for float safety (e.g., 2.99999999 -> 3)
            if (Math.abs(logValue % 1) > 0.00001 && Math.abs(logValue % 1) < 0.99999) {
                return ""; // Return empty string to hide label
            }
            // 4. Round to handle floating point drift (e.g., 2.999 -> 3)
            const exponent = Math.round(logValue);
            // 5. Superscript Mapping
            const supers = {
                '-': '⁻', '0': '⁰', '1': '¹', '2': '²', '3': '³', '4': '⁴',
                '5': '⁵', '6': '⁶', '7': '⁷', '8': '⁸', '9': '⁹'
            };
            let result = "10";
            for (let char of exponent.toString()) {
                result += supers[char] || char;
            }
            return result;
        };
        this.dotNetReference = dotNetReference;
    }
    InitializeChart(guid, chartParameters) {
        this.chartGuid = guid;
        this.axisMode = chartParameters.AxisMode;
        let container = document.getElementById(guid);
        if (container === null) {
            console.error(`Container ${guid} not found`);
            return;
        }
        this.disableEvents = chartParameters.DisableEvents;
        this.mainChart = undefined;
        if (chartParameters.Logarithmic) {
            this.mainChart = new CanvasJS.Chart(container, {
                colorSet: "customColorSet1",
                zoomEnabled: false,
                animationEnabled: true,
                exportEnabled: true,
                zoomType: "xy",
                title: {
                    text: chartParameters.Title
                },
                data: [],
                axisX: {
                    labelFontColor: chartParameters.DisableXLabels === true ? "transparent" : "#000000",
                    gridThickness: 0,
                    //interval: 10, // Interval between ticks (logarithmic interval)
                    //minimum: 0.1, // Minimum value
                    //maximum: 1000, // Maximum value
                    logarithmic: chartParameters.Logarithmic,
                    crosshair: {
                        enabled: true,
                        color: "orange",
                        labelFontColor: "#F8F8F8"
                    },
                    labelFormatter: this.toLogString
                },
                toolTip: {
                    contentFormatter: function (e) {
                        // Custom tooltip to show readable numbers
                        var x = e.entries[0].dataPoint.x;
                        var y = e.entries[0].dataPoint.y;
                        return "τ: <strong>" + x.toExponential(2) + " s</strong><br/>σ: <strong>" + y.toExponential(2) + "</strong>";
                    }
                },
                axisY: {
                    tickThickness: 0,
                    logarithmic: chartParameters.Logarithmic,
                    reversed: chartParameters.InvertYAxis,
                    labelFormatter: this.toLogString,
                    gridThickness: 0,
                },
                stripLines: []
            });
        }
        else {
            this.mainChart = new CanvasJS.Chart(container, {
                colorSet: "customColorSet1",
                zoomEnabled: true,
                zoomType: "xy",
                exportEnabled: true,
                title: {
                    text: chartParameters.Title
                },
                data: [],
                axisX: {
                    labelFontColor: chartParameters.DisableXLabels === true ? "transparent" : "#000000",
                    gridThickness: 1,
                    crosshair: {
                        enabled: true,
                        color: "orange",
                        labelFontColor: "#F8F8F8"
                    }
                },
                axisY: {
                    reversed: chartParameters.InvertYAxis
                },
                rangeChanged: (e) => {
                    if (this.disableEvents === true) {
                        return;
                    }
                    let newVP = new ViewportParameters(e.axisX[0].viewportMinimum, e.axisX[0].viewportMaximum, e.axisY[0].viewportMinimum, e.axisY[0].viewportMaximum, this.axisMode);
                    this.RaiseRangeChanged(newVP);
                    if (e.type === "reset") {
                        this.dispatchEvent(new CustomEvent('RangeReset', {
                            detail: {
                                guid: this.chartGuid
                            }
                        }));
                    }
                },
                stripLines: []
            });
        }
        this.Render();
    }
    GetViewportParameters() {
        if (this.mainChart === undefined) {
            console.error("Chart not initialized");
            return new ViewportParameters(0, 0, 0, 0, this.axisMode);
        }
        if (this.mainChart.axisX.length < 1 || this.mainChart.axisY.length < 1) {
            console.error("Axis not found: ", this.mainChart);
            return new ViewportParameters(0, 0, 0, 0, this.axisMode);
        }
        return new ViewportParameters(this.mainChart.axisX[0].get("viewportMinimum"), this.mainChart.axisX[0].get("viewportMaximum"), this.mainChart.axisY[0].get("viewportMinimum"), this.mainChart.axisY[0].get("viewportMaximum"), this.axisMode);
    }
    SetViewportParameters(viewportParameters) {
        if (this.debugMode) {
            console.trace("Setting vp:", viewportParameters);
        }
        //somtimes new vp is not working without this default set
        //this.mainChart.axisX[0].set("viewportMaximum", null, false);
        //this.mainChart.axisY[0].set("viewportMaximum", null, false);
        //this.mainChart.axisX[0].set("viewportMinimum", null, false);
        //this.mainChart.axisY[0].set("viewportMinimum", null, false);
        this.mainChart.options.axisX.viewportMinimum = viewportParameters.MinX;
        this.mainChart.options.axisX.viewportMaximum = viewportParameters.MaxX;
        this.mainChart.options.axisY.viewportMinimum = viewportParameters.MinY;
        this.mainChart.options.axisY.viewportMaximum = viewportParameters.MaxY;
        this.Render();
        this.RaiseRangeChanged(viewportParameters);
    }
    ResetViewport() {
        if (this.debugMode) {
            console.trace("VP reset.");
        }
        this.mainChart.axisX[0].set("viewportMaximum", null, false);
        this.mainChart.axisY[0].set("viewportMaximum", null, false);
        this.mainChart.axisX[0].set("viewportMinimum", null, false);
        this.mainChart.axisY[0].set("viewportMinimum", null, false);
        this.Render(); //called to calculate auto values
        this.RaiseRangeChanged(this.GetViewportParameters());
        this.dispatchEvent(new CustomEvent('RangeReset', {
            detail: {
                guid: this.chartGuid
            }
        }));
    }
    SetChartParameters(chartParams) {
        if (this.debugMode) {
            console.trace("Setting chart params:", chartParams);
        }
        if (this.mainChart === undefined) {
            console.error("Chart not initialized");
            return;
        }
        this.mainChart.options.title.text = chartParams.Title;
    }
    Render() {
        if (this.debugMode) {
            console.log("Render");
        }
        if (this.mainChart === undefined) {
            console.error("Chart not initialized");
            return;
        }
        if (!this.isInitialized) {
            this.mainChart.render();
            this.isInitialized = true;
        }
        if (this.isInitialized) {
            if (this.mainChart.axisX.length > 0 && this.mainChart.axisX[0].get("logarithmic") === true) {
                this.AddMinorLogarithmicGridLines();
            }
            this.mainChart.render();
        }
    }
    AddDataSet(dataSet) {
        if (this.debugMode) {
            console.trace("Adding dataset", dataSet);
        }
        if (this.mainChart === undefined) {
            console.error("Chart not initialized");
            return;
        }
        const dataSeriesObject = {};
        dataSeriesObject.type = "spline";
        dataSeriesObject.dataPoints = dataSet;
        this.mainChart.options.data.push(dataSeriesObject);
    }
    ClearDataSets() {
        if (this.debugMode) {
            console.trace("Clear dataset");
        }
        if (this.mainChart === undefined) {
            console.error("Chart not initialized");
            return;
        }
        this.mainChart.options.data = [];
    }
    AdjustToVisibleData() {
        if (this.debugMode) {
            console.trace("Adjusting to visible");
        }
        if (this.mainChart === undefined) {
            console.error("Chart not initialized");
            return;
        }
        const chartParams = this.GetViewportParameters();
        const minMaxValues = this.FindYMinMaxValue(chartParams);
        let visibleOffset = 0;
        if (this.mainChart.axisX[0].get("logarithmic") === true) {
            visibleOffset = ((minMaxValues.MaxValue - minMaxValues.MinValue) / 2) * 0.1;
            this.SetViewportParameters(new ViewportParameters(chartParams.MinX, chartParams.MaxX, this.getStartDecade(minMaxValues.MinValue), this.getEndDecade(minMaxValues.MaxValue), this.axisMode));
        }
        else {
            visibleOffset = ((minMaxValues.MaxValue - minMaxValues.MinValue) / 2) * 0.1;
            this.SetViewportParameters(new ViewportParameters(chartParams.MinX, chartParams.MaxX, minMaxValues.MinValue - visibleOffset, minMaxValues.MaxValue + visibleOffset, this.axisMode));
        }
    }
    //canvasjs RaiseChanged is only raised when user interacts with chart. This function is a default way of handling RangeChanged either by interaction or logic.
    RaiseRangeChanged(newVP) {
        if (this.disableEvents === true) {
            return;
        }
        if (this.debugMode) {
            console.trace("Range changed raised");
        }
        this.dispatchEvent(new CustomEvent('RangeChanged', {
            detail: {
                guid: this.chartGuid,
                viewport: newVP
            }
        }));
        if (this.debugMode) {
            console.log(newVP);
        }
        if (newVP.Type == AxisMode.Date) {
            this.dotNetReference.invokeMethodAsync("ViewportChanged", this.chartGuid, new Date(newVP.MinX).toISOString(), new Date(newVP.MaxX).toISOString(), newVP.MinY, newVP.MaxY, newVP.Type);
        }
        else {
            this.dotNetReference.invokeMethodAsync("ViewportChanged", this.chartGuid, newVP.MinX, newVP.MaxX, newVP.MinY, newVP.MaxY, newVP.Type);
        }
    }
    FindYMinMaxValue(viewport) {
        if (this.mainChart === undefined) {
            console.error("Chart not initialized");
            return { MinValue: 0, MaxValue: 0 };
        }
        if (!viewport) {
            viewport = this.GetViewportParameters();
        }
        let minVisibleValue = undefined;
        let maxVisibleValue = undefined;
        this.mainChart.options.data.forEach((dataset) => {
            dataset.dataPoints.filter(dp => dp.x >= viewport.MinX && dp.x <= viewport.MaxX)
                .forEach((dataparam) => {
                if (maxVisibleValue === undefined || dataparam.y > maxVisibleValue) {
                    maxVisibleValue = dataparam.y;
                }
                if (minVisibleValue === undefined || dataparam.y < minVisibleValue) {
                    minVisibleValue = dataparam.y;
                }
            });
        });
        if (minVisibleValue === undefined || maxVisibleValue === undefined) {
            console.warn("Failed to find min and max value for visible data!");
            console.info(minVisibleValue, maxVisibleValue, viewport);
        }
        const result = {
            MinValue: minVisibleValue !== null && minVisibleValue !== void 0 ? minVisibleValue : 0,
            MaxValue: maxVisibleValue !== null && maxVisibleValue !== void 0 ? maxVisibleValue : 0
        };
        return result;
    }
    getStartDecade(min) {
        if (min <= 0)
            return 0.1; // Safety for Log scale
        var exponent = Math.floor(Math.log10(min));
        return Math.pow(10, exponent);
    }
    getEndDecade(max) {
        if (max <= 0)
            return 0.1; // Safety for Log scale
        var exponent = Math.ceil(Math.log10(max));
        return Math.pow(10, exponent);
    }
    AddMinorLogarithmicGridLines() {
        const viewportParameters = this.GetViewportParameters();
        if (!viewportParameters || viewportParameters.MinX instanceof Date || viewportParameters.MaxX instanceof Date) {
            console.warn("Failed to add logarithmic lines");
            return;
        }
        const xGridLines = [];
        var xStart = this.getStartDecade(viewportParameters.MinX);
        // Generate minor grid lines for x-axis
        for (var decade = xStart; decade < viewportParameters.MaxX; decade *= 10) {
            for (var multiplier = 1; multiplier <= 10; multiplier++) {
                var x = decade * multiplier;
                if (x > viewportParameters.MaxX)
                    break;
                xGridLines.push({
                    value: x,
                    lineDashType: multiplier == 10 ? "line" : "dot",
                    thickness: multiplier == 10 ? 1 : 0.8,
                    color: multiplier == 10 ? "#000000" : "#808080"
                });
            }
        }
        const yGridLines = [];
        var yStart = this.getStartDecade(viewportParameters.MinY);
        // Generate minor grid lines for y-axis
        for (var decade = yStart; decade < viewportParameters.MaxY; decade *= 10) {
            for (var multiplier = 1; multiplier <= 10; multiplier++) {
                var y = decade * multiplier;
                if (y > viewportParameters.MaxY)
                    break;
                yGridLines.push({
                    value: y,
                    lineDashType: multiplier == 10 ? "line" : "dot",
                    thickness: multiplier == 10 ? 1 : 0.8,
                    color: multiplier == 10 ? "#000000" : "#808080"
                });
            }
        }
        this.mainChart.options.axisX.stripLines = xGridLines;
        this.mainChart.options.axisY.stripLines = yGridLines;
    }
}
//# sourceMappingURL=CanvasChart.js.map