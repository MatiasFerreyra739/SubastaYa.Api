// Lógica de subasta.html — Sala en vivo

let subastaId = null;
let subastaActual = null;
let intervaloPolling = null;
let intervaloReloj = null;
let ultimaFechaFin = null;
let conexionSignalR = null;

document.addEventListener('DOMContentLoaded', async () => {
    const params = new URLSearchParams(location.search);
    subastaId = params.get('id');

    if (!subastaId) {
        mostrarToast('Falta el ID de la subasta', 'error');
        setTimeout(() => location.href = 'index.html', 1500);
        return;
    }

    document.getElementById('btn-usar-sugerida').addEventListener('click', () => {
        document.getElementById('puja-monto').value =
            document.getElementById('puja-sugerida').value;
    });

    document.getElementById('btn-pujar').addEventListener('click', onPujar);

    await cargarSubasta();

    iniciarSignalR();

    // Se mantiene como respaldo mientras verificamos SignalR.
    iniciarPolling();

    iniciarReloj();
});

async function cargarSubasta() {
    const res = await api.getSubasta(subastaId);

    if (!res.ok) {
        document.getElementById('loader-principal').classList.add('d-none');

        mostrarToast('Subasta no encontrada', 'error');

        setTimeout(() => location.href = 'index.html', 1500);

        return;
    }

    subastaActual = res.data;
    ultimaFechaFin = subastaActual.fechaFin;

    document.getElementById('loader-principal').classList.add('d-none');
    document.getElementById('contenido').classList.remove('d-none');

    renderSubasta();
}

function renderSubasta() {
    const s = subastaActual;

    document.getElementById('subasta-imagen').src =
        s.urlImagen ||
        'https://via.placeholder.com/800x400?text=SubastaYa';

    document.getElementById('subasta-titulo').textContent = s.titulo;
    document.getElementById('subasta-descripcion').textContent = s.descripcion;
    document.getElementById('subasta-categoria').textContent = s.categoriaNombre;

    document.getElementById('subasta-precio-base').textContent =
        formatearMoneda(s.precioBase);

    document.getElementById('subasta-incremento').textContent =
        formatearMoneda(s.incrementoMinimo);

    document.getElementById('subasta-cantidad-pujas').textContent =
        s.cantidadPujas ?? 0;

    const estadoBadge = document.getElementById('subasta-estado');

    estadoBadge.textContent = s.estado;

    estadoBadge.className =
        'badge mb-2 ' +
        ({
            ACTIVA: 'bg-success',
            PROGRAMADA: 'bg-info',
            FINALIZADA: 'bg-secondary',
            DESIERTA: 'bg-dark'
        }[s.estado] ?? 'bg-secondary');

    const pujaActual =
        s.pujaActual ?? s.precioBase;

    document.getElementById('puja-actual').textContent =
        formatearMoneda(pujaActual);

    const sugerida =
        s.pujaActual
            ? s.pujaActual + s.incrementoMinimo
            : s.precioBase;

    document.getElementById('puja-sugerida').value = sugerida;
    document.getElementById('puja-monto').placeholder = sugerida;

    renderEstadoUsuario();
    renderHistorial(s.pujas ?? []);

    const activa = s.estado === 'ACTIVA';

    document.getElementById('btn-pujar').disabled = !activa;
    document.getElementById('puja-monto').disabled = !activa;
    document.getElementById('btn-usar-sugerida').disabled = !activa;
}

function renderEstadoUsuario() {
    const s = subastaActual;

    const badge =
        document.getElementById('estado-usuario');

    const texto =
        document.getElementById('estado-texto');

    const userId =
        Number(getUserId());

    if (!s.ultimaPujaUsuarioId || s.estado !== 'ACTIVA') {
        badge.classList.add('d-none');
        return;
    }

    badge.classList.remove('d-none');

    if (s.ultimaPujaUsuarioId === userId) {
        badge.className =
            'text-center p-2 rounded bg-success text-white';

        texto.textContent =
            '¡Estás liderando!';
    } else {
        badge.className =
            'text-center p-2 rounded bg-warning';

        texto.textContent =
            'Fuiste superado';
    }
}

function renderHistorial(pujas) {
    const div =
        document.getElementById('historial');

    if (!pujas || pujas.length === 0) {
        div.innerHTML =
            '<p class="text-muted small mb-0">Sin ofertas aún.</p>';

        return;
    }

    const ordenadas =
        [...pujas].sort(
            (a, b) =>
                new Date(b.fechaPuja) -
                new Date(a.fechaPuja)
        );

    div.innerHTML =
        ordenadas
            .map((p, i) => `
                <div class="d-flex justify-content-between align-items-center border-bottom py-2 ${i === 0 ? 'bg-light' : ''}">
                    <div>
                        <div class="fw-bold small">
                            ${p.seudonimo}
                        </div>

                        <div class="text-muted"
                             style="font-size:.75rem;">
                            ${formatearFecha(p.fechaPuja)}
                        </div>
                    </div>

                    <div class="fw-bold ${i === 0 ? 'text-success' : ''}">
                        ${formatearMoneda(p.monto)}
                    </div>
                </div>
            `)
            .join('');
}

/*
 * ============================
 * SIGNALR
 * ============================
 */

