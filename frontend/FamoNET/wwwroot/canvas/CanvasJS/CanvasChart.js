import { ViewportParameters } from '../Model/ViewportParameters.js';
import { AxisMode } from "../Model/Enums.js";
export class CanvasChart extends EventTarget {
    constructor(dotNetReference) {
        super();
        this.debugMode = true;
        this.mainChart = undefined;
        this.dotNetReference = undefined;
        this.isInitialized = false;
        this.disableEvents = false;
        this.chartGuid = "";
        this.axisMode = AxisMode.MJD;
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
        this.mainChart = new CanvasJS.Chart(container, {
            colorSet: "customColorSet1",
            zoomEnabled: true,
            zoomType: "xy",
            title: {
                text: chartParameters.Title
            },
            data: [],
            axisX: {
                labelFontColor: chartParameters.DisableXLabels === true ? "transparent" : "#000000",
                valueFormatString: chartParameters.Logarithmic ? "#.####E+0" : null,
                gridThickness: 1,
                interval: chartParameters.Logarithmic ? 1 : null, // Interval between ticks (logarithmic interval)
                minimum: chartParameters.Logarithmic ? 0.1 : null, // Minimum value
                maximum: chartParameters.Logarithmic ? 1000 : null, // Maximum value
                logarithmic: chartParameters.Logarithmic,
                crosshair: {
                    enabled: true,
                    color: "orange",
                    labelFontColor: "#F8F8F8"
                },
            },
            axisY: {
                logarithmic: chartParameters.Logarithmic,
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
        let visibleOffset = ((minMaxValues.MaxValue - minMaxValues.MinValue) / 2) * 0.1;
        this.SetViewportParameters(new ViewportParameters(chartParams.MinX, chartParams.MaxX, minMaxValues.MinValue - visibleOffset, minMaxValues.MaxValue + visibleOffset, this.axisMode));
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
        if (newVP.Type == AxisMode.Date) {
            console.log(newVP);
            this.dotNetReference.invokeMethodAsync("ViewportChanged", this.chartGuid, new Date(newVP.MinX).toISOString(), new Date(newVP.MaxX).toISOString(), newVP.MinY, newVP.MaxY, newVP.Type);
        }
        else {
            console.log(newVP);
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
    AddMinorLogarithmicGridLines() {
        const viewportParameters = this.GetViewportParameters();
        if (!viewportParameters || viewportParameters.MinX instanceof Date || viewportParameters.MaxX instanceof Date) {
            console.warn("Failed to add logarithmic lines");
            return;
        }
        const xGridLines = [];
        // Generate minor grid lines for x-axis
        for (var decade = 1; decade < viewportParameters.MaxX; decade *= 10) {
            for (var multiplier = 2; multiplier <= 9; multiplier++) {
                var x = decade * multiplier;
                if (x > viewportParameters.MaxX)
                    break;
                xGridLines.push({
                    value: x,
                    type: "line",
                    axisXType: "logarithmic",
                    axisYType: "logarithmic",
                    toolTipContent: null,
                    lineThickness: 0.5, // Thinner lines for minor grid
                    lineColor: "#000000", // Light gray color					
                });
            }
        }
        const yGridLines = [];
        // Generate minor grid lines for y-axis
        for (var decade = 1; decade < viewportParameters.MaxY; decade *= 10) {
            for (var multiplier = 2; multiplier <= 9; multiplier++) {
                var x = decade * multiplier;
                if (x > viewportParameters.MaxY)
                    break;
                yGridLines.push({
                    value: x,
                    type: "line",
                    axisXType: "logarithmic",
                    axisYType: "logarithmic",
                    toolTipContent: null,
                    lineThickness: 0.5,
                    lineColor: "#000000",
                });
            }
        }
        this.mainChart.options.axisX.stripLines = xGridLines;
        this.mainChart.options.axisY.stripLines = yGridLines;
    }
}
//# sourceMappingURL=CanvasChart.js.map