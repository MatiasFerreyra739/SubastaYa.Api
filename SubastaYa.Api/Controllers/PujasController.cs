using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;
using SubastaYa.Api.Servicios;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PujasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PujaService _pujaService;

        public PujasController(
            ApplicationDbContext context,
            PujaService pujaService)
        {
            _context = context;
            _pujaService = pujaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPujas()
        {
            var pujas = await _context.Pujas.ToListAsync();

            return Ok(pujas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPuja(int id)
        {
            var puja = await _context.Pujas.FindAsync(id);

            if (puja == null)
                return NotFound();

            return Ok(puja);
        }

        [HttpPost]
        public async Task<IActionResult> CrearPuja(Puja puja)
        {
            try
            {
                var resultado = await _pujaService
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
            var pujaExistente = await _context.Pujas
                .FindAsync(id);

            if (pujaExistente == null)
                return NotFound();

            pujaExistente.subasta_id = puja.subasta_id;
            pujaExistente.comprador_id = puja.comprador_id;
            pujaExistente.monto = puja.monto;
            pujaExistente.fecha_puja = puja.fecha_puja;

            await _context.SaveChangesAsync();

            return Ok(pujaExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPuja(int id)
        {
            var puja = await _context.Pujas
                .FindAsync(id);

            if (puja == null)
                return NotFound();

            _context.Pujas.Remove(puja);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}