async function iniciarSignalR() {

    if (typeof signalR === 'undefined') {
        console.error('SignalR no está cargado.');
        return;
    }

    try {

        conexionSignalR =
            new signalR.HubConnectionBuilder()
                .withUrl('https://localhost:7113/hubs/subastas')
                .withAutomaticReconnect()
                .build();

        /*
         * Evento enviado por el backend cuando
         * una puja fue registrada correctamente.
         */
        conexionSignalR.on(
            'NuevaPuja',
            async (data) => {

                console.log(
                    'Nueva puja recibida por SignalR:',
                    data
                );

                /*
                 * Actualizamos la información completa
                 * de la subasta desde la API.
                 *
                 * Esto nos permite obtener:
                 * - puja actual
                 * - cantidad de pujas
                 * - historial
                 * - líder
                 * - fecha de finalización
                 */
                await cargarSubasta();

                /*
                 * Si la fecha de finalización cambió,
                 * mostramos el aviso de anti-sniping.
                 */
                if (
                    ultimaFechaFin &&
                    data.fechaFin &&
                    new Date(data.fechaFin) >
                    new Date(ultimaFechaFin)
                ) {
                    mostrarToast(
                        '⏱ Tiempo extendido por anti-sniping',
                        'warning'
                    );
                }
            }
        );

        await conexionSignalR.start();

        console.log(
            'SignalR conectado correctamente.'
        );

        /*
         * Nos unimos al grupo correspondiente
         * a esta subasta.
         */
        await conexionSignalR.invoke(
            'UnirseASubasta',
            Number(subastaId)
        );

        console.log(
            `Unido a la subasta ${subastaId}.`
        );

    } catch (error) {

        console.error(
            'Error conectando con SignalR:',
            error
        );

        /*
         * El polling continúa funcionando como
         * mecanismo de respaldo.
         */
    }
}

function iniciarPolling() {

    if (intervaloPolling) {
        clearInterval(intervaloPolling);
    }

    intervaloPolling =
        setInterval(async () => {

            const res =
                await api.getSubasta(subastaId);

            if (!res.ok) {

                if (res.status === 404) {

                    clearInterval(
                        intervaloPolling
                    );

                    mostrarToast(
                        'La subasta ya no está disponible',
                        'warning'
                    );
                }

                return;
            }

            const nueva = res.data;

            if (
                ultimaFechaFin &&
                new Date(nueva.fechaFin) >
                new Date(ultimaFechaFin)
            ) {
                mostrarToast(
                    '⏱ Tiempo extendido por anti-sniping',
                    'warning'
                );
            }

            ultimaFechaFin =
                nueva.fechaFin;

            subastaActual =
                nueva;

            renderSubasta();

        }, 2000);
}

function iniciarReloj() {

    if (intervaloReloj) {
        clearInterval(intervaloReloj);
    }

    const actualizar = () => {

        if (!subastaActual) {
            return;
        }

        const el =
            document.getElementById('timer');

        const {
            texto,
            clase
        } =
            estadoTimer(
                subastaActual.fechaFin
            );

        el.textContent = texto;

        el.className =
            'timer-grande ' + clase;
    };

    actualizar();

    intervaloReloj =
        setInterval(
            actualizar,
            1000
        );
}

async function onPujar() {

    const monto =
        Number(
            document.getElementById('puja-monto').value
        );

    const sugerida =
        Number(
            document.getElementById('puja-sugerida').value
        );

    if (!monto || monto <= 0) {

        mostrarToast(
            'Ingresá un monto válido',
            'warning'
        );

        return;
    }

    if (monto < sugerida) {

        mostrarToast(
            `El monto mínimo es ${formatearMoneda(sugerida)}`,
            'warning'
        );

        return;
    }

    const btn =
        document.getElementById('btn-pujar');

    btn.disabled = true;

    btn.innerHTML =
        `<span class="spinner-border spinner-border-sm"></span> Enviando…`;

    const res =
        await api.pujar(
            subastaId,
            monto
        );

    btn.disabled = false;

    btn.innerHTML =
        `<i class="bi bi-hammer"></i> Confirmar puja`;

    if (res.ok) {

        mostrarToast(
            '¡Puja registrada!',
            'success'
        );

        document.getElementById(
            'puja-monto'
        ).value = '';

        /*
         * La actualización principal llegará
         * por SignalR.
         *
         * Por ahora también recargamos para
         * mantener el comportamiento anterior.
         */
        await cargarSubasta();

        return;
    }

    switch (res.status) {

        case 409:

            mostrarToast(
                'Otra puja se registró antes. Actualizando…',
                'warning'
            );

            await cargarSubasta();

            break;

        case 422:

            mostrarToast(
                'Fondos insuficientes',
                'error'
            );

            break;

        case 400:

            mostrarToast(
                res.error ?? 'Datos inválidos',
                'error'
            );

            break;

        case 404:

            mostrarToast(
                'La subasta no existe',
                'error'
            );

            break;

        default:

            mostrarToast(
                res.error ?? 'Error al pujar',
                'error'
            );
    }
}

window.addEventListener(
    'beforeunload',
    async () => {

        if (intervaloPolling) {
            clearInterval(intervaloPolling);
        }

        if (intervaloReloj) {
            clearInterval(intervaloReloj);
        }

        if (conexionSignalR) {

            try {

                await conexionSignalR.invoke(
                    'SalirDeSubasta',
                    Number(subastaId)
                );

                await conexionSignalR.stop();

            } catch (error) {

                console.error(
                    'Error cerrando SignalR:',
                    error
                );
            }
        }
    }
);