declare global {
  const CanvasJS: {
      Chart: new (
          containerId: any,
          options: any
      ) => any;
  };

  export {};
}