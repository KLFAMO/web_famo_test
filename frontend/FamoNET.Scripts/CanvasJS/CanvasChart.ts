//import * as CanvasJS from "../Lib/canvasjs.min.js";
//import * from '../Model/Global/CanvasJS.js';
declare let CanvasJS: any;
import { ChartParameters } from '../Model/ChartParameters.js';
import { ViewportParameters } from '../Model/ViewportParameters.js';
import { DataSet } from "../Model/DataSet.js"


interface IMinMaxValues {
	MinValue: number;
	MaxValue: number
}

export class CanvasChart {
	private mainChart: any = undefined;
	private dotNetReference: any = undefined;	
	private isInitialized: boolean = false;

	constructor(dotNetReference: any) {		
		this.dotNetReference = dotNetReference;
	}

	RangeChanged?: (e: ViewportParameters) => void;		

	InitializeChart(container?: HTMLElement | null, title?: string, logarithmic?: boolean) {
		
		if (container === undefined || container === null) {
			console.error("Failed to initialize chart module. Could not find chart container.")
			return;
		}		

		this.mainChart = new CanvasJS.Chart("chartElement", {
			colorSet: "customColorSet1",
			zoomEnabled: true,
			zoomType: "xy",
			title: {
				text: title
			},
			data: [],
			axisX: {
				//valueFormatString: "#####.0K"
				valueFormatString: logarithmic ? "#.####E+0" : null,
				gridThickness: 1,
				interval: logarithmic ? 1 : null, // Interval between ticks (logarithmic interval)
				minimum: logarithmic ? 0.1 : null, // Minimum value
				maximum: logarithmic ? 1000 : null, // Maximum value
				logarithmic: logarithmic
			},
			axisY: {
				valueFormatString: "#.####E+0",
				logarithmic: logarithmic
			},
			rangeChanged: (e: any) => {
				if (e.type === "reset") {
					return;
				}
				if (this.RangeChanged) {
					this.RangeChanged(new ViewportParameters(e.axisX[0].viewportMinimum,
						e.axisX[0].viewportMaximum,
						e.axisY[0].viewportMinimum,
						e.axisY[0].viewportMaximum));
				}
				
				console.log(`Adding view to history: ${e.axisX[0].viewportMinimum} ${e.axisX[0].viewportMaximum} ${e.axisY[0].viewportMinimum} ${e.axisY[0].viewportMaximum}`)
			},
			stripLines: []
		});				

		this.Render();	
		console.log("Chart object: ", this.mainChart);
	}	

	GetViewportParameters(): ViewportParameters {
		if (this.mainChart === undefined) {
			console.error("Chart not initialized");
			return new ViewportParameters(0, 0, 0, 0);				
		}

		if (this.mainChart.axisX.length < 1 || this.mainChart.axisY.length < 1) {
			console.error("Axis not found: ", this.mainChart);
			return new ViewportParameters(0, 0, 0, 0);
		}

		return new ViewportParameters(
			this.mainChart.axisX[0].get("viewportMinimum"),
			this.mainChart.axisX[0].get("viewportMaximum"),
			this.mainChart.axisY[0].get("viewportMinimum"),
			this.mainChart.axisY[0].get("viewportMaximum"));
	}

	SetChartParameters(chartParams?: ChartParameters): void {
		if (this.mainChart === undefined) {
			console.error("Chart not initialized");
			return;
		}

		if (this.mainChart.axisX.length < 1 || this.mainChart.axisY.length < 1) {
			console.error("Axis not found");
			return;
		}

		if (chartParams === undefined) {
			this.mainChart.axisX[0].set("viewportMaximum", null, false);
			this.mainChart.axisY[0].set("viewportMaximum", null, false);
			this.mainChart.axisX[0].set("viewportMinimum", null, false);
			this.mainChart.axisY[0].set("viewportMinimum", null, false);
			return;
		}

		if (chartParams.Title != undefined) {
			this.mainChart.options.title.text = chartParams.Title;
		}

		if (chartParams.ViewportParameters === undefined) {
			return;
		}

		if (chartParams.ViewportParameters.MinX != undefined) {
			this.mainChart.options.axisX.viewportMinimum = chartParams.ViewportParameters.MinX;
		}

		if (chartParams.ViewportParameters.MaxX != undefined) {
			this.mainChart.options.axisX.viewportMaximum = chartParams.ViewportParameters.MaxX;
		}

		if (chartParams.ViewportParameters.MinY != undefined) {
			this.mainChart.options.axisY.viewportMinimum = chartParams.ViewportParameters.MinY;
		}

		if (chartParams.ViewportParameters.MaxY != undefined) {
			this.mainChart.options.axisY.viewportMaximum = chartParams.ViewportParameters.MaxY;
		}
	}

