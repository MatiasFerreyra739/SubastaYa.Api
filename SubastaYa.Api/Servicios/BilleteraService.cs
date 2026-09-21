using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Servicios
{
    public class BilleteraService
    {
        private readonly ApplicationDbContext _context;

        public BilleteraService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Billetera> DepositarAsync(
            int billeteraId,
            decimal monto)
        {
            if (monto <= 0)
                throw new Exception(
                    "[CODE-ERROR] - MONTO_DEPOSITO_INVALIDO");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var billetera = await _context.Billeteras
                    .FirstOrDefaultAsync(
                        b => b.id == billeteraId);

                if (billetera == null)
                    throw new Exception(
                        "[CODE-ERROR] - BILLETERA_NO_ENCONTRADA");

                billetera.saldo_total += monto;
                billetera.saldo_disponible += monto;
                billetera.version++;

                var transaccionLedger =
                    new Transaccion_Ledger
                    {
                        billetera_id = billetera.id,
                        tipo = "DEPOSITO",
                        monto = monto,
                        fecha = DateTime.Now,
                        subasta_id = null
                    };

                _context.Transacciones_Ledgers
                    .Add(transaccionLedger);

                var auditoria =
                    new Auditoria_Log
                    {
                        entidad = "BILLETERA",
                        entidad_id = billetera.id,
                        accion = "DEPOSITO_MANUAL",
                        usuario_id = billetera.usuario_id,
                        detalle_json =
                            $"{{\"monto\":{monto}}}",
                        fecha = DateTime.Now
                    };

                _context.Auditorias_Log
                    .Add(auditoria);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return billetera;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();

                throw new Exception(
                    "[CODE-ERROR] - CONFLICTO_CONCURRENCIA");
            }
        }
    }
}