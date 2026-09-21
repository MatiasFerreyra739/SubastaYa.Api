using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransaccionesLedgersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransaccionesLedgersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransaccionesLedgers()
        {
            var transacciones = await _context.Transacciones_Ledgers
                .ToListAsync();

            return Ok(transacciones);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaccionLedger(int id)
        {
            var transaccion = await _context.Transacciones_Ledgers
                .FindAsync(id);

            if (transaccion == null)
                return NotFound();

            return Ok(transaccion);
        }
    }
}