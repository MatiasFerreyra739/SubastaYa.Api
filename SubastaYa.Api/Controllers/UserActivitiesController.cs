using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/users/{usuarioId}")]
    public class UserActivitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserActivitiesController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("bids")]
        public async Task<IActionResult> GetMisPujas(
            int usuarioId)
        {
            var pujas =
                await _context.Pujas
                    .AsNoTracking()
                    .Where(
                        p => p.comprador_id ==
                        usuarioId)
                    .OrderByDescending(
                        p => p.fecha_puja)
                    .ToListAsync();

            var resultado =
                new List<object>();

            foreach (var puja in pujas)
            {
                var subasta =
                    await _context.Subastas
                        .AsNoTracking()
                        .FirstOrDefaultAsync(
                            s => s.id ==
                            puja.subasta_id);

                if (subasta == null)
                {
                    continue;
                }

                resultado.Add(new
                {
                    id = puja.id,
                    subastaId = puja.subasta_id,
                    titulo = subasta.titulo,
                    monto = puja.monto,
                    fechaPuja = puja.fecha_puja,
                    estado = subasta.estado,
                    fechaFin = subasta.fecha_fin
                });
            }

            return Ok(resultado);
        }

        [HttpGet("auctions")]
        public async Task<IActionResult> GetMisPublicaciones(
            int usuarioId)
        {
            var subastas =
                await _context.Subastas
                    .AsNoTracking()
                    .Where(
                        s => s.vendedor_id ==
                        usuarioId)
                    .OrderByDescending(
                        s => s.fecha_inicio)
                    .ToListAsync();

            var resultado =
                new List<object>();

            foreach (var subasta in subastas)
            {
                var cantidadPujas =
                    await _context.Pujas
                        .CountAsync(
                            p => p.subasta_id ==
                            subasta.id);

                var ultimaPuja =
                    await _context.Pujas
                        .AsNoTracking()
                        .Where(
                            p => p.subasta_id ==
                            subasta.id)
                        .OrderByDescending(
                            p => p.fecha_puja)
                        .FirstOrDefaultAsync();

                resultado.Add(new
                {
                    id = subasta.id,
                    titulo = subasta.titulo,
                    descripcion = subasta.descripcion,
                    precioBase = subasta.precio_base,
                    incrementoMinimo =
                        subasta.incremento_minimo,
                    pujaActual =
                        ultimaPuja?.monto,
                    cantidadPujas,
                    fechaInicio =
                        subasta.fecha_inicio,
                    fechaFin =
                        subasta.fecha_fin,
                    estado = subasta.estado
                });
            }

            return Ok(resultado);
        }
    }
}