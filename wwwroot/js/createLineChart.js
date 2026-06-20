function createChart(chartId, data, color, labels) {
    const canvas = document.getElementById(chartId);
    const newCanvas = document.createElement('canvas');
    newCanvas.id = chartId;
    canvas.parentNode.replaceChild(newCanvas, canvas);
    const ctx = newCanvas.getContext('2d');

    const chartData = {
        labels: labels,
        datasets: [{
            label: '',
            data: data,
            borderColor: color,
            borderWidth: 1,
            pointBackgroundColor: color,
            pointRadius: 4,
            pointHoverRadius: 8,
            pointHitRadius: 10,
            pointStyle: 'circle',
            tension: 0
        }]
    };

    new Chart(ctx, {
        type: 'line',
        data: chartData,
        options: {
            scales: {
                y: { beginAtZero: true },
                x: { display: true }
            },
            plugins: {
                annotation: {
                    annotations: chartData.datasets[0].data.map(function (value, index) {
                        return {
                            type: 'line',
                            mode: 'vertical',
                            scaleID: 'x',
                            value: chartData.labels[index],
                            borderColor: 'transparent',
                            borderWidth: 1,
                            label: {
                                content: value.toString(),
                                enabled: false,
                                position: 'start'
                            }
                        };
                    })
                }
            }
        }
    });
}