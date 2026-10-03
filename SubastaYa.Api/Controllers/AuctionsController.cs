using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Servicios;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly SubastaService _subastaService;

        public AuctionsController(
            SubastaService subastaService)
        {
            _subastaService = subastaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuctions()
        {
            int? usuarioId = ObtenerUsuarioId();

            var subastas =
                await _subastaService
                    .GetSubastasListadoAsync(usuarioId);

            return Ok(subastas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuction(
            int id)
        {
            int? usuarioId = ObtenerUsuarioId();

            var subasta =
                await _subastaService
                    .GetSubastaDetalleAsync(
                        id,
                        usuarioId);

            if (subasta == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "[CODE-ERROR] - SUBASTA_NO_ENCONTRADA"
                });
            }

            return Ok(subasta);
        }

        private int? ObtenerUsuarioId()
        {
            if (!Request.Headers.TryGetValue(
                    "X-User-Id",
                    out var valor))
            {
                return null;
            }

            if (int.TryParse(
                    valor.ToString(),
                    out var usuarioId))
            {
                return usuarioId;
            }

            return null;
        }
    }
}