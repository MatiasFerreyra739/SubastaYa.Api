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

        public UserActivitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("bids")]
        public async Task<IActionResult> GetMisPujas(int usuarioId)
        {
            var subastas =
                await _context.Subastas
                    .AsNoTracking()
                    .ToListAsync();

            var resultado = new List<object>();

            foreach (var subasta in subastas)
            {
                var pujas =
                    await _context.Pujas
                        .AsNoTracking()
                        .Where(p =>
                            p.subasta_id ==
                            subasta.id)
                        .OrderByDescending(
                            p => p.fecha_puja)
                        .ToListAsync();

                var misPujas =
                    pujas
                        .Where(p =>
                            p.comprador_id ==
                            usuarioId)
                        .ToList();

                if (misPujas.Count == 0)
                    continue;

                var ultimaPuja =
                    pujas.FirstOrDefault();

                var miPuja =
                    misPujas.Max(
                        p => p.monto);

                var pujaActual =
                    ultimaPuja?.monto ??
                    subasta.precio_base;

                var esLider =
                    ultimaPuja != null &&
                    ultimaPuja.comprador_id ==
                    usuarioId;

                var gano =
                    subasta.estado ==
                    "FINALIZADA" &&
                    esLider;

                resultado.Add(new
                {
                    subastaId = subasta.id,
                    titulo = subasta.titulo,
                    urlImagen = subasta.url_imagen,
                    subastaEstado = subasta.estado,
                    miPuja,
                    pujaActual,
                    esLider,
                    gano
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
                    .Where(s =>
                        s.vendedor_id ==
                        usuarioId)
                    .OrderByDescending(
                        s => s.fecha_inicio)
                    .ToListAsync();

            var resultado = new List<object>();

            foreach (var subasta in subastas)
            {
                var cantidadPujas =
                    await _context.Pujas
                        .CountAsync(
                            p =>
                                p.subasta_id ==
                                subasta.id);

                var ultimaPuja =
                    await _context.Pujas
                        .AsNoTracking()
                        .Where(
                            p =>
                                p.subasta_id ==
                                subasta.id)
                        .OrderByDescending(
                            p => p.fecha_puja)
                        .FirstOrDefaultAsync();

                var recaudacion =
                    subasta.estado ==
                    "FINALIZADA"
                        ? ultimaPuja?.monto
                        : 0;

                resultado.Add(new
                {
                    id = subasta.id,
                    titulo = subasta.titulo,
                    urlImagen = subasta.url_imagen,
                    estado = subasta.estado,
                    pujaActual =
                        ultimaPuja?.monto ??
                        subasta.precio_base,
                    cantidadPujas,
                    recaudacion
                });
            }

            return Ok(resultado);
        }
    }
}
