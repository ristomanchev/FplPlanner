// FPL Planner UI: plain JavaScript on top of the REST API (no framework, no build step).

const $ = id => document.getElementById(id);

const POSITION_LABELS = { Goalkeeper: 'GK', Defender: 'DEF', Midfielder: 'MID', Forward: 'FWD' };
const POSITION_ORDER = ['Goalkeeper', 'Defender', 'Midfielder', 'Forward'];
const PAGE_SIZE = 25;

const state = {
    nextGameweek: null,
    managerId: null,
    playersPage: 0,
};

// ---------- helpers ----------

function escapeHtml(value) {
    return String(value ?? '').replace(/[&<>"']/g, c =>
        ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

const fmt = (value, digits = 1) => Number(value ?? 0).toFixed(digits);

function formatDate(iso) {
    return iso ? new Date(iso).toLocaleString('mk-MK', { dateStyle: 'medium', timeStyle: 'short' }) : '';
}

let toastTimer;
function toast(message, isError = false) {
    const el = $('toast');
    el.textContent = message;
    el.className = isError ? 'toast error' : 'toast';
    el.hidden = false;
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => (el.hidden = true), isError ? 8000 : 3500);
}

// Disables the button while the request runs and reports failures in a toast.
async function withBusy(button, action) {
    button.disabled = true;
    try {
        return await action();
    } catch (error) {
        toast(error.message, true);
    } finally {
        button.disabled = false;
    }
}

// ---------- tabs ----------

function showTab(name) {
    if (!document.getElementById(`tab-${name}`)) name = 'manager';
    history.replaceState(null, '', `#${name}`);
    document.querySelectorAll('.tab').forEach(t => t.classList.toggle('active', t.dataset.tab === name));
    document.querySelectorAll('.tab-panel').forEach(p => (p.hidden = p.id !== `tab-${name}`));
    if (name === 'players') loadPlayers();
    if (name === 'data') loadEtlLogs();
}

// ---------- manager ----------

async function loadManagers(selectId) {
    const managers = await api.get('/api/managers');
    const select = $('manager-select');
    select.innerHTML = '<option value="">— избери —</option>' + managers
        .map(m => `<option value="${m.id}">${escapeHtml(m.teamName)} (${escapeHtml(m.managerName)})</option>`)
        .join('');

    const id = selectId ?? state.managerId ?? managers[0]?.id;
    if (id) {
        select.value = id;
        await selectManager(id);
    }
}

async function selectManager(id) {
    state.managerId = id || null;
    $('manager-view').hidden = !id;
    $('manager-empty').hidden = !!id;
    if (!id) return;

    $('export-squad').href = `/api/export/squads/${id}`;

    // The squad works without predictions; lineup and transfers need them.
    try {
        renderSquad(await api.get(`/api/squads/${id}`));
    } catch (error) {
        $('squad-title').textContent = 'Нема тим';
        $('squad-stats').innerHTML = `<span class="muted">${escapeHtml(error.message)}</span>`;
    }
    await Promise.all([loadLineup(), loadTransfers()]);
}

function renderSquad(squad) {
    $('squad-title').textContent = `${squad.teamName} — коло ${squad.gameweekNumber}`;

    const badge = $('squad-validation');
    badge.textContent = squad.validation.isValid ? 'Валиден тим' : 'Невалиден тим';
    badge.className = `badge ${squad.validation.isValid ? 'ok' : 'bad'}`;
    $('squad-errors').innerHTML = squad.validation.errors.map(e => `<li>${escapeHtml(e)}</li>`).join('');

    const flagged = squad.members.filter(m => m.status !== 'Available').length;
    $('squad-stats').innerHTML = [
        ['Вредност', `£${fmt(squad.squadValue)}m`],
        ['Банка', `£${fmt(squad.bank)}m`],
        ['Повредени/сомнителни', flagged],
    ].map(([label, value]) => `<div class="stat"><div class="value">${value}</div><div class="label">${label}</div></div>`).join('');
}

async function loadLineup() {
    try {
        const lineup = await api.get(`/api/squads/${state.managerId}/lineup`);
        $('lineup-title').textContent = `Најдобра постава за коло ${lineup.gameweekNumber} (${lineup.formation})`;
        $('lineup-points').textContent = `${fmt(lineup.expectedPoints)} xP`;

        $('pitch').innerHTML = POSITION_ORDER
            .map(position => lineup.startingEleven.filter(p => p.position === position))
            .map(row => `<div class="pitch-row">${row.map(p => playerCard(p, lineup)).join('')}</div>`)
            .join('');
        $('bench').innerHTML = lineup.bench.map(p => playerCard(p, lineup)).join('');
    } catch (error) {
        $('lineup-title').textContent = 'Најдобра постава';
        $('lineup-points').textContent = '';
        $('pitch').innerHTML = `<div class="pitch-row"><div class="player">${escapeHtml(error.message)}</div></div>`;
        $('bench').innerHTML = '';
    }
}

function playerCard(player, lineup) {
    const armband = player.playerId === lineup.captain?.playerId ? 'C'
        : player.playerId === lineup.viceCaptain?.playerId ? 'V' : '';
    const flagged = player.status !== 'Available';
    const title = flagged ? `${player.status}${player.news ? ': ' + player.news : ''}` : '';

    return `
        <div class="player ${flagged ? 'flagged' : ''}" title="${escapeHtml(title)}">
            ${armband ? `<span class="armband">${armband}</span>` : ''}
            <div class="name">${escapeHtml(player.webName)}</div>
            <div class="meta">${escapeHtml(player.clubShortName)} · ${POSITION_LABELS[player.position]} · £${fmt(player.price)}m</div>
            <div class="xp">${fmt(player.expectedPoints)} xP</div>
        </div>`;
}

async function loadTransfers() {
    const container = $('transfers');
    try {
        const horizon = $('transfer-horizon').value;
        const advice = await api.get(`/api/squads/${state.managerId}/transfer-suggestions?horizon=${horizon}&maxTransfers=3`);
        const plan = advice.recommendedPlan;
        const gameweeks = `кола ${advice.gameweekNumbers[0]}–${advice.gameweekNumbers.at(-1)}`;

        if (plan.transferCount === 0) {
            container.innerHTML = `<p>Нема трансфер што јасно го подобрува тимот (${gameweeks}). Препорака: зачувај го бесплатниот трансфер.</p>`;
            return;
        }

        const hit = plan.hitCost > 0 ? `, −${plan.hitCost} за дополнителни трансфери, нето <b>+${fmt(plan.netGain)}</b>` : '';
        container.innerHTML = `
            <p class="plan-summary muted">xP на играчот е збир за ${gameweeks}.</p>
            <p class="plan-summary">Препорака за ${gameweeks}: <b>+${fmt(plan.gain)}</b> очекувани поени${hit}.
               Банка после: £${fmt(plan.bankAfter)}m</p>
            ${plan.transfers.map(t => `
                <div class="transfer">
                    <span class="out">↓ ${escapeHtml(t.playerOut.webName)} <span class="muted">£${fmt(t.playerOut.price)}m</span></span>
                    <span class="muted">→</span>
                    <span class="in">↑ ${escapeHtml(t.playerIn.webName)} <span class="muted">${escapeHtml(t.playerIn.clubShortName)} £${fmt(t.playerIn.price)}m</span></span>
                    <span title="Збир за избраните кола">${fmt(t.playerIn.expectedPoints)} xP</span>
                </div>`).join('')}`;
    } catch (error) {
        container.innerHTML = `<p class="muted">${escapeHtml(error.message)}</p>`;
    }
}

async function importManager(event) {
    event.preventDefault();
    const button = event.submitter;
    await withBusy(button, async () => {
        const manager = await api.post('/api/managers/import', {
            fplEntryId: Number($('import-entry').value),
            email: $('import-email').value || null,
        });
        toast(`Увезен: ${manager.teamName}`);
        $('import-form').reset();
        await loadManagers(manager.id);
    });
}

async function sendReport(event) {
    await withBusy(event.currentTarget, async () => {
        const report = await api.post(`/api/reports/${state.managerId}/weekly/send`);
        toast(`Извештајот за коло ${report.gameweekNumber} е испратен на ${report.email}`);
    });
}

async function importSquadFile(event) {
    const file = event.target.files[0];
    event.target.value = '';
    if (!file || !state.nextGameweek) return;

    const formData = new FormData();
    formData.append('file', file);
    try {
        await api.postForm(`/api/import/squads/${state.managerId}/gameweeks/${state.nextGameweek.number}`, formData);
        toast(`Тимот е увезен за коло ${state.nextGameweek.number}`);
        await selectManager(state.managerId);
    } catch (error) {
        toast(error.message, true);
    }
}

// ---------- players ----------

async function loadPlayers() {
    if (!state.nextGameweek) return;
    const position = $('players-position').value;
    const query = new URLSearchParams({
        gameweekNumber: state.nextGameweek.number,
        pageNumber: state.playersPage,
        pageSize: PAGE_SIZE,
    });
    if (position) query.set('position', position);

    try {
        const page = await api.get(`/api/playerpredictions?${query}`);
        const offset = page.pageNumber * page.pageSize;
        $('players-body').innerHTML = page.items.map((p, i) => `
            <tr>
                <td class="muted">${offset + i + 1}</td>
                <td><b>${escapeHtml(p.playerWebName)}</b></td>
                <td>${escapeHtml(p.clubShortName)}</td>
                <td>${POSITION_LABELS[p.playerPosition]}</td>
                <td class="num">£${fmt(p.playerPrice)}m</td>
                <td class="num">${fmt(p.expectedMinutes, 0)}</td>
                <td class="num">${fmt(p.breakdown.goals, 2)}</td>
                <td class="num">${fmt(p.breakdown.assists, 2)}</td>
                <td class="num">${fmt(p.breakdown.cleanSheet, 2)}</td>
                <td class="num">${fmt(p.breakdown.bonus, 2)}</td>
                <td class="num"><b>${fmt(p.expectedPoints, 2)}</b></td>
            </tr>`).join('') || '<tr><td colspan="11" class="muted">Нема предвидувања. Пресметај ги во „Податоци“.</td></tr>';

        $('players-page').textContent = `Страна ${page.pageNumber + 1} од ${Math.max(page.totalPages, 1)} · коло ${state.nextGameweek.number}`;
        $('players-prev').disabled = !page.hasPreviousPage;
        $('players-next').disabled = !page.hasNextPage;
    } catch (error) {
        toast(error.message, true);
    }
}

// ---------- data (ETL, predictions) ----------

async function loadEtlLogs() {
    try {
        const logs = await api.get('/api/etl/logs?count=15');
        $('etl-logs').innerHTML = logs.map(l => `
            <tr>
                <td>${formatDate(l.startedAt)}</td>
                <td><span class="badge ${l.success ? 'ok' : 'bad'}">${l.success ? 'Успешно' : 'Неуспешно'}</span></td>
                <td class="num">${l.clubsLoaded}</td>
                <td class="num">${l.playersLoaded}</td>
                <td class="num">${l.fixturesLoaded}</td>
                <td class="num">${l.durationSeconds ?? ''} s</td>
                <td>${l.predictionRecalculationQueued ? 'пратена порака' : '—'}</td>
                <td class="muted">${escapeHtml(l.errorMessage ?? '')}</td>
            </tr>`).join('') || '<tr><td colspan="8" class="muted">Сè уште нема извршувања.</td></tr>';
    } catch (error) {
        toast(error.message, true);
    }
}

async function runEtl(event) {
    await withBusy(event.currentTarget, async () => {
        $('data-status').textContent = 'Се влечат податоци од FPL…';
        const log = await api.post('/api/etl/run');
        $('data-status').textContent = `${log.playersLoaded} играчи, ${log.fixturesLoaded} натпревари за ${log.durationSeconds}s.` +
            (log.predictionRecalculationQueued ? ' Предвидувањата се пресметуваат преку RabbitMQ.' : '');
        await loadEtlLogs();
        await loadNextGameweek();
    });
}

async function recalculate(event) {
    await withBusy(event.currentTarget, async () => {
        const result = await api.post('/api/playerpredictions/recalculate?horizon=6');
        $('data-status').textContent = `${result.predictionsSaved} предвидувања за кола ${result.gameweekNumbers.join(', ')}.`;
    });
}

// ---------- startup ----------

async function loadNextGameweek() {
    try {
        state.nextGameweek = await api.get('/api/gameweeks/next');
        $('next-gameweek').textContent =
            `Следно коло: ${state.nextGameweek.number} · рок ${formatDate(state.nextGameweek.deadline)}`;
    } catch {
        $('next-gameweek').textContent = 'Нема податоци — повлечи ги од FPL во „Податоци“.';
    }
}

document.querySelectorAll('.tab').forEach(tab => tab.addEventListener('click', () => showTab(tab.dataset.tab)));
$('manager-select').addEventListener('change', e => selectManager(e.target.value));
$('import-form').addEventListener('submit', importManager);
$('send-report').addEventListener('click', sendReport);
$('import-squad-file').addEventListener('change', importSquadFile);
$('transfer-horizon').addEventListener('change', loadTransfers);
$('players-position').addEventListener('change', () => { state.playersPage = 0; loadPlayers(); });
$('players-prev').addEventListener('click', () => { state.playersPage--; loadPlayers(); });
$('players-next').addEventListener('click', () => { state.playersPage++; loadPlayers(); });
$('run-etl').addEventListener('click', runEtl);
$('recalculate').addEventListener('click', recalculate);

(async () => {
    await loadNextGameweek();
    await loadManagers().catch(error => toast(error.message, true));
    showTab(location.hash.slice(1) || 'manager');
})();
