import { CanvasCore } from "./CanvasJS/CanvasCore.js";
import { ChartParameters } from "./Model/ChartParameters.js";
import { ViewportParameters } from "./Model/ViewportParameters.js"
import { DataSet } from "./Model/DataSet.js"
import { DataPoint } from "./Model/DataPoint.js"

let canvasCore: CanvasCore | undefined = undefined;
let dotNetReference: any = undefined;

export function SetDotNetReference(reference: any) {
	dotNetReference = reference;	
};

export function InitializeChart(title?: string, logarithmic?: boolean) {
	canvasCore = new CanvasCore(dotNetReference, title, logarithmic);
}

export function Unzoom() {
	canvasCore?.zoomService?.Unzoom();
}

export function AddDataSet(dataSet) {	
	let dataPoints : Array<DataPoint> = [];
	dataSet.forEach(dp =>
	{
		dataPoints.push(new DataPoint(dp.x, dp.y));
	})
	
	canvasCore?.dataService?.AddDataSet(new DataSet(dataPoints));
}

export function ClearDataSets() {
	canvasCore?.dataService?.ClearDataSets();
}

export function SetChartParameters(newParams) {
	let chartParameters = new ChartParameters(new ViewportParameters(newParams.minX, newParams.maxX, newParams.minY, newParams.maxY), newParams.title);	
	canvasCore?.SetChartParameters(chartParameters);
}

export function GetViewportParameters() {
	let viewportParameters = canvasCore?.GetChartParameters();
	return [viewportParameters?.MinX, viewportParameters?.MaxX, viewportParameters?.MinY, viewportParameters?.MaxY]
}

export function AdjustToVisibleData() {
	canvasCore?.AdjustToVisibleData();
}

export function RenderChart() {
	canvasCore?.RenderChart();
}

export function ConvertToDate() {
	canvasCore?.dataService?.ConvertToDate();
}

export function ConvertToMjd() {
	canvasCore?.dataService?.ConvertToMjd();
}