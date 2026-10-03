// Helpers de UI: toasts, spinners, formateo

function mostrarToast(mensaje, tipo = 'info') {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const colores = {
        success: 'text-bg-success',
        error:   'text-bg-danger',
        warning: 'text-bg-warning',
        info:    'text-bg-primary'
    };
    const iconos = {
        success: 'check-circle',
        error:   'x-circle',
        warning: 'exclamation-triangle',
        info:    'info-circle'
    };

    const id = 'toast-' + Date.now();
    const html = `
        <div id="${id}" class="toast align-items-center ${colores[tipo] ?? colores.info} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi bi-${iconos[tipo] ?? iconos.info} me-2"></i>${mensaje}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>`;
    container.insertAdjacentHTML('beforeend', html);

    const el = document.getElementById(id);
    const toast = new bootstrap.Toast(el, { delay: 3500 });
    toast.show();
    el.addEventListener('hidden.bs.toast', () => el.remove());
}

function mostrarLoader(mostrar) {
    const loader = document.getElementById('loader');
    if (!loader) return;
    loader.classList.toggle('d-none', !mostrar);
}

function formatearMoneda(valor) {
    return new Intl.NumberFormat('es-AR', {
        style: 'currency',
        currency: 'ARS',
        minimumFractionDigits: 0
    }).format(valor ?? 0);
}

function formatearFecha(fecha) {
    const d = new Date(fecha);
    return d.toLocaleString('es-AR', {
        day: '2-digit', month: '2-digit', year: 'numeric',
        hour: '2-digit', minute: '2-digit'
    });
}

function formatearDuracion(ms) {
    const totalSeg = Math.max(0, Math.floor(ms / 1000));
    const h = Math.floor(totalSeg / 3600);
    const m = Math.floor((totalSeg % 3600) / 60);
    const s = totalSeg % 60;
    const pad = (n) => String(n).padStart(2, '0');
    return h > 0 ? `${h}:${pad(m)}:${pad(s)}` : `${pad(m)}:${pad(s)}`;
}

function estadoTimer(fechaFin) {
    const restante = new Date(fechaFin) - new Date();
    if (restante <= 0)      return { texto: 'Finalizada', clase: 'text-muted' };
    if (restante <= 30_000) return { texto: formatearDuracion(restante), clase: 'text-danger fw-bold' };
    if (restante <= 60_000) return { texto: formatearDuracion(restante), clase: 'text-warning fw-bold' };
    return { texto: formatearDuracion(restante), clase: 'text-dark' };
}

function debounce(fn, ms) {
    let t;
    return (...args) => {
        clearTimeout(t);
        t = setTimeout(() => fn(...args), ms);
    };
}