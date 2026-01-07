// src/core/resize.ts
function resizeCanvasToDisplaySize(canvas) {
  const dpr = window.devicePixelRatio || 1;
  const rect = canvas.getBoundingClientRect();
  const displayWidth = Math.max(1, Math.round(rect.width));
  const displayHeight = Math.max(1, Math.round(rect.height));
  const neededWidth = Math.round(displayWidth * dpr);
  const neededHeight = Math.round(displayHeight * dpr);
  const changed = canvas.width !== neededWidth || canvas.height !== neededHeight;
  if (changed) {
    canvas.width = neededWidth;
    canvas.height = neededHeight;
  }
  return { changed, dpr, displayWidth, displayHeight };
}

// src/core/CanvasPlot.ts
var CanvasPlot = class {
  canvas;
  ctx;
  ro = null;
  options;
  constructor(canvas, options = {}) {
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
  setOptions(partial) {
    this.options = { ...this.options, ...partial };
    this.render();
  }
  render() {
    const { displayWidth, displayHeight, dpr } = resizeCanvasToDisplaySize(this.canvas);
    this.ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    this.ctx.clearRect(0, 0, displayWidth, displayHeight);
    this.ctx.fillStyle = this.options.background;
    this.ctx.fillRect(0, 0, displayWidth, displayHeight);
  }
  destroy() {
    this.ro?.disconnect();
    this.ro = null;
  }
};
export {
  CanvasPlot
};
//# sourceMappingURL=index.js.map