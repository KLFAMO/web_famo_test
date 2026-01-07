import { CanvasPlot } from "/static/lib/CanvasPlot/dist/index.js";

let plot = null;

function getDataArrays() {
  const { x_tab, y_tab } = window.andaData;
  return { x_tab, y_tab };
}

// Ograniczamy liczbę etykiet
function mjdAxisValues(u, vals) {
    const res = [];
    const maxLabels = Math.max(3, Math.floor(u.width / 80));
    const step = Math.max(1, Math.ceil(vals.length / maxLabels));

    for (let i = 0; i < vals.length; i++) {
        if (i % step === 0) {
            res.push(vals[i].toFixed(5));
        } else {
            res.push("");
        }
    }
    return res;
}

function createOrUpdatePlot() {
    const { x_tab, y_tab } = getDataArrays();
    const plotElem = document.getElementById('data_plot');

    const opts = {
        title: "Data to plot",
        width: plotElem.clientWidth,
        height: 480,
        scales: {
            x: { time: false },
            y: { auto: true }
        },
        axes: [
            {
                label: "MJD",
                values: mjdAxisValues,
            },
            {
                label: "Value"
            }
        ],
        series: [
            {}, // X
            {
                label: "val",
                width: 1.5,
                stroke: "steelblue",
            }
        ]
    };

    const data = [x_tab, y_tab];

    if (dataPlot) {
        dataPlot.destroy();
        dataPlot = null;
    }

    dataPlot = new uPlot(opts, data, plotElem);

    window.addEventListener('resize', () => {
        if (!dataPlot) return;
        dataPlot.setSize({
            width: plotElem.clientWidth,
            height: 480
        });
    });
}

function export_data_to_csv() {
  const { x_tab, y_tab } = getDataArrays();

  let csvContent = "data:text/csv;charset=utf-8,";
  csvContent += "mjd,val\n";

  x_tab.forEach((x_value, index) => {
    const y_value = y_tab[index];
    csvContent += `${x_value},${y_value}\n`;
  });

  const encodedUri = encodeURI(csvContent);
  const link = document.createElement("a");
  link.setAttribute("href", encodedUri);
  link.setAttribute("download", "data_export.csv");
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

window.addEventListener('DOMContentLoaded', () => {
    createOrUpdatePlot();
});