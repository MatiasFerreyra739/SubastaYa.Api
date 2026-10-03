document.addEventListener('DOMContentLoaded', async () => {
    await cargarCategorias();
    prellenarFechas();

    document.getElementById('form-publicar').addEventListener('submit', onPublicar);

    ['titulo', 'descripcion', 'urlImagen', 'precioBase', 'incrementoMinimo'].forEach(id => {
        document.getElementById(id).addEventListener('input', actualizarPreview);
    });
});

async function cargarCategorias() {
    const res = await api.getCategorias();
    if (!res.ok) return;
    const select = document.getElementById('categoriaId');
    res.data.forEach(c => {
        select.insertAdjacentHTML('beforeend', `<option value="${c.id}">${c.nombre}</option>`);
    });
}

function prellenarFechas() {
    const ahora = new Date();
    const enUnaHora = new Date(ahora.getTime() + 60 * 60 * 1000);
    const enUnDia = new Date(ahora.getTime() + 24 * 60 * 60 * 1000);

    const fmt = (d) => {
        const pad = (n) => String(n).padStart(2, '0');
        return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
    };

    document.getElementById('fechaInicio').value = fmt(enUnaHora);
    document.getElementById('fechaFin').value = fmt(enUnDia);
}

function actualizarPreview() {
    document.getElementById('preview-titulo').textContent =
        document.getElementById('titulo').value || 'Título del producto';
    document.getElementById('preview-descripcion').textContent =
        document.getElementById('descripcion').value || 'Descripción…';
    document.getElementById('preview-precio').textContent =
        formatearMoneda(Number(document.getElementById('precioBase').value) || 0);
    document.getElementById('preview-incremento').textContent =
        formatearMoneda(Number(document.getElementById('incrementoMinimo').value) || 0);

    const url = document.getElementById('urlImagen').value;
    if (url) document.getElementById('preview-imagen').src = url;
}

async function onPublicar(e) {
    e.preventDefault();
    const form = document.getElementById('form-publicar');

    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        mostrarToast('Revisá los campos obligatorios', 'warning');
        return;
    }

    const inicio = new Date(document.getElementById('fechaInicio').value);
    const fin = new Date(document.getElementById('fechaFin').value);
    const precioBase = Number(document.getElementById('precioBase').value);
    const incrementoMinimo = Number(document.getElementById('incrementoMinimo').value);

    if (fin <= inicio) {
        mostrarToast('La fecha de fin debe ser posterior a la de inicio', 'error');
        return;
    }
    if (precioBase <= 0 || incrementoMinimo <= 0) {
        mostrarToast('Precio base e incremento deben ser positivos', 'error');
        return;
    }

    const body = {
        titulo: document.getElementById('titulo').value.trim(),
        descripcion: document.getElementById('descripcion').value.trim(),
        urlImagen: document.getElementById('urlImagen').value.trim() || null,
        categoriaId: Number(document.getElementById('categoriaId').value),
        precioBase,
        incrementoMinimo,
        fechaInicio: inicio.toISOString(),
        fechaFin: fin.toISOString()
    };

    const btn = document.getElementById('btn-publicar');
    btn.disabled = true;
    btn.innerHTML = `<span class="spinner-border spinner-border-sm"></span> Publicando…`;

    const res = await api.crearSubasta(body);

    btn.disabled = false;
    btn.innerHTML = `<i class="bi bi-send"></i> Publicar subasta`;

    if (!res.ok) {
        mostrarToast(res.error ?? 'Error al publicar la subasta', 'error');
        return;
    }

    mostrarToast('¡Subasta publicada!', 'success');
    setTimeout(() => location.href = `subasta.html?id=${res.data.id}`, 800);
}