import { ZoomService } from "./ZoomService.js";
import { CanvasChart } from "./CanvasChart.js";
import { DataService } from "./DataService.js";
export class CanvasCore {
    constructor(dotNetReference, title, logarithmic) {
        this.dotNetReference = undefined;
        this.zoomService = undefined;
        this.canvasChart = undefined;
        this.dataService = undefined;
        let container = document.getElementById("chartElement");
        container.oncontextmenu = () => {
            var _a, _b;
            (_a = this.zoomService) === null || _a === void 0 ? void 0 : _a.Unzoom();
            (_b = this.canvasChart) === null || _b === void 0 ? void 0 : _b.Render();
            return false;
        };
        this.canvasChart = new CanvasChart(dotNetReference);
        this.canvasChart.InitializeChart(container, title, logarithmic);
        this.zoomService = new ZoomService(this.canvasChart);
        this.dataService = new DataService(this.canvasChart);
    }
    GetChartParameters() {
        var _a;
        return (_a = this.canvasChart) === null || _a === void 0 ? void 0 : _a.GetViewportParameters();
    }
    SetChartParameters(chartParameters) {
        var _a;
        (_a = this.canvasChart) === null || _a === void 0 ? void 0 : _a.SetChartParameters(chartParameters);
    }
    AdjustToVisibleData() {
        var _a;
        (_a = this.canvasChart) === null || _a === void 0 ? void 0 : _a.AdjustToVisibleData();
    }
    RenderChart() {
        var _a;
        (_a = this.canvasChart) === null || _a === void 0 ? void 0 : _a.Render();
    }
}
//# sourceMappingURL=CanvasCore.js.map