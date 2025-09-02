import { AxisMode } from "./Enums.js";
export class DataSet {
    constructor(collection) {
        this.axisMode = AxisMode.MJD;
        this.Collection = [];
        this.originalX = [];
        this.Collection = collection;
        this.Collection.forEach((dp) => this.originalX.push(dp.x));
    }
    MJDToDate(mjd) {
        // MJD to Julian Date offset
        const JD = mjd + 2400000.5;
        // Julian Date to standard date conversion
        const unixTime = (JD - 2440587.5) * 86400000; // Convert JD to Unix time (ms since 1970-01-01)
        const date = new Date(unixTime);
        // Format date to dd.MM.yyyyTHH:mm:ss	
        const [yyyy, mm, dd] = [date.getUTCFullYear(), date.getUTCMonth() + 1, date.getUTCDate()];
        const [hh, mi, ss] = [date.getUTCHours(), date.getUTCMinutes(), date.getUTCSeconds()];
        return new Date(yyyy, mm, dd, hh, mi, ss);
    }
    ConvertToDate() {
        if (this.axisMode == AxisMode.Date) {
            return;
        }
        this.axisMode = AxisMode.Date;
        this.Collection.forEach((dp, i) => {
            dp.x = this.MJDToDate(this.originalX[i]);
        });
    }
    ConvertToMjd() {
        if (this.axisMode == AxisMode.MJD) {
            return;
        }
        this.axisMode = AxisMode.MJD;
        this.Collection.forEach((dp, i) => {
            dp.x = this.originalX[i];
        });
    }
    ConvertToTime(startMjd) {
        if (this.axisMode == AxisMode.Time) {
            return;
        }
        this.axisMode = AxisMode.Time;
        this.Collection.forEach((dp, i) => {
            dp.x = (this.originalX[i] - startMjd) * 86400; //elapsed day * seconds in day
        });
        console.log(this.Collection);
    }
}
//# sourceMappingURL=DataSet.js.map