import { ChartParameters } from "../Model/ChartParameters.js";
import { ViewportParameters } from "../Model/ViewportParameters.js"
import { CanvasChart } from "./CanvasChart.js"

export class ZoomService {    
	private chartHistory: Array<any> = []	
	private chart: CanvasChart | undefined = undefined;	

	constructor(chart: CanvasChart) {
		this.chart = chart;
	}

	AddHistory(viewport: any): void {
		this.chartHistory.push(viewport);
	}

	ResetHistory() : void {
		this.chartHistory = [];
	}	

	Unzoom() : void {		
		//pop current	
		var popped = this.chartHistory.pop();		

		let i = this.chartHistory.length - 1;
		let previousPosition = i > -1 ? this.chartHistory[i] : undefined;
		

		if (previousPosition == undefined) {			
			this.chart?.SetChartParameters(undefined)				
			console.log("Previous position not found. Returning to default.");		
			return;
		}
		else {			
			console.log(`Returning to view: ${previousPosition.axisX[0].viewportMinimum} ${previousPosition.axisX[0].viewportMaximum} ${previousPosition.axisY[0].viewportMinimum} ${previousPosition.axisY[0].viewportMaximum}`)
			let chartParams = new ViewportParameters(previousPosition.axisX[0].viewportMinimum,
				previousPosition.axisX[0].viewportMaximum,
				previousPosition.axisY[0].viewportMinimum,
				previousPosition.axisY[0].viewportMaximum);				

			this.chart?.SetChartParameters(new ChartParameters(chartParams));
		}		
	}	
}
