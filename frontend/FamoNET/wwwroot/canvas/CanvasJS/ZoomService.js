import { ChartParameters } from "../Model/ChartParameters.js";
import { ViewportParameters } from "../Model/ViewportParameters.js";
export class ZoomService {
    constructor(chart) {
        this.chartHistory = [];
        this.chart = undefined;
        this.chart = chart;
    }
    AddHistory(viewport) {
        this.chartHistory.push(viewport);
    }
    ResetHistory() {
        this.chartHistory = [];
    }
    Unzoom() {
        var _a, _b;
        //pop current	
        var popped = this.chartHistory.pop();
        let i = this.chartHistory.length - 1;
        let previousPosition = i > -1 ? this.chartHistory[i] : undefined;
        if (previousPosition == undefined) {
            (_a = this.chart) === null || _a === void 0 ? void 0 : _a.SetChartParameters(undefined);
            console.log("Previous position not found. Returning to default.");
            return;
        }
        else {
            console.log(`Returning to view: ${previousPosition.axisX[0].viewportMinimum} ${previousPosition.axisX[0].viewportMaximum} ${previousPosition.axisY[0].viewportMinimum} ${previousPosition.axisY[0].viewportMaximum}`);
            let chartParams = new ViewportParameters(previousPosition.axisX[0].viewportMinimum, previousPosition.axisX[0].viewportMaximum, previousPosition.axisY[0].viewportMinimum, previousPosition.axisY[0].viewportMaximum);
            (_b = this.chart) === null || _b === void 0 ? void 0 : _b.SetChartParameters(new ChartParameters(chartParams));
        }
    }
}
//# sourceMappingURL=ZoomService.js.map