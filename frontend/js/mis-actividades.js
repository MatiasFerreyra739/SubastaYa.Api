document.addEventListener('DOMContentLoaded', async () => {
    await Promise.all([cargarCompras(), cargarPublicaciones()]);
});

async function cargarCompras() {
    const loader = document.getElementById('loader-compras');
    const lista = document.getElementById('lista-compras');

    const res = await api.getMisPujas();
    loader.classList.add('d-none');

    if (!res.ok) {
        lista.innerHTML = `<div class="col-12 text-center text-danger">Error al cargar tus pujas</div>`;
        return;
    }

    if (!res.data || res.data.length === 0) {
        lista.innerHTML = `<div class="col-12 text-center text-muted py-4">
            <i class="bi bi-inbox display-4"></i>
            <p class="mt-2">Todavía no participaste en ninguna subasta.</p>
        </div>`;
        return;
    }

    lista.innerHTML = res.data.map(item => {
        const badge = item.gano
            ? '<span class="badge bg-success">Ganada</span>'
            : item.subastaEstado === 'ACTIVA'
                ? (item.esLider
                    ? '<span class="badge bg-info">Liderando</span>'
                    : '<span class="badge bg-warning">Superado</span>')
                : '<span class="badge bg-secondary">Perdida</span>';

        return `
        <div class="col-12 col-md-6 col-lg-4">
            <div class="card h-100 shadow-sm">
                <img src="${item.urlImagen || 'https://via.placeholder.com/400x300?text=SubastaYa'}"
                     class="card-img-top" style="height:160px;object-fit:cover;"
                     onerror="this.src='https://via.placeholder.com/400x300?text=SubastaYa'">
                <div class="card-body">
                    ${badge}
                    <h6 class="card-title mt-2">${item.titulo}</h6>
                    <div class="d-flex justify-content-between small">
                        <span class="text-muted">Mi puja</span>
                        <strong>${formatearMoneda(item.miPuja)}</strong>
                    </div>
                    <div class="d-flex justify-content-between small">
                        <span class="text-muted">Puja actual</span>
                        <strong>${formatearMoneda(item.pujaActual)}</strong>
                    </div>
                    <a href="subasta.html?id=${item.subastaId}" class="btn btn-outline-primary btn-sm w-100 mt-3">
                        Ver subasta
                    </a>
                </div>
            </div>
        </div>`;
    }).join('');
}

async function cargarPublicaciones() {
    const loader = document.getElementById('loader-publicaciones');
    const lista = document.getElementById('lista-publicaciones');

    const res = await api.getMisPublicaciones();
    loader.classList.add('d-none');

    if (!res.ok) {
        lista.innerHTML = `<div class="col-12 text-center text-danger">Error al cargar tus publicaciones</div>`;
        return;
    }

    if (!res.data || res.data.length === 0) {
        lista.innerHTML = `<div class="col-12 text-center text-muted py-4">
            <i class="bi bi-megaphone display-4"></i>
            <p class="mt-2">Todavía no publicaste ninguna subasta.</p>
            <a href="publicar.html" class="btn btn-primary mt-2">Publicar la primera</a>
        </div>`;
        return;
    }

    lista.innerHTML = res.data.map(item => {
        const badge = {
            ACTIVA:     '<span class="badge bg-success">Activa</span>',
            PROGRAMADA: '<span class="badge bg-info">Programada</span>',
            FINALIZADA: '<span class="badge bg-secondary">Finalizada</span>',
            DESIERTA:   '<span class="badge bg-dark">Desierta</span>'
        }[item.estado] ?? '';

        return `
        <div class="col-12 col-md-6 col-lg-4">
            <div class="card h-100 shadow-sm">
                <img src="${item.urlImagen || 'https://via.placeholder.com/400x300?text=SubastaYa'}"
                     class="card-img-top" style="height:160px;object-fit:cover;"
                     onerror="this.src='https://via.placeholder.com/400x300?text=SubastaYa'">
                <div class="card-body">
                    ${badge}
                    <h6 class="card-title mt-2">${item.titulo}</h6>
                    <div class="d-flex justify-content-between small">
                        <span class="text-muted">Puja actual</span>
                        <strong>${formatearMoneda(item.pujaActual)}</strong>
                    </div>
                    <div class="d-flex justify-content-between small">
                        <span class="text-muted">Ofertas</span>
                        <span>${item.cantidadPujas}</span>
                    </div>
                    <div class="d-flex justify-content-between small">
                        <span class="text-muted">Recaudación</span>
                        <strong class="text-success">${formatearMoneda(item.recaudacion ?? 0)}</strong>
                    </div>
                    <a href="subasta.html?id=${item.id}" class="btn btn-outline-primary btn-sm w-100 mt-3">
                        Ver subasta
                    </a>
                </div>
            </div>
        </div>`;
    }).join('');
}