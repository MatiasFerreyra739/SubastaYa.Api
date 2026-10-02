using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Models;
using SubastaYa.Api.Servicios;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BilleterasController : ControllerBase
    {
        private readonly BilleteraService _billeteraService;

        public BilleterasController(
            BilleteraService billeteraService)
        {
            _billeteraService = billeteraService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBilleteras()
        {
            var billeteras =
                await _billeteraService
                    .GetBilleterasAsync();

            return Ok(billeteras);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBilletera(int id)
        {
            var billetera =
                await _billeteraService
                    .GetBilleteraAsync(id);

            if (billetera == null)
            {
                return NotFound();
            }

            return Ok(billetera);
        }

        [HttpPost]
        public async Task<IActionResult> CrearBilletera(
            Billetera billetera)
        {
            var billeteraCreada =
                await _billeteraService
                    .CrearBilleteraAsync(billetera);

            return CreatedAtAction(
                nameof(GetBilletera),
                new { id = billeteraCreada.id },
                billeteraCreada);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarBilletera(
            int id,
            Billetera billetera)
        {
            var billeteraActualizada =
                await _billeteraService
                    .ActualizarBilleteraAsync(
                        id,
                        billetera);

            if (billeteraActualizada == null)
            {
                return NotFound();
            }

            return Ok(billeteraActualizada);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarBilletera(int id)
        {
            var eliminado =
                await _billeteraService
                    .EliminarBilleteraAsync(id);

            if (!eliminado)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("{id}/depositos")]
        public async Task<IActionResult> Depositar(
            int id,
            [FromBody] decimal monto)
        {
            try
            {
                var billetera =
                    await _billeteraService
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