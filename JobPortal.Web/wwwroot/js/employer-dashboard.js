(function () {
  const canvas = document.getElementById('employerApplicantChart');
  const filter = document.getElementById('employerChartDays');
  if (!canvas || typeof Chart === 'undefined') {
    return;
  }

  let chartInstance = null;

  function readInitialData() {
    const el = document.getElementById('employerChartData');
    if (!el) {
      return { labels: [], counts: [] };
    }
    try {
      const data = JSON.parse(el.textContent || '{}');
      const points = data.points || [];
      return {
        labels: points.map((p) => p.label),
        counts: points.map((p) => p.count)
      };
    } catch {
      return { labels: [], counts: [] };
    }
  }

  function buildChart(labels, counts) {
    const max = Math.max(1, ...counts, 0);
    const suggestedMax = max <= 5 ? Math.max(max, 5) : undefined;

    if (chartInstance) {
      chartInstance.destroy();
    }

    chartInstance = new Chart(canvas, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Số lượt apply',
            data: counts,
            backgroundColor: 'rgba(0, 0, 238, 0.75)',
            borderColor: '#0000ee',
            borderWidth: 1,
            borderRadius: 4,
            maxBarThickness: 48
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label(ctx) {
                return ` ${ctx.parsed.y} lượt apply`;
              }
            }
          }
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: { color: '#6b7280', font: { size: 11 } }
          },
          y: {
            beginAtZero: true,
            suggestedMax,
            ticks: {
              stepSize: max <= 5 ? 1 : undefined,
              color: '#6b7280',
              font: { size: 11 }
            },
            grid: { color: '#e5e7eb' }
          }
        }
      }
    });
  }

  async function loadChart(days) {
    const baseUrl = canvas.getAttribute('data-api-base');
    if (!baseUrl) {
      return;
    }

    try {
      const res = await fetch(`${baseUrl}?days=${days}`, { credentials: 'same-origin' });
      if (!res.ok) {
        return;
      }
      const json = await res.json();
      const points = json?.points || json?.data?.points || json?.Data?.Points || [];
      buildChart(
        points.map((p) => p.label || p.Label),
        points.map((p) => p.count ?? p.Count ?? 0)
      );
    } catch {
      /* giữ biểu đồ hiện tại */
    }
  }

  const initial = readInitialData();
  buildChart(initial.labels, initial.counts);

  if (filter) {
    filter.addEventListener('change', () => {
      const days = parseInt(filter.value, 10) || 7;
      loadChart(days);
    });
  }
})();
