using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Servicios
{
    public class SubastaService
    {
        private readonly ApplicationDbContext _context;

        public SubastaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Subasta>> GetSubastasAsync()
        {
            return await _context.Subastas
                .ToListAsync();
        }

        public async Task<Subasta?> GetSubastaAsync(int id)
        {
            return await _context.Subastas
                .FindAsync(id);
        }

        public async Task<Subasta> CrearSubastaAsync(
            Subasta subasta)
        {
            _context.Subastas.Add(subasta);

            await _context.SaveChangesAsync();

            return subasta;
        }

        public async Task<Subasta?> ActualizarSubastaAsync(
            int id,
            Subasta subasta)
        {
            var subastaExistente =
                await _context.Subastas.FindAsync(id);

            if (subastaExistente == null)
            {
                return null;
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

            return subastaExistente;
        }

        public async Task<bool> EliminarSubastaAsync(int id)
        {
            var subasta =
                await _context.Subastas.FindAsync(id);

            if (subasta == null)
            {
                return false;
            }

            _context.Subastas.Remove(subasta);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}