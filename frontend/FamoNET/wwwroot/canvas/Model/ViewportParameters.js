import { DataOperations } from "../Utils/DataOperations.js";
import { AxisMode } from "./Enums.js";
export class ViewportParameters {
    constructor(minX, maxX, minY, maxY, type) {
        this.MinX = minX;
        this.MaxX = maxX;
        this.MinY = minY;
        this.MaxY = maxY;
        this.Type = type;
    }
    CopyFrom(source) {
        this.MinX = source.MinX;
        this.MaxX = source.MaxX;
        this.MinY = source.MinY;
        this.MaxY = source.MaxY;
        this.Type = source.Type;
    }
    ConvertToMJD() {
        let convertedVP = new ViewportParameters(0, 0, 0, 0, AxisMode.MJD);
        convertedVP.CopyFrom(this);
        if ((this.MinX instanceof Date) && (this.MaxX instanceof Date)) {
            convertedVP.MinX = DataOperations.DateToMJD(new Date(convertedVP.MinX));
            convertedVP.MaxX = DataOperations.DateToMJD(new Date(convertedVP.MaxX));
        }
        return convertedVP;
    }
}
//# sourceMappingURL=ViewportParameters.js.map