export class ViewportParameters {
    MinX: number | undefined;
    MaxX: number | undefined;
    MinY: number | undefined;
    MaxY: number | undefined;

    public constructor(minX?: number, maxX?: number, minY?: number, maxY?: number) {
        this.MinX = minX;
        this.MaxX = maxX;
        this.MinY = minY;
        this.MaxY = maxY;
    }
}