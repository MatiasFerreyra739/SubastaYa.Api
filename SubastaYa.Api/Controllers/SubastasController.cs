using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubastasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SubastasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSubastas()
        {
            var subastas = await _context.Subastas.ToListAsync();

            return Ok(subastas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubasta(int id)
        {
            var subasta = await _context.Subastas.FindAsync(id);

            if (subasta == null)
            {
                return NotFound();
            }

            return Ok(subasta);
        }

        [HttpPost]
        public async Task<IActionResult> CrearSubasta(Subasta subasta)
        {
            _context.Subastas.Add(subasta);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetSubasta),
                new { id = subasta.id },
                subasta);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarSubasta(int id, Subasta subasta)
        {
            var subastaExistente = await _context.Subastas.FindAsync(id);

            if (subastaExistente == null)
            {
                return NotFound();
            }

            subastaExistente.vendedor_id = subasta.vendedor_id;
            subastaExistente.categoria_id = subasta.categoria_id;
            subastaExistente.titulo = subasta.titulo;
            subastaExistente.descripcion = subasta.descripcion;
            subastaExistente.url_imagen = subasta.url_imagen;
            subastaExistente.precio_base = subasta.precio_base;
            subastaExistente.incremento_minimo = subasta.incremento_minimo;
            subastaExistente.fecha_inicio = subasta.fecha_inicio;
            subastaExistente.fecha_fin = subasta.fecha_fin;
            subastaExistente.estado = subasta.estado;
            subastaExistente.version = subasta.version;

            await _context.SaveChangesAsync();

            return Ok(subastaExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarSubasta(int id)
        {
            var subasta = await _context.Subastas.FindAsync(id);

            if (subasta == null)
            {
                return NotFound();
            }

            _context.Subastas.Remove(subasta);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}