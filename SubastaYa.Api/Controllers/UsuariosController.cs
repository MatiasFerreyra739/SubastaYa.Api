using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Models;
using SubastaYa.Api.Servicios;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;

        public UsuariosController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios =
                await _usuarioService.GetUsuariosAsync();

            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var usuario =
                await _usuarioService.GetUsuarioAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario(
            Usuario usuario)
        {
            var usuarioCreado =
                await _usuarioService
                    .CrearUsuarioAsync(usuario);

            return CreatedAtAction(
                nameof(GetUsuario),
                new { id = usuarioCreado.id },
                usuarioCreado);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarUsuario(
            int id,
            Usuario usuario)
        {
            var usuarioActualizado =
                await _usuarioService
                    .ActualizarUsuarioAsync(
                        id,
                        usuario);

            if (usuarioActualizado == null)
            {
                return NotFound();
            }

            return Ok(usuarioActualizado);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var eliminado =
                await _usuarioService
                    .EliminarUsuarioAsync(id);

            if (!eliminado)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}