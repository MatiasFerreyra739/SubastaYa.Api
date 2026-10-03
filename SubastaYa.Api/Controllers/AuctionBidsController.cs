using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Models;
using SubastaYa.Api.Servicios;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auctions/{subastaId}/bids")]
    public class AuctionBidsController : ControllerBase
    {
        private readonly PujaService _pujaService;

        public AuctionBidsController(
            PujaService pujaService)
        {
            _pujaService = pujaService;
        }

        [HttpPost]
        public async Task<IActionResult> CrearPuja(
            int subastaId,
            [FromBody] CrearPujaRequest request)
        {
            var usuarioId = ObtenerUsuarioId();

            if (usuarioId == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "[CODE-ERROR] - USUARIO_NO_INDICADO"
                });
            }

            if (request.Monto <= 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "[CODE-ERROR] - MONTO_INVALIDO"
                });
            }

            var puja = new Puja
            {
                subasta_id = subastaId,
                comprador_id = usuarioId.Value,
                monto = request.Monto,
                fecha_puja = DateTime.Now
            };

            try
            {
                var resultado =
                    await _pujaService
                        .CrearPujaAsync(puja);

                return Ok(new
                {
                    id = resultado.id,
                    subastaId = resultado.subasta_id,
                    compradorId = resultado.comprador_id,
                    monto = resultado.monto,
                    fechaPuja = resultado.fecha_puja
                });
            }
            catch (Exception ex)
            {
                if (ex.Message ==
                    "[CODE-ERROR] - CONFLICTO_CONCURRENCIA")
                {
                    return Conflict(new
                    {
                        mensaje = ex.Message
                    });
                }

                if (ex.Message ==
                    "[CODE-ERROR] - SALDO_INSUFICIENTE")
                {
                    return UnprocessableEntity(new
                    {
                        mensaje = ex.Message
                    });
                }

                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
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

    public class CrearPujaRequest
    {
        public decimal Monto { get; set; }
    }
}