import { ChartParameters } from '../Model/ChartParameters.js';
import { ViewportParameters } from '../Model/ViewportParameters.js';
import { ZoomService } from "./ZoomService.js"
import { CanvasChart } from "./CanvasChart.js"
import { DataService } from "./DataService.js"

export class CanvasCore {
	private dotNetReference: any = undefined;
	private canvasChart: CanvasChart | undefined = undefined;

	public zoomService: ZoomService | undefined = undefined; 	
	public dataService: DataService | undefined = undefined;

	constructor(dotNetReference, containerGuid: string, chartParameters: ChartParameters) {		
		this.canvasChart = new CanvasChart(dotNetReference);
		this.canvasChart.InitializeChart(containerGuid, chartParameters);
		

		this.zoomService = new ZoomService(this.canvasChart);
		this.dataService = new DataService(this.canvasChart);
	}	

	GetViewportParameters(): ViewportParameters {
		return this.canvasChart?.GetViewportParameters()!;
	}

	SetChartParameters(chartParameters: ChartParameters) {		
		this.canvasChart?.SetChartParameters(chartParameters);
	}

	SetViewportParameters(vp: ViewportParameters) {
		this.canvasChart?.SetViewportParameters(vp);								
	}

	AdjustToVisibleData() {
		this.canvasChart?.AdjustToVisibleData();
	}

	ResetViewport() {
		this.canvasChart?.ResetViewport();
	}
	
	RenderChart() {
		this.canvasChart?.Render();
	}	
}