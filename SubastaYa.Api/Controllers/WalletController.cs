using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Servicios;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/wallet")]
    public class WalletController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BilleteraService _billeteraService;

        public WalletController(
            ApplicationDbContext context,
            BilleteraService billeteraService)
        {
            _context = context;
            _billeteraService = billeteraService;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
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

            var billetera =
                await _context.Billeteras
                    .FirstOrDefaultAsync(
                        b => b.usuario_id ==
                        usuarioId.Value);

            if (billetera == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "[CODE-ERROR] - BILLETERA_NO_ENCONTRADA"
                });
            }

            return Ok(new
            {
                saldoTotal = billetera.saldo_total,
                saldoRetenido = billetera.saldo_retenido,
                saldoDisponible = billetera.saldo_disponible
            });
        }

        [HttpPost("deposits")]
        public async Task<IActionResult> Depositar(
            [FromBody] DepositoRequest request)
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

            var billetera =
                await _context.Billeteras
                    .FirstOrDefaultAsync(
                        b => b.usuario_id ==
                        usuarioId.Value);

            if (billetera == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "[CODE-ERROR] - BILLETERA_NO_ENCONTRADA"
                });
            }

            try
            {
                var resultado =
                    await _billeteraService
                        .DepositarAsync(
                            billetera.id,
                            request.Monto);

                return Ok(new
                {
                    saldoTotal =
                        resultado.saldo_total,

                    saldoRetenido =
                        resultado.saldo_retenido,

                    saldoDisponible =
                        resultado.saldo_disponible
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
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

            var billetera =
                await _context.Billeteras
                    .FirstOrDefaultAsync(
                        b => b.usuario_id ==
                        usuarioId.Value);

            if (billetera == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "[CODE-ERROR] - BILLETERA_NO_ENCONTRADA"
                });
            }

            var movimientos =
                await _context.Transacciones_Ledgers
                    .Where(
                        t => t.billetera_id ==
                        billetera.id)
                    .OrderByDescending(
                        t => t.fecha)
                    .ToListAsync();

            return Ok(movimientos);
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

    public class DepositoRequest
    {
        public decimal Monto { get; set; }
    }
}