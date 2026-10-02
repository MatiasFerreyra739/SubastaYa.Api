using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Servicios
{
    public class UsuarioService
    {
        private readonly ApplicationDbContext _context;

        public UsuarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            return await _context.Usuarios
                .ToListAsync();
        }

        public async Task<Usuario?> GetUsuarioAsync(int id)
        {
            return await _context.Usuarios
                .FindAsync(id);
        }

        public async Task<Usuario> CrearUsuarioAsync(
            Usuario usuario)
        {
            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return usuario;
        }

        public async Task<Usuario?> ActualizarUsuarioAsync(
            int id,
            Usuario usuario)
        {
            var usuarioExistente =
                await _context.Usuarios.FindAsync(id);

            if (usuarioExistente == null)
            {
                return null;
            }

            usuarioExistente.email = usuario.email;
            usuarioExistente.nombre = usuario.nombre;
            usuarioExistente.password_hash = usuario.password_hash;
            usuarioExistente.fecha_registro = usuario.fecha_registro;

            await _context.SaveChangesAsync();

            return usuarioExistente;
        }

        public async Task<bool> EliminarUsuarioAsync(int id)
        {
            var usuario =
                await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return false;
            }

            _context.Usuarios.Remove(usuario);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}