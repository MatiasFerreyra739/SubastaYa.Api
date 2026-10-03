document.addEventListener('DOMContentLoaded', async () => {
    await cargarSaldos();
    await cargarMovimientos();

    document.getElementById('form-deposito').addEventListener('submit', onDepositar);
});

async function cargarSaldos() {
    const res = await api.getSaldo();
    if (!res.ok) {
        mostrarToast('No se pudieron cargar los saldos', 'error');
        return;
    }

    document.getElementById('saldo-total').textContent      = formatearMoneda(res.data.saldoTotal);
    document.getElementById('saldo-retenido').textContent   = formatearMoneda(res.data.saldoRetenido);
    document.getElementById('saldo-disponible').textContent = formatearMoneda(res.data.saldoDisponible);
}

async function cargarMovimientos() {
    const tbody = document.getElementById('tabla-movimientos');
    const res = await api.getMovimientos();

    if (!res.ok) {
        tbody.innerHTML = `<tr><td colspan="4" class="text-center text-danger py-4">Error al cargar movimientos</td></tr>`;
        return;
    }

    if (!res.data || res.data.length === 0) {
        tbody.innerHTML = `<tr><td colspan="4" class="text-center text-muted py-4">Sin movimientos</td></tr>`;
        return;
    }

    const colores = {
        DEPOSITO:   'success',
        RETENCION:  'warning',
        LIBERACION: 'info',
        PAGO:       'danger',
        COBRO:      'success'
    };

    tbody.innerHTML = res.data.map(m => `
        <tr>
            <td class="small text-muted">${formatearFecha(m.fecha)}</td>
            <td><span class="badge text-bg-${colores[m.tipo] ?? 'secondary'}">${m.tipo}</span></td>
            <td>${m.subastaId ? `<a href="subasta.html?id=${m.subastaId}">#${m.subastaId}</a>` : '—'}</td>
            <td class="text-end fw-bold">${formatearMoneda(m.monto)}</td>
        </tr>
    `).join('');
}

async function onDepositar(e) {
    e.preventDefault();
    const input = document.getElementById('monto-deposito');
    const monto = Number(input.value);

    if (!monto || monto <= 0) {
        mostrarToast('El monto debe ser mayor a 0', 'warning');
        return;
    }

    const btn = document.getElementById('btn-depositar');
    btn.disabled = true;
    btn.innerHTML = `<span class="spinner-border spinner-border-sm"></span> Procesando…`;

    const res = await api.depositar(monto);

    btn.disabled = false;
    btn.innerHTML = `<i class="bi bi-plus-circle"></i> Depositar`;

    if (!res.ok) {
        mostrarToast(res.error ?? 'Error al depositar', 'error');
        return;
    }

    mostrarToast(`Depósito de ${formatearMoneda(monto)} acreditado`, 'success');
    input.value = '';
    await cargarSaldos();
    await cargarMovimientos();
}