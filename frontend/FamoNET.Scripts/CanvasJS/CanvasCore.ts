import { ChartParameters } from '../Model/ChartParameters.js';
import { ViewportParameters } from '../Model/ViewportParameters.js';
import { ZoomService } from "./ZoomService.js"
import { CanvasChart } from "./CanvasChart.js"
import { DataService } from "./DataService.js"

export class CanvasCore {
	private dotNetReference: any = undefined;
	public zoomService: ZoomService | undefined = undefined; 
	private canvasChart: CanvasChart | undefined = undefined;
	public dataService: DataService | undefined = undefined;

	constructor(dotNetReference, title?: string, logarithmic?: boolean) {
		let container = document.getElementById("chartElement");
		container!.oncontextmenu = () => {
			this.zoomService?.Unzoom();
			this.canvasChart?.Render();
			return false;
		}

		this.canvasChart = new CanvasChart(dotNetReference);		
		this.canvasChart.InitializeChart(container, title, logarithmic);

		this.zoomService = new ZoomService(this.canvasChart);
		this.dataService = new DataService(this.canvasChart);
	}	

	GetChartParameters(): ViewportParameters {
		return this.canvasChart?.GetViewportParameters()!;
	}

	SetChartParameters(chartParameters: ChartParameters) {
		this.canvasChart?.SetChartParameters(chartParameters);
	}

	AdjustToVisibleData() {
		this.canvasChart?.AdjustToVisibleData();
	}

	RenderChart() {
		this.canvasChart?.Render();
	}
}