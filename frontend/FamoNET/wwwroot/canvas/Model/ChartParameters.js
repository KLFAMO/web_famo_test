import { AxisMode } from "./Enums.js";
export class ChartParameters {
    constructor(title, logarithmic, disableXLabels, disableEvents, invertYAxis, axisMode) {
        this.Title = "";
        this.Logarithmic = false;
        this.DisableXLabels = false;
        this.DisableEvents = false;
        this.InvertYAxis = false;
        this.AxisMode = AxisMode.MJD;
        this.Title = title;
        if (disableXLabels) {
            this.DisableXLabels = disableXLabels;
        }
        if (logarithmic) {
            this.Logarithmic = logarithmic;
        }
        if (disableEvents) {
            this.DisableEvents = disableEvents;
        }
        if (invertYAxis) {
            this.InvertYAxis = invertYAxis;
        }
        if (axisMode) {
            this.AxisMode = axisMode;
        }
    }
}
//# sourceMappingURL=ChartParameters.js.map