using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Servicios
{
    public class PujaService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditoriaService _auditoriaService;

        public PujaService(
            ApplicationDbContext context,
            AuditoriaService auditoriaService)
        {
            _context = context;
            _auditoriaService = auditoriaService;
        }

        public async Task<Puja> CrearPujaAsync(Puja puja)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var subasta = await _context.Subastas
                    .FirstOrDefaultAsync(
                        s => s.id == puja.subasta_id);

                if (subasta == null)
                    throw new Exception(
                        "[CODE-ERROR] - SUBASTA_NO_ENCONTRADA");

                if (subasta.estado != "ACTIVA")
                    throw new Exception(
                        "[CODE-ERROR] - SUBASTA_NO_ACTIVA");

                var ahora = DateTime.Now;

                if (ahora < subasta.fecha_inicio ||
                    ahora >= subasta.fecha_fin)
                    throw new Exception(
                        "[CODE-ERROR] - SUBASTA_FUERA_DE_HORARIO");

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(
                        u => u.id == puja.comprador_id);

                if (usuario == null)
                    throw new Exception(
                        "[CODE-ERROR] - COMPRADOR_NO_ENCONTRADO");

                if (subasta.vendedor_id == puja.comprador_id)
                    throw new Exception(
                        "[CODE-ERROR] - VENDEDOR_NO_PUEDE_PUJAR");

                var billetera = await _context.Billeteras
                    .FirstOrDefaultAsync(
                        b => b.usuario_id == puja.comprador_id);

                if (billetera == null)
                    throw new Exception(
                        "[CODE-ERROR] - BILLETERA_NO_ENCONTRADA");

                if (puja.monto < subasta.precio_base)
                    throw new Exception(
                        "[CODE-ERROR] - MONTO_MENOR_PRECIO_BASE");

                if (billetera.saldo_disponible < puja.monto)
                    throw new Exception(
                        "[CODE-ERROR] - SALDO_INSUFICIENTE");

                var ultimaPuja = await _context.Pujas
                    .Where(
                        p => p.subasta_id == puja.subasta_id)
                    .OrderByDescending(
                        p => p.fecha_puja)
                    .FirstOrDefaultAsync();

                if (ultimaPuja != null &&
                    puja.monto <
                    ultimaPuja.monto +
                    subasta.incremento_minimo)
                {
                    throw new Exception(
                        "[CODE-ERROR] - INCREMENTO_MINIMO_NO_CUMPLIDO");
                }

                if (ultimaPuja != null)
                {
                    var billeteraAnterior =
                        await _context.Billeteras
                            .FirstOrDefaultAsync(
                                b => b.usuario_id ==
                                ultimaPuja.comprador_id);

                    if (billeteraAnterior != null)
                    {
                        billeteraAnterior.saldo_retenido -=
                            ultimaPuja.monto;

                        billeteraAnterior.saldo_disponible +=
                            ultimaPuja.monto;

                        billeteraAnterior.version++;

                        var liberacionLedger =
                            new Transaccion_Ledger
                            {
                                billetera_id =
                                    billeteraAnterior.id,

                                tipo = "LIBERACION",

                                monto = ultimaPuja.monto,

                                fecha = DateTime.Now,

                                subasta_id =
                                    ultimaPuja.subasta_id
                            };

                        _context.Transacciones_Ledgers
                            .Add(liberacionLedger);
                    }
                }

                billetera.saldo_disponible -=
                    puja.monto;

                billetera.saldo_retenido +=
                    puja.monto;

                billetera.version++;

                puja.fecha_puja = DateTime.Now;

                _context.Pujas.Add(puja);

                var transaccionLedger =
                    new Transaccion_Ledger
                    {
                        billetera_id =
                            billetera.id,

                        tipo = "RETENCION",

                        monto = puja.monto,

                        fecha = DateTime.Now,

                        subasta_id =
                            puja.subasta_id
                    };

                _context.Transacciones_Ledgers
                    .Add(transaccionLedger);

                var segundosRestantes =
                    (subasta.fecha_fin - DateTime.Now).TotalSeconds;

                if (segundosRestantes <= 60)
                {
                    subasta.fecha_fin =
                        subasta.fecha_fin.AddMinutes(2);

                    var auditoria =
                        new Auditoria_Log
                        {
                            entidad = "SUBASTA",

                            entidad_id = subasta.id,

                            accion = "EXTENSION_ANTI_SNIPING",

                            usuario_id = puja.comprador_id,

                            detalle_json =
                                "{\"segundos_extension\":120}",

                            fecha = DateTime.Now
                        };

                    _context.Auditorias_Log.Add(auditoria);
                }

                // Cada puja aceptada modifica la versión
                // de la subasta para controlar concurrencia.
                subasta.version++;

                // Punto de sincronización utilizado únicamente
                // durante la prueba de concurrencia.
                if (ConcurrencyTestHook.OnSubastaLeida != null)
                {
                    await ConcurrencyTestHook.OnSubastaLeida();
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return puja;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();

                _context.ChangeTracker.Clear();

                var auditoria =
                    new Auditoria_Log
                    {
                        entidad = "PUJA",

                        entidad_id = puja.subasta_id,

                        accion = "PUJA_RECHAZADA",

                        usuario_id = puja.comprador_id,

                        detalle_json =
                            "{\"error\":\"[CODE-ERROR] - CONFLICTO_CONCURRENCIA\"}",

                        fecha = DateTime.Now
                    };

                _context.Auditorias_Log.Add(auditoria);

                await _context.SaveChangesAsync();

                throw new Exception(
                    "[CODE-ERROR] - CONFLICTO_CONCURRENCIA");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _context.ChangeTracker.Clear();

                var auditoria =
                    new Auditoria_Log
                    {
                        entidad = "PUJA",

                        entidad_id = puja.subasta_id,

                        accion = "PUJA_RECHAZADA",

                        usuario_id = puja.comprador_id,

                        detalle_json =
                            $"{{\"error\":\"{ex.Message}\"}}",

                        fecha = DateTime.Now
                    };

                _context.Auditorias_Log.Add(auditoria);

                await _context.SaveChangesAsync();

                throw;
            }
        }
    }
}