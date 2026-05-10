// =============================================================================
// Admin statistics page — fetches aggregates from /Admin/GetStatistics* endpoints
// and renders KPI cards, a stacked sessions-over-time chart and breakdown tables.
// Chart.js is loaded from CDN by Statistics.cshtml.
// =============================================================================

(function () {
    'use strict';

    const PALETTE = [
        '#7c3aed', '#0ea5e9', '#22c55e', '#f59e0b', '#ef4444',
        '#14b8a6', '#a855f7', '#f97316', '#3b82f6', '#84cc16',
    ];

    const els = {
        rangePreset:    () => document.getElementById('statsRangePreset'),
        customRange:    () => document.getElementById('statsCustomRange'),
        fromDate:       () => document.getElementById('statsFromDate'),
        toDate:         () => document.getElementById('statsToDate'),
        bucket:         () => document.getElementById('statsBucket'),
        gameFilter:     () => document.getElementById('statsGameFilter'),
        refreshBtn:     () => document.getElementById('statsRefresh'),
        loadError:      () => document.getElementById('statsLoadError'),
        rangeSummary:   () => document.getElementById('statsRangeSummary'),
        overviewGrid:   () => document.getElementById('statsOverviewGrid'),
        chartCanvas:    () => document.getElementById('statsChart'),
        chartLegend:    () => document.getElementById('statsChartLegend'),
        chartHint:      () => document.getElementById('statsChartHint'),
        chartEmpty:     () => document.getElementById('statsChartEmpty'),
        byGameTable:    () => document.querySelector('#statsByGameTable tbody'),
        byGameLoading:  () => document.getElementById('statsByGameLoading'),
        byGameEmpty:    () => document.getElementById('statsByGameEmpty'),
        topTable:       () => document.querySelector('#statsTopPlayersTable tbody'),
        topEmpty:       () => document.getElementById('statsTopPlayersEmpty'),
    };

    let chartInstance = null;
    let games = [];

    // ---------- Helpers --------------------------------------------------

    function showError(message) {
        const el = els.loadError();
        if (!el) return;
        el.textContent = message;
        el.hidden = false;
    }

    function clearError() {
        const el = els.loadError();
        if (el) el.hidden = true;
    }

    function escapeHtml(value) {
        if (value === null || value === undefined) return '';
        return String(value).replace(/[&<>"']/g, ch => ({
            '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;',
        }[ch]));
    }

    function formatNumber(value) {
        if (value === null || value === undefined || isNaN(value)) return '—';
        return Number(value).toLocaleString();
    }

    function formatDuration(totalSeconds) {
        if (!totalSeconds || totalSeconds <= 0) return '—';
        const seconds = Math.round(totalSeconds);
        if (seconds < 60) return `${seconds}s`;
        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = seconds % 60;
        if (minutes < 60) return `${minutes}m ${remainingSeconds}s`;
        const hours = Math.floor(minutes / 60);
        const remainingMinutes = minutes % 60;
        return `${hours}h ${remainingMinutes}m`;
    }

    function formatRangeLabel(fromIso, toIso) {
        const from = new Date(fromIso);
        const to = new Date(toIso);
        const opts = { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' };
        return `${from.toLocaleString(undefined, opts)} → ${to.toLocaleString(undefined, opts)}`;
    }

    function formatBucketLabel(iso, bucketName) {
        const date = new Date(iso);
        switch ((bucketName || '').toLowerCase()) {
            case 'hour':
                return date.toLocaleString(undefined, { month: 'short', day: 'numeric', hour: '2-digit' });
            case 'week':
                return `Wk ${date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' })}`;
            case 'month':
                return date.toLocaleDateString(undefined, { year: 'numeric', month: 'short' });
            default:
                return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
        }
    }

    // ---------- Range resolution ----------------------------------------

    function getRangeInput() {
        const preset = els.rangePreset()?.value || '7d';
        const now = new Date();
        if (preset === 'custom') {
            const from = els.fromDate()?.value;
            const to = els.toDate()?.value;
            if (!from || !to) {
                throw new Error('Please pick both From and To dates.');
            }
            const fromDate = new Date(`${from}T00:00:00`);
            const toDate = new Date(`${to}T23:59:59`);
            if (toDate <= fromDate) {
                throw new Error('End date must be after start date.');
            }
            return { from: fromDate, to: toDate };
        }
        const presetMap = {
            '24h':  { hours: 24 },
            '7d':   { days: 7 },
            '30d':  { days: 30 },
            '90d':  { days: 90 },
            '365d': { days: 365 },
        };
        const offset = presetMap[preset] ?? { days: 7 };
        const from = new Date(now);
        if (offset.hours) from.setHours(from.getHours() - offset.hours);
        if (offset.days)  from.setDate(from.getDate() - offset.days);
        return { from, to: now };
    }

    function buildQueryString(range) {
        const params = new URLSearchParams();
        params.set('from', range.from.toISOString());
        params.set('to', range.to.toISOString());
        const bucket = els.bucket()?.value || 'auto';
        if (bucket && bucket !== 'auto') params.set('bucket', bucket);
        const gameFilter = els.gameFilter()?.value || '';
        if (gameFilter) params.set('minigameType', gameFilter);
        return params;
    }

    // ---------- Fetch ----------------------------------------------------

    async function fetchJson(url) {
        const response = await fetch(url, { headers: { 'Accept': 'application/json' } });
        if (!response.ok) {
            const text = await response.text().catch(() => '');
            throw new Error(text || `Request failed: ${response.status}`);
        }
        return response.json();
    }

    // ---------- Renderers ------------------------------------------------

    function renderOverview(overview) {
        const grid = els.overviewGrid();
        if (!grid) return;

        const cards = [
            {
                title: 'Sessions in range',
                value: formatNumber(overview.totalSessionsInRange),
                caption: `${formatNumber(overview.finishedSessionsInRange)} finished · ${overview.completionRatePercent}% completion`,
                icon: 'stats',
                accent: 'primary',
            },
            {
                title: 'Avg concurrent sessions',
                value: formatNumber(overview.averageConcurrentSessionsInRange),
                caption: 'Mean number of games in progress over the range.',
                icon: 'live',
                accent: 'primary',
            },
            {
                title: 'Live right now',
                value: formatNumber(overview.liveActiveSessions),
                caption: `${formatNumber(overview.liveLobbyPlayers)} in ${formatNumber(overview.liveLobbyRooms)} lobbies`,
                icon: 'live',
                accent: overview.liveActiveSessions > 0 ? 'warning' : 'primary',
            },
            {
                title: 'Distinct players',
                value: formatNumber(overview.distinctPlayersInRange),
                caption: `${formatNumber(overview.newUsersInRange)} new in this range.`,
                icon: 'users',
                accent: 'primary',
            },
            {
                title: 'Avg session duration',
                value: formatDuration(overview.averageSessionDurationSeconds),
                caption: 'Across finished sessions in this range.',
                icon: 'clock',
                accent: 'primary',
            },
            {
                title: 'Total platform users',
                value: formatNumber(overview.totalUsers),
                caption: `${formatNumber(overview.totalSessionsAllTime)} sessions all time.`,
                icon: 'users',
                accent: 'primary',
            },
        ];

        grid.innerHTML = cards.map(card => `
            <article class="admin-stat-card admin-accent-${escapeHtml(card.accent)}" data-stat-key="${escapeHtml(card.icon)}">
                <div class="admin-stat-card__icon" aria-hidden="true">${iconSvg(card.icon)}</div>
                <div class="admin-stat-card__body">
                    <p class="admin-stat-card__title">${escapeHtml(card.title)}</p>
                    <p class="admin-stat-card__value">${escapeHtml(card.value)}</p>
                    <p class="admin-stat-card__caption">${escapeHtml(card.caption)}</p>
                </div>
            </article>
        `).join('');
    }

    function iconSvg(key) {
        switch (key) {
            case 'users':
                return `<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><path d="M23 21v-2a4 4 0 0 0-3-3.87"></path><path d="M16 3.13a4 4 0 0 1 0 7.75"></path></svg>`;
            case 'stats':
                return `<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 3v18h18"></path><rect x="6" y="12" width="3" height="6"></rect><rect x="11" y="8" width="3" height="10"></rect><rect x="16" y="4" width="3" height="14"></rect></svg>`;
            case 'live':
                return `<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="3"></circle><path d="M16.24 7.76a6 6 0 0 1 0 8.49"></path><path d="M7.76 16.24a6 6 0 0 1 0-8.49"></path></svg>`;
            case 'clock':
                return `<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 16 14"></polyline></svg>`;
            default:
                return `<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line></svg>`;
        }
    }

    function renderChart(timeSeries) {
        const canvas = els.chartCanvas();
        const empty = els.chartEmpty();
        if (!canvas || typeof Chart === 'undefined') return;

        const totalPoints = (timeSeries.series || []).reduce(
            (sum, series) => sum + series.points.reduce((s, p) => s + p.count, 0), 0);

        if (chartInstance) {
            chartInstance.destroy();
            chartInstance = null;
        }

        if (totalPoints === 0) {
            canvas.style.display = 'none';
            if (empty) empty.hidden = false;
            renderChartLegend([]);
            return;
        }
        canvas.style.display = '';
        if (empty) empty.hidden = true;

        const labels = (timeSeries.buckets || []).map(b => formatBucketLabel(b, timeSeries.bucket));
        const datasets = (timeSeries.series || []).map((series, idx) => ({
            label: series.displayName,
            data: series.points.map(p => p.count),
            backgroundColor: PALETTE[idx % PALETTE.length],
            borderColor: PALETTE[idx % PALETTE.length],
            borderWidth: 1,
            stack: 'sessions',
        }));

        chartInstance = new Chart(canvas.getContext('2d'), {
            type: 'bar',
            data: { labels, datasets },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: { mode: 'index', intersect: false },
                },
                scales: {
                    x: { stacked: true, grid: { display: false } },
                    y: { stacked: true, beginAtZero: true, ticks: { precision: 0 } },
                },
            },
        });

        renderChartLegend(datasets.map(d => ({ label: d.label, color: d.backgroundColor })));
        const hint = els.chartHint();
        if (hint) {
            hint.textContent = `Bucket: ${timeSeries.bucket.toLowerCase()} · ${labels.length} buckets · ${formatNumber(totalPoints)} sessions plotted.`;
        }
    }

    function renderChartLegend(items) {
        const container = els.chartLegend();
        if (!container) return;
        if (!items || items.length === 0) {
            container.innerHTML = '';
            return;
        }
        container.innerHTML = items.map(it => `
            <span class="stats-chart-legend__item">
                <span class="stats-chart-legend__swatch" style="background:${escapeHtml(it.color)}"></span>
                ${escapeHtml(it.label)}
            </span>
        `).join('');
    }

    function renderByGameTable(rows) {
        const tbody = els.byGameTable();
        const empty = els.byGameEmpty();
        if (!tbody) return;
        tbody.innerHTML = '';
        if (!rows || rows.length === 0) {
            if (empty) empty.hidden = false;
            return;
        }
        if (empty) empty.hidden = true;
        rows.forEach(row => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${escapeHtml(row.displayName)}</td>
                <td>${formatNumber(row.totalSessions)}</td>
                <td>${row.sharePercent}%</td>
                <td>${formatNumber(row.finishedSessions)}</td>
                <td>${formatNumber(row.distinctPlayers)}</td>
                <td>${formatNumber(row.liveActiveSessions)}</td>
                <td>${escapeHtml(formatDuration(row.averageDurationSeconds))}</td>
            `;
            tbody.appendChild(tr);
        });
    }

    function renderTopPlayers(players) {
        const tbody = els.topTable();
        const empty = els.topEmpty();
        if (!tbody) return;
        tbody.innerHTML = '';
        if (!players || players.length === 0) {
            if (empty) empty.hidden = false;
            return;
        }
        if (empty) empty.hidden = true;
        players.forEach((player, idx) => {
            const tr = document.createElement('tr');
            const avatarCell = player.avatarUrl
                ? `<img src="${escapeHtml(player.avatarUrl)}" alt="" class="stats-avatar" loading="lazy" onerror="this.remove()">`
                : '';
            tr.innerHTML = `
                <td>${idx + 1}</td>
                <td><span class="stats-player-cell">${avatarCell}<span>${escapeHtml(player.userName)}</span></span></td>
                <td>${formatNumber(player.sessionsPlayed)}</td>
                <td>${formatNumber(player.sessionsWon)}</td>
                <td>${formatNumber(player.totalScore)}</td>
            `;
            tbody.appendChild(tr);
        });
    }

    // ---------- Loaders --------------------------------------------------

    async function loadGameOptions() {
        try {
            games = await fetchJson('/Admin/GetStatisticsGames');
            const select = els.gameFilter();
            if (!select) return;
            const current = select.value;
            select.innerHTML = '<option value="">All games</option>' + games
                .map(g => `<option value="${escapeHtml(g.minigameTypeId)}">${escapeHtml(g.displayName)}</option>`)
                .join('');
            if (current) select.value = current;
        } catch (err) {
            console.error('Failed to load games', err);
        }
    }

    async function loadAll() {
        clearError();
        let range;
        try {
            range = getRangeInput();
        } catch (err) {
            showError(err.message);
            return;
        }
        const params = buildQueryString(range);
        const summary = els.rangeSummary();
        if (summary) summary.textContent = `Showing ${formatRangeLabel(range.from.toISOString(), range.to.toISOString())}`;

        const loadingFlags = [els.byGameLoading()];
        loadingFlags.forEach(el => { if (el) el.hidden = false; });

        const tasks = [
            fetchJson(`/Admin/GetStatisticsOverview?${params.toString()}`),
            fetchJson(`/Admin/GetStatisticsByGame?${params.toString()}`),
            fetchJson(`/Admin/GetStatisticsTimeSeries?${params.toString()}`),
            fetchJson(`/Admin/GetStatisticsTopPlayers?${params.toString()}&limit=10`),
        ];

        try {
            const [overview, byGame, timeSeries, topPlayers] = await Promise.all(tasks);
            renderOverview(overview);
            renderByGameTable(byGame);
            renderChart(timeSeries);
            renderTopPlayers(topPlayers);
        } catch (err) {
            console.error('Failed to load statistics', err);
            showError(err.message || 'Could not load statistics.');
        } finally {
            loadingFlags.forEach(el => { if (el) el.hidden = true; });
        }
    }

    // ---------- Init -----------------------------------------------------

    function attachHandlers() {
        const preset = els.rangePreset();
        const custom = els.customRange();
        if (preset) {
            preset.addEventListener('change', () => {
                if (custom) custom.hidden = preset.value !== 'custom';
                if (preset.value !== 'custom') loadAll();
            });
        }
        ['statsBucket', 'statsGameFilter'].forEach(id => {
            const el = document.getElementById(id);
            if (el) el.addEventListener('change', loadAll);
        });
        ['statsFromDate', 'statsToDate'].forEach(id => {
            const el = document.getElementById(id);
            if (el) el.addEventListener('change', () => {
                if ((els.rangePreset()?.value || '') === 'custom') loadAll();
            });
        });
        const refresh = els.refreshBtn();
        if (refresh) refresh.addEventListener('click', loadAll);
    }

    function defaultCustomDates() {
        const today = new Date();
        const weekAgo = new Date(today);
        weekAgo.setDate(weekAgo.getDate() - 7);
        if (els.fromDate()) els.fromDate().value = weekAgo.toISOString().slice(0, 10);
        if (els.toDate())   els.toDate().value = today.toISOString().slice(0, 10);
    }

    document.addEventListener('DOMContentLoaded', async () => {
        attachHandlers();
        defaultCustomDates();
        await loadGameOptions();
        await loadAll();
    });
})();
