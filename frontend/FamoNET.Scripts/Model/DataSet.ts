declare let CanvasJS;
import { AxisMode } from "./Enums.js";
import { DataPoint } from "./DataPoint.js";
export class DataSet {    
    private axisMode: AxisMode = AxisMode.MJD;    
    Collection: Array<DataPoint> = [];

    private originalX: any[] = [];
    
    constructor(collection: Array<DataPoint>) {
        this.Collection = collection;
        this.Collection.forEach((dp) => this.originalX.push(dp.x));
    }

    ConvertToDate() : void {
        this.axisMode = AxisMode.Date;

        this.Collection.forEach((dp, i) =>
        {
            const mjd = dp.x;
            //from chatgpt

            // MJD to Julian Date offset
            const JD = mjd + 2400000.5;

            // Julian Date to standard date conversion
            const unixTime = (JD - 2440587.5) * 86400000; // Convert JD to Unix time (ms since 1970-01-01)
            const date = new Date(unixTime);

            // Format date to dd.MM.yyyyTHH:mm:ss	
            const [yyyy, mm, dd] = [date.getUTCFullYear(), date.getUTCMonth() + 1, date.getUTCDate()];
            const [hh, mi, ss] = [date.getUTCHours(), date.getUTCMinutes(), date.getUTCSeconds()];

            dp.x = new Date(yyyy, mm, dd, hh, mi, ss) as Date;
        });    

        console.log(this.Collection);
    }

    ConvertToMjd() : void {
        this.axisMode = AxisMode.MJD;
        this.Collection.forEach((dp, i) =>
        {
            dp.x = this.originalX[i];
        });
        console.log(this.Collection);
    }

    SubtractOffset(mjdOffset: number): void {
        let result: Array<any> = [];

        this.Collection.forEach((dp, i) => {
            dp.x = this.originalX[i] - mjdOffset;
        });         

        switch (this.axisMode) {
            case AxisMode.MJD: {                
                break;
            }
            case AxisMode.Date: {
                this.ConvertToDate();
                break;
            }
        }
    }
}