using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _context.Usuarios.ToListAsync();

            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetUsuario),
                new { id = usuario.id },
                usuario);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarUsuario(int id, Usuario usuario)
        {
            var usuarioExistente = await _context.Usuarios.FindAsync(id);

            if (usuarioExistente == null)
            {
                return NotFound();
            }

            usuarioExistente.email = usuario.email;
            usuarioExistente.nombre = usuario.nombre;
            usuarioExistente.password_hash = usuario.password_hash;
            usuarioExistente.fecha_registro = usuario.fecha_registro;

            await _context.SaveChangesAsync();

            return Ok(usuarioExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}