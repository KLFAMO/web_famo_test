import { CanvasCore } from "./CanvasJS/CanvasCore.js";
import { ChartParameters } from "./Model/ChartParameters.js";
import { ViewportParameters } from "./Model/ViewportParameters.js"

import { DataPoint } from "./Model/DataPoint.js"
import { AxisMode } from "./Model/Enums.js";

export class Interop {
	canvasCore: CanvasCore | undefined = undefined;
	dotNetReference: any = undefined;	
}

let instances = new Map<string, Interop>();

export function SetDotNetReference(guid: string, dotnetReference: any) {
	if (instances.has(guid)) {
		console.error("Interop instance already exists!");
		return;
	}

	instances.set(guid, new Interop());

	let instance = instances.get(guid);
	instance!.dotNetReference = dotnetReference;
};

export function InitializeChart(guid: string, chartParametersJson) {
	let interop = instances.get(guid);
	interop!.canvasCore = new CanvasCore(interop!.dotNetReference, guid, new ChartParameters(chartParametersJson.title, chartParametersJson.logarithmic, chartParametersJson.disableXLabels, chartParametersJson.disableEvents, chartParametersJson.invertYAxis, chartParametersJson.axisMode));
}

export function AddDataSet(guid: string, dataPoints) {	
	let interop = instances.get(guid);	
	let dataSet: DataPoint[] = [];
	dataPoints.forEach(dp => {
		if (typeof dp.x == 'string') {
			dataSet.push(new DataPoint(new Date(dp.x), dp.y));
		}
		else if (typeof dp.x == 'number') {
			dataSet.push(new DataPoint(dp.x, dp.y));			
		}		
	})	
	
	interop!.canvasCore?.dataService?.AddDataSet(dataSet);
}

export function ClearDataSets(guid: string) {
	let interop = instances.get(guid);
	interop!.canvasCore?.dataService?.ClearDataSets();
}

export function SetChartParameters(guid:string, chartParameters) {
	let interop = instances.get(guid);
	interop!.canvasCore?.SetChartParameters(new ChartParameters(chartParameters.title));
}

export function SetViewportParameters(guid: string, viewportParams) {
	console.log("interop: ", viewportParams);
	let interop = instances.get(guid);	
	let chartParameters = new ViewportParameters(viewportParams.minX, viewportParams.maxX, viewportParams.minY, viewportParams.maxY, viewportParams.axisMode);
	interop!.canvasCore?.SetViewportParameters(chartParameters);
}

export function GetViewportParameters(guid:string) {
	let interop = instances.get(guid);
	let viewportParameters = interop!.canvasCore?.GetViewportParameters();
	return [viewportParameters?.MinX, viewportParameters?.MaxX, viewportParameters?.MinY, viewportParameters?.MaxY, viewportParameters?.Type]
}

export function ResetViewport(guid:string) {
	let interop = instances.get(guid);
	interop!.canvasCore?.ResetViewport();
}

export function AdjustToVisibleData(guid:string) {
	let interop = instances.get(guid);
	interop!.canvasCore?.AdjustToVisibleData();
}

export function RenderChart(guid:string) {
	let interop = instances.get(guid);
	interop!.canvasCore?.RenderChart();
}

export function PopPreviousViewport(guid:string) {
	let interop = instances.get(guid);
	let previousViewport = interop!.canvasCore?.zoomService?.PopPreviousViewport();	
	return [previousViewport?.MinX, previousViewport?.MaxX, previousViewport?.MinY, previousViewport?.MaxY, previousViewport?.Type]
}

export function Dispose(guid: string) {
	instances.delete(guid);
}