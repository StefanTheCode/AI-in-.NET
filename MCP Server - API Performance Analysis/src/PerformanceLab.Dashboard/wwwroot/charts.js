// Performance Lab — Chart.js helpers
// Called from Blazor via IJSRuntime.InvokeVoidAsync

window.performanceCharts = (function () {
    const charts = {};

    function renderBarChart(canvasId, labels, datasets, yLabel) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        // Destroy previous instance to avoid "canvas already in use" error
        if (charts[canvasId]) {
            charts[canvasId].destroy();
            delete charts[canvasId];
        }

        const chart = new Chart(canvas, {
            type: 'bar',
            data: { labels, datasets },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        labels: { color: '#c9d1d9' }
                    },
                    tooltip: {
                        callbacks: {
                            label: ctx => `${ctx.dataset.label}: ${ctx.parsed.y.toFixed(1)} ms`
                        }
                    }
                },
                scales: {
                    x: {
                        ticks: { color: '#8b949e' },
                        grid:  { color: '#21262d' }
                    },
                    y: {
                        title: { display: true, text: yLabel, color: '#8b949e' },
                        ticks: { color: '#8b949e', callback: v => v + ' ms' },
                        grid:  { color: '#21262d' }
                    }
                }
            }
        });

        charts[canvasId] = chart;
    }

    return { renderBarChart };
})();
