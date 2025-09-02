export class ZoomService {
    constructor(chart) {
        this.chartHistory = [];
        this.chart = chart;
        this.boundHandleRangeChanged = this.HandleRangeChanged.bind(this);
        this.boundHandleRangeReset = this.HandleRangeReset.bind(this);
        this.chart.addEventListener('RangeChanged', this.boundHandleRangeChanged);
        this.chart.addEventListener('RangeReset', this.boundHandleRangeReset);
    }
    destroy() {
        this.chart.removeEventListener('RangeChanged', this.boundHandleRangeChanged);
        this.chart.removeEventListener('RangeReset', this.boundHandleRangeReset);
    }
    HandleRangeChanged(e) {
        const viewport = e.detail.viewport;
        //console.log(`Adding view to history: ${viewport.MinX} ${viewport.MaxX} ${viewport.MinY} ${viewport.MaxY}`)
        this.chartHistory.push(viewport);
    }
    HandleRangeReset(e) {
        this.chartHistory = [];
        console.log("History reset");
    }
    /**
     * Pops previous viewport from stack
     * @return{ViewportParameters | undefined} ViewportParameters on success pop or undefined on history not found.
     */
    PopPreviousViewport() {
        //console.log("Current history: ", this.chartHistory);
        var _a;
        var currentPosition = this.chartHistory.pop();
        var previousPosition = this.chartHistory.pop();
        //let i = this.chartHistory.length - 1;
        //let previousPosition = i > -1 ? this.chartHistory[i] : undefined;
        if (previousPosition == undefined) {
            (_a = this.chart) === null || _a === void 0 ? void 0 : _a.ResetViewport();
            console.log("Previous position not found. Returning to default.");
            return undefined;
        }
        else {
            //console.log("Returning to view:", previousPosition);
            return previousPosition;
        }
    }
}
//# sourceMappingURL=ZoomService.js.map