using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Models;
using SubastaYa.Api.Servicios;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PujasController : ControllerBase
    {
        private readonly PujaService _pujaService;

        public PujasController(PujaService pujaService)
        {
            _pujaService = pujaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPujas()
        {
            var pujas =
                await _pujaService.GetPujasAsync();

            return Ok(pujas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPuja(int id)
        {
            var puja =
                await _pujaService.GetPujaAsync(id);

            if (puja == null)
            {
                return NotFound();
            }

            return Ok(puja);
        }

        [HttpPost]
        public async Task<IActionResult> CrearPuja(Puja puja)
        {
            try
            {
                var resultado =
                    await _pujaService
                        .CrearPujaAsync(puja);

                return CreatedAtAction(
                    nameof(GetPuja),
                    new { id = resultado.id },
                    resultado);
            }
            catch (Exception ex)
            {
                if (ex.Message ==
                    "[CODE-ERROR] - CONFLICTO_CONCURRENCIA")
                {
                    return Conflict(new
                    {
                        error = ex.Message
                    });
                }

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPuja(
            int id,
            Puja puja)
        {
            var pujaActualizada =
                await _pujaService
                    .ActualizarPujaAsync(id, puja);

            if (pujaActualizada == null)
            {
                return NotFound();
            }

            return Ok(pujaActualizada);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPuja(int id)
        {
            var eliminado =
                await _pujaService
                    .EliminarPujaAsync(id);

            if (!eliminado)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}