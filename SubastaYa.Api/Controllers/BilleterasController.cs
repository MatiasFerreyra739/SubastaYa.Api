using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;
using SubastaYa.Api.Servicios;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BilleterasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BilleteraService _billeteraService;

        public BilleterasController(
            ApplicationDbContext context,
            BilleteraService billeteraService)
        {
            _context = context;
            _billeteraService = billeteraService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBilleteras()
        {
            var billeteras = await _context.Billeteras.ToListAsync();

            return Ok(billeteras);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBilletera(int id)
        {
            var billetera = await _context.Billeteras.FindAsync(id);

            if (billetera == null)
                return NotFound();

            return Ok(billetera);
        }

        [HttpPost]
        public async Task<IActionResult> CrearBilletera(Billetera billetera)
        {
            _context.Billeteras.Add(billetera);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetBilletera),
                new { id = billetera.id },
                billetera);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarBilletera(
            int id,
            Billetera billetera)
        {
            var billeteraExistente = await _context.Billeteras.FindAsync(id);

            if (billeteraExistente == null)
                return NotFound();

            billeteraExistente.usuario_id = billetera.usuario_id;
            billeteraExistente.saldo_total = billetera.saldo_total;
            billeteraExistente.saldo_retenido = billetera.saldo_retenido;
            billeteraExistente.saldo_disponible = billetera.saldo_disponible;
            billeteraExistente.version = billetera.version;

            await _context.SaveChangesAsync();

            return Ok(billeteraExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarBilletera(int id)
        {
            var billetera = await _context.Billeteras.FindAsync(id);

            if (billetera == null)
                return NotFound();

            _context.Billeteras.Remove(billetera);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/depositos")]
        public async Task<IActionResult> Depositar(
            int id,
            [FromBody] decimal monto)
        {
            try
            {
                var billetera = await _billeteraService
                    .DepositarAsync(id, monto);

                return Ok(billetera);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }
    }
}