using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Servicios;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoriaService _categoriaService;

        public CategoriesController(
            CategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categorias =
                await _categoriaService
                    .GetCategoriasAsync();

            var resultado =
                categorias.Select(c => new
                {
                    id = c.Id,
                    nombre = c.nombre,
                    urlIcono = c.url_icono
                });

            return Ok(resultado);
        }
    }
}