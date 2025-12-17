export class DataService {
    constructor(chart) {
        this.dataSets = [];
        this.chart = chart;
        this.boundHandleRangeChanged = this.HandleRangeChange.bind(this);
        this.chart.addEventListener('RangeChanged', this.boundHandleRangeChanged);
    }
    destroy() {
        this.chart.removeEventListener('RangeChanged', this.boundHandleRangeChanged);
    }
    HandleRangeChange(e) {
    }
    AddDataSet(dataSet) {
        var _a, _b;
        if (this.dataSets.length < 1) {
            this.dataSets.push(dataSet);
            (_a = this.chart) === null || _a === void 0 ? void 0 : _a.AddDataSet(dataSet);
            //this.chart?.Render(); //for values to take effect for AdjustToVisibleData to work correctly
            //this.chart?.AdjustToVisibleData();
        }
        else {
            this.dataSets.push(dataSet);
            (_b = this.chart) === null || _b === void 0 ? void 0 : _b.AddDataSet(dataSet);
        }
    }
    ClearDataSets() {
        var _a;
        this.dataSets = [];
        (_a = this.chart) === null || _a === void 0 ? void 0 : _a.ClearDataSets();
    }
}
//# sourceMappingURL=DataService.js.map