import { CanvasCore } from "./CanvasJS/CanvasCore.js";
import { ChartParameters } from "./Model/ChartParameters.js";
import { ViewportParameters } from "./Model/ViewportParameters.js";
import { DataSet } from "./Model/DataSet.js";
import { DataPoint } from "./Model/DataPoint.js";
let canvasCore = undefined;
let dotNetReference = undefined;
export function SetDotNetReference(reference) {
    dotNetReference = reference;
}
;
export function InitializeChart(title, logarithmic) {
    canvasCore = new CanvasCore(dotNetReference, title, logarithmic);
}
export function Unzoom() {
    var _a;
    (_a = canvasCore === null || canvasCore === void 0 ? void 0 : canvasCore.zoomService) === null || _a === void 0 ? void 0 : _a.Unzoom();
}
export function AddDataSet(dataSet) {
    var _a;
    let dataPoints = [];
    dataSet.forEach(dp => {
        dataPoints.push(new DataPoint(dp.x, dp.y));
    });
    (_a = canvasCore === null || canvasCore === void 0 ? void 0 : canvasCore.dataService) === null || _a === void 0 ? void 0 : _a.AddDataSet(new DataSet(dataPoints));
}
export function ClearDataSets() {
    var _a;
    (_a = canvasCore === null || canvasCore === void 0 ? void 0 : canvasCore.dataService) === null || _a === void 0 ? void 0 : _a.ClearDataSets();
}
export function SetChartParameters(newParams) {
    let chartParameters = new ChartParameters(new ViewportParameters(newParams.minX, newParams.maxX, newParams.minY, newParams.maxY), newParams.title);
    canvasCore === null || canvasCore === void 0 ? void 0 : canvasCore.SetChartParameters(chartParameters);
}
export function GetViewportParameters() {
    let viewportParameters = canvasCore === null || canvasCore === void 0 ? void 0 : canvasCore.GetChartParameters();
    return [viewportParameters === null || viewportParameters === void 0 ? void 0 : viewportParameters.MinX, viewportParameters === null || viewportParameters === void 0 ? void 0 : viewportParameters.MaxX, viewportParameters === null || viewportParameters === void 0 ? void 0 : viewportParameters.MinY, viewportParameters === null || viewportParameters === void 0 ? void 0 : viewportParameters.MaxY];
}
export function AdjustToVisibleData() {
    canvasCore === null || canvasCore === void 0 ? void 0 : canvasCore.AdjustToVisibleData();
}
export function RenderChart() {
    canvasCore === null || canvasCore === void 0 ? void 0 : canvasCore.RenderChart();
}
export function ConvertToDate() {
    var _a;
    (_a = canvasCore === null || canvasCore === void 0 ? void 0 : canvasCore.dataService) === null || _a === void 0 ? void 0 : _a.ConvertToDate();
}
export function ConvertToMjd() {
    var _a;
    (_a = canvasCore === null || canvasCore === void 0 ? void 0 : canvasCore.dataService) === null || _a === void 0 ? void 0 : _a.ConvertToMjd();
}
//# sourceMappingURL=interop.js.map