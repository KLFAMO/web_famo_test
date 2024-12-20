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

export function CreateEmptyChart(title, type = "spline") {
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
			valueFormatString: "#.00E+0"
		},
		rangeChanged: (e) => {
			if (e.type === "reset") {
				chartHistory = [];
				hasZoomed = false;
				return;
			}
			hasZoomed = true;
			console.log("added to hisotry");			
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

	return new Date(yyyy,mm,dd,hh,mi,ss)
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

export function SetChartParameters(chartParams) {	
	if (chartParams.title != undefined) {		
		mainChart.options.title.text = chartParams.title;
	}

	if (chartParams.minX != undefined) {
		mainChart.axisX[0].set("viewportMinimum", chartParams.minX, false);		
	}

	if (chartParams.maxX != undefined) {
		mainChart.axisX[0].set("viewportMaximum", chartParams.maxX, false);		
	}

	if (chartParams.minY != undefined) {
		mainChart.axisY[0].set("viewportMinimum", chartParams.minY, false);		
	}

	if (chartParams.maxY != undefined) {
		mainChart.axisY[0].set("viewportMaximum", chartParams.maxY, false);		
	}	
}

export function GetChartParameters() {
	return [mainChart.axisX[0].get("minimum"), mainChart.axisX[0].get("maximum"), mainChart.axisY[0].get("minimum"), mainChart.axisY[0].get("maximum") ]
}

export function Unzoom() {
	console.log("Unzooming.");	

	//pop current	
	chartHistory.pop();

	let i = chartHistory.length - 1;
	let previousPosition = i > -1 ? chartHistory[i] : undefined;		

	if (previousPosition == undefined) {
		mainChart.axisX[0].set("viewportMaximum", null, false);
		mainChart.axisY[0].set("viewportMaximum", null, false);
		mainChart.axisX[0].set("viewportMinimum", null, false);
		mainChart.axisY[0].set("viewportMinimum", null, false);		
		console.log("Previous position not found. Returning to default.");
		return;
	}
	else {
		let chartParams = {
			minX: previousPosition.axisX[0].viewportMinimum,
			maxX: previousPosition.axisX[0].viewportMaximum,
			minY: previousPosition.axisY[0].viewportMinimum,
			maxY: previousPosition.axisY[0].viewportMaximum
		};

		SetChartParameters(chartParams);
	}
	hasZoomed = false;
}

export function RenderChart() {	
	mainChart.render();
	console.log("Chart render called");
}