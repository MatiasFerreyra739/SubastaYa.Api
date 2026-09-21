using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Servicios
{
    public class CierreSubastasWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CierreSubastasWorker> _logger;

        public CierreSubastasWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<CierreSubastasWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CerrarSubastasVencidasAsync(
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error al cerrar subastas vencidas.");
                }

                // El Worker vuelve a revisar cada 10 segundos.
                await Task.Delay(
                    TimeSpan.FromSeconds(10),
                    stoppingToken);
            }
        }

        private async Task CerrarSubastasVencidasAsync(
            CancellationToken stoppingToken)
        {
            using var scope =
                _scopeFactory.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            var subastasVencidas =
                await context.Subastas
                    .Where(
                        s =>
                            s.estado == "ACTIVA" &&
                            s.fecha_fin <= DateTime.Now)
                    .ToListAsync(stoppingToken);

            foreach (var subasta in subastasVencidas)
            {
                if (stoppingToken.IsCancellationRequested)
                    return;

                await CerrarSubastaAsync(
                    context,
                    subasta,
                    stoppingToken);
            }
        }

        private async Task CerrarSubastaAsync(
            ApplicationDbContext context,
            Subasta subasta,
            CancellationToken stoppingToken)
        {
            await using var transaction =
                await context.Database
                    .BeginTransactionAsync(stoppingToken);

            try
            {
                var ultimaPuja =
                    await context.Pujas
                        .Where(
                            p =>
                                p.subasta_id ==
                                subasta.id)
                        .OrderByDescending(
                            p => p.monto)
                        .ThenByDescending(
                            p => p.fecha_puja)
                        .FirstOrDefaultAsync(
                            stoppingToken);

                if (ultimaPuja == null)
                {
                    subasta.estado = "DESIERTA";
                    subasta.version++;

                    var detalleDesierta =
                        JsonSerializer.Serialize(
                            new
                            {
                                motivo = "SIN_PUJAS"
                            });

                    var auditoria =
                        new Auditoria_Log
                        {
                            entidad = "SUBASTA",
                            entidad_id = subasta.id,
                            accion = "SUBASTA_DESIERTA",
                            usuario_id = null,
                            detalle_json = detalleDesierta,
                            fecha = DateTime.Now
                        };

                    context.Auditorias_Log
                        .Add(auditoria);

                    await context.SaveChangesAsync(
                        stoppingToken);

                    await transaction.CommitAsync(
                        stoppingToken);

                    _logger.LogInformation(
                        "Subasta {SubastaId} cerrada como DESIERTA.",
                        subasta.id);

                    return;
                }

                var comprador =
                    await context.Billeteras
                        .FirstOrDefaultAsync(
                            b =>
                                b.usuario_id ==
                                ultimaPuja.comprador_id,
                            stoppingToken);

                var vendedor =
                    await context.Billeteras
                        .FirstOrDefaultAsync(
                            b =>
                                b.usuario_id ==
                                subasta.vendedor_id,
                            stoppingToken);

                if (comprador == null)
                    throw new Exception(
                        "[CODE-ERROR] - BILLETERA_COMPRADOR_NO_ENCONTRADA");

                if (vendedor == null)
                    throw new Exception(
                        "[CODE-ERROR] - BILLETERA_VENDEDOR_NO_ENCONTRADA");

                if (comprador.saldo_retenido <
                    ultimaPuja.monto)
                {
                    throw new Exception(
                        "[CODE-ERROR] - SALDO_RETENIDO_INSUFICIENTE");
                }

                // El dinero retenido deja de pertenecer
                // al comprador y pasa al vendedor.
                comprador.saldo_retenido -=
                    ultimaPuja.monto;

                comprador.saldo_total -=
                    ultimaPuja.monto;

                comprador.version++;

                vendedor.saldo_total +=
                    ultimaPuja.monto;

                vendedor.saldo_disponible +=
                    ultimaPuja.monto;

                vendedor.version++;

                subasta.estado = "FINALIZADA";
                subasta.version++;

                var transferencia =
                    new Transaccion_Ledger
                    {
                        billetera_id =
                            vendedor.id,

                        tipo = "VENTA",

                        monto =
                            ultimaPuja.monto,

                        fecha = DateTime.Now,

                        subasta_id =
                            subasta.id
                    };

                context.Transacciones_Ledgers
                    .Add(transferencia);

                // Generamos JSON válido independientemente
                // de la configuración regional.
                var detalleFinalizacion =
                    JsonSerializer.Serialize(
                        new
                        {
                            comprador_id =
                                ultimaPuja.comprador_id,

                            monto =
                                ultimaPuja.monto
                        });

                var auditoriaFinalizacion =
                    new Auditoria_Log
                    {
                        entidad = "SUBASTA",

                        entidad_id =
                            subasta.id,

                        accion =
                            "SUBASTA_FINALIZADA",

                        usuario_id =
                            ultimaPuja.comprador_id,

                        detalle_json =
                            detalleFinalizacion,

                        fecha = DateTime.Now
                    };

                context.Auditorias_Log
                    .Add(auditoriaFinalizacion);

                await context.SaveChangesAsync(
                    stoppingToken);

                await transaction.CommitAsync(
                    stoppingToken);

                _logger.LogInformation(
                    "Subasta {SubastaId} finalizada. Ganador: usuario {UsuarioId}. Monto: {Monto}.",
                    subasta.id,
                    ultimaPuja.comprador_id,
                    ultimaPuja.monto);
            }
            catch
            {
                await transaction.RollbackAsync(
                    stoppingToken);

                throw;
            }
        }
    }
}