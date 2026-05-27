(function () {
    const dataEl = document.getElementById('adminPaymentsChartData');
    const canvas = document.getElementById('chartAdminRevenue');
    if (!dataEl || !canvas || typeof Chart === 'undefined') return;

    let points;
    try {
        points = JSON.parse(dataEl.textContent.trim());
    } catch {
        return;
    }

    const labels = (points || []).map(p => p.label ?? p.Label ?? '');
    const values = (points || []).map(p => p.value ?? p.Value ?? 0);

    Chart.defaults.font.family = "'Be Vietnam Pro', system-ui, sans-serif";
    Chart.defaults.color = '#64748b';

    new Chart(canvas, {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                label: 'Doanh thu (₫)',
                data: values,
                backgroundColor: 'rgba(79, 70, 229, 0.75)',
                borderColor: '#4f46e5',
                borderWidth: 1,
                borderRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label(ctx) {
                            const v = ctx.parsed.y ?? 0;
                            return new Intl.NumberFormat('vi-VN').format(v) + ' ₫';
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback(value) {
                            if (value >= 1_000_000) return (value / 1_000_000).toFixed(1) + 'M';
                            if (value >= 1_000) return (value / 1_000).toFixed(0) + 'K';
                            return value;
                        }
                    }
                }
            }
        }
    });
})();
