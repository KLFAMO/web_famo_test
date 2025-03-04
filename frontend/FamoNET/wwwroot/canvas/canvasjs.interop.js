let dotNetReference = undefined;

let mainChart = undefined;
let labelXMode = "MJD";
let chartHistory = [];

let hasZoomed = false;

export function SetDotNetReference(reference) {
	dotNetReference = reference;
};


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

export function InitializeChart(title, type = "spline") {
	document.getElementById("chartElement").oncontextmenu = () => {
		dotNetReference.invokeMethodAsync('Unzoom');
		return false;
	}

	mainChart = new CanvasJS.Chart("chartElement", {
		colorSet: "customColorSet1",
		zoomEnabled: true,
		zoomType: "xy",
		title: {
			text: title
		},
		data: [],
		axisX: {
			//valueFormatString: "#####.0K"
		},
		axisY: {
			valueFormatString: "#.####E+0"			
		},
		rangeChanged: (e) => {
			if (e.type === "reset") {
				chartHistory = [];
				hasZoomed = false;
				return;
			}
			hasZoomed = true;
			console.log(`Adding view to history: ${e.axisX[0].viewportMinimum} ${e.axisX[0].viewportMaximum} ${e.axisY[0].viewportMinimum} ${e.axisY[0].viewportMaximum}`)
			chartHistory.push(e);
		}

	});
	mainChart.render();
	console.log(mainChart);
}

export function AddDataSet(dataParam) {
	const dataSeriesObject = {};
	dataSeriesObject.type = "spline";
	dataSeriesObject.dataPoints = dataParam;

	mainChart.options.data.push(dataSeriesObject)
}

export function ClearDataSets() {
	mainChart.options.data = []
	labelXMode = "MJD";
}

function MJDToDate(e) {
	const mjd = e
	//from chatgpt

	// MJD to Julian Date offset
	const JD = mjd + 2400000.5;

	// Julian Date to standard date conversion
	const unixTime = (JD - 2440587.5) * 86400000; // Convert JD to Unix time (ms since 1970-01-01)
	const date = new Date(unixTime);

	// Format date to dd.MM.yyyyTHH:mm:ss	
	const [yyyy, mm, dd] = [date.getUTCFullYear(), date.getUTCMonth() + 1, date.getUTCDate()];
	const [hh, mi, ss] = [date.getUTCHours(), date.getUTCMinutes(), date.getUTCSeconds()];

	return new Date(yyyy, mm, dd, hh, mi, ss)
}

export function ToggleXLabels() {
	if (labelXMode == "MJD") {
		mainChart.options.data.forEach((dataset) => {
			let saveMjd = dataset.originalx === undefined;
			if (saveMjd) {
				dataset.originalx = [];
			}

			dataset.dataPoints.forEach((dataparam) => {
				if (saveMjd) {
					dataset.originalx.push(dataparam.x);
				}
				dataparam.x = MJDToDate(dataparam.x);
			});
		});
		labelXMode = "Date";
	}
	else {
		mainChart.options.data.forEach((dataset) => {
			for (let i = 0; i < dataset.dataPoints.length; ++i) {
				dataset.dataPoints[i].x = dataset.originalx[i];
			}
		});
		labelXMode = "MJD";
	}
}

//export function SetChartParameters(chartParams) {
//	if (chartParams.title != undefined) {
//		mainChart.options.title.text = chartParams.title;
//	}

//	if (chartParams.minX != undefined) {
//		mainChart.axisX[0].set("viewportMinimum", chartParams.minX, false);
//	}

//	if (chartParams.maxX != undefined) {
//		mainChart.axisX[0].set("viewportMaximum", chartParams.maxX, false);
//	}

//	if (chartParams.minY != undefined) {
//		mainChart.axisY[0].set("viewportMinimum", chartParams.minY, false);
//	}

//	if (chartParams.maxY != undefined) {
//		mainChart.axisY[0].set("viewportMaximum", chartParams.maxY, false);
//	}
//}

