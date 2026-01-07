import { resizeCanvasToDisplaySize } from "./resize";

export type CanvasPlotOptions = {
  background?: string;
};

export class CanvasPlot {
  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;
  private ro: ResizeObserver | null = null;

  private options: Required<CanvasPlotOptions>;

  constructor(canvas: HTMLCanvasElement, options: CanvasPlotOptions = {}) {
    this.canvas = canvas;

    const ctx = canvas.getContext("2d");
    if (!ctx) {
      throw new Error("CanvasPlot: could not acquire 2D context.");
    }
    this.ctx = ctx;

    this.options = {
      background: options.background ?? "#eeeeee"
    };

    this.ro = new ResizeObserver(() => this.render());
    this.ro.observe(this.canvas);

    this.render();
  }

  setOptions(partial: CanvasPlotOptions) {
    this.options = { ...this.options, ...partial };
    this.render();
  }

  render() {
    const { displayWidth, displayHeight, dpr } = resizeCanvasToDisplaySize(this.canvas);

    // Rysujemy w jednostkach CSS px, ale w ostrej rozdzielczości DPR.
    this.ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

    this.ctx.clearRect(0, 0, displayWidth, displayHeight);
    this.ctx.fillStyle = this.options.background;
    this.ctx.fillRect(0, 0, displayWidth, displayHeight);
  }

  destroy() {
    this.ro?.disconnect();
    this.ro = null;
  }
}
