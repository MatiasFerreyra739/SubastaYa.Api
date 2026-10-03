let intervaloReloj = null;

document.addEventListener('DOMContentLoaded', async () => {
    await cargarCategorias();
    await cargarSubastas();

    document.querySelectorAll('#filtro-estado, #filtro-categoria, #filtro-orden')
        .forEach(el => el.addEventListener('change', cargarSubastas));

    document.querySelectorAll('#filtro-precio-min, #filtro-precio-max')
        .forEach(el => el.addEventListener('input', debounce(cargarSubastas, 400)));
});

async function cargarCategorias() {
    const res = await api.getCategorias();
    if (!res.ok) return;
    const select = document.getElementById('filtro-categoria');
    res.data.forEach(c => {
        select.insertAdjacentHTML('beforeend', `<option value="${c.id}">${c.nombre}</option>`);
    });
}

async function cargarSubastas() {
    mostrarLoader(true);

    const filtros = {
        estado:      document.getElementById('filtro-estado').value,
        categoriaId: document.getElementById('filtro-categoria').value,
        precioMin:   document.getElementById('filtro-precio-min').value,
        precioMax:   document.getElementById('filtro-precio-max').value,
        orden:       document.getElementById('filtro-orden').value,
    };
    Object.keys(filtros).forEach(k => !filtros[k] && delete filtros[k]);

    const res = await api.getSubastas(filtros);
    mostrarLoader(false);

    if (!res.ok) {
        mostrarToast('Error al cargar subastas', 'error');
        return;
    }

    renderSubastas(res.data);
    iniciarRelojes();
}

function renderSubastas(subastas) {
    const grid = document.getElementById('grid-subastas');
    const empty = document.getElementById('empty-state');
    grid.innerHTML = '';

    if (!subastas || subastas.length === 0) {
        empty.classList.remove('d-none');
        return;
    }
    empty.classList.add('d-none');

    subastas.forEach(s => {
        const card = `
        <div class="col-12 col-sm-6 col-lg-4">
          <div class="card h-100 shadow-sm card-subasta">
            <img src="${s.urlImagen || 'https://via.placeholder.com/600x400?text=SubastaYa'}" class="card-img-top" alt="${s.titulo}"
                 style="height:200px;object-fit:cover;"
                 onerror="this.src='https://via.placeholder.com/600x400?text=SubastaYa'">
            <div class="card-body d-flex flex-column">
              <span class="badge bg-secondary mb-2 align-self-start">${s.categoriaNombre}</span>
              <h5 class="card-title">${s.titulo}</h5>
              <p class="card-text text-muted small mb-2">${(s.descripcion ?? '').substring(0, 80)}…</p>

              <div class="mt-auto">
                <div class="d-flex justify-content-between">
                  <span class="text-muted small">Puja actual</span>
                  <strong>${formatearMoneda(s.pujaActual ?? s.precioBase)}</strong>
                </div>
                <div class="d-flex justify-content-between">
                  <span class="text-muted small">Ofertas</span>
                  <span>${s.cantidadPujas}</span>
                </div>
                <div class="d-flex justify-content-between align-items-center mt-2">
                  <span class="text-muted small">Cierra en</span>
                  <span class="timer-card fw-bold" data-fin="${s.fechaFin}">—</span>
                </div>
                <a href="subasta.html?id=${s.id}" class="btn btn-primary w-100 mt-3">Ver subasta</a>
              </div>
            </div>
          </div>
        </div>`;
        grid.insertAdjacentHTML('beforeend', card);
    });
}

function iniciarRelojes() {
    if (intervaloReloj) clearInterval(intervaloReloj);
    const actualizar = () => {
        document.querySelectorAll('.timer-card').forEach(el => {
            const { texto, clase } = estadoTimer(el.dataset.fin);
            el.textContent = texto;
            el.className = `timer-card fw-bold ${clase}`;
        });
    };
    actualizar();
    intervaloReloj = setInterval(actualizar, 1000);
}