import { ViewportParameters } from "./ViewportParameters.js"

export class ChartParameters {
    ViewportParameters? : ViewportParameters;
    Title?: string;

    public constructor(viewport?: ViewportParameters, title? : string) {
        this.ViewportParameters = viewport;
        this.Title = title;
    }
}