export function SetChartParameters(chartParams) {
	if (chartParams.title != undefined) {
		mainChart.options.title.text = chartParams.title;
	}

	if (chartParams.minX != undefined) {
		mainChart.options.axisX.viewportMinimum = chartParams.minX;		
	}

	if (chartParams.maxX != undefined) {
		mainChart.options.axisX.viewportMaximum = chartParams.maxX;		
	}

	if (chartParams.minY != undefined) {
		mainChart.options.axisY.viewportMinimum = chartParams.minY;		
	}

	if (chartParams.maxY != undefined) {
		mainChart.options.axisY.viewportMaximum = chartParams.maxY;		
	}
}



export function AdjustToVisibleData() {
	let chartParams = GetViewportParameters();

	let minVisibleValue = undefined;
	let maxVisibleValue = undefined;

	mainChart.options.data.forEach((dataset) => {		
		dataset.dataPoints.filter(dp => dp.x >= chartParams[0] && dp.x <= chartParams[1]).forEach((dataparam) => {
			console.log("Point " + dataparam.x + " " + dataparam.y);
			if (dataparam.y > maxVisibleValue || maxVisibleValue === undefined) {
				maxVisibleValue = dataparam.y;
			}
			if (dataparam.y < minVisibleValue || minVisibleValue === undefined) {
				minVisibleValue = dataparam.y;
			}			
		});
	});	

	//console.log("x (min max): " + chartParams[0] + " " + chartParams[1]);
	//console.log("MinVisibleValue: " + minVisibleValue);
	//console.log("MaxVisibleValue: " + maxVisibleValue);
	//console.log(mainChart.axisX[0].get("viewportMaximum"));
	//console.log(mainChart.axisX[0].get("viewportMinimum"));	

	const height = maxVisibleValue - minVisibleValue;
	const offset = height * 0.1;

	let newParams = {
		minY : minVisibleValue-offset,
		maxY : maxVisibleValue+offset
	};

	SetChartParameters(newParams);
}

//export function GetChartParameters() {
//	return [mainChart.axisX[0].get("minimum"), mainChart.axisX[0].get("maximum"), mainChart.axisY[0].get("minimum"), mainChart.axisY[0].get("maximum")]
//}

export function GetChartParameters() {
	return [mainChart.axisX[0].viewportMinimum, mainChart.axisX[0].viewportMaximum, mainChart.axisY[0].viewportMinimum, mainChart.axisY[0].viewportMaximum]
}

export function GetViewportParameters() {
	return [mainChart.axisX[0].get("viewportMinimum"), mainChart.axisX[0].get("viewportMaximum"), mainChart.axisY[0].get("viewportMinimum"), mainChart.axisY[0].get("viewportMaximum")]
}

export function Unzoom() {
	console.log("Unzooming.");

	//pop current	
	var popped = chartHistory.pop();
	if (popped !== undefined) {
		console.log(`Popped view: ${popped.axisX[0].viewportMinimum} ${popped.axisX[0].viewportMaximum} ${popped.axisY[0].viewportMinimum} ${popped.axisY[0].viewportMaximum}`);
	}
	

	let i = chartHistory.length - 1;
	let previousPosition = i > -1 ? chartHistory[i] : undefined;

	if (previousPosition !== undefined) {
		
	}
	
	if (previousPosition == undefined) {
		mainChart.axisX[0].set("viewportMaximum", null, false);
		mainChart.axisY[0].set("viewportMaximum", null, false);
		mainChart.axisX[0].set("viewportMinimum", null, false);
		mainChart.axisY[0].set("viewportMinimum", null, false);
		console.log("Previous position not found. Returning to default.");
		hasZoomed = false;
		return;
	}
	else {
		console.log(`Current: ${GetViewportParameters()}`);
		console.log(`Returning to view: ${previousPosition.axisX[0].viewportMinimum} ${previousPosition.axisX[0].viewportMaximum} ${previousPosition.axisY[0].viewportMinimum} ${previousPosition.axisY[0].viewportMaximum}`)
		let chartParams = {
			minX: previousPosition.axisX[0].viewportMinimum,
			maxX: previousPosition.axisX[0].viewportMaximum,
			minY: previousPosition.axisY[0].viewportMinimum,
			maxY: previousPosition.axisY[0].viewportMaximum
		};

		SetChartParameters(chartParams);
		hasZoomed = false;
	}	
}

export function RenderChart() {
	mainChart.render();	
}