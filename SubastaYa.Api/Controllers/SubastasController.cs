using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Models;
using SubastaYa.Api.Servicios;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubastasController : ControllerBase
    {
        private readonly SubastaService _subastaService;

        public SubastasController(SubastaService subastaService)
        {
            _subastaService = subastaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSubastas()
        {
            var subastas =
                await _subastaService.GetSubastasAsync();

            return Ok(subastas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubasta(int id)
        {
            var subasta =
                await _subastaService.GetSubastaAsync(id);

            if (subasta == null)
            {
                return NotFound();
            }

            return Ok(subasta);
        }

        [HttpPost]
        public async Task<IActionResult> CrearSubasta(
            Subasta subasta)
        {
            var subastaCreada =
                await _subastaService
                    .CrearSubastaAsync(subasta);

            return CreatedAtAction(
                nameof(GetSubasta),
                new { id = subastaCreada.id },
                subastaCreada);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarSubasta(
            int id,
            Subasta subasta)
        {
            var subastaActualizada =
                await _subastaService
                    .ActualizarSubastaAsync(id, subasta);

            if (subastaActualizada == null)
            {
                return NotFound();
            }

            return Ok(subastaActualizada);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarSubasta(int id)
        {
            var eliminado =
                await _subastaService
                    .EliminarSubastaAsync(id);

            if (!eliminado)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}