	Render(): void {
		if (this.mainChart === undefined) {
			console.error("Chart not initialized");
			return;
		}

		if (!this.isInitialized) {
			this.mainChart.render();
			this.isInitialized = true;
		}
		
		if (this.isInitialized) {			
			if (this.mainChart.axisX.length>0 && this.mainChart.axisX[0].get("logarithmic") === true) {
				this.AddMinorLogarithmicGridLines();
			}			
			this.mainChart.render();
		}				
	}

	AddDataSet(dataSet: DataSet): void {
		if (this.mainChart === undefined) {
			console.error("Chart not initialized");
			return;
		}

		const dataSeriesObject : any = {};
		dataSeriesObject.type = "spline";
		dataSeriesObject.dataPoints = dataSet.Collection;

		this.mainChart.options.data.push(dataSeriesObject)
	}

	ClearDataSets(): void {
		if (this.mainChart === undefined) {
			console.error("Chart not initialized");
			return;
		}

		this.mainChart.options.data = [];		
	}

	AdjustToVisibleData(): void {	
		if (this.mainChart === undefined) {
			console.error("Chart not initialized");
			return;
		}

		const chartParams = this.GetViewportParameters();
		const minMaxValues = this.FindYMinMaxValue(chartParams);

		this.SetChartParameters(new ChartParameters(new ViewportParameters(chartParams.MinX, chartParams.MaxX, minMaxValues.MinValue, minMaxValues.MaxValue)))
	}

	private FindYMinMaxValue(chartParams?: ViewportParameters): IMinMaxValues {
		if (this.mainChart === undefined) {
			console.error("Chart not initialized");
			return { MinValue: 0, MaxValue: 0 } as IMinMaxValues;
		}

		if (!chartParams) {
			chartParams = this.GetViewportParameters();
		}
		
		let minVisibleValue: number | undefined = undefined;
		let maxVisibleValue: number | undefined = undefined;

		this.mainChart.options.data.forEach((dataset) => {
			dataset.dataPoints.filter(dp => dp.x >= chartParams.MinX! && dp.x <= chartParams.MaxX!)
				.forEach((dataparam) => {					
					if (maxVisibleValue === undefined || dataparam.y > maxVisibleValue) {
						maxVisibleValue = dataparam.y;
					}
					if (minVisibleValue === undefined || dataparam.y < minVisibleValue) {
						minVisibleValue = dataparam.y;
					}
				});
		});

		if (minVisibleValue === undefined || maxVisibleValue === undefined) {
			console.warn("Failed to find min and max value for visible data!");
			console.info(minVisibleValue, maxVisibleValue, chartParams);
		}

		const result: IMinMaxValues = {
			MinValue: minVisibleValue ?? 0,
			MaxValue: maxVisibleValue ?? 0
		}

		return result;
	}

	private AddMinorLogarithmicGridLines(): void {
		const viewportParameters = this.GetViewportParameters();

		if (!viewportParameters || !viewportParameters.MaxX || !viewportParameters.MaxY) {
			console.warn("Failed to add logarithmic lines");
			return;
		}
		const xGridLines: any = [];

		// Generate minor grid lines for x-axis
		for (var decade = 1; decade < viewportParameters.MaxX; decade *= 10) {
			for (var multiplier = 2; multiplier <= 9; multiplier++) {
				var x = decade * multiplier;
				if (x > viewportParameters.MaxX) break;
				xGridLines.push({
					value: x,
					type: "line",
					axisXType: "logarithmic",
					axisYType: "logarithmic",
					toolTipContent: null,
					lineThickness: 0.5, // Thinner lines for minor grid
					lineColor: "#000000", // Light gray color					
				});
			}
		}

		const yGridLines: any = [];
		// Generate minor grid lines for y-axis
		for (var decade = 1; decade < viewportParameters.MaxY; decade *= 10) {
			for (var multiplier = 2; multiplier <= 9; multiplier++) {
				var x = decade * multiplier;
				if (x > viewportParameters.MaxY) break;
				yGridLines.push({
					value: x,
					type: "line",
					axisXType: "logarithmic",
					axisYType: "logarithmic",
					toolTipContent: null,
					lineThickness: 0.5,
					lineColor: "#000000",				
				});
			}			
		}

		this.mainChart.options.axisX.stripLines = xGridLines;
		this.mainChart.options.axisY.stripLines = yGridLines;
	}
}