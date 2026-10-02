using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Servicios
{
    public class CategoriaService
    {
        private readonly ApplicationDbContext _context;

        public CategoriaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            return await _context.Categorias
                .ToListAsync();
        }

        public async Task<Categoria?> GetCategoriaAsync(int id)
        {
            return await _context.Categorias
                .FindAsync(id);
        }

        public async Task<Categoria> CrearCategoriaAsync(
            Categoria categoria)
        {
            _context.Categorias.Add(categoria);

            await _context.SaveChangesAsync();

            return categoria;
        }

        public async Task<Categoria?> ActualizarCategoriaAsync(
            int id,
            Categoria categoria)
        {
            var categoriaExistente =
                await _context.Categorias.FindAsync(id);

            if (categoriaExistente == null)
            {
                return null;
            }

            categoriaExistente.nombre = categoria.nombre;
            categoriaExistente.url_icono = categoria.url_icono;

            await _context.SaveChangesAsync();

            return categoriaExistente;
        }

        public async Task<bool> EliminarCategoriaAsync(int id)
        {
            var categoria =
                await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return false;
            }

            _context.Categorias.Remove(categoria);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}