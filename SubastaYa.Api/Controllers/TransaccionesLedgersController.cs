using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Models;
using SubastaYa.Api.Servicios;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransaccionesLedgersController : ControllerBase
    {
        private readonly TransaccionLedgerService _transaccionLedgerService;

        public TransaccionesLedgersController(
            TransaccionLedgerService transaccionLedgerService)
        {
            _transaccionLedgerService = transaccionLedgerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransaccionesLedgers()
        {
            var transacciones =
                await _transaccionLedgerService
                    .GetTransaccionesLedgersAsync();

            return Ok(transacciones);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaccionLedger(int id)
        {
            var transaccion =
                await _transaccionLedgerService
                    .GetTransaccionLedgerAsync(id);

            if (transaccion == null)
            {
                return NotFound();
            }

            return Ok(transaccion);
        }
    }
}