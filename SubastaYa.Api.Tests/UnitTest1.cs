using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using SubastaYa.Api.Data;
using SubastaYa.Api.Hubs;
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
            var connectionString =
                "Server=.\\SQLEXPRESS;Database=SubastaYaDb;Trusted_Connection=True;TrustServerCertificate=True;";

            var subastaId = 1;
            var comprador1Id = 2;
            var comprador2Id = 3;
            var montoPuja = 125000m;

            int versionSubastaOriginal;
            string estadoOriginal;
            DateTime fechaInicioOriginal;
            DateTime fechaFinOriginal;

            decimal saldoDisponible1Original;
            decimal saldoRetenido1Original;
            int versionBilletera1Original;

            decimal saldoDisponible2Original;
            decimal saldoRetenido2Original;
            int versionBilletera2Original;

            await using (var contextoOriginal =
                new ApplicationDbContext(
                    new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseSqlServer(connectionString)
                        .Options))
            {
                var subasta =
                    await contextoOriginal.Subastas
                        .AsNoTracking()
                        .FirstAsync(s => s.id == subastaId);

                var billetera1 =
                    await contextoOriginal.Billeteras
                        .AsNoTracking()
                        .FirstAsync(b => b.usuario_id == comprador1Id);

                var billetera2 =
                    await contextoOriginal.Billeteras
                        .AsNoTracking()
                        .FirstAsync(b => b.usuario_id == comprador2Id);

                versionSubastaOriginal = subasta.version;
                estadoOriginal = subasta.estado;
                fechaInicioOriginal = subasta.fecha_inicio;
                fechaFinOriginal = subasta.fecha_fin;

                saldoDisponible1Original =
                    billetera1.saldo_disponible;

                saldoRetenido1Original =
                    billetera1.saldo_retenido;

                versionBilletera1Original =
                    billetera1.version;

                saldoDisponible2Original =
                    billetera2.saldo_disponible;

                saldoRetenido2Original =
                    billetera2.saldo_retenido;

                versionBilletera2Original =
                    billetera2.version;
            }

            await using (var contextoInicial =
                new ApplicationDbContext(
                    new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseSqlServer(connectionString)
                        .Options))
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

                subasta.estado = "ACTIVA";
                subasta.fecha_inicio = DateTime.Now.AddMinutes(-10);
                subasta.fecha_fin = DateTime.Now.AddMinutes(10);

                billetera1.saldo_retenido = 0;
                billetera1.saldo_disponible =
                    billetera1.saldo_total;

                billetera2.saldo_retenido = 0;
                billetera2.saldo_disponible =
                    billetera2.saldo_total;

                await contextoInicial.SaveChangesAsync();
            }

            var barrera =
                new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            var contadorLecturas = 0;

            ConcurrencyTestHook.OnSubastaLeida =
                async () =>
                {
                    if (Interlocked.Increment(
                            ref contadorLecturas) == 2)
                    {
                        barrera.TrySetResult(true);
                    }

                    await barrera.Task.WaitAsync(
                        TimeSpan.FromSeconds(10));
                };

            var resultados =
                new[]
                {
                    (Exito: false, Error: ""),
                    (Exito: false, Error: "")
                };

            try
            {
                var hubClients =
                    new Mock<IHubClients>();

                var clientProxy =
                    new Mock<IClientProxy>();

                hubClients
                    .Setup(
                        h => h.Group(
                            It.IsAny<string>()))
                    .Returns(clientProxy.Object);

                clientProxy
                    .Setup(
                        c => c.SendCoreAsync(
                            It.IsAny<string>(),
                            It.IsAny<object?[]>(),
                            It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var hubContext =
                    new Mock<IHubContext<SubastaHub>>();

                hubContext
                    .SetupGet(h => h.Clients)
                    .Returns(hubClients.Object);

                await using var contexto1 =
                    new ApplicationDbContext(
                        new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseSqlServer(connectionString)
                            .Options);

                await using var contexto2 =
                    new ApplicationDbContext(
                        new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseSqlServer(connectionString)
                            .Options);

                var auditoria1 =
                    new AuditoriaService(contexto1);

                var auditoria2 =
                    new AuditoriaService(contexto2);

                var servicio1 =
                    new PujaService(
                        contexto1,
                        auditoria1,
                        hubContext.Object);

                var servicio2 =
                    new PujaService(
                        contexto2,
                        auditoria2,
                        hubContext.Object);

                var puja1 =
                    new Puja
                    {
                        subasta_id = subastaId,
                        comprador_id = comprador1Id,
                        monto = montoPuja,
                        fecha_puja = DateTime.Now
                    };

                var puja2 =
                    new Puja
                    {
                        subasta_id = subastaId,
                        comprador_id = comprador2Id,
                        monto = montoPuja,
                        fecha_puja = DateTime.Now
                    };

                var tarea1 =
                    Task.Run(
                        async () =>
                        {
                            try
                            {
                                await servicio1.CrearPujaAsync(puja1);
                                resultados[0] =
                                    (true, "");
                            }
                            catch (Exception ex)
                            {
                                resultados[0] =
                                    (false, ex.Message);
                            }
                        });

                var tarea2 =
                    Task.Run(
                        async () =>
                        {
                            try
                            {
                                await servicio2.CrearPujaAsync(puja2);
                                resultados[1] =
                                    (true, "");
                            }
                            catch (Exception ex)
                            {
                                resultados[1] =
                                    (false, ex.Message);
                            }
                        });

                await Task.WhenAll(tarea1, tarea2);

                Console.WriteLine(
                    $"RESULTADO 1: Exito={resultados[0].Exito}, Error={resultados[0].Error}");

                Console.WriteLine(
                    $"RESULTADO 2: Exito={resultados[1].Exito}, Error={resultados[1].Error}");

                var exitos =
                    resultados.Count(
                        r => r.Exito);

                var conflictos =
                    resultados.Count(
                        r => r.Error ==
                            "[CODE-ERROR] - CONFLICTO_CONCURRENCIA");

                Assert.Equal(1, exitos);
                Assert.Equal(1, conflictos);
            }
            finally
            {
                ConcurrencyTestHook.OnSubastaLeida = null;

                await using var contextoLimpieza =
                    new ApplicationDbContext(
                        new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseSqlServer(connectionString)
                            .Options);

                var pujasNuevas =
                    await contextoLimpieza.Pujas
                        .Where(
                            p =>
                                p.subasta_id == subastaId &&
                                (p.comprador_id == comprador1Id ||
                                 p.comprador_id == comprador2Id) &&
                                p.monto == montoPuja)
                        .ToListAsync();

                if (pujasNuevas.Count > 0)
                {
                    contextoLimpieza.Pujas.RemoveRange(
                        pujasNuevas);
                }

                var ledgersNuevos =
                    await contextoLimpieza.Transacciones_Ledgers
                        .Where(
                            l =>
                                l.subasta_id == subastaId &&
                                (l.tipo == "RETENCION" ||
                                 l.tipo == "LIBERACION"))
                        .ToListAsync();

                if (ledgersNuevos.Count > 0)
                {
                    contextoLimpieza.Transacciones_Ledgers
                        .RemoveRange(
                            ledgersNuevos);
                }

                var auditoriasNuevas =
                    await contextoLimpieza.Auditorias_Log
                        .Where(
                            a =>
                                a.entidad_id == subastaId &&
                                (a.accion == "PUJA_RECHAZADA" ||
                                 a.accion == "EXTENSION_ANTI_SNIPING"))
                        .ToListAsync();

                if (auditoriasNuevas.Count > 0)
                {
                    contextoLimpieza.Auditorias_Log
                        .RemoveRange(
                            auditoriasNuevas);
                }

                var subasta =
                    await contextoLimpieza.Subastas
                        .FirstAsync(s => s.id == subastaId);

                var billetera1 =
                    await contextoLimpieza.Billeteras
                        .FirstAsync(b => b.usuario_id == comprador1Id);

                var billetera2 =
                    await contextoLimpieza.Billeteras
                        .FirstAsync(b => b.usuario_id == comprador2Id);

                subasta.version =
                    versionSubastaOriginal;

                subasta.estado =
                    estadoOriginal;

                subasta.fecha_inicio =
                    fechaInicioOriginal;

                subasta.fecha_fin =
                    fechaFinOriginal;

                billetera1.saldo_disponible =
                    saldoDisponible1Original;

                billetera1.saldo_retenido =
                    saldoRetenido1Original;

                billetera1.version =
                    versionBilletera1Original;

                billetera2.saldo_disponible =
                    saldoDisponible2Original;

                billetera2.saldo_retenido =
                    saldoRetenido2Original;

                billetera2.version =
                    versionBilletera2Original;

                await contextoLimpieza.SaveChangesAsync();
            }
        }
    }
}