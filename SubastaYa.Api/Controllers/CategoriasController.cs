using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Models;
using SubastaYa.Api.Servicios;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly CategoriaService _categoriaService;

        public CategoriasController(
            CategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias =
                await _categoriaService
                    .GetCategoriasAsync();

            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoria(int id)
        {
            var categoria =
                await _categoriaService
                    .GetCategoriaAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            return Ok(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCategoria(
            Categoria categoria)
        {
            var categoriaCreada =
                await _categoriaService
                    .CrearCategoriaAsync(categoria);

            return CreatedAtAction(
                nameof(GetCategoria),
                new { id = categoriaCreada.Id },
                categoriaCreada);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCategoria(
            int id,
            Categoria categoria)
        {
            var categoriaActualizada =
                await _categoriaService
                    .ActualizarCategoriaAsync(
                        id,
                        categoria);

            if (categoriaActualizada == null)
            {
                return NotFound();
            }

            return Ok(categoriaActualizada);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var eliminado =
                await _categoriaService
                    .EliminarCategoriaAsync(id);

            if (!eliminado)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}