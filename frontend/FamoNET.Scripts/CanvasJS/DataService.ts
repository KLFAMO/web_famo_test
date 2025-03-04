import { DataSet } from "../Model/DataSet.js";
import { CanvasChart } from "./CanvasChart.js";
import { ChartParameters } from "../Model/ChartParameters.js";
import { ViewportParameters } from "../Model/ViewportParameters.js";
import { AxisMode } from "../Model/Enums.js";

export class DataService {
    private chart: CanvasChart | undefined = undefined;
    private axisMode: AxisMode = AxisMode.MJD;

    private dataSets: Array<DataSet> = [];    

    constructor(chart: CanvasChart) {
        this.chart = chart;        
    }

    AddDataSet(dataSet: DataSet) {           
        this.dataSets.push(dataSet);
        this.chart?.AddDataSet(dataSet);        
    }

    ClearDataSets() {
        this.dataSets = [];
        this.chart?.ClearDataSets();
    }

    Reload() {
        this.chart?.ClearDataSets();
        this.dataSets.forEach((dataSet) =>
        {
            this.chart?.AddDataSet(dataSet);
        });
    }

    ConvertToDate() {           
        this.dataSets.forEach((dataSet: DataSet) => dataSet.ConvertToDate());
        this.Reload();

        if (this.dataSets.length < 1) {
            console.log("No datasets to convert");
            return;
        }

        let viewportParams = new ViewportParameters((this.dataSets[0].Collection[0].x as Date).getTime(),
                                                    (this.dataSets[0].Collection[this.dataSets[0].Collection.length - 1].x as Date).getTime());

        this.chart?.SetChartParameters(new ChartParameters(viewportParams));        
    }

    ConvertToMjd() {
        
        this.dataSets.forEach((dataSet: DataSet) => dataSet.ConvertToMjd());
        this.Reload();

        let viewportParams = new ViewportParameters(this.dataSets[0].Collection[0].x,
                                                    this.dataSets[0].Collection[this.dataSets[0].Collection.length - 1].x);

        this.chart?.SetChartParameters(new ChartParameters(viewportParams));        
    }
}