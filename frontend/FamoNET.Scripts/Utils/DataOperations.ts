///Functions shouldn't process collections. To do later
export class DataOperations {               
    static MJDToDate(mjd: number): Date {
        // MJD to Julian Date offset
        const JD = mjd + 2400000.5;

        // Julian Date to standard date conversion
        const unixTime = (JD - 2440587.5) * 86400000; // Convert JD to Unix time (ms since 1970-01-01)

        return new Date(unixTime);
    }    

    static ConvertToDate(mjdCollection: number[]): Date[] {        
        let result: Date[] = [];

        mjdCollection.forEach((mjd, i) => {
            result.push(this.MJDToDate(mjd));
        });        

        return result;
    }    

    static ConvertToTime(mjdCollection: number[], startMjd: number): number[] {
        let result: number[] = [];

        mjdCollection.forEach((dp, i) => {
            result.push(dp - startMjd) * 86400; //elapsed day * seconds in day
        });       

        return result;
    }

    static TimeToMJD(time: number, offset: number): number {
        return offset + (time / 86400);
    }

    static MJDToTime(mjd: number, offset: number): number {
        return (mjd - offset) * 86400;
    }

    static DateToMJD(date: Date): number {       

        // Calculate Julian Date (JD)
        const year = date.getUTCFullYear();
        const month = date.getUTCMonth()+1; // JS months are 0-based
        const day = date.getUTCDate();
        const hours = date.getUTCHours();
        const minutes = date.getUTCMinutes();
        const seconds = date.getUTCSeconds();
        const milliseconds = date.getUTCMilliseconds();

        // Calculate the fractional part of the day
        const dayFraction = (hours + (minutes / 60) + (seconds / 3600) + (milliseconds / 3600000)) / 24;

        // Formula for Julian Date (simplified for 2000+ years)
        const a = Math.floor((14 - month) / 12);
        const y = year + 4800 - a;
        const m = month + 12 * a - 3;

        let jd = day + Math.floor((153 * m + 2) / 5) + 365 * y + Math.floor(y / 4) - Math.floor(y / 100) + Math.floor(y / 400) - 32045;
        jd += dayFraction - 0.5; // Adjust for JD starting at noon

        // Convert JD to MJD (MJD = JD - 2400000.5)
        const mjd = jd - 2400000.5;

        return mjd;
    }
}