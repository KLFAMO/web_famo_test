let mainChart = undefined;

export function CreateEmptyChart(title, type = "spline") {
	mainChart = new CanvasJS.Chart("chartElement", {
		title: {
			text: title
		},
		data: [],
		axisX: {},
		axisY: {}
	});
	mainChart.render();
}

export function AddDataSet(dataParam) {	
	const dataSeriesObject = {};
	dataSeriesObject.type = "spline";
	dataSeriesObject.dataPoints = dataParam;	

	mainChart.options.data.push(dataSeriesObject)		
}

export function SetChartParameters(chartParams) {	
	if (chartParams.title != undefined) {
		mainChart.options.title.text = chartParams.title;
	}

	if (chartParams.minX != undefined) {		
		mainChart.options.axisX.minimum = chartParams.minX;
	}

	if (chartParams.minX != undefined) {
		mainChart.options.axisX.maximum = chartParams.maxX;
	}

	if (chartParams.minX != undefined) {
		mainChart.options.axisY.minimum = chartParams.minY;
	}

	if (chartParams.minX != undefined) {
		mainChart.options.axisY.maximum = chartParams.maxY;
	}	
}

export function GetChartParameters() {
	return [mainChart.axisX[0].get("minimum"), mainChart.axisX[0].get("maximum"), mainChart.axisY[0].get("minimum"), mainChart.axisY[0].get("maximum") ]
}

export function RenderChart() {	
	mainChart.render()
}