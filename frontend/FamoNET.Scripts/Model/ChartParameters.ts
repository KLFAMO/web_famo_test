import { AxisMode } from "./Enums.js";
import { ViewportParameters } from "./ViewportParameters.js"

export class ChartParameters {    
    Title: string = "";
    Logarithmic: boolean = false;    
    DisableXLabels: boolean = false;
    DisableEvents: boolean = false;
    InvertYAxis: boolean = false;
    AxisMode: AxisMode = AxisMode.MJD

    public constructor(title: string, logarithmic?: boolean, disableXLabels?: boolean, disableEvents?: boolean, invertYAxis?: boolean, axisMode?: AxisMode) {        
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