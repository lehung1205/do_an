(function () {
    const root = document.getElementById('adminDashboardRoot');
    if (!root) return;

    const dataEl = document.getElementById('adminChartsData');
    const raw = dataEl?.textContent?.trim();
    if (!raw) return;

    let charts;
    try {
        charts = JSON.parse(raw);
    } catch {
        return;
    }

    const palette = {
        primary: '#4f46e5',
        primaryLight: 'rgba(79, 70, 229, 0.15)',
        success: '#059669',
        successLight: 'rgba(5, 150, 105, 0.15)',
        pie: ['#4f46e5', '#7c3aed', '#2563eb', '#0891b2', '#059669', '#d97706', '#dc2626', '#64748b']
    };

    const fontFamily = "'Be Vietnam Pro', system-ui, sans-serif";

    Chart.defaults.font.family = fontFamily;
    Chart.defaults.color = '#64748b';
    Chart.defaults.plugins.legend.labels.usePointStyle = true;

    function labelsFrom(points) {
        return (points || []).map(p => p.label ?? p.Label ?? '');
    }

    function valuesFrom(points) {
        return (points || []).map(p => p.value ?? p.Value ?? 0);
    }

    const lineCtx = document.getElementById('chartRecruitmentLine');
    if (lineCtx) {
        const trend = charts.recruitmentTrend ?? charts.RecruitmentTrend ?? [];
        const apps = charts.monthlyApplications ?? charts.MonthlyApplications ?? [];
        new Chart(lineCtx, {
            type: 'line',
            data: {
                labels: labelsFrom(trend),
                datasets: [
                    {
                        label: 'Tin đăng mới',
                        data: valuesFrom(trend),
                        borderColor: palette.primary,
                        backgroundColor: palette.primaryLight,
                        fill: true,
                        tension: 0.35,
                        pointRadius: 4,
                        pointHoverRadius: 6
                    },
                    {
                        label: 'Đơn ứng tuyển',
                        data: valuesFrom(apps),
                        borderColor: palette.success,
                        backgroundColor: palette.successLight,
                        fill: true,
                        tension: 0.35,
                        pointRadius: 4,
                        pointHoverRadius: 6
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: {
                    legend: { position: 'top', align: 'end' }
                },
                scales: {
                    y: { beginAtZero: true, ticks: { precision: 0 } },
                    x: { grid: { display: false } }
                }
            }
        });
    }

    const pieCtx = document.getElementById('chartCategoryPie');
    if (pieCtx) {
        const slices = charts.jobsByCategory ?? charts.JobsByCategory ?? [];
        new Chart(pieCtx, {
            type: 'doughnut',
            data: {
                labels: slices.map(s => s.categoryName ?? s.CategoryName ?? ''),
                datasets: [{
                    data: slices.map(s => s.count ?? s.Count ?? 0),
                    backgroundColor: palette.pie,
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '58%',
                plugins: {
                    legend: { position: 'bottom' }
                }
            }
        });
    }

    const barAppsCtx = document.getElementById('chartApplicationsBar');
    if (barAppsCtx) {
        const apps = charts.monthlyApplications ?? charts.MonthlyApplications ?? [];
        new Chart(barAppsCtx, {
            type: 'bar',
            data: {
                labels: labelsFrom(apps),
                datasets: [{
                    label: 'Đơn ứng tuyển',
                    data: valuesFrom(apps),
                    backgroundColor: palette.success,
                    borderRadius: 8,
                    maxBarThickness: 48
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: { beginAtZero: true, ticks: { precision: 0 } },
                    x: { grid: { display: false } }
                }
            }
        });
    }

    const barStatusCtx = document.getElementById('chartStatusBar');
    if (barStatusCtx) {
        const status = charts.jobsByStatus ?? charts.JobsByStatus ?? [];
        new Chart(barStatusCtx, {
            type: 'bar',
            data: {
                labels: labelsFrom(status),
                datasets: [{
                    label: 'Số tin',
                    data: valuesFrom(status),
                    backgroundColor: ['#f59e0b', '#059669', '#dc2626', '#64748b', '#94a3b8'],
                    borderRadius: 8,
                    maxBarThickness: 56
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    x: { beginAtZero: true, ticks: { precision: 0 } },
                    y: { grid: { display: false } }
                }
            }
        });
    }
})();
