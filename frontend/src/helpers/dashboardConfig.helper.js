// Chart colors
export const CHART_COLORS = {
    pie: ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF', '#FF9F40', '#E7E9ED', '#8DD1E1']
};

// Region chart configuration
export const REGION_CHART_OPTIONS = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: {
            position: 'bottom',
            labels: {
                padding: 10,
                usePointStyle: true,
                font: {
                    size: 11
                }
            }
        },
        tooltip: {
            callbacks: {
                label: function (context) {
                    const label = context.label || '';
                    const value = context.parsed || 0;
                    const total = context.dataset.data.reduce((a, b) => a + b, 0);
                    const percentage = ((value / total) * 100).toFixed(1);
                    return `${label}: ${value} (${percentage}%)`;
                }
            }
        }
    }
};

// Dashboard constants
export const DASHBOARD_CONFIG = {
    REFRESH_INTERVAL: 60000, // 1 minute
    DEFAULT_REGION_CHART: {
        labels: [],
        datasets: [{
            data: [],
            backgroundColor: [],
            hoverBackgroundColor: []
        }]
    }
};
