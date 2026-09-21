using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;
using SubastaYa.Api.Servicios;
using Xunit;

namespace SubastaYa.Api.Tests
{
    public class ConcurrenciaTests
    {
        [Fact]
        public async Task DosPujasSimultaneas_UnaAceptada_OtraEnConflicto()
        {
            var options =
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(
                        "Server=.\\SQLEXPRESS;Database=SubastaYaDb;Trusted_Connection=True;TrustServerCertificate=True;")
                    .Options;

            int subastaId = 1;
            int comprador1Id = 2;
            int comprador2Id = 3;

            decimal montoPuja = 115000;

            DateTime fechaFinOriginal;
            string estadoOriginal;
            int versionOriginal;

            decimal saldoTotal1Original;
            decimal saldoRetenido1Original;
            decimal saldoDisponible1Original;
            int versionBilletera1Original;

            decimal saldoTotal2Original;
            decimal saldoRetenido2Original;
            decimal saldoDisponible2Original;
            int versionBilletera2Original;

            int ultimoIdPujaOriginal;
            int ultimoIdLedgerOriginal;
            int ultimoIdAuditoriaOriginal;

            using (var contextoInicial =
                new ApplicationDbContext(options))
            {
                var subasta =
                    await contextoInicial.Subastas
                        .FirstAsync(s => s.id == subastaId);

                var billetera1 =
                    await contextoInicial.Billeteras
                        .FirstAsync(b => b.usuario_id == comprador1Id);

                var billetera2 =
                    await contextoInicial.Billeteras
                        .FirstAsync(b => b.usuario_id == comprador2Id);

                fechaFinOriginal = subasta.fecha_fin;
                estadoOriginal = subasta.estado;
                versionOriginal = subasta.version;

                saldoTotal1Original = billetera1.saldo_total;
                saldoRetenido1Original = billetera1.saldo_retenido;
                saldoDisponible1Original = billetera1.saldo_disponible;
                versionBilletera1Original = billetera1.version;

                saldoTotal2Original = billetera2.saldo_total;
                saldoRetenido2Original = billetera2.saldo_retenido;
                saldoDisponible2Original = billetera2.saldo_disponible;
                versionBilletera2Original = billetera2.version;

                ultimoIdPujaOriginal =
                    await contextoInicial.Pujas
                        .Where(p => p.subasta_id == subastaId)
                        .Select(p => (int?)p.id)
                        .MaxAsync() ?? 0;

                ultimoIdLedgerOriginal =
                    await contextoInicial.Transacciones_Ledgers
                        .Select(t => (int?)t.id)
                        .MaxAsync() ?? 0;

                ultimoIdAuditoriaOriginal =
                    await contextoInicial.Auditorias_Log
                        .Select(a => (int?)a.id)
                        .MaxAsync() ?? 0;

                // Preparamos la subasta para que esté vigente
                // durante la prueba.
                subasta.estado = "ACTIVA";
                subasta.fecha_inicio =
                    DateTime.Now.AddMinutes(-5);
                subasta.fecha_fin =
                    DateTime.Now.AddMinutes(10);

                await contextoInicial.SaveChangesAsync();
            }

            try
            {
                var ambosListos =
                    new TaskCompletionSource<bool>(
                        TaskCreationOptions.RunContinuationsAsynchronously);

                int llegaronAlPuntoDeControl = 0;

                ConcurrencyTestHook.OnSubastaLeida =
                    async () =>
                    {
                        if (
                            Interlocked.Increment(
                                ref llegaronAlPuntoDeControl) == 2)
                        {
                            ambosListos.TrySetResult(true);
                        }

                        await ambosListos.Task;
                    };

                await using var contexto1 =
                    new ApplicationDbContext(options);

                await using var contexto2 =
                    new ApplicationDbContext(options);

                var auditoria1 =
                    new AuditoriaService(contexto1);

                var auditoria2 =
                    new AuditoriaService(contexto2);

                var servicio1 =
                    new PujaService(
                        contexto1,
                        auditoria1);

                var servicio2 =
                    new PujaService(
                        contexto2,
                        auditoria2);

                var puja1 =
                    new Puja
                    {
                        subasta_id = subastaId,
                        comprador_id = comprador1Id,
                        monto = montoPuja
                    };

                var puja2 =
                    new Puja
                    {
                        subasta_id = subastaId,
                        comprador_id = comprador2Id,
                        monto = montoPuja
                    };

                var tarea1 =
                    EjecutarPujaAsync(
                        servicio1,
                        puja1);

                var tarea2 =
                    EjecutarPujaAsync(
                        servicio2,
                        puja2);

                var resultados =
                    await Task.WhenAll(
                        tarea1,
                        tarea2);

                var exitos =
                    resultados.Count(r => r.Exito);

                var conflictos =
                    resultados.Count(
                        r =>
                            !r.Exito &&
                            r.Error ==
                            "[CODE-ERROR] - CONFLICTO_CONCURRENCIA");

                Assert.Equal(1, exitos);

                Assert.Equal(1, conflictos);
            }
            finally
            {
                // Desactivamos el punto de sincronización.
                ConcurrencyTestHook.OnSubastaLeida = null;

                // Restauramos la base de datos al estado
                // anterior a la prueba.
                await using var contextoLimpieza =
                    new ApplicationDbContext(options);

                var pujasNuevas =
                    await contextoLimpieza.Pujas
                        .Where(
                            p =>
                                p.subasta_id == subastaId &&
                                p.id > ultimoIdPujaOriginal)
                        .ToListAsync();

                contextoLimpieza.Pujas.RemoveRange(
                    pujasNuevas);

                var ledgersNuevos =
                    await contextoLimpieza.Transacciones_Ledgers
                        .Where(
                            t =>
                                t.id > ultimoIdLedgerOriginal &&
                                t.subasta_id == subastaId)
                        .ToListAsync();

                contextoLimpieza.Transacciones_Ledgers
                    .RemoveRange(ledgersNuevos);

                var auditoriasNuevas =
                    await contextoLimpieza.Auditorias_Log
                        .Where(
                            a =>
                                a.id > ultimoIdAuditoriaOriginal &&
                                a.entidad_id == subastaId)
                        .ToListAsync();

                contextoLimpieza.Auditorias_Log
                    .RemoveRange(auditoriasNuevas);

                var billetera1 =
                    await contextoLimpieza.Billeteras
                        .FirstAsync(
                            b =>
                                b.usuario_id ==
                                comprador1Id);

                billetera1.saldo_total =
                    saldoTotal1Original;

                billetera1.saldo_retenido =
                    saldoRetenido1Original;

                billetera1.saldo_disponible =
                    saldoDisponible1Original;

                billetera1.version =
                    versionBilletera1Original;

                var billetera2 =
                    await contextoLimpieza.Billeteras
                        .FirstAsync(
                            b =>
                                b.usuario_id ==
                                comprador2Id);

                billetera2.saldo_total =
                    saldoTotal2Original;

                billetera2.saldo_retenido =
                    saldoRetenido2Original;

                billetera2.saldo_disponible =
                    saldoDisponible2Original;

                billetera2.version =
                    versionBilletera2Original;

                var subasta =
                    await contextoLimpieza.Subastas
                        .FirstAsync(
                            s =>
                                s.id == subastaId);

                subasta.fecha_fin =
                    fechaFinOriginal;

                subasta.estado =
                    estadoOriginal;

                subasta.version =
                    versionOriginal;

                await contextoLimpieza.SaveChangesAsync();
            }
        }

        private static async Task<(bool Exito, string? Error)>
            EjecutarPujaAsync(
                PujaService servicio,
                Puja puja)
        {
            try
            {
                await servicio.CrearPujaAsync(puja);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}