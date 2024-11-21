let mainChart = undefined;

CanvasJS.addColorSet("customColorSet1",
	[
     "#4661EE",
     "#EC5657",
     "#1BCDD1",
     "#8FAABB",
     "#B08BEB",
     "#3EA0DD",
     "#F5A52A",
     "#23BFAA",
     "#FAA586",
     "#EB8CC6",
	]);

export function CreateEmptyChart(title, type = "spline") {
	mainChart = new CanvasJS.Chart("chartElement", {
		colorSet: "customColorSet1",
		zoomEnabled: true, 
		title: {
			text: title
		},
		data: [],
		axisX: {
			//valueFormatString: "#####.0K"
		},
		axisY: {
			valueFormatString: "#.00E+0"
		}
	});
	mainChart.render();
}

export function AddDataSet(dataParam) {	
	const dataSeriesObject = {};
	dataSeriesObject.type = "spline";
	dataSeriesObject.dataPoints = dataParam;	

	mainChart.options.data.push(dataSeriesObject)		
}

export function ClearDataSets() {
	mainChart.options.data = []
}

function MJDToDate(e) {
	const mjd = e.value
	//from chatgpt
	
	// MJD to Julian Date offset
	const JD = mjd + 2400000.5;

	// Julian Date to standard date conversion
	const unixTime = (JD - 2440587.5) * 86400000; // Convert JD to Unix time (ms since 1970-01-01)
	const date = new Date(unixTime);

	// Format date to dd.MM.yyyyTHH:mm:ss
	const formattedDate = date.toISOString().replace('T', ' ').substring(0, 19); // ISO format yyyy-MM-ddTHH:mm:ss
	const [yyyy, mm, dd] = [date.getUTCFullYear(), date.getUTCMonth() + 1, date.getUTCDate()];
	const [hh, mi, ss] = [date.getUTCHours(), date.getUTCMinutes(), date.getUTCSeconds()];

	return `${dd.toString().padStart(2, '0')}.${mm.toString().padStart(2, '0')}.${yyyy}T${hh.toString().padStart(2, '0')}:${mi.toString().padStart(2, '0')}:${ss.toString().padStart(2, '0')}`;
}

export function ToggleXLabels() {	
	if (mainChart.options.axisX.labelFormatter == MJDToDate) {
		mainChart.options.axisX.labelFormatter = undefined;
	} else {
		mainChart.options.axisX.labelFormatter = MJDToDate;
	}	
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