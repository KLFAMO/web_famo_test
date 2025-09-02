import { ChartParameters } from "../Model/ChartParameters.js";
import { ViewportParameters } from "../Model/ViewportParameters.js"
import { CanvasChart } from "./CanvasChart.js"

export class ZoomService {    
	private chartHistory: Array<ViewportParameters> = []	
	private chart: CanvasChart;	

	//To maintain "this" context
	private boundHandleRangeChanged: (e: Event) => void;
	private boundHandleRangeReset: (e: Event) => void;

	constructor(chart: CanvasChart) {
		this.chart = chart;

		this.boundHandleRangeChanged = this.HandleRangeChanged.bind(this);		
		this.boundHandleRangeReset = this.HandleRangeReset.bind(this);		

		this.chart.addEventListener('RangeChanged', this.boundHandleRangeChanged)
		this.chart.addEventListener('RangeReset', this.boundHandleRangeReset)		
	}

	public destroy() {
		this.chart.removeEventListener('RangeChanged', this.boundHandleRangeChanged);
		this.chart.removeEventListener('RangeReset', this.boundHandleRangeReset)
	}

	HandleRangeChanged(e: Event) {
		const viewport = (e as CustomEvent).detail.viewport as ViewportParameters;
		
		//console.log(`Adding view to history: ${viewport.MinX} ${viewport.MaxX} ${viewport.MinY} ${viewport.MaxY}`)
		this.chartHistory.push(viewport);
	}

	HandleRangeReset(e: Event) {
		this.chartHistory = [];
		console.log("History reset");
	}			

	/**
	 * Pops previous viewport from stack
	 * @return{ViewportParameters | undefined} ViewportParameters on success pop or undefined on history not found.
	 */
	PopPreviousViewport(): ViewportParameters | undefined{
		//console.log("Current history: ", this.chartHistory);
		
		var currentPosition = this.chartHistory.pop();		
		var previousPosition = this.chartHistory.pop();
		
		//let i = this.chartHistory.length - 1;
		//let previousPosition = i > -1 ? this.chartHistory[i] : undefined;
		

		if (previousPosition == undefined) {
			this.chart?.ResetViewport()				
			console.log("Previous position not found. Returning to default.");		
			return undefined;
		}
		else {
			//console.log("Returning to view:", previousPosition);
							
			return previousPosition;
		}		
	}	
}
