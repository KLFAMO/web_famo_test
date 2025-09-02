import { CanvasCore } from "./CanvasJS/CanvasCore.js";
import { ChartParameters } from "./Model/ChartParameters.js";
import { ViewportParameters } from "./Model/ViewportParameters.js";
import { DataPoint } from "./Model/DataPoint.js";
export class Interop {
    constructor() {
        this.canvasCore = undefined;
        this.dotNetReference = undefined;
    }
}
let instances = new Map();
export function SetDotNetReference(guid, dotnetReference) {
    if (instances.has(guid)) {
        console.error("Interop instance already exists!");
        return;
    }
    instances.set(guid, new Interop());
    let instance = instances.get(guid);
    instance.dotNetReference = dotnetReference;
}
;
export function InitializeChart(guid, chartParametersJson) {
    let interop = instances.get(guid);
    interop.canvasCore = new CanvasCore(interop.dotNetReference, guid, new ChartParameters(chartParametersJson.title, chartParametersJson.logarithmic, chartParametersJson.disableXLabels, chartParametersJson.disableEvents, chartParametersJson.invertYAxis, chartParametersJson.axisMode));
}
export function AddDataSet(guid, dataPoints) {
    var _a, _b;
    let interop = instances.get(guid);
    let dataSet = [];
    dataPoints.forEach(dp => {
        if (typeof dp.x == 'string') {
            dataSet.push(new DataPoint(new Date(dp.x), dp.y));
        }
        else if (typeof dp.x == 'number') {
            dataSet.push(new DataPoint(dp.x, dp.y));
        }
    });
    (_b = (_a = interop.canvasCore) === null || _a === void 0 ? void 0 : _a.dataService) === null || _b === void 0 ? void 0 : _b.AddDataSet(dataSet);
}
export function ClearDataSets(guid) {
    var _a, _b;
    let interop = instances.get(guid);
    (_b = (_a = interop.canvasCore) === null || _a === void 0 ? void 0 : _a.dataService) === null || _b === void 0 ? void 0 : _b.ClearDataSets();
}
export function SetChartParameters(guid, chartParameters) {
    var _a;
    let interop = instances.get(guid);
    (_a = interop.canvasCore) === null || _a === void 0 ? void 0 : _a.SetChartParameters(new ChartParameters(chartParameters.title));
}
export function SetViewportParameters(guid, viewportParams) {
    var _a;
    console.log("interop: ", viewportParams);
    let interop = instances.get(guid);
    let chartParameters = new ViewportParameters(viewportParams.minX, viewportParams.maxX, viewportParams.minY, viewportParams.maxY, viewportParams.axisMode);
    (_a = interop.canvasCore) === null || _a === void 0 ? void 0 : _a.SetViewportParameters(chartParameters);
}
export function GetViewportParameters(guid) {
    var _a;
    let interop = instances.get(guid);
    let viewportParameters = (_a = interop.canvasCore) === null || _a === void 0 ? void 0 : _a.GetViewportParameters();
    return [viewportParameters === null || viewportParameters === void 0 ? void 0 : viewportParameters.MinX, viewportParameters === null || viewportParameters === void 0 ? void 0 : viewportParameters.MaxX, viewportParameters === null || viewportParameters === void 0 ? void 0 : viewportParameters.MinY, viewportParameters === null || viewportParameters === void 0 ? void 0 : viewportParameters.MaxY, viewportParameters === null || viewportParameters === void 0 ? void 0 : viewportParameters.Type];
}
export function ResetViewport(guid) {
    var _a;
    let interop = instances.get(guid);
    (_a = interop.canvasCore) === null || _a === void 0 ? void 0 : _a.ResetViewport();
}
export function AdjustToVisibleData(guid) {
    var _a;
    let interop = instances.get(guid);
    (_a = interop.canvasCore) === null || _a === void 0 ? void 0 : _a.AdjustToVisibleData();
}
export function RenderChart(guid) {
    var _a;
    let interop = instances.get(guid);
    (_a = interop.canvasCore) === null || _a === void 0 ? void 0 : _a.RenderChart();
}
export function PopPreviousViewport(guid) {
    var _a, _b;
    let interop = instances.get(guid);
    let previousViewport = (_b = (_a = interop.canvasCore) === null || _a === void 0 ? void 0 : _a.zoomService) === null || _b === void 0 ? void 0 : _b.PopPreviousViewport();
    return [previousViewport === null || previousViewport === void 0 ? void 0 : previousViewport.MinX, previousViewport === null || previousViewport === void 0 ? void 0 : previousViewport.MaxX, previousViewport === null || previousViewport === void 0 ? void 0 : previousViewport.MinY, previousViewport === null || previousViewport === void 0 ? void 0 : previousViewport.MaxY, previousViewport === null || previousViewport === void 0 ? void 0 : previousViewport.Type];
}
export function Dispose(guid) {
    instances.delete(guid);
}
//# sourceMappingURL=interop.js.map