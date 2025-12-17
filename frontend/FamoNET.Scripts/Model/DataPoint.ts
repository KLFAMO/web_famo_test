export class DataPoint {
    x: number | Date
    y: number

    constructor(x: number | Date, y: number) {
        this.x = x;
        this.y = y;
    }
}