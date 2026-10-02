using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Servicios
{
    public class TransaccionLedgerService
    {
        private readonly ApplicationDbContext _context;

        public TransaccionLedgerService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Transaccion_Ledger>>
            GetTransaccionesLedgersAsync()
        {
            return await _context.Transacciones_Ledgers
                .ToListAsync();
        }

        public async Task<Transaccion_Ledger?>
            GetTransaccionLedgerAsync(int id)
        {
            return await _context.Transacciones_Ledgers
                .FindAsync(id);
        }
    }
}