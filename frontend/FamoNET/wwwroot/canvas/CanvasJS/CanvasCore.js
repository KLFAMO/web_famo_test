import { ZoomService } from "./ZoomService.js";
import { CanvasChart } from "./CanvasChart.js";
import { DataService } from "./DataService.js";
export class CanvasCore {
    constructor(dotNetReference, containerGuid, chartParameters) {
        this.dotNetReference = undefined;
        this.canvasChart = undefined;
        this.zoomService = undefined;
        this.dataService = undefined;
        this.canvasChart = new CanvasChart(dotNetReference);
        this.canvasChart.InitializeChart(containerGuid, chartParameters);
        this.zoomService = new ZoomService(this.canvasChart);
        this.dataService = new DataService(this.canvasChart);
    }
    GetViewportParameters() {
        var _a;
        return (_a = this.canvasChart) === null || _a === void 0 ? void 0 : _a.GetViewportParameters();
    }
    SetChartParameters(chartParameters) {
        var _a;
        (_a = this.canvasChart) === null || _a === void 0 ? void 0 : _a.SetChartParameters(chartParameters);
    }
    SetViewportParameters(vp) {
        var _a;
        (_a = this.canvasChart) === null || _a === void 0 ? void 0 : _a.SetViewportParameters(vp);
    }
    AdjustToVisibleData() {
        var _a;
        (_a = this.canvasChart) === null || _a === void 0 ? void 0 : _a.AdjustToVisibleData();
    }
    ResetViewport() {
        var _a;
        (_a = this.canvasChart) === null || _a === void 0 ? void 0 : _a.ResetViewport();
    }
    RenderChart() {
        var _a;
        (_a = this.canvasChart) === null || _a === void 0 ? void 0 : _a.Render();
    }
}
//# sourceMappingURL=CanvasCore.js.map