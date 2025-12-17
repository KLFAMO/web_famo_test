import { CanvasChart } from "./CanvasChart.js";
import { ChartParameters } from "../Model/ChartParameters.js";
import { ViewportParameters } from "../Model/ViewportParameters.js";
import { AxisMode } from "../Model/Enums.js";
import { DataPoint } from "../Model/DataPoint.js"
import { title } from "process";

export class DataService {
    private chart: CanvasChart;
    private dataSets: Array<DataPoint[]> = [];    
    //To maintain "this" context
    private boundHandleRangeChanged: (e: Event) => void;

    constructor(chart: CanvasChart) {
        this.chart = chart;        
        this.boundHandleRangeChanged = this.HandleRangeChange.bind(this);
        this.chart.addEventListener('RangeChanged', this.boundHandleRangeChanged);                           
    }

    public destroy() {        
        this.chart.removeEventListener('RangeChanged', this.boundHandleRangeChanged)
    }

    HandleRangeChange(e: Event): void {
        
    }

    AddDataSet(dataSet: DataPoint[]) {        
        if (this.dataSets.length < 1) {
            this.dataSets.push(dataSet);                       
            this.chart?.AddDataSet(dataSet);

            //this.chart?.Render(); //for values to take effect for AdjustToVisibleData to work correctly
            //this.chart?.AdjustToVisibleData();
        }
        else {
            this.dataSets.push(dataSet); 
            this.chart?.AddDataSet(dataSet);
        }                            
    }   

    ClearDataSets() {
        this.dataSets = [];            
        this.chart?.ClearDataSets();
    }                
}