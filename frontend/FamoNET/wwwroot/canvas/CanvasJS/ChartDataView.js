import { AxisMode } from "../Model/Enums.js";
import { DataOperations } from "../Utils/DataOperations.js";
import { ViewportParameters } from "../Model/ViewportParameters.js";
import { DataPoint } from "../Model/DataPoint.js";
export class ChartDataView {
    constructor(mjdDataSet, axisMode, mjdViewportParameters, offset) {
        this.Collections = []; //array of lists
        this.AxisMode = axisMode;
        this.Offset = offset ? offset : mjdDataSet[0].x;
        this.AddCollection(mjdDataSet);
        if (mjdViewportParameters) {
            this.CurrentViewportParameters = mjdViewportParameters;
        }
        else {
            this.CurrentViewportParameters = new ViewportParameters();
            this.AutoAdjustViewportParameters();
        }
    }
    AutoAdjustViewportParameters() {
        let minX = this.Collections[0][0].x;
        let maxX = this.Collections[0][0].x;
        let minY = this.Collections[0][0].y;
        let maxY = this.Collections[0][0].y;
        this.Collections.forEach(collection => {
            collection.forEach(dp => {
                if (dp.x > maxX) {
                    maxX = dp.x;
                }
                else if (dp.x < minX) {
                    minX = dp.x;
                }
                if (dp.y > maxY) {
                    maxY = dp.y;
                }
                else if (dp.y < minY) {
                    minY = dp.y;
                }
            });
        });
        this.CurrentViewportParameters.MaxX = maxX;
        this.CurrentViewportParameters.MinX = minX;
        this.CurrentViewportParameters.MaxY = maxY;
        this.CurrentViewportParameters.MinY = minY;
    }
    //to do: embbed it to ViewportParameters, not here
    SetViewportParameters(sourceViewportParameters) {
        if (this.AxisMode == AxisMode.MJD) {
            switch (sourceViewportParameters.Type) {
                case AxisMode.MJD:
                    this.CurrentViewportParameters = sourceViewportParameters;
                    return;
                case AxisMode.Date:
                    this.CurrentViewportParameters.MinX = DataOperations.DateToMJD(new Date(sourceViewportParameters.MinX));
                    this.CurrentViewportParameters.MaxX = DataOperations.DateToMJD(new Date(sourceViewportParameters.MaxX));
                    break;
                case AxisMode.Time:
                    this.CurrentViewportParameters.MinX = DataOperations.TimeToMJD(sourceViewportParameters.MinX, this.Offset);
                    this.CurrentViewportParameters.MaxX = DataOperations.TimeToMJD(sourceViewportParameters.MaxX, this.Offset);
                    break;
            }
        }
        else if (this.AxisMode == AxisMode.Date) {
            switch (sourceViewportParameters.Type) {
                case AxisMode.MJD:
                    this.CurrentViewportParameters.MinX = DataOperations.MJDToDate(sourceViewportParameters.MinX);
                    this.CurrentViewportParameters.MaxX = DataOperations.MJDToDate(sourceViewportParameters.MaxX);
                    return;
                case AxisMode.Date:
                    this.CurrentViewportParameters = sourceViewportParameters;
                    break;
                case AxisMode.Time:
                    this.CurrentViewportParameters.MinX = DataOperations.MJDToDate(DataOperations.TimeToMJD(sourceViewportParameters.MinX, this.Offset));
                    this.CurrentViewportParameters.MaxX = DataOperations.MJDToDate(DataOperations.TimeToMJD(sourceViewportParameters.MaxX, this.Offset));
                    break;
            }
        }
        else if (this.AxisMode == AxisMode.Time) {
            switch (sourceViewportParameters.Type) {
                case AxisMode.MJD:
                    this.CurrentViewportParameters.MinX = DataOperations.MJDToTime(sourceViewportParameters.MinX, this.Offset);
                    this.CurrentViewportParameters.MaxX = DataOperations.MJDToTime(sourceViewportParameters.MaxX, this.Offset);
                    return;
                case AxisMode.Date:
                    this.CurrentViewportParameters.MinX = DataOperations.MJDToTime(DataOperations.DateToMJD(new Date(sourceViewportParameters.MinX)), this.Offset);
                    this.CurrentViewportParameters.MaxX = DataOperations.MJDToTime(DataOperations.DateToMJD(new Date(sourceViewportParameters.MaxX)), this.Offset);
                    break;
                case AxisMode.Time:
                    this.CurrentViewportParameters = sourceViewportParameters;
                    break;
            }
        }
    }
    AddCollection(dataSet) {
        let result = [];
        if (this.AxisMode === AxisMode.MJD) {
            this.Collections.push(dataSet);
            return;
        }
        else if (this.AxisMode === AxisMode.Time) {
            let timeSeries = DataOperations.ConvertToTime(dataSet.map(dp => dp.x), this.Offset);
            timeSeries.forEach((t, i) => {
                result.push(new DataPoint(t, dataSet[i].y));
            });
        }
        else if (this.AxisMode === AxisMode.Date) {
            let dateSeries = DataOperations.ConvertToDate(dataSet.map(dp => dp.x));
            dateSeries.forEach((t, i) => {
                result.push(new DataPoint(t, dataSet[i].y));
            });
        }
        this.Collections.push(result);
    }
}
//# sourceMappingURL=ChartDataView.js.map