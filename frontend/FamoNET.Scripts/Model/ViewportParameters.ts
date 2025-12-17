import { DataOperations } from "../Utils/DataOperations.js";
import { AxisMode } from "./Enums.js";

export class ViewportParameters {
    MinX: number | Date;
    MaxX: number | Date;
    MinY: number;
    MaxY: number;    
    Type: AxisMode;

    public constructor(minX: number | Date, maxX: number | Date, minY: number, maxY: number, type: AxisMode) {
        this.MinX = minX;
        this.MaxX = maxX;
        this.MinY = minY;
        this.MaxY = maxY;        
        this.Type = type;
    }

    public CopyFrom(source: ViewportParameters) {
        this.MinX = source.MinX;
        this.MaxX = source.MaxX;

        this.MinY = source.MinY;
        this.MaxY = source.MaxY;

        this.Type = source.Type;
    }

    public ConvertToMJD(): ViewportParameters {

        let convertedVP: ViewportParameters = new ViewportParameters(0, 0, 0, 0, AxisMode.MJD);
        convertedVP.CopyFrom(this);

        if ((this.MinX instanceof Date) && (this.MaxX instanceof Date)) {
            convertedVP.MinX = DataOperations.DateToMJD(new Date(convertedVP.MinX));
            convertedVP.MaxX = DataOperations.DateToMJD(new Date(convertedVP.MaxX));            
        }

        return convertedVP;
    }
}