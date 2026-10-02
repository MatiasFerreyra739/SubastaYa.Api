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

        public async Task<List<Billetera>> GetBilleterasAsync()
        {
            return await _context.Billeteras
                .ToListAsync();
        }

        public async Task<Billetera?> GetBilleteraAsync(int id)
        {
            return await _context.Billeteras
                .FindAsync(id);
        }

        public async Task<Billetera> CrearBilleteraAsync(
            Billetera billetera)
        {
            _context.Billeteras.Add(billetera);

            await _context.SaveChangesAsync();

            return billetera;
        }

        public async Task<Billetera?> ActualizarBilleteraAsync(
            int id,
            Billetera billetera)
        {
            var billeteraExistente =
                await _context.Billeteras.FindAsync(id);

            if (billeteraExistente == null)
            {
                return null;
            }

            billeteraExistente.usuario_id =
                billetera.usuario_id;

            billeteraExistente.saldo_total =
                billetera.saldo_total;

            billeteraExistente.saldo_retenido =
                billetera.saldo_retenido;

            billeteraExistente.saldo_disponible =
                billetera.saldo_disponible;

            billeteraExistente.version =
                billetera.version;

            await _context.SaveChangesAsync();

            return billeteraExistente;
        }

        public async Task<bool> EliminarBilleteraAsync(int id)
        {
            var billetera =
                await _context.Billeteras.FindAsync(id);

            if (billetera == null)
            {
                return false;
            }

            _context.Billeteras.Remove(billetera);

            await _context.SaveChangesAsync();

            return true;
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