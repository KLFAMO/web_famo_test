import { ChartParameters } from "../Model/ChartParameters.js";
import { ViewportParameters } from "../Model/ViewportParameters.js";
import { AxisMode } from "../Model/Enums.js";
export class DataService {
    constructor(chart) {
        this.chart = undefined;
        this.axisMode = AxisMode.MJD;
        this.dataSets = [];
        this.chart = chart;
    }
    AddDataSet(dataSet) {
        var _a;
        this.dataSets.push(dataSet);
        (_a = this.chart) === null || _a === void 0 ? void 0 : _a.AddDataSet(dataSet);
    }
    ClearDataSets() {
        var _a;
        this.dataSets = [];
        (_a = this.chart) === null || _a === void 0 ? void 0 : _a.ClearDataSets();
    }
    Reload() {
        var _a;
        (_a = this.chart) === null || _a === void 0 ? void 0 : _a.ClearDataSets();
        this.dataSets.forEach((dataSet) => {
            var _a;
            (_a = this.chart) === null || _a === void 0 ? void 0 : _a.AddDataSet(dataSet);
        });
    }
    ConvertToDate() {
        var _a;
        this.dataSets.forEach((dataSet) => dataSet.ConvertToDate());
        this.Reload();
        if (this.dataSets.length < 1) {
            console.log("No datasets to convert");
            return;
        }
        let viewportParams = new ViewportParameters(this.dataSets[0].Collection[0].x.getTime(), this.dataSets[0].Collection[this.dataSets[0].Collection.length - 1].x.getTime());
        (_a = this.chart) === null || _a === void 0 ? void 0 : _a.SetChartParameters(new ChartParameters(viewportParams));
    }
    ConvertToMjd() {
        var _a;
        this.dataSets.forEach((dataSet) => dataSet.ConvertToMjd());
        this.Reload();
        let viewportParams = new ViewportParameters(this.dataSets[0].Collection[0].x, this.dataSets[0].Collection[this.dataSets[0].Collection.length - 1].x);
        (_a = this.chart) === null || _a === void 0 ? void 0 : _a.SetChartParameters(new ChartParameters(viewportParams));
    }
}
//# sourceMappingURL=DataService.js.map