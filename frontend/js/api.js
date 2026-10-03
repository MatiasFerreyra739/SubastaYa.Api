```javascript
// =====================================================================
// Capa única de comunicación con la API.
// USE_MOCK = true  → devuelve datos falsos, no llama al backend.
// USE_MOCK = false → llama al backend real.
// =====================================================================

const USE_MOCK = false;
const API_BASE = 'https://localhost:7113/api/v1';

function getUserId() {
    return localStorage.getItem('userId') ?? '2';
}

function setUserId(id) {
    localStorage.setItem('userId', id);
}

// ---------------------------------------------------------------------
// Fetch real
// ---------------------------------------------------------------------
async function apiFetch(path, options = {}) {
    const headers = {
        'Content-Type': 'application/json',
        'X-User-Id': getUserId(),
        ...(options.headers ?? {})
    };

    try {
        const res = await fetch(
            `${API_BASE}${path}`,
            {
                ...options,
                headers
            }
        );

        const data =
            res.status === 204
                ? null
                : await res.json().catch(() => null);

        if (!res.ok) {
            return {
                ok: false,
                status: res.status,
                data,
                error:
                    data?.mensaje ??
                    'Error en la petición'
            };
        }

        return {
            ok: true,
            status: res.status,
            data
        };

    } catch (e) {

        console.error(
            'Error comunicando con la API:',
            e
        );

        return {
            ok: false,
            status: 0,
            error:
                'No se pudo conectar con el servidor'
        };
    }
}

// ---------------------------------------------------------------------
// Datos mock
// ---------------------------------------------------------------------
const MOCK = {
    categorias: [
        { id: 1, nombre: 'Tecnología' },
        { id: 2, nombre: 'Coleccionables' },
        { id: 3, nombre: 'Indumentaria' },
        { id: 4, nombre: 'Vehículos' },
    ],

    subastas: [
        {
            id: 1,
            titulo: 'iPhone 15 Pro 256GB',
            descripcion:
                'Impecable, con caja y cargador original.',
            urlImagen:
                'https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=600',
            categoriaId: 1,
            categoriaNombre: 'Tecnología',
            precioBase: 40000,
            incrementoMinimo: 1000,
            pujaActual: 45000,
            cantidadPujas: 2,
            fechaInicio:
                new Date(
                    Date.now() - 3600_000
                ).toISOString(),
            fechaFin:
                new Date(
                    Date.now() + 25 * 60_000
                ).toISOString(),
            estado: 'ACTIVA',
            ultimaPujaUsuarioId: 2,
        },

        {
            id: 2,
            titulo: 'Reloj vintage Omega',
            descripcion:
                'Modelo de 1968, funciona perfecto.',
            urlImagen:
                'https://images.unsplash.com/photo-1523170335258-f5ed11844a49?w=600',
            categoriaId: 2,
            categoriaNombre: 'Coleccionables',
            precioBase: 150000,
            incrementoMinimo: 5000,
            pujaActual: 150000,
            cantidadPujas: 0,
            fechaInicio:
                new Date(
                    Date.now() - 60_000
                ).toISOString(),
            fechaFin:
                new Date(
                    Date.now() + 90_000
                ).toISOString(),
            estado: 'ACTIVA',
            ultimaPujaUsuarioId: null,
        },

        {
            id: 3,
            titulo: 'Campera de cuero',
            descripcion:
                'Talle L, poco uso.',
            urlImagen:
                'https://images.unsplash.com/photo-1551028719-00167b16eac5?w=600',
            categoriaId: 3,
            categoriaNombre: 'Indumentaria',
            precioBase: 20000,
            incrementoMinimo: 500,
            pujaActual: 20000,
            cantidadPujas: 0,
            fechaInicio:
                new Date(
                    Date.now() + 24 * 3600_000
                ).toISOString(),
            fechaFin:
                new Date(
                    Date.now() + 48 * 3600_000
                ).toISOString(),
            estado: 'PROGRAMADA',
            ultimaPujaUsuarioId: null,
        },

        {
            id: 4,
            titulo: 'Bicicleta MTB rodado 29',
            descripcion:
                'Vendida con ganador, esperando liquidación.',
            urlImagen:
                'https://images.unsplash.com/photo-1485965120184-e220f721d03e?w=600',
            categoriaId: 4,
            categoriaNombre: 'Vehículos',
            precioBase: 80000,
            incrementoMinimo: 2000,
            pujaActual: 95000,
            cantidadPujas: 5,
            fechaInicio:
                new Date(
                    Date.now() - 3 * 3600_000
                ).toISOString(),
            fechaFin:
                new Date(
                    Date.now() - 60_000
                ).toISOString(),
            estado: 'FINALIZADA',
            ultimaPujaUsuarioId: 3,
        },

        {
            id: 5,
            titulo: 'Poster firmado',
            descripcion:
                'Sin ofertas al cierre.',
            urlImagen:
                'https://images.unsplash.com/photo-1513475382585-d06e58bcb0e0?w=600',
            categoriaId: 2,
            categoriaNombre: 'Coleccionables',
            precioBase: 5000,
            incrementoMinimo: 500,
            pujaActual: null,
            cantidadPujas: 0,
            fechaInicio:
                new Date(
                    Date.now() - 4 * 3600_000
                ).toISOString(),
            fechaFin:
                new Date(
                    Date.now() - 120_000
                ).toISOString(),
            estado: 'DESIERTA',
            ultimaPujaUsuarioId: null,
        },
    ],

    pujas: {
        1: [
            {
                id: 1,
                seudonimo: 'Postor A',
                monto: 40000,
                fechaPuja:
                    new Date(
                        Date.now() - 30 * 60_000
                    ).toISOString(),
                compradorId: 2
            },

            {
                id: 2,
                seudonimo: 'Postor B',
                monto: 45000,
                fechaPuja:
                    new Date(
                        Date.now() - 5 * 60_000
                    ).toISOString(),
                compradorId: 3
            },
        ],

        2: [],
    },

    saldos: {
        '1': {
            saldoTotal: 0,
            saldoRetenido: 0,
            saldoDisponible: 0
        },

        '2': {
            saldoTotal: 150000,
            saldoRetenido: 45000,
            saldoDisponible: 105000
        },

        '3': {
            saldoTotal: 200000,
            saldoRetenido: 0,
            saldoDisponible: 200000
        },

        '4': {
            saldoTotal: 500,
            saldoRetenido: 0,
            saldoDisponible: 500
        },
    },

    movimientos: [
        {
            id: 1,
            tipo: 'DEPOSITO',
            monto: 150000,
            fecha:
                new Date(
                    Date.now() - 2 * 3600_000
                ).toISOString(),
            subastaId: null
        },

        {
            id: 2,
            tipo: 'RETENCION',
            monto: 45000,
            fecha:
                new Date(
                    Date.now() - 5 * 60_000
                ).toISOString(),
            subastaId: 1
        },
    ],
};

// ---------------------------------------------------------------------
// Helpers mock
// ---------------------------------------------------------------------

const delay =
    (ms) =>
        new Promise(
            r => setTimeout(r, ms)
        );

const mockOk =
    (data) => ({
        ok: true,
        status: 200,
        data
    });

const mockError =
    (status, mensaje) => ({
        ok: false,
        status,
        error: mensaje,
        data: {
            mensaje
        }
    });

// ---------------------------------------------------------------------
// Persistencia del MOCK en localStorage
// ---------------------------------------------------------------------

const MOCK_KEY =
    'subastaya_mock_v1';

function cargarMockDeStorage() {

    const raw =
        localStorage.getItem(
            MOCK_KEY
        );

    if (!raw) {
        return;
    }

    try {

        const guardado =
            JSON.parse(raw);

        if (guardado.subastas) {
            MOCK.subastas =
                guardado.subastas;
        }

        if (guardado.pujas) {
            MOCK.pujas =
                guardado.pujas;
        }

        if (guardado.saldos) {
            MOCK.saldos =
                guardado.saldos;
        }

        if (guardado.movimientos) {
            MOCK.movimientos =
                guardado.movimientos;
        }

    } catch (e) {

        console.warn(
            'No se pudo cargar el mock guardado',
            e
        );
    }
}

function guardarMockEnStorage() {

    localStorage.setItem(
        MOCK_KEY,
        JSON.stringify({
            subastas:
                MOCK.subastas,

            pujas:
                MOCK.pujas,

            saldos:
                MOCK.saldos,

            movimientos:
                MOCK.movimientos
        })
    );
}

cargarMockDeStorage();

// ---------------------------------------------------------------------
// API pública
// ---------------------------------------------------------------------

const api = {

    // ================================================================
    // SUBASTAS
    // ================================================================

    async getSubastas(filtros = {}) {

        if (!USE_MOCK) {

            const q =
                new URLSearchParams(
                    filtros
                ).toString();

            return apiFetch(
                `/auctions${
                    q
                        ? '?' + q
                        : ''
                }`
            );
        }

        await delay(300);

        let lista =
            [...MOCK.subastas];

        if (filtros.estado) {
            lista =
                lista.filter(
                    s =>
                        s.estado ===
                        filtros.estado
                );
        }

        if (filtros.categoriaId) {
            lista =
                lista.filter(
                    s =>
                        s.categoriaId ===
                        Number(
                            filtros.categoriaId
                        )
                );
        }

        if (filtros.precioMin) {
            lista =
                lista.filter(
                    s =>
                        (
                            s.pujaActual ??
                            s.precioBase
                        ) >=
                        Number(
                            filtros.precioMin
                        )
                );
        }

        if (filtros.precioMax) {
            lista =
                lista.filter(
                    s =>
                        (
                            s.pujaActual ??
                            s.precioBase
                        ) <=
                        Number(
                            filtros.precioMax
                        )
                );
        }

        if (
            filtros.orden ===
            'tiempo'
        ) {

            lista.sort(
                (a, b) =>
                    new Date(
                        a.fechaFin
                    ) -
                    new Date(
                        b.fechaFin
                    )
            );

        } else if (
            filtros.orden ===
            'puja'
        ) {

            lista.sort(
                (a, b) =>
                    (
                        b.pujaActual ??
                        b.precioBase
                    ) -
                    (
                        a.pujaActual ??
                        a.precioBase
                    )
            );
        }

        return mockOk(lista);
    },

    // ================================================================
    // DETALLE DE SUBASTA
    // ================================================================

    async getSubasta(id) {

        if (!USE_MOCK) {
            return apiFetch(
                `/auctions/${id}`
            );
        }

        await delay(200);

        const s =
            MOCK.subastas.find(
                x =>
                    x.id ===
                    Number(id)
            );

        if (!s) {
            return mockError(
                404,
                'Subasta no encontrada'
            );
        }

        return mockOk({
            ...s,
            pujas:
                MOCK.pujas[s.id] ??
                []
        });
    },

    // ================================================================
    // CREAR SUBASTA
    // ================================================================

    async crearSubasta(body) {

        if (!USE_MOCK) {

            return apiFetch(
                '/auctions',
                {
                    method: 'POST',
                    body:
                        JSON.stringify(
                            body
                        )
                }
            );
        }

        await delay(400);

        const nueva = {

            id:
                MOCK.subastas.length +
                1,

            ...body,

            categoriaNombre:
                MOCK.categorias.find(
                    c =>
                        c.id ===
                        body.categoriaId
                )?.nombre ??
                '—',

            pujaActual: null,

            cantidadPujas: 0,

            estado:
                new Date(
                    body.fechaInicio
                ) <= new Date()
                    ? 'ACTIVA'
                    : 'PROGRAMADA',

            ultimaPujaUsuarioId:
                null
        };

        MOCK.subastas.push(
            nueva
        );

        guardarMockEnStorage();

        return mockOk(nueva);
    },

    // ================================================================
    // PUJAS
    // ================================================================

    async pujar(
        subastaId,
        monto
    ) {

        if (!USE_MOCK) {

            return apiFetch(
                `/auctions/${subastaId}/bids`,
                {
                    method: 'POST',
                    body:
                        JSON.stringify({
                            monto
                        })
                }
            );
        }

        await delay(300);

        const s =
            MOCK.subastas.find(
                x =>
                    x.id ===
                    Number(
                        subastaId
                    )
            );

        if (!s) {
            return mockError(
                404,
                'Subasta no encontrada'
            );
        }

        if (
            s.estado !==
            'ACTIVA'
        ) {
            return mockError(
                400,
                'La subasta no está activa'
            );
        }

        const minimo =
            s.pujaActual
                ? s.pujaActual +
                  s.incrementoMinimo
                : s.precioBase;

        if (monto < minimo) {
            return mockError(
                400,
                `El monto mínimo es ${minimo}`
            );
        }

        const userId =
            getUserId();

        const saldo =
            MOCK.saldos[userId];

        if (
            saldo &&
            saldo.saldoDisponible <
            monto
        ) {
            return mockError(
                422,
                'Fondos insuficientes'
            );
        }

        const nueva = {

            id:
                (
                    MOCK.pujas[
                        subastaId
                    ]?.length ??
                    0
                ) + 1,

            seudonimo:
                `Postor ${userId}`,

            monto,

            fechaPuja:
                new Date()
                    .toISOString(),

            compradorId:
                Number(userId)
        };

        MOCK.pujas[
            subastaId
        ] =
            MOCK.pujas[
                subastaId
            ] ?? [];

        MOCK.pujas[
            subastaId
        ].push(nueva);

        s.pujaActual =
            monto;

        s.cantidadPujas =
            MOCK.pujas[
                subastaId
            ].length;

        s.ultimaPujaUsuarioId =
            Number(userId);

        const restante =
            new Date(
                s.fechaFin
            ) -
            new Date();

        if (
            restante <=
            60_000 &&
            restante > 0
        ) {

            s.fechaFin =
                new Date(
                    new Date(
                        s.fechaFin
                    ).getTime() +
                    2 * 60_000
                ).toISOString();
        }

        guardarMockEnStorage();

        return mockOk(nueva);
    },

    // ================================================================
    // BILLETERA
    // ================================================================

    async getSaldo() {

        if (!USE_MOCK) {
            return apiFetch(
                '/wallet/balance'
            );
        }

        await delay(200);

        return mockOk(
            MOCK.saldos[
                getUserId()
            ] ??
            {
                saldoTotal: 0,
                saldoRetenido: 0,
                saldoDisponible: 0
            }
        );
    },

    async depositar(monto) {

        if (!USE_MOCK) {

            return apiFetch(
                '/wallet/deposits',
                {
                    method: 'POST',
                    body:
                        JSON.stringify({
                            monto
                        })
                }
            );
        }

        await delay(300);

        const userId =
            getUserId();

        const s =
            MOCK.saldos[userId] ??
            {
                saldoTotal: 0,
                saldoRetenido: 0,
                saldoDisponible: 0
            };

        s.saldoTotal += monto;
        s.saldoDisponible += monto;

        MOCK.saldos[userId] =
            s;

        MOCK.movimientos.unshift({
            id:
                MOCK.movimientos.length +
                1,

            tipo:
                'DEPOSITO',

            monto,

            fecha:
                new Date()
                    .toISOString(),

            subastaId:
                null
        });

        guardarMockEnStorage();

        return mockOk(s);
    },

    async getMovimientos() {

        if (!USE_MOCK) {
            return apiFetch(
                '/wallet/transactions'
            );
        }

        await delay(250);

        return mockOk(
            MOCK.movimientos
        );
    },

    // ================================================================
    // CATEGORÍAS
    // ================================================================

    async getCategorias() {

        if (!USE_MOCK) {
            return apiFetch(
                '/categories'
            );
        }

        await delay(100);

        return mockOk(
            MOCK.categorias
        );
    },

    // ================================================================
    // USUARIO
    // ================================================================

    async getMisPujas() {

        if (!USE_MOCK) {

            return apiFetch(
                `/users/${getUserId()}/bids`
            );
        }

        await delay(250);

        const userId =
            Number(
                getUserId()
            );

        const resultado = [];

        for (
            const s of MOCK.subastas
        ) {

            const pujas =
                MOCK.pujas[s.id] ??
                [];

            const mias =
                pujas.filter(
                    p =>
                        p.compradorId ===
                        userId
                );

            if (
                mias.length === 0
            ) {
                continue;
            }

            resultado.push({

                subastaId:
                    s.id,

                titulo:
                    s.titulo,

                urlImagen:
                    s.urlImagen,

                subastaEstado:
                    s.estado,

                miPuja:
                    Math.max(
                        ...mias.map(
                            p =>
                                p.monto
                        )
                    ),

                pujaActual:
                    s.pujaActual ??
                    s.precioBase,

                esLider:
                    s.ultimaPujaUsuarioId ===
                    userId,

                gano:
                    s.estado ===
                    'FINALIZADA' &&
                    s.ultimaPujaUsuarioId ===
                    userId
            });
        }

        return mockOk(
            resultado
        );
    },

    async getMisPublicaciones() {

        if (!USE_MOCK) {

            return apiFetch(
                `/users/${getUserId()}/auctions`
            );
        }

        await delay(250);

        if (
            Number(
                getUserId()
            ) !== 1
        ) {
            return mockOk([]);
        }

        return mockOk(
            MOCK.subastas.map(
                s => ({
                    id: s.id,
                    titulo: s.titulo,
                    urlImagen:
                        s.urlImagen,
                    estado:
                        s.estado,
                    pujaActual:
                        s.pujaActual ??
                        s.precioBase,
                    cantidadPujas:
                        s.cantidadPujas,
                    recaudacion:
                        s.estado ===
                        'FINALIZADA'
                            ? s.pujaActual
                            : 0
                })
            )
        );
    },
};

// ---------------------------------------------------------------------
// Restablecer datos mock
// ---------------------------------------------------------------------

function resetMock() {

    localStorage.removeItem(
        MOCK_KEY
    );

    location.reload();